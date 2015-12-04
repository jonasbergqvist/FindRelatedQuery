using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace BrilliantCut.RelatedQuery
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class RelatedQueryInitializationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            context.Locate.Advanced.GetInstance<RelatedFilterRegistry>().RegisterRelatedQueries();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
