using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        var result = new List<FittingInfo>();

        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();
            var links = RevitLinks.Links;
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
                    var view = ViewsOfType.Create(doc, PluginView3D.Type, fitting);
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
