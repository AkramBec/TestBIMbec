using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<ResultInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;
        
        var parkingMetaDatas = 
            ElementCollector.CollectAll(doc, FilterRule.ByCategory(BuiltInCategory.OST_Parking));
        var floorMetaDatas = 
            ElementInfo.CollectAndFinedBottomFace(doc, FilterRule.ByCategory(BuiltInCategory.OST_Floors));
        var FittingMetaDatas =
            ElementCollector.CollectAll(doc, FilterRule.ByCategories(JsonCategories.BuiltInCategories));

        var result = new List<ResultInfo>();

        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();
            
            foreach (var fittingMetaData  in FittingMetaDatas)
            {
                var fitting = fittingMetaData.Element as FamilyInstance;
                var view = ViewsOfType.Create(doc, PluginView3D.Type, fitting);
                var fittingInfo = new ResultInfo(fitting, fittingMetaData.DocTitle, view.Id);

                view.Name = fittingInfo.ViewName;
                    
                result.Add(fittingInfo);
            }

            foreach (ElementMetaData parkingMetadata in parkingMetaDatas)
            {
                var parking = parkingMetadata.Element as FamilyInstance;
                var parkGlobalBB = CorrectBoundingBox.ComputeGlobal(parking);
                var minZ = parkGlobalBB.Min.Z;
            }

            tx.Commit();
        }

        return result;
    }
}
