namespace NetCoreIntegrationTestsSample.Infrastructure
{
    public class RabbitMqConfig
    {
        public string HostUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string LocalQueue { get; set; }
    }
}