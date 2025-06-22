namespace SearchFittingsInParkingSpaces.Models;

public class ElementCollector
{
    private static List<FamilyInstance> _parkingPlaceses;
    public static List<FamilyInstance> ParkingPlaces { get => _parkingPlaceses; }
    public static List<FamilyInstance> GetParkingPlaces(Document doc)
    {
        _parkingPlaceses = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Parking)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .ToList();
        return _parkingPlaceses;
    }
}