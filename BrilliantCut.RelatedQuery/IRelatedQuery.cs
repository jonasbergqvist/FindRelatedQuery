
namespace BrilliantCut.RelatedQuery
{
    /// <summary>
    /// Registration of related queries that can be used by the <see cref="RelatedQueryFactory"/> to get related items
    /// </summary>
    public interface IRelatedQuery
    {
        /// <summary>
        /// Registration of a related query
        /// </summary>
        /// <param name="relatedFilterRegistration">The related filter registration</param>
        /// <returns></returns>
        RelatedQueryRegistration RegistryQuery(RelatedQueryRegistration relatedFilterRegistration);
    }
}
