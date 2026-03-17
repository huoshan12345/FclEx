// ReSharper disable all
// ReSharper disable InvalidXmlDocComment
#nullable disable
#pragma warning disable IDE0002 // Simplify member access.
#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable IDE0350 // Use implicitly typed lambda
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable xUnit2015 // Do not use typeof expression to check the exception type.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication;
using Microsoft.Win32;
using Xunit;

namespace System
{
    public static partial class PlatformDetection
    {
        public static bool IsRiscV64Process => (int)RuntimeInformation.ProcessArchitecture == 9; // Architecture.RiscV64;
    }
}
