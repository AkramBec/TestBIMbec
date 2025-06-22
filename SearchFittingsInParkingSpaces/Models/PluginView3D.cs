namespace SearchFittingsInParkingSpaces.Models;

public class PluginView3D
{
    private static ViewFamilyType _type;
    public static ViewFamilyType Type { get => _type; }

    public static ViewFamilyType GetType(Document doc)
    {
        _type = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(vft => vft.ViewFamily == ViewFamily.ThreeDimensional &&
                          vft.Name.Equals("Сгенерированный вид", StringComparison.OrdinalIgnoreCase));
        return _type;
    }
    
    public static void Initialize(Document doc)
    {
        using (var txDelete = new Transaction(doc, "Удаление старых видов"))
        {
            txDelete.Start();
            ViewsOfType.Delete(doc, Type);
            txDelete.Commit();
        }
    }
}