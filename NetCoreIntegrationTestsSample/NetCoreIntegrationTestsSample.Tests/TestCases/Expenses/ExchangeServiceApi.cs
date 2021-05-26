using System;
using System.Net;
using System.Threading.Tasks;
using NetCoreIntegrationTestsSample.Infrastructure;
using NetCoreIntegrationTestsSample.Tests.Infrastructure;

namespace NetCoreIntegrationTestsSample.Tests.TestCases.Expenses
{
    public class ExchangeServiceApi : IDisposable
    {
        private readonly ExchangeServiceConfig _exchangeServiceConfig;
        private ApiMock _exchangeServiceApiMock;
        
        public ExchangeServiceApi(
            ExchangeServiceConfig exchangeServiceConfig)
        {
            _exchangeServiceConfig = exchangeServiceConfig;
        }

        public void start_with(
            decimal exchangeRate)
        {
            var endpoint = new Endpoint()
            {
                HttpCode = HttpStatusCode.OK,
                HttpMethod = HttpMethod.Get,
                Result = exchangeRate.ToString(),
                Url = $"/exchange"
            };
            
            _exchangeServiceApiMock = new ApiMock(
                _exchangeServiceConfig.Url, endpoint);
            
            _exchangeServiceApiMock.Start();
        }
        
        public void Dispose()
        {
            _exchangeServiceApiMock.Dispose();
        }
    }
}