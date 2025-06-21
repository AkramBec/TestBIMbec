namespace SearchFittingsInParkingSpaces.Models;

public class PluginView3D
{
    private readonly Document _doc;
    private ViewFamilyType _type;
    public ViewFamilyType Type { get => _type;}

    public PluginView3D(Document doc)
    {
        _doc = doc;
        _type = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(vft => vft.ViewFamily == ViewFamily.ThreeDimensional &&
                          vft.Name.Equals("Сгенерированный вид", StringComparison.OrdinalIgnoreCase));
    }
}