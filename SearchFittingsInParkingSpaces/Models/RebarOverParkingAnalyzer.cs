using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var doc = uiApp.ActiveUIDocument.Document;
        var result = new List<FittingInfo>();
        
        var viewType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(vft => vft.ViewFamily == ViewFamily.ThreeDimensional &&
            vft.Name.Equals("Сгенерированный вид", StringComparison.OrdinalIgnoreCase));

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

            foreach (var fitting in linkFittings)
            {
                // Используем саму связь как точку обзора
                var viewId = CreateIsometricViewWithSectionBox(doc, viewType, linkInstance);

                result.Add(new FittingInfo
                {
                    ElementId = fitting.Id.IntegerValue,
                    Category = fitting.Category?.Name ?? "Нет категории",
                    DocumentTitle = linkDoc.Title,
                    LinkInstanceId = linkInstance.Id,
                    ViewId = viewId
                });
            }
        }
        return result;
    }
    
    private static ElementId CreateIsometricViewWithSectionBox(Document doc, ViewFamilyType viewType, Element element)
    {
        ElementId viewId = ElementId.InvalidElementId;
        using (var tx = new Transaction(doc, "Create 3D View"))
        {
            tx.Start();
            var view = View3D.CreateIsometric(doc, viewType.Id);
            var box = element.get_BoundingBox(null);
            if (box != null)
            {
                var expand = 0.5;
                box.Min -= new XYZ(expand, expand, expand);
                box.Max += new XYZ(expand, expand, expand);
                view.SetSectionBox(box);
            }

            viewId = view.Id;
            tx.Commit();
        }

        return viewId;
    }
}
