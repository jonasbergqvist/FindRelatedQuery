using System.Collections.Generic;
using System.Linq;
using EPiServer.Find;

namespace BrilliantCut.RelatedQuery
{
    public class RelatedSearchResults<TResult> : IEnumerable<TResult>
    {
        private IEnumerable<TResult> _relatedSearchResult;

        public RelatedSearchResults(SearchResults<TResult> searchResults)
        {
            UnfilteredSearchResult = searchResults;
        }

        public IEnumerable<TResult> RelatedSearchResult
        {
            get { return _relatedSearchResult ?? (_relatedSearchResult = UnfilteredSearchResult.Hits.Where(x => x.Score > 1).Select(x => x.Document)); }
        }

        public SearchResults<TResult> UnfilteredSearchResult { get; private set; }

        public IEnumerator<TResult> GetEnumerator()
        {
            return RelatedSearchResult.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
