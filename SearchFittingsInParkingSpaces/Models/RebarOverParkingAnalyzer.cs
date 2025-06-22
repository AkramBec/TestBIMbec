using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        var result = new List<FittingInfo>();

        var viewType = PluginView3D.GetType(doc);

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
        
        // Поиск перекрытий в АР и КР
        var floors = new List<Element>();

        floors.AddRange(new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WhereElementIsNotElementType());

        foreach (var link in links)
        {
            var linkDoc = link.GetLinkDocument();
            if (linkDoc == null) continue;
            
            Transform linkTransform = link.GetTransform();

            var linkFloors = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList();

            floors.AddRange(linkFloors);
        }

        //Удаление старых видов
        using (var txDelete = new Transaction(doc, "Удаление старых видов"))
        {
            txDelete.Start();
            ViewsOfType.Delete(doc, viewType);
            txDelete.Commit();
        }
        
        // Арматура труб и воздуховодов в связях

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

                var linkFittings = new FilteredElementCollector(linkDoc)
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementMulticategoryFilter(categories))
                    .ToList();

                foreach (var fitting in linkFittings)
                {
                    var view = ViewsOfType.Create(doc, viewType, fitting);
                    var fittingInfo = new FittingInfo(fitting, linkDoc.Title, view.Id);

                    view.Name = fittingInfo.ViewName;
                    
                    result.Add(fittingInfo);
                }
            }

            tx.Commit();
        }

        return result;
    }
}
