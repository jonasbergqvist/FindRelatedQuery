using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Querying.Queries;
using EPiServer.Find.Cms;

namespace BrilliantCut.RelatedQuery
{
    public class ExcludeContentFilterQuery : IModifyFilterQuery
    {
        public ITypeSearch<TQuery> Filter<TQuery>(IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery, IEnumerable<object> content)
        {
            var contentItems = content.OfType<IContent>();

            var typeBuilder = new FilterBuilder<IContent>(boostQuery.Client);
            typeBuilder = contentItems.Aggregate(typeBuilder, (current, contentItem) => current.And(x => !x.ContentLink.Match(contentItem.ContentLink.ToReferenceWithoutVersion())));

            return boostQuery.Filter(typeBuilder);
        }
    }
}
