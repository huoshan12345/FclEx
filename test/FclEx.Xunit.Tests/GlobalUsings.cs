// Global using directives

global using FclEx.Extensions;
global using FclEx.Helpers;
global using FclEx.Utils;
global using Xunit.Sdk;
global using Xunit;
global using System.Reflection;
global using System.Linq.Expressions;
global using System.Text.Json.Serialization;

#if FCLEX_XUNIT_V3
global using Xunit.v3;
#else
global using Xunit.Abstractions;
#endif