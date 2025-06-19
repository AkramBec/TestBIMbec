namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<ElementId> FindFittingsOverParking(Document doc)
    {
        var result = new List<ElementId>();

        // Парковочные места
        var parkingPlaces = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Parking)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .ToList();

        // Арматура труб и воздуховодов
        var fittings = new FilteredElementCollector(doc)
            .WherePasses(new LogicalOrFilter(
                new ElementCategoryFilter(BuiltInCategory.OST_DuctFitting),
                new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting)))
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .ToList();

        foreach (var fitting in fittings)
        {
            var fittingBox = fitting.get_BoundingBox(null);
            if (fittingBox == null) continue;

            var fittingZ = (fittingBox.Min.Z + fittingBox.Max.Z) / 2;

            foreach (var parking in parkingPlaces)
            {
                var parkingBox = parking.get_BoundingBox(null);
                if (parkingBox == null) continue;

                var parkingZ = (parkingBox.Min.Z + parkingBox.Max.Z) / 2;

                if (fittingZ > parkingZ && IsHorizontallyAligned(parkingBox, fittingBox))
                {
                    result.Add(fitting.Id);
                    break;
                }
            }
        }

        return result;
    }

    private static bool IsHorizontallyAligned(BoundingBoxXYZ parking, BoundingBoxXYZ fitting)
    {
        bool xOverlap = parking.Min.X < fitting.Max.X && parking.Max.X > fitting.Min.X;
        bool yOverlap = parking.Min.Y < fitting.Max.Y && parking.Max.Y > fitting.Min.Y;
        return xOverlap && yOverlap;
    }
}
