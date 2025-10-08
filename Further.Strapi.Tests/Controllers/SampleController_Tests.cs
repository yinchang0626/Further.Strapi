using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Further.Strapi.Samples;

namespace Further.Strapi.Tests.Controllers;

/* This is a demo test class to show how to test HTTP API controllers.
 * You can delete this class freely.
 *
 * See https://docs.abp.io/en/abp/latest/Testing for more about automated tests.
 */

public class ExampleController_Tests : StrapiIntegrationTestBase
{
    [Fact]
    public async Task GetAsync()
    {
        var response = await GetResponseAsObjectAsync<SampleDto>("api/strapi/example");
        response.Value.ShouldBe(42);
    }
}