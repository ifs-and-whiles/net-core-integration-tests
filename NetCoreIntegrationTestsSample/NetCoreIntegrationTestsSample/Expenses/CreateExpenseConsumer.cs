using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Marten;
using MassTransit;
using NetCoreIntegrationTestsSample.Infrastructure;
using V1 = NetCoreIntegrationTestsSample.Expenses.Contracts.Expenses.V1;

namespace NetCoreIntegrationTestsSample.Expenses
{
    public class CreateExpenseConsumer : IConsumer<V1.Messages.CreateExpenseCommand>
    {
        private readonly IDocumentStore _documentStore;
        private readonly ExchangeService _exchangeService;

        public CreateExpenseConsumer(
            IDocumentStore documentStore,
            ExchangeService exchangeService)
        {
            _documentStore = documentStore;
            _exchangeService = exchangeService;
        }
        
        public async Task Consume(ConsumeContext<V1.Messages.CreateExpenseCommand> context)
        {
            var exchangeRate = await _exchangeService
                .GetExchangeRateFromPLNToEUR();
            
            using var session = _documentStore.OpenSession();

            session.Store(new DbExpense()
            {
                Id = context.Message.Id,
                Date = context.Message.Date,
                Title = context.Message.Title,
                TotalAmountInPLN = context.Message.TotalAmountInPLN,
                TotalAmountInEUR = context.Message.TotalAmountInPLN * exchangeRate
            });
            
            await session.SaveChangesAsync();

            await context.Publish(new V1.Messages.ExpenseCreatedEvent()
            {
                Id = context.Message.Id
            });
        }
    }

    public class ExchangeService
    {
        private readonly ExchangeServiceConfig _exchangeServiceConfig;

        public ExchangeService(ExchangeServiceConfig exchangeServiceConfig)
        {
            _exchangeServiceConfig = exchangeServiceConfig;
        }

        public async Task<decimal> GetExchangeRateFromPLNToEUR()
        {
            var amountInEUR = await _exchangeServiceConfig
                .Url
                .AppendPathSegment("exchange")
                .SetQueryParams(new
                {
                    to = "EUR",
                    from = "PLN"
                })
                .WithHeaders(new
                {
                    x_rapidapi_key = _exchangeServiceConfig.XRapidAPIKey,
                    x_rapidapi_host = _exchangeServiceConfig.XRapidAPIHost,
                    useQueryString = true
                }, replaceUnderscoreWithHyphen: true)
                .GetStringAsync();

            return decimal.Parse(amountInEUR);
        }
    }
}