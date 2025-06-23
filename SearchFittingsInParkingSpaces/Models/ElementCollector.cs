namespace SearchFittingsInParkingSpaces.Models;

public class ElementCollector
{
    private static List<ElementMetaData> _elementsMetaData = new();
    public static IReadOnlyList<ElementMetaData> ElementsMetaData  => _elementsMetaData; 
    
    private static List<Element> _elements;
    public static List<Element> Elements {get=>_elements;}

    public static List<ElementMetaData> CollectAll(Document doc, FilterRule filterRule)
    {
        _elementsMetaData.Clear();
        _elements = new List<Element>();
        _elements.AddRange(filterRule.Apply(doc));
        
        _elementsMetaData.AddRange(_elements.Select(el =>new ElementMetaData(el,Transform.Identity)));

        foreach (var link in RevitLinks.Links)
        {
            var linkDoc = link.GetLinkDocument();
            if (linkDoc == null) continue;
            var linkTransform = link.GetTotalTransform();

            var linkFloors = filterRule.Apply(linkDoc);

            _elements.AddRange(linkFloors);

            _elementsMetaData.AddRange(linkFloors.Select(el =>new ElementMetaData(el,Transform.Identity,linkDoc.Title)));
        }

        return _elementsMetaData;
    }
}