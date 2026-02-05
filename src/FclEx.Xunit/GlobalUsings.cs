// Global using directives

global using System;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Text;
global using FclEx.Extensions;
global using FclEx.Utils;
global using FclEx.Xunit;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Xunit;
global using Xunit.Sdk;

#if FCLEX_XUNIT_V3
global using Xunit.v3;
#else
global using Xunit.Abstractions;
#endif