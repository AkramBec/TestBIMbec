
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

        }
        
        [RelayCommand]
        private void FindFittings()
        {
            _actionEventHandler.Raise(uiApp =>
            {
                var uiDoc = uiApp.ActiveUIDocument; 
                var doc = uiDoc.Document;
                
                var viewType = new PluginView3D(doc).Type;
                uiDoc.ActiveView = RebarOverParkingAnalyzer.SwitchToSafeViewIfNeeded(uiApp,viewType);
                
                Fittings.Clear();

                var results = RebarOverParkingAnalyzer.FindFittingsOverParking(uiApp);
                
                foreach (var fitting in results)
                    Fittings.Add(fitting);
            });
        }
    }
}