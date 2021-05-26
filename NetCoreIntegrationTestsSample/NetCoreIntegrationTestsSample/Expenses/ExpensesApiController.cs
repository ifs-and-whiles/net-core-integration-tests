using System;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using NetCoreIntegrationTestsSample.Infrastructure;
using V1 = NetCoreIntegrationTestsSample.Expenses.Contracts.Expenses.V1;
namespace NetCoreIntegrationTestsSample.Expenses
{
    [ApiController, Route( "api/v1/expenses")]
    public class ExpensesApiController : ControllerBase
    {
        private readonly IDocumentStore _documentStore;
        private readonly IBus _bus;

        public ExpensesApiController(IDocumentStore documentStore,
            IBus bus)
        {
            _documentStore = documentStore;
            _bus = bus;
        }
        
        [HttpPost, Route("create")]
        public async Task<V1.Commands.CreateResponse> CreateExpense(
            [FromBody]V1.Commands.Create command)
        {
            var expenseId = Guid.NewGuid();
            
            await _bus.Send(new V1.Messages.CreateExpenseCommand()
            {
                Title = command.Title,
                Date = command.Date,
                Id = expenseId,
                TotalAmountInPLN = command.TotalAmountInPLN
            });
               
            return new V1.Commands.CreateResponse()
            {
                ExpenseId = expenseId
            };
        }

        [HttpPost, Route("get-expense")]
        public async Task<V1.Queries.Expense> GetExpense(
            [FromBody]V1.Queries.GetExpense query)
        {
            using var session = _documentStore.OpenSession();
            
            var expense = await session
                .Query<DbExpense>()
                .Where(x => x.Id == query.Id)
                .FirstOrDefaultAsync();

            if (expense == null)
                throw new NotFoundException(
                    $"Expense with Id {query.Id} not found");

            return new V1.Queries.Expense()
            {
                Id = expense.Id,
                Date = expense.Date,
                Title = expense.Title,
                TotalAmountInPLN = expense.TotalAmountInPLN,
                TotalAmountInEUR = expense.TotalAmountInEUR
            };
        }
    }
    
    public class DbExpense
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Title { get; set; }
        public decimal TotalAmountInPLN { get; set; }
        public decimal TotalAmountInEUR { get; set; }
                    
    }
}