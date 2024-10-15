using System.Collections.Generic;
using Microsoft.Collections.Extensions;

namespace FclEx.Wmi.SourceGenerator.Models;

public class Qualifiers
{
    public List<string> Descriptions { get; } = [];
    public MultiValueDictionary<string, string> Others { get; } = new();
}