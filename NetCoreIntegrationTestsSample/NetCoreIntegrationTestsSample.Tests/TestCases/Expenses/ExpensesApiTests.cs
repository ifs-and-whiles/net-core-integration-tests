using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using NetCoreIntegrationTestsSample.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using V1 = NetCoreIntegrationTestsSample.Expenses.Contracts.Expenses.V1;

namespace NetCoreIntegrationTestsSample.Tests.TestCases.Expenses
{
    [Collection(FixtureHostsCollection.Name)]
    public class ExpensesApiTests : TestFixture
    {
        public ExpensesApiTests(HostFixture hostFixture, ITestOutputHelper output) 
            : base(hostFixture, output)
        {
        }
        
        [Theory, AutoData]
        public async Task can_create_expense(V1.Commands.Create createCommand)
        {
            var plnToEurExchangeRate = 0.22m;
            
            ExchangeServiceApi.start_with(plnToEurExchangeRate);
            
            var id = await CreateExpense(createCommand);

            await assert_message_in_queue<V1.Messages.ExpenseCreatedEvent>(
                x=> x.Id.Should().Be(id));
            
            var result = await GetSingleExpenseFromAPI(id);

            result.Should().BeEquivalentTo(new V1.Queries.Expense()
            {
                Id = id,
                Date = createCommand.Date,
                Title = createCommand.Title,
                TotalAmountInPLN = createCommand.TotalAmountInPLN,
                TotalAmountInEUR = createCommand.TotalAmountInPLN * plnToEurExchangeRate
            });
        }

        [Fact]
        public async Task should_return_404_when_expense_does_not_exist()
        {
            var fakeGuid = Guid.NewGuid();
            
            Func<Task> result = async () => await GetSingleExpenseFromAPI(fakeGuid);
            
            result.Should().ThrowExactly<TestApiCallErrorException>()
                .And
                .ErrorDetails
                .StatusCode
                .Should().Be(404);
        }

        private async Task<V1.Queries.Expense> GetSingleExpenseFromAPI(Guid id) =>
            await post_to_sut_api_with_response<V1.Queries.GetExpense, V1.Queries.Expense>(
                "api/v1/expenses/get-expense", new V1.Queries.GetExpense()
                {
                    Id = id
                });

        private async Task<Guid> CreateExpense(V1.Commands.Create command)
        {
            var response = await post_to_sut_api_with_response<V1.Commands.Create, V1.Commands.CreateResponse>(
                "api/v1/expenses/create", command);
            
            return response.ExpenseId;
        }
    }
}