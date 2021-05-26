using System;
using System.Collections.Generic;

namespace NetCoreIntegrationTestsSample.Expenses
{
    public class Contracts
    {
        public class Expenses
        {
            public static class V1
            {
                public static class Commands
                {
                    public class Create
                    {
                        public DateTimeOffset Date { get; set; }
                        public string Title { get; set; }
                        public decimal TotalAmountInPLN { get; set; }
                    }
                    
                    public class CreateResponse
                    {
                        public Guid ExpenseId { get; set; }
                    }
                }

                public static class Queries
                {
                    public class GetExpense
                    {
                        public Guid Id { get; set; }
                    }
                    
                    public class Expense
                    {
                        public Guid Id { get; set; }
                        public DateTimeOffset Date { get; set; }
                        public string Title { get; set; }
                        public decimal TotalAmountInPLN { get; set; }
                        public decimal TotalAmountInEUR { get; set; }
                        
                    }
                }

                public static class Messages
                {
                    public class CreateExpenseCommand
                    {
                        public Guid Id { get; set; }
                        public DateTimeOffset Date { get; set; }
                        public string Title { get; set; }
                        public decimal TotalAmountInPLN { get; set; }
                    }

                    public class ExpenseCreatedEvent
                    {
                        public Guid Id { get; set; }
                    }
                }
            }
        }
    }
}