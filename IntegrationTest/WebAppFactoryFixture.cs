using Xunit;

namespace IntegrationTest
{
    public class WebAppFactoryFixture : IClassFixture<WebAppFactory>
    {
        protected readonly WebAppFactory Factory;

        public WebAppFactoryFixture(WebAppFactory factory)
        {
            Factory = factory;
        }
    }
}
