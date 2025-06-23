using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<FittingInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;
        
        var parkingMetaDatas = 
            ElementCollector.CollectAll(doc, FilterRule.ByCategory(BuiltInCategory.OST_Parking));
        var floorMetaDatas = 
            ElementInfo.CollectAndFinedBottomFace(doc, FilterRule.ByCategory(BuiltInCategory.OST_Floors));
        var FittingMetaDatas =
            ElementCollector.CollectAll(doc, FilterRule.ByCategories(JsonCategories.BuiltInCategories));

        var result = new List<FittingInfo>();

        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();
            
            

            foreach (var linkInstance in RevitLinks.Links)
            {
                var linkDoc = linkInstance.GetLinkDocument();
                if (linkDoc == null) continue;

                var linkFittings = new FilteredElementCollector(linkDoc)
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementMulticategoryFilter(JsonCategories.BuiltInCategories))
                    .ToList();

                foreach (var fitting in linkFittings)
                {
                    var view = ViewsOfType.Create(doc, PluginView3D.Type, fitting);
                    var fittingInfo = new FittingInfo(fitting, linkDoc.Title, view.Id);

                    view.Name = fittingInfo.ViewName;
                    
                    result.Add(fittingInfo);
                }
            }

            foreach (var parking in parkingMetaDatas)
            {
                var parkGlobalBB = CorrectBoundingBox.ComputeGlobal(parking.Element as FamilyInstance);
                var minZ = parkGlobalBB.Min.Z;
            }

            tx.Commit();
        }

        return result;
    }
}
