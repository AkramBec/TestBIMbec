using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class RebarOverParkingAnalyzer
{
    public static List<ResultInfo> FindFittingsOverParking(UIApplication uiApp)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;

        var parkingsCollector = new ElementCollector(doc, FilterRule.ByCategory(BuiltInCategory.OST_Parking));

        var result = new List<ResultInfo>();
        
        using (var tx = new Transaction(doc, "Создание видов с арматурой над парковкой"))
        {
            tx.Start();

            foreach (var parkingMetadata in parkingsCollector.ElementsMetaData)
            {
                var parking = parkingMetadata.Element as FamilyInstance;
                var inverseTransform = Transformer.GetInverse(parking);
                var length = ParameterCollector.GetFromSymbolByGuid(parking, "748a2515-4cc9-4b74-9a69-339a8d65a212");
                var width = ParameterCollector.GetFromSymbolByGuid(parking, "8f2e4f93-9472-4941-a65d-0ac468fd6a5d");
                
                var parkBB = parking.get_BoundingBox(null);
                Outline parkOutline = new Outline(
                    parkBB.Min, 
                    new XYZ(parkBB.Max.X, parkBB.Max.Y,parkBB.Max.Z + 12000/304.8));
                BoundingBoxIntersectsFilter bBoxFilter = new BoundingBoxIntersectsFilter(parkOutline);

                var floorsMetaData = new ElementCollector(
                    doc, FilterRule.ByCategoryAndElementFilter(BuiltInCategory.OST_Floors,bBoxFilter));
                var floorsInfo = ElementInfo.CollectAndFinedBottomFace(floorsMetaData);
                
                var FittingsCollector = new ElementCollector(
                    doc, FilterRule.ByCategoriesAndElementFilter(JsonCategories.BuiltInCategories,bBoxFilter));

                foreach (var fittingMetaData in FittingsCollector.ElementsMetaData)
                {
                    var fitting = fittingMetaData.Element as FamilyInstance;
                    var fittingBB = fitting.get_BoundingBox(null);
                    var fittingMidBB = (fittingBB.Min + fittingBB.Max) / 2;

                    var floorsMaybeBlocking = floorsInfo
                        .Where(f => f.originGlobal.Z > parkBB.Min.Z + 0.01 && f.originGlobal.Z < fittingMidBB.Z)
                        .Select(f => new{f.bottomFace,f.elementMetaData});
                
                    bool isBlocking = false;
                    foreach (var floor in floorsMaybeBlocking)
                    {
                        var ptLocal = floor.elementMetaData.Transform.Inverse.OfPoint(fittingMidBB);
                        var projection = floor.bottomFace.Project(ptLocal);
                        if (projection != null)
                        {
                            isBlocking = true; 
                            break;
                        }
                    }

                    if (!isBlocking)
                    {
                        XYZ localParkFittingPoint = inverseTransform.OfPoint(fittingMidBB);
                        bool isInside = 
                            localParkFittingPoint.X >= -width / 2 && localParkFittingPoint.X <= width / 2 &&
                            localParkFittingPoint.Y >= -length / 2  && localParkFittingPoint.Y <= length / 2;

                        if (isInside)
                        {
                            var viewBB = BoundingBoxUtils.CombineBoundingBoxes(new List<BoundingBoxXYZ> { fittingBB, parkBB });
                            var view = ViewsOfType.Create(doc, PluginView3D.Type, viewBB);
                            var fittingInfo = new ResultInfo(fittingMetaData.Element, fittingMetaData.DocTitle,
                                view.Id);
                            try { view.Name = fittingInfo.ViewName; } catch { }

                            result.Add(fittingInfo);
                        }
                    }
                }
            }
            tx.Commit();
        }
        return result;
    }
}
