namespace FclEx.Extensions;

public static class ConsoleExtensions
{
    extension(Console)
    {
        public static void WriteLineIfNotTesting(string value)
        {
            if (Environment.IsUnderTest == false)
                Console.WriteLine(value);
        }
    }
}