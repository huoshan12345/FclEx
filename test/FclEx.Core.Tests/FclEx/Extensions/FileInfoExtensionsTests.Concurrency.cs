namespace FclEx.Extensions;

public partial class FileInfoExtensionsTests
{
    public static TheoryData<bool, bool, bool> ConcurrentTransferCases => new()
    {
        { true, false, false },
        { false, true, false },
        { true, true, true },
        { false, false, true },
        { true, false, true },
        { false, true, true },
    };

    public static TheoryData<bool, bool> ConcurrentDuplicateCases => new()
    {
        { true, true },
        { false, false },
        { true, false },
        { false, true },
    };

    [Theory]
    [MemberData(nameof(ConcurrentTransferCases))]
    public async Task ConcurrentAutoRename_PreservesBothTransfers(
        bool copyFirst, bool copySecond, bool competeForRenamedPath)
    {
        await AssertConcurrentAutoRenameAsync(copyFirst, copySecond, competeForRenamedPath);
    }

    [Theory]
    [MemberData(nameof(ConcurrentDuplicateCases))]
    public async Task ConcurrentIgnoreDuplicate_KeepsOneDestinationAndDeletesOnlyMovedSources(
        bool copyFirst, bool copySecond)
    {
        var directory = CreateTempDir();
        try
        {
            var sourceDirectory = directory.CreateSubdirectory("sources");
            var destinationDirectory = directory.CreateSubdirectory("destinations");
            var destinationPath = Path.Combine(destinationDirectory.FullName, "report.txt");
            var options = new FileTransferOptions { IgnoreConflictIfDuplicate = true };

            for (var iteration = 0; iteration < 20; iteration++)
            {
                var firstSource = CreateTempFile("same", sourceDirectory.FullName, "first.txt");
                var secondSource = CreateTempFile("same", sourceDirectory.FullName, "second.txt");
                var firstSourcePath = firstSource.FullName;
                var secondSourcePath = secondSource.FullName;
                using var start = new Barrier(2);

                var results = await Task.WhenAll(
                    StartConcurrentTransfer(start, firstSource, destinationPath, copyFirst, options),
                    StartConcurrentTransfer(start, secondSource, destinationPath, copySecond, options));

                Assert.All(results, result => Assert.Equal(destinationPath, result.FullName));
                Assert.Equal("same", await File.ReadAllTextAsync(destinationPath));
                Assert.Single(destinationDirectory.GetFiles());
                Assert.Equal(copyFirst, File.Exists(firstSourcePath));
                Assert.Equal(copySecond, File.Exists(secondSourcePath));
                File.Delete(destinationPath);
            }
        }
        finally
        {
            Cleanup(directory);
        }
    }

    [Fact]
    public async Task CopyToAsync_CanceledBeforePublication_RemovesStagedFile()
    {
        var directory = CreateTempDir();
        try
        {
            var source = CreateTempFile("source", directory.FullName, "source.txt");
            var destinationPath = Path.Combine(directory.FullName, "report.txt");
            using var cancellation = new CancellationTokenSource();
            Task<FileInfo> copy;
            bool staged;

            using (FileTransferPathLock.Acquire(destinationPath))
            {
                copy = Task.Factory.StartNew(() => source.CopyToAsync(
                        // ReSharper disable once AccessToDisposedClosure
                        new FileInfo(destinationPath), AutoRenameOptions, cancellation.Token),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
                staged = SpinWait.SpinUntil(
                    () => directory.GetFiles(".fclex-*.tmp").Any(), TimeSpan.FromSeconds(10));
                await cancellation.CancelAsync();
            }

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => copy.WaitAsync(TimeSpan.FromSeconds(10), cancellation.Token));
            Assert.True(staged);
            Assert.False(File.Exists(destinationPath));
            Assert.Empty(directory.GetFiles(".fclex-*.tmp"));
            Assert.Equal("source", await File.ReadAllTextAsync(source.FullName, cancellation.Token));
        }
        finally
        {
            Cleanup(directory);
        }
    }

    private static async Task AssertConcurrentAutoRenameAsync(
        bool copyFirst, bool copySecond, bool competeForRenamedPath)
    {
        var directory = CreateTempDir();
        try
        {
            var sourceDirectory = directory.CreateSubdirectory("sources");
            var destinationDirectory = directory.CreateSubdirectory("destinations");
            var destinationPath = Path.Combine(destinationDirectory.FullName, "report.txt");
            var secondDestinationPath = competeForRenamedPath
                ? Path.Combine(destinationDirectory.FullName, "report_1.txt")
                : destinationPath;

            for (var iteration = 0; iteration < 100; iteration++)
            {
                var firstSource = CreateTempFile("first", sourceDirectory.FullName, "first.txt");
                var secondSource = CreateTempFile("second", sourceDirectory.FullName, "second.txt");
                var firstSourcePath = firstSource.FullName;
                var secondSourcePath = secondSource.FullName;
                if (competeForRenamedPath)
                    await File.WriteAllTextAsync(destinationPath, "existing");

                using var start = new Barrier(2);
                var results = await Task.WhenAll(
                    StartConcurrentTransfer(start, firstSource, destinationPath, copyFirst),
                    StartConcurrentTransfer(start, secondSource, secondDestinationPath, copySecond));

                Assert.NotEqual(results[0].FullName, results[1].FullName);
                Assert.Equal("first", await File.ReadAllTextAsync(results[0].FullName));
                Assert.Equal("second", await File.ReadAllTextAsync(results[1].FullName));
                Assert.Equal(copyFirst, File.Exists(firstSourcePath));
                Assert.Equal(copySecond, File.Exists(secondSourcePath));
                Assert.Equal(competeForRenamedPath ? 3 : 2, destinationDirectory.GetFiles().Length);
                if (competeForRenamedPath)
                    Assert.Equal("existing", await File.ReadAllTextAsync(destinationPath));

                foreach (var file in destinationDirectory.GetFiles())
                    file.Delete();
            }
        }
        finally
        {
            Cleanup(directory);
        }
    }

    private static Task<FileInfo> StartConcurrentTransfer(
        Barrier start, FileInfo source, string destinationPath, bool copy, FileTransferOptions? options = null)
    {
        // Dedicated workers avoid thread-pool starvation while waiting for their peer.
        return Task.Factory.StartNew(() =>
        {
            if (!start.SignalAndWait(TimeSpan.FromSeconds(30)))
                throw new TimeoutException("The concurrent transfer did not reach the start barrier.");

            var destination = new FileInfo(destinationPath);
            return copy
                ? source.CopyToAsync(destination, options ?? AutoRenameOptions).GetAwaiter().GetResult()
                : source.MoveTo(destination, options ?? AutoRenameOptions);
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
}
