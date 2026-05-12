// Global using directives

global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Net.Http;
global using System.Net.NetworkInformation;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using Duende.IdentityModel;
global using FclEx.DependencyInjection;
global using FclEx.Extensions;
global using FclEx.Helpers;
global using FclEx.Http;
global using FclEx.Http.Tests;
global using FclEx.Http.Tests.Models;
global using FclEx.Utils;
global using FclEx.Xunit;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Http;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Microsoft.IdentityModel.Tokens;
global using Polly.Timeout;
global using Xunit;
global using xRetry.v3;
global using static FclEx.Http.Tests.HttpServerFixture;

#if !NET5_0_OR_GREATER
global using SocketsHttpHandler = System.Net.Http.StandardSocketsHttpHandler;
#endif

#if NET8_0_OR_GREATER
global using FclEx.AspNetCore;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.RequestDecompression;
#endif