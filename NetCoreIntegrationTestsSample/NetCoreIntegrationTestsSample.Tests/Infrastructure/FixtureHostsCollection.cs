using Xunit;

namespace NetCoreIntegrationTestsSample.Tests.Infrastructure
{
    [CollectionDefinition(Name)]
    public class FixtureHostsCollection : ICollectionFixture<HostFixture>
    {
        public const string Name = "Hosts collection";
    }
}