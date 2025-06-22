namespace SearchFittingsInParkingSpaces.Models;

public class RevitLinks
{
    private static IEnumerable<RevitLinkInstance> _links;
    public static IEnumerable<RevitLinkInstance> Links { get => _links;}
    
    public static IEnumerable<RevitLinkInstance> LinkInstances(Document doc)
    {
        _links = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>();
        return _links;
    }
}