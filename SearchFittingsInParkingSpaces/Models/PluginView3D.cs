namespace SearchFittingsInParkingSpaces.Models;

public class PluginView3D
{
    public static ViewFamilyType GetType(Document doc)
    {
        var Type = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(vft => vft.ViewFamily == ViewFamily.ThreeDimensional &&
                          vft.Name.Equals("Сгенерированный вид", StringComparison.OrdinalIgnoreCase));
        return Type;
    }
}