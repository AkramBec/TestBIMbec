
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External.Handlers;
using SearchFittingsInParkingSpaces.Models;

namespace SearchFittingsInParkingSpaces.ViewModels
{
    public sealed partial class SearchFittingsInParkingSpacesViewModel : ObservableObject
    {
        public event Action? RequestClose;
        private readonly UIDocument _uiDoc;
        private readonly ActionEventHandler _actionEventHandler = new ActionEventHandler();
        public SearchFittingsInParkingSpacesViewModel(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            Fittings = new ObservableCollection<FittingInfo>();
        }
        
        [ObservableProperty]
        private ObservableCollection<FittingInfo> fittings;

        [ObservableProperty]
        private FittingInfo selectedFitting;

        partial void OnSelectedFittingChanged(FittingInfo value)
        {
            if (value == null)
                return;
            _actionEventHandler.Raise(uiApp =>
            {
                var uiDoc = uiApp.ActiveUIDocument; 
                var doc = uiDoc.Document;

                var view = doc.GetElement(value.ViewId) as View;
                if (view == null || !view.CanBePrinted) return;

                uiDoc.ActiveView = view;
            });
            //RequestClose?.Invoke();

        }
        
        [RelayCommand]
        private void FindFittings()
        {
            _actionEventHandler.Raise(uiApp =>
            {
                var uiDoc = uiApp.ActiveUIDocument; 
                var doc = uiDoc.Document;
                
                var viewType = new PluginView3D(doc).Type;
                uiDoc.ActiveView = SwitchToSafeViewIfNeeded(uiApp,viewType);
                
                Fittings.Clear();

                var results = RebarOverParkingAnalyzer.FindFittingsOverParking(uiApp);
                
                foreach (var fitting in results)
                    Fittings.Add(fitting);
            });
        }
        
        private static View3D SwitchToSafeViewIfNeeded(UIApplication uiApp, ViewFamilyType pluginViewType)
        {
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;
            var activeView = uiDoc.ActiveView as View3D;

            // Если активный вид имеет тип, совпадающий с плагинным — заменяем
            if (activeView.GetTypeId() == pluginViewType.Id)
            {
                // Ищем существующий вид, начинающийся с {3D
                var safeView = new FilteredElementCollector(doc)
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .FirstOrDefault(v => !v.IsTemplate &&
                                         v.Name.Contains("3D", StringComparison.OrdinalIgnoreCase) &&
                                         v.GetTypeId() != pluginViewType.Id);

                if (safeView != null)
                {
                    return safeView;
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
                        
                        return default3DView;
                    }
                    
                }
            }
            return activeView;
        }
    }
}