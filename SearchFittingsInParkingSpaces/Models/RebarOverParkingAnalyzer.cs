using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<ResultInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        var parkingsCollector = new ElementCollector(doc, FilterRule.ByCategory(BuiltInCategory.OST_Parking));
        var floorsMetaData = new ElementCollector(doc, FilterRule.ByCategory(BuiltInCategory.OST_Floors));
        var floorsInfo = ElementInfo.CollectAndFinedBottomFace(floorsMetaData);
        var FittingsCollector = new ElementCollector(doc, FilterRule.ByCategories(JsonCategories.BuiltInCategories));
        
        var result = new List<ResultInfo>();
        
        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();

            foreach (var fittingMetaData in FittingsCollector.ElementsMetaData)
            {
                var fitting = fittingMetaData.Element as FamilyInstance;
                var fittingGlobalBB = CorrectBoundingBox.ComputeGlobal(fitting);
                var fittingMidBB = (fittingGlobalBB.Min + fittingGlobalBB.Max) / 2;

                foreach (var parkingMetadata in parkingsCollector.ElementsMetaData)
                {
                    var parking = parkingMetadata.Element as FamilyInstance;
                    var parkGlobalBB = CorrectBoundingBox.ComputeGlobal(parking);
                    var parkMinZ = parkGlobalBB.Min.Z;

                    var parkCorners = BoundingBoxUtils.GetOrientedBottomCorners(fittingMetaData);
                    if (!BoundingBoxUtils.PointInPolygonXY(fittingMidBB, parkCorners))
                        continue;

                    //плоскости перекрытий между фитингом и парковкой
                    var floorsMayBlocking = floorsInfo
                        .Where(f => f.originGlobal.Z > parkMinZ + 0.01 && f.originGlobal.Z < fittingMidBB.Z)
                        .Select(f => new{f.bottomFace,f.elementMetaData});
            
                    bool isBlocking = false;
                    foreach (var floor in floorsMayBlocking)
                    {
                        var ptLocal = floor.elementMetaData.Transform.Inverse.OfPoint(fittingMidBB);
                        var projection = floor.bottomFace.Project(ptLocal);
                        if (projection != null)
                        {
                            isBlocking = true;
                        }
                    }

                    if (isBlocking == false)
                    {
                        var viewBB = BoundingBoxUtils
                            .CombineBoundingBoxes(new List<BoundingBoxXYZ>
                                {fittingGlobalBB, parkGlobalBB});
                        var view = ViewsOfType.Create(doc, PluginView3D.Type, viewBB);
                        var fittingInfo = new ResultInfo(fittingMetaData.Element, fittingMetaData.DocTitle, view.Id);
                        view.Name = fittingInfo.ViewName;
                        result.Add(fittingInfo);
                    }
                        
                }
            }
            tx.Commit();
        }
        return result;
    }
}
