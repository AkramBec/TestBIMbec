namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(Document doc)
    {
        var result = new List<FittingInfo>();

        // Парковочные места
        var parkingPlaces = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Parking)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>()
            .ToList();
        
        // Связи
        
        var links = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>();
        
        // Арматура труб и воздуховодов в связях

        foreach (var linkInstance in links)
        {
            var linkDoc = linkInstance.GetLinkDocument();
            if (linkDoc == null) continue;

            var linkFittings = new FilteredElementCollector(linkDoc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticategoryFilter(new[]
                {
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_DuctTerminal
                }))
                .ToList();

            result.AddRange(linkFittings.Select(e => new FittingInfo
            {
                ElementId = e.Id.IntegerValue,
                Category = e.Category?.Name?? "Нет категории",
                DocumentTitle = linkDoc.Title,
                LinkInstanceId = linkInstance.Id
            }));
        }
        
        // Арматура труб и воздуховодов в модели
        
        var mainFittings = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .WherePasses(new ElementMulticategoryFilter(new[]
            {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_PipeFitting
            }))
            .ToList();
        
        result.AddRange(mainFittings.Select(e => new FittingInfo
        {
            ElementId = e.Id.IntegerValue,
            Category = e.Category?.Name?? "Нет категории",
            DocumentTitle = doc.Title,
            LinkInstanceId = null
        }));

        return result;
    }
}
