namespace Xunit;

partial class AssertEx
{
    extension(Assert)
    {
        public static void True([DoesNotReturnIf(false)] bool condition, Func<string?>? userMessage)
        {
            if (condition == false)
                Assert.True(condition, userMessage?.Invoke());
        }

        public static void False([DoesNotReturnIf(true)] bool condition, Func<string?>? userMessage)
        {
            if (condition)
                Assert.False(condition, userMessage?.Invoke());
        }

        public static void Equal<T>(T? expected, T? actual, Func<string?>? userMessage)
        {
            try
            {
                Assert.Equal(expected, actual);
            }
            catch (EqualException)
            {
                throw EqualException.ForMismatchedValues(
                    expected: expected.ToAssertionString(),
                    actual: actual.ToAssertionString(),
                    banner: userMessage?.Invoke());
            }
        }

        public static void NotEqual<T>(T? expected, T? actual, Func<string?>? userMessage)
        {
            try
            {
                Assert.NotEqual(expected, actual);
            }
            catch (NotEqualException)
            {
                throw NotEqualException.ForEqualValues(
                    expected: expected.ToAssertionString(),
                    actual: actual.ToAssertionString(),
                    banner: userMessage?.Invoke());
            }
        }

        public static void Default<T>(T? actual, Func<string?>? userMessage = null)
        {
            Assert.Equal(default, actual, userMessage);
        }

        public static void NotDefault<T>(T? actual, Func<string?>? userMessage = null)
        {
            Assert.NotEqual(default, actual, userMessage);
        }

        public static void FileExist(string path)
        {
            Assert.True(File.Exists(path), () => $"File does not exist: {path}");
        }

        public static void DirectoryExist(string path)
        {
            Assert.True(Directory.Exists(path), () => $"Directory does not exist: {path}");
        }

        public static void Exist(FileSystemInfo path)
        {
            Assert.True(path.Exists, () => $"Path does not exist: {path.FullName}");
        }
    }
}