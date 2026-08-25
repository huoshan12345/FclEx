public static class GlobalConstants
{
    public static class Directories
    {
        public static DirectoryInfo TestData { get; } = new(Path.Combine(AppContext.BaseDirectory, "TestData"));
    }
}