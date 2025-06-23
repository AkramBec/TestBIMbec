namespace SearchFittingsInParkingSpaces.Models;

public class ParkingCollector
{
    private static List<FamilyInstance> _parkingPlaces;
    public static List<FamilyInstance> ParkingPlaces { get => _parkingPlaces; }
    public static List<FamilyInstance> GetParkingPlaces(Document doc)
    {
        _parkingPlaces = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Parking)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .ToList();
        return _parkingPlaces;
    }
}