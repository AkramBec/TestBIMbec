namespace SearchFittingsInParkingSpaces.Models;

public class ElementInfo
{
    public static 
        List<(ElementMetaData elementMetaData, PlanarFace bottomFace, XYZ originGlobal)> CollectAndFinedBottomFace
        (ElementCollector elementCollector)
    {
        var bottomOfElement = new List<(ElementMetaData elementMetaData, PlanarFace bottomFace, XYZ originGlobal)>();
        foreach (ElementMetaData elementMetaData in elementCollector.ElementsMetaData)
        {
            var floorGeometry = new ElementGeometry(elementMetaData);
            var bottomFace = floorGeometry.GetBottomFace();
            var originGlobal = floorGeometry.OriginGlobal;

            bottomOfElement.Add((elementMetaData, bottomFace, originGlobal));
        }

        return bottomOfElement;
    }
}