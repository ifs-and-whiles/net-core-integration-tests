using Marten;
using NetCoreIntegrationTestsSample.Expenses;

namespace NetCoreIntegrationTestsSample.Infrastructure
{
    public class DocumentStoreFactory
    {
        public static IDocumentStore Create(DatabaseConfig config)
        {
            return DocumentStore.For(_ =>
            {
                _.Connection(config.ConnectionString);
                _.UseDefaultSerialization(casing: Casing.CamelCase);
                
                _.Storage.MappingFor(typeof(Contracts.Expenses.V1.Queries.Expense));
            });
        }
    }
}