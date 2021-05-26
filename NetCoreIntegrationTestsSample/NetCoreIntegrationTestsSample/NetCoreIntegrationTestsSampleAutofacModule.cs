using Autofac;
using Marten;
using NetCoreIntegrationTestsSample.Expenses;
using NetCoreIntegrationTestsSample.Infrastructure;

namespace NetCoreIntegrationTestsSample
{
    public class NetCoreIntegrationTestsSampleAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx => DocumentStoreFactory.Create(ctx.Resolve<DatabaseConfig>()))
                .As<IDocumentStore>()
                .SingleInstance();

            builder.RegisterType<ExchangeService>().AsSelf();
        }
    }
}