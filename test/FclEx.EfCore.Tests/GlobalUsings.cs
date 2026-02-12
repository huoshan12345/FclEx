// Global using directives

global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Linq.Expressions;
global using Dapper;
global using FclEx.Domain;
global using FclEx.Extensions;
global using FclEx.Helpers;
global using FclEx.Xunit;
global using Microsoft.Data.SqlClient;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Conventions;
global using Microsoft.EntityFrameworkCore.Storage;
global using Renci.SshNet;
global using Xunit;
global using Xunit.Sdk;
global using static FclEx.EfCore.EfCoreFixture;
#if !DISABLE_NPGSQL
global using Npgsql;
#endif
#if !DISABLE_MYSQL
global using MySql.Data.MySqlClient;
global using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
global using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
global using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
#endif
