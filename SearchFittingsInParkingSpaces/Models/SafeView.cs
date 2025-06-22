using Autodesk.Revit.UI;

namespace SearchFittingsInParkingSpaces.Models;

public class SafeView
{
    public static View SwitchIfNeeded(UIApplication uiApp, ViewFamilyType pluginViewType)
    {
        var uiDoc = uiApp.ActiveUIDocument;
        var doc = uiDoc.Document;
        var activeView = uiDoc.ActiveView;

        // Если активный вид имеет тип, совпадающий с плагинным — заменяем
        if (activeView.GetTypeId() == pluginViewType.Id)
        {
            // Ищем существующий вид, содержащий 3D
            var safeView = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate &&
                                     v.Name.Contains("3D", StringComparison.OrdinalIgnoreCase) &&
                                     v.GetTypeId() != pluginViewType.Id);

            if (safeView != null)
            {
                return safeView as View;
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
                    
                    return default3DView as View;
                }
                
            }
        }
        return activeView;
    }
}