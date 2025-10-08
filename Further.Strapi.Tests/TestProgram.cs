using Further.Strapi.Tests;
using Microsoft.AspNetCore.Builder;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("Further.Strapi.csproj"); 
await builder.RunAbpModuleAsync<StrapiTestsModule>(applicationName: "Further.Strapi");

public partial class TestProgram
{
}
