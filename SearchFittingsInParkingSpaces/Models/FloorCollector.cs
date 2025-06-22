namespace SearchFittingsInParkingSpaces.Models;

public class FloorCollector
{
    private static List<Element> _floors;
    public static List<Element> Floors {get=>_floors;}

    public static List<Element> CollectAll(Document doc)
    {
        _floors = new List<Element>();

        _floors.AddRange(new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WhereElementIsNotElementType());
        
        var links = RevitLinks.Links;

        foreach (var link in links)
        {
            var linkDoc = link.GetLinkDocument();
            if (linkDoc == null) continue;

            Transform linkTransform = link.GetTransform();

            var linkFloors = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList();

            _floors.AddRange(linkFloors);
        }

        return _floors;
    }
}