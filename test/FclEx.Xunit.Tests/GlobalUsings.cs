// Global using directives

global using System.Linq.Expressions;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using FclEx.Extensions;
global using FclEx.Helpers;
global using FclEx.Utils;
global using Xunit;
global using Xunit.Sdk;

#if FCLEX_XUNIT_V3
global using Xunit.v3;
#else
global using Xunit.Abstractions;
#endif