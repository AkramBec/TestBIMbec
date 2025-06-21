using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiApp.ActiveUIDocument.Document;

        var result = new List<FittingInfo>();

        var viewType = new PluginView3D(doc).Type;

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
        
        View3D fallbackViewToActivate = null;
        
        using (var txDelete = new Transaction(doc, "Удаление старых видов"))
        {
            txDelete.Start();
            DeleteViewsOfType(doc, viewType);
            txDelete.Commit();
        }

        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();
            foreach (var linkInstance in links)
            {
                var linkDoc = linkInstance.GetLinkDocument();
                if (linkDoc == null) continue;

                var linkFittings = new FilteredElementCollector(linkDoc)
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementMulticategoryFilter(new[]
                    {
                        BuiltInCategory.OST_PipeAccessory,
                        BuiltInCategory.OST_DuctAccessory,
                        BuiltInCategory.OST_DuctTerminal
                    }))
                    .ToList();

                foreach (var fitting in linkFittings)
                {
                    var view = CreateIsometricViewWithSectionBox(doc, viewType, fitting);
                    var fittingInfo = new FittingInfo(fitting, linkDoc.Title, view.Id);

                    view.Name = fittingInfo.ViewName;
                    
                    result.Add(fittingInfo);
                }
            }

            tx.Commit();
        }

        return result;
    }
    
    private static View3D CreateIsometricViewWithSectionBox(Document doc, ViewFamilyType viewType, Element element)
    {
        ElementId viewId = ElementId.InvalidElementId;

        var view = View3D.CreateIsometric(doc, viewType.Id);
        var box = element.get_BoundingBox(null);
        if (box != null)
        {
            var expand = 0.5;
            box.Min -= new XYZ(expand, expand, expand);
            box.Max += new XYZ(expand, expand, expand);
            view.SetSectionBox(box);
        }

        return view;
    }
    
    private static void DeleteViewsOfType(Document doc, ViewFamilyType viewType)
    {
        var viewsToDelete = new FilteredElementCollector(doc)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .Where(v => !v.IsTemplate && v.GetTypeId() == viewType.Id)
            .Select(v => v.Id)
            .ToList();

        if (viewsToDelete.Any())
        {
            doc.Delete(viewsToDelete);
        }
    }
}
