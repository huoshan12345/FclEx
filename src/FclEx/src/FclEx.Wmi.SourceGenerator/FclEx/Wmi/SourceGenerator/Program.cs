using System;
using System.Linq;

namespace FclEx.Wmi.SourceGenerator;

internal class Program
{
    private static void Main(string[] args)
    {
        SourceGenerator.GenerateToFiles(args.FirstOrDefault());
    }
}