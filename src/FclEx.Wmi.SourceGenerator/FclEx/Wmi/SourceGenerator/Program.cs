using System.Linq;

namespace FclEx.Wmi.SourceGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            SourceGenerator.Generate(args.First());
        }
    }
}