public static class GlobalConstants
{
    public static class Directories
    {
        public static DirectoryInfo TestData { get; } = new(Path.Combine(AppContext.BaseDirectory, "TestData"));

        private static readonly Lazy<DirectoryInfo?> _solutionRoot = new(() => FindSolutionRoot(AppContext.BaseDirectory, "FclEx"));
        public static DirectoryInfo SolutionRoot => _solutionRoot.Value ?? throw new InvalidOperationException("Solution root not found.");

        private static DirectoryInfo? FindSolutionRoot(string path, string solutionName)
        {
            DirectoryInfo? cur = new(path);
            while (cur != null)
            {
                if (cur.Name is "src" or "test"
                   && cur.Parent?.Name == solutionName)
                {
                    return cur.Parent;
                }

                cur = cur.Parent;
            }

            return null;
        }
    }
}