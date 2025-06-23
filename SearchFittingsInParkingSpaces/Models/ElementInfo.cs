namespace SearchFittingsInParkingSpaces.Models;

public class ElementInfo
{
    private static Element _element;
    public static Element  Element { get => _element;}

    private static PlanarFace _bottomFace;
    public static PlanarFace BottomFace { get => _bottomFace; }

    public static List<(Element element, PlanarFace bottomFace)> CollectAndFinedBottomFace (Document doc, FilterRule filterRule)
    {
        ElementCollector.CollectAll(doc, filterRule);
        var result = new List<(Element, PlanarFace)>();
        foreach (ElementMetaData elementMetaData in ElementCollector.ElementsMetaData)
        {
            _element = elementMetaData.Element;
            var floorGeometry = new ElementGeometry(elementMetaData);
            _bottomFace = floorGeometry.GetBottomFace();

            result.Add((_element, _bottomFace));
        }

        return result;
    }
}