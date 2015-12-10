# FindRelatedQuery
Introduction

A common site scenario is that you want to get related content for some content. This can be used on a commerce site to get additional sales, but also on big CMS sites where you want the user to find related content. An easy CMS example is a news site, where you want to show related news to the current news item.

Nuget source: http://nuget.jobe.employee.episerver.com/

Git hub: https://github.com/jonasbergqvist/FindRelatedQuery

BrilliantCut.RelatedQuery

I have created a nuget package that I call "BrilliantCut.RelatedQuery", which uses EPiServer.Find to get related content automatically. The only thing you need to do is to specify which properties that should be involved in the related query. Here is an example of a registration, which will use two properties with different boosting:

public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .AddRangeFilter<ISize>(x => x.Size, 1.2)
            .AddRangeFilter<ITemperature>(x => x.Temperature, 1.4);
    }
}
The registrated query can now be used to search for related content:

private ITypeSearch<ProductContent> CreateRelatedQuery(RelatedQueryFactory relatedQueryFactory, params object[] content)
{
    return relatedQueryFactory.CreateQuery<DefaultRelationQuery, ProductContent>(content);
}
Register a query

A query needs to be regitred to be able to create a related query. A query will be reigstrated by creating a class, which implements IRelatedQuery. The interface contains one method, which has a "RelatedQueryRegistration" as parameter. The method returns a "RelatedFilterRegistration". The "RelatedQueryRegistration" class contains methods for register boost filters, modify the main search, and modify the filters.

public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .ModifySearchQuery<DefaultSearchQuery>()
            .ModifyExclusionFilterQuery<ExcludeTypeFilterQuery>()
            .AddMatchFilter<ISeason>(x => x.Season, 1.2)
            .AddMatchFilter<IEvent>(x => x.Event, 1.4)
            .AddRangeFilter<ISize>(x => x.Size, 1.6)
            .AddRangeFilter<ITemperature>(x => x.Temperature, 1.3);
    }
}
AddMatchFilter

AddMatchFilter registers a property match. Specify an interface, base class, or concrete class as the generic argument, and choose which property that will be matched in the query. An exact match will be done between the content send in to the related search, and the indexed content.

public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .AddMatchFilter<ISeason>(x => x.Season, 2)
    }
}
AddRangeFilter

AddRangeFilter registers a filter that will take the min and max value from the supplied content for a specific property. Specify an interface, base class, or concrete class as the generic argument, and choose which property that will be used in the query. An range filter will be done between the content send in to the related search, and the indexed content.

public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .AddRangeFilter<ITemperature>(x => x.Temperature, 2);
    }
}
AddFilter

AddFilter registers a custom filter. Specify an interface, base class, or concrete class as the generic argument, and choose which property that will be used in the query. The filter will be used between the content send in to the related search, and the indexed content.

public class CustomFilter : IRelatedFilter<ProductContent>
{
    private readonly IClient _client;
 
    public CustomFilter(IClient client)
    {
        _client = client;
    }
 
    public Filter CreateFilter(IEnumerable<object> content)
    {
        return new FilterBuilder<ProductContent>(_client)
            .And(x => x.Created.GreaterThan(new DateTime(2015, 1, 1)));
    }
 
    public double Boost { get { return 1.5; }}
}

public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .AddFilter<CustomFilter>()
    }
}
Change the default search

The methods AddMatchFilter, AddRangeFilter, and AddFilter will perform boost matching against the index. A search query will be created before those boost filters are added to the query. The search query can be modified, to contain whatever you like before the boost matches are added. The default implementation is a ".For('')" on IClient.

public class CustomSearchQuery : IModifySearchQuery
{
    public IQueriedSearch<TQuery, QueryStringQuery> CreateSearchQuery<TQuery>(IClient client, IEnumerable<object> content)
    {
        return client.Search<TQuery>()
            .For("something");
    }
}
public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .ModifySearchQuery<CustomSearchQuery>()
            .AddRangeFilter<ITemperature>(x => x.Temperature, 1.3);
    }
}
Change the default exclusion filtering

When the boost filters has been added, some "real" filters will be used to exclude some content from the result. The default implementation will exclude the types that was supplied to the related query search. This will result in related content that is of a different type, but that has properties with similar values.

There is also another filter implementation build in that will exclude the items send in to the related query search. Other items of the same types will be returned by the related query search if this one is used.

public class CustomExclusionFilterQuery : IModifyExclusionFilterQuery
{
    public ITypeSearch<TQuery> Filter<TQuery>(IQueriedSearch<TQuery, CustomFiltersScoreQuery> boostQuery, IEnumerable<object> content)
    {
        var typeBuilder = new FilterBuilder<ProductContent>(boostQuery.Client)
            .And(x => x.Created.GreaterThan(new DateTime(2015, 1, 1)));
 
        return boostQuery.Filter(typeBuilder);
    }
}
public class DefaultRelationQuery : IRelatedQuery
{
    public RelatedFilterRegistration RegistryQuery(RelatedQueryRegistration relatedQueryRegistration)
    {
        return relatedQueryRegistration
            .ModifyExclusionFilterQuery<CustomExclusionFilterQuery>()
            .AddMatchFilter<ISeason>(x => x.Season, 1.2)
    }
}
Search for related items

RelatedQueryFactory is the class that will create a query. The method "CreateQuery<T>" takes ContentReferences or objects as parameters, and will return an ITypeSearch<T>. Other filters can then be added to the query, like "FilterForVisitor". It's also possible, and recommended, to use projection before executing the query to elastic search. Finaly it's recomended to use "GetRelatedResult" as executing method. This method will do post-filtering, to make sure only related items are recieved.

private IEnumerable<ContentReference> GetRelatedContentReferences(RelatedQueryFactory relatedQueryFactory, params object[] content)
{
    return relatedQueryFactory.CreateQuery<DefaultRelationQuery, ProductContent>(content)
        .FilterForVisitor()
        .Take(5)
        .Select(x => x.ContentLink)
        .GetRelatedResult();
}
Install package

To install the package on your site, add the following source as a package source in Visual studio: http://nuget.jobe.employee.episerver.com/

For more information how to add a package source, look at the package source section at: https://docs.nuget.org/consume/package-manager-dialog

Get the source code

The source code can be found on github: https://github.com/jonasbergqvist/FindRelatedQuery
