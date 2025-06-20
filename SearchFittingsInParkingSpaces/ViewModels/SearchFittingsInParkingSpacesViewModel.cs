
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
            if (value is null) return;

            _uiDoc.ShowElements(value.ViewId);

        }
        
        [RelayCommand]
        private void FindFittings()
        {
            _actionEventHandler.Raise(uiApp =>
            {
                var uiDoc = uiApp.ActiveUIDocument; 
                var doc = uiDoc.Document;
                var results = RebarOverParkingAnalyzer.FindFittingsOverParking(uiApp);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Fittings.Clear();
                    foreach (var fitting in results)
                        Fittings.Add(fitting);
                });
            });
        }
    }
}