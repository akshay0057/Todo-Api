using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApi.IntegrationTests.Helper
{
    public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly HttpClient Client;
        protected readonly CustomWebApplicationFactory Factory;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
        }
    }
}
