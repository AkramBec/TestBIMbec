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
                var parkGlobalBB = CorrectBoundingBox.ComputeGlobal(parking);
                var parkBB = parking.get_BoundingBox(null);
                var parkMinZ = parkBB.Min.Z;
                var parkMin = parkBB.Min;
                XYZ parkMax = new XYZ(parkBB.Max.X, parkBB.Max.Y,parkBB.Max.Z + 12000/304.8);

                Outline parkOutline = new Outline(parkMin, parkMax);
                BoundingBoxIntersectsFilter bBoxFilter = new BoundingBoxIntersectsFilter(parkOutline);
                
                var FittingsCollector = new ElementCollector(
                    doc, FilterRule.ByCategoriesAndElementFilter(JsonCategories.BuiltInCategories,bBoxFilter));

                Guid lengthGuid = new Guid("748a2515-4cc9-4b74-9a69-339a8d65a212");

                Parameter lengthParam = parking.Symbol.get_Parameter(lengthGuid);
                double length = lengthParam?.AsDouble() ?? 0;   // в футах
                
                Guid widthGuid = new Guid("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");
                Parameter widthParam = parking.Symbol.get_Parameter(widthGuid);
                double width = widthParam?.AsDouble() ?? 0;   // в футах

                // Построим прямоугольник в локальной системе координат (до поворота)
                
                LocationPoint location = parking.Location as LocationPoint;
                XYZ origin = location.Point;                // Центр парковки
                double rotation = location.Rotation;

                List<XYZ> localCorners = new List<XYZ>
                {
                    new XYZ( -width / 2,-length / 2, parkMinZ),
                    new XYZ( -width / 2,length / 2, parkMinZ),
                    new XYZ( width / 2,length / 2, parkMinZ),
                    new XYZ( width / 2,-length / 2, parkMinZ)
                };
                Transform transform = Transform.Identity;
                Transform rotationTransform  = Transform.CreateRotation(XYZ.BasisZ, rotation);
                Transform translationTransform = Transform.CreateTranslation(origin);
                transform = translationTransform.Multiply(rotationTransform);
                
                
                List<XYZ> worldCorners = localCorners
                    .Select(p => transform.OfPoint(p))
                    .ToList();

                Transform inverseTransform = transform.Inverse;
                
                
                
                foreach (var fittingMetaData in FittingsCollector.ElementsMetaData)
                {
                    var fitting = fittingMetaData.Element as FamilyInstance;
                    var fittingGlobalBB = CorrectBoundingBox.ComputeGlobal(fitting);
                    var fittingBB = fitting.get_BoundingBox(null);
                    var fittingMidBB = (fittingBB.Min + fittingBB.Max) / 2;
                    
                    var floorsMetaData = new ElementCollector(
                        doc, FilterRule.ByCategoryAndElementFilter(BuiltInCategory.OST_Floors,bBoxFilter));
                    var floorsInfo = ElementInfo.CollectAndFinedBottomFace(floorsMetaData);
                
                    var floorsMaybeBlocking = floorsInfo
                        .Where(f => f.originGlobal.Z > parkMinZ + 0.01 && f.originGlobal.Z < fittingMidBB.Z)
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

                    if (isBlocking == false)
                    {
                        XYZ localParkFittingPoint = inverseTransform.OfPoint(fittingMidBB);
                        bool isInside = 
                            localParkFittingPoint.X >= -width / 2 && localParkFittingPoint.X <= width / 2 &&
                            localParkFittingPoint.Y >= -length / 2  && localParkFittingPoint.Y <= length / 2;

                        if (isInside)
                        {
                            var viewBB = BoundingBoxUtils
                                .CombineBoundingBoxes(new List<BoundingBoxXYZ>
                                    { fittingBB, parkBB });
                            var view = ViewsOfType.Create(doc, PluginView3D.Type, viewBB);
                            var fittingInfo = new ResultInfo(fittingMetaData.Element, fittingMetaData.DocTitle,
                                view.Id);
                            try
                            {
                                view.Name = fittingInfo.ViewName;
                            }
                            catch
                            {
                            }

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
