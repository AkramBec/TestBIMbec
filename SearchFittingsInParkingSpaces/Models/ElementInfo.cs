namespace SearchFittingsInParkingSpaces.Models;

public class ElementInfo
{
    private static Element _element;
    public static Element  Element { get => _element;}

    private static PlanarFace _bottomFace;
    public static PlanarFace BottomFace { get => _bottomFace; }

    //private static List<(ElementMetaData elementMetaData, PlanarFace bottomFace)> _bottomOfElement;
    //public static List<(ElementMetaData elementMetaData, PlanarFace bottomFace, )> BottomOfElement  => _bottomOfElement;

    public static List<(ElementMetaData elementMetaData, PlanarFace bottomFace, XYZ originGlobal)> CollectAndFinedBottomFace (Document doc, FilterRule filterRule)
    {
        var bottomOfElement = new List<(ElementMetaData elementMetaData, PlanarFace bottomFace, XYZ originGlobal)>();
        ElementCollector.CollectAll(doc, filterRule);
        foreach (ElementMetaData elementMetaData in ElementCollector.ElementsMetaData)
        {
            _element = elementMetaData.Element;
            var floorGeometry = new ElementGeometry(elementMetaData);
            _bottomFace = floorGeometry.GetBottomFace();
            var originGlobal = floorGeometry.OriginGlobal;

            bottomOfElement.Add((elementMetaData, _bottomFace, originGlobal));
        }

        return bottomOfElement;
    }
}