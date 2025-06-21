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
                
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string folder = System.IO.Path.GetDirectoryName(assemblyPath);
                string jsonPath = System.IO.Path.Combine(folder, "familyCategories.json");
                List<BuiltInCategory> categories = CategoryLoader.LoadCategories(jsonPath);
                
                /*string jsonPath = @"C:\Path\To\familyCategories.json"; // Или путь рядом с DLL
                List<BuiltInCategory> categories = CategoryLoader.LoadCategories(jsonPath);*/

                var linkFittings = new FilteredElementCollector(linkDoc)
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementMulticategoryFilter(categories))
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
    
    public static View3D SwitchToSafeViewIfNeeded(UIApplication uiApp, ViewFamilyType pluginViewType)
        {
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;
            var activeView = uiDoc.ActiveView as View3D;

            // Если активный вид имеет тип, совпадающий с плагинным — заменяем
            if (activeView.GetTypeId() == pluginViewType.Id)
            {
                // Ищем существующий вид, начинающийся с {3D
                var safeView = new FilteredElementCollector(doc)
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .FirstOrDefault(v => !v.IsTemplate &&
                                         v.Name.Contains("3D", StringComparison.OrdinalIgnoreCase) &&
                                         v.GetTypeId() != pluginViewType.Id);

                if (safeView != null)
                {
                    return safeView;
                }
    
                // Найти ViewFamilyType для обычного 3D-вида, исключая pluginViewType
                var safeViewType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft =>
                        vft.ViewFamily == ViewFamily.ThreeDimensional &&
                        vft.Id != pluginViewType.Id);

                // Если ни один не подошёл — создаём новый стандартный 3D вид
                if (safeViewType != null)
                {
                    using (var tx = new Transaction(doc, "Создание безопасного вида"))
                    {
                        tx.Start();
                        var default3DView = View3D.CreateIsometric(doc, safeViewType.Id);
                        default3DView.Name = "3D";
                        tx.Commit();
                        
                        return default3DView;
                    }
                    
                }
            }
            return activeView;
        }
}
