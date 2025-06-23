using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;
        
        var categories = JsonCategories.BuiltInCategories;

        var result = new List<FittingInfo>();

        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();
            var links = RevitLinks.Links;
            foreach (var linkInstance in links)
            {
                var linkDoc = linkInstance.GetLinkDocument();
                if (linkDoc == null) continue;

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
