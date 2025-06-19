
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
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
            ElementIds = new ObservableCollection<int>();
        }
        
        [ObservableProperty]
        private ObservableCollection<int> elementIds;
        
        [RelayCommand]
        private void FindFittings()
        {
            ElementIds.Clear();

            var doc = _uiDoc.Document;
            var fittingIds = RebarOverParkingAnalyzer.FindFittingsOverParking(doc);

            foreach (var id in fittingIds)
                ElementIds.Add(id.IntegerValue);
        }
    }
}