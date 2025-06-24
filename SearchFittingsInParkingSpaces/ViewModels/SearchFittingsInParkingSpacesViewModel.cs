
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nice3point.Revit.Toolkit;
using Nice3point.Revit.Toolkit.External.Handlers;
using SearchFittingsInParkingSpaces.Models;
using FilterRule = SearchFittingsInParkingSpaces.Models.FilterRule;

namespace SearchFittingsInParkingSpaces.ViewModels
{
    public sealed partial class SearchFittingsInParkingSpacesViewModel : ObservableObject
    {
        private const double InitialHeight = 80;   // высота до поиска
        private const double ExpandedHeight = 320;  // высота после поиска
        
        private readonly UIDocument _uiDoc;
        private readonly ActionEventHandler _actionEventHandler = new ActionEventHandler();
        public SearchFittingsInParkingSpacesViewModel(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            Fittings = new ObservableCollection<ResultInfo>();

            WindowHeight = InitialHeight;
        }
        
        [ObservableProperty]
        private double windowHeight;
        
        [ObservableProperty]
        private ObservableCollection<ResultInfo> fittings;

        [ObservableProperty]
        private ResultInfo selectedResult;
        
        [ObservableProperty]
        private bool hasRunSearch;
        
        [RelayCommand]
        private void ExportCsv()
        {
            // здесь логика сохранения Fittings в CSV
        }

        partial void OnSelectedResultChanged(ResultInfo value)
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
                RevitLinks.LinkInstances(doc);
                PluginView3D.GetType(doc);
                
                uiDoc.ActiveView = SafeView.SwitchIfNeeded(uiApp,PluginView3D.Type);
                
                Fittings.Clear();
                PluginView3D.Initialize(doc);

                var results = RebarOverParkingAnalyzer.FindFittingsOverParking(uiApp);
                
                foreach (var fitting in results)
                    Fittings.Add(fitting);
                
                WindowHeight = ExpandedHeight;
                HasRunSearch = true;
            });
        }
    }
}