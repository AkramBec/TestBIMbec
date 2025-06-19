
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SearchFittingsInParkingSpaces.Models;

namespace SearchFittingsInParkingSpaces.ViewModels
{
    public sealed partial class SearchFittingsInParkingSpacesViewModel : ObservableObject
    {
        private readonly UIDocument _uiDoc;
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
            
            var doc = _uiDoc.Document;

            if (value.LinkInstanceId is null)
            {
                // Локальный элемент — выделяем
                var elementId = new ElementId(value.ElementId);
                _uiDoc.ShowElements(elementId);
                _uiDoc.Selection.SetElementIds(new[] { elementId });
            }
            else
            {
                // Элемент из связи — выделяем саму связь
                _uiDoc.ShowElements(value.LinkInstanceId);
                _uiDoc.Selection.SetElementIds(new[] { value.LinkInstanceId });

                TaskDialog.Show("Информация",
                    $"Элемент ID {value.ElementId} находится в связанной модели «{value.DocumentTitle}».\n" +
                    "Revit не позволяет выделить элемент напрямую, но мы показали его связь.");
            }
            
        }
        
        [RelayCommand]
        private void FindFittings()
        {
            Fittings.Clear();

            var doc = _uiDoc.Document;
            var results = RebarOverParkingAnalyzer.FindFittingsOverParking(doc);

            foreach (var fitting in results)
                Fittings.Add(fitting);
        }
    }
}