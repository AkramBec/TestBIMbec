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

            foreach (var fittingMetaData in FittingMetaDatas)
            {
                var fitting = fittingMetaData.Element as FamilyInstance;
                var fittingGlobalBB = CorrectBoundingBox.ComputeGlobal(fitting);
                var fittingMidBB = (fittingGlobalBB.Min + fittingGlobalBB.Max) / 2;

                foreach (ElementMetaData parkingMetadata in parkingMetaDatas)
                {
                    var parking = parkingMetadata.Element as FamilyInstance;
                    var parkGlobalBB = CorrectBoundingBox.ComputeGlobal(parking);
                    var parkMinZ = parkGlobalBB.Min.Z;

                    var parkPlane = BoundingBoxUtils.CreateBottomPlane(parkGlobalBB);
                    var parkPoly = BoundingBoxUtils.GetBottomUVPolygon(parkGlobalBB);
                    
                    var fittingUV = BoundingBoxUtils.ProjectPointToBottomUV(parkPlane, fittingMidBB);

                    if (BoundingBoxUtils.PointInPolygon(fittingUV, parkPoly))
                    {
                        //плоскости перекрытий между фитингом и парковкой
                        var floorsMayBlocking = floorMetaDatas
                            .Where(f => f.originGlobal.Z > parkMinZ + 0.01 && f.originGlobal.Z < fittingMidBB.Z)
                            .Select(f => new{f.bottomFace,f.elementMetaData});
                        foreach (var floor in floorsMayBlocking)
                        {
                            var ptLocal = floor.elementMetaData.Transform.Inverse.OfPoint(fittingMidBB);
                            var projection = floor.bottomFace.Project(ptLocal);
                            if (projection == null)
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
                }
            }
            tx.Commit();
        }
        return result;
    }
}
