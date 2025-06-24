namespace SearchFittingsInParkingSpaces.Models;

public class FilterRule
{
    
    private readonly Func<Document, List<Element>> _filterFunc;

    public FilterRule(Func<Document, List<Element>> filterFunc)
    {
        _filterFunc = filterFunc;
    }

    public List<Element> Apply(Document doc)
    {
        return _filterFunc(doc);
    }

    public static FilterRule ByCategory(BuiltInCategory builtInCategory)
    {
        return new FilterRule(doc=> 
            new FilteredElementCollector(doc)
            .OfCategory(builtInCategory)
            .WhereElementIsNotElementType()
            .ToList());
    }
    
    public static FilterRule ByCategories(List<BuiltInCategory> builtInCategories)
    {
        return new FilterRule(doc=>
            new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .WherePasses(new ElementMulticategoryFilter(builtInCategories))
            .ToList());
    }
    public static FilterRule ByCategoryAndElementFilter(BuiltInCategory builtInCategory, ElementFilter filter)
    {
        return new FilterRule(doc=> 
            new FilteredElementCollector(doc)
                .OfCategory(builtInCategory)
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .ToList());
    }
    public static FilterRule ByCategoriesAndElementFilter(List<BuiltInCategory> builtInCategories, ElementFilter filter)
    {
        return new FilterRule(doc=>
            new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticategoryFilter(builtInCategories))
                .WherePasses(filter)
                .ToList());
    }
}