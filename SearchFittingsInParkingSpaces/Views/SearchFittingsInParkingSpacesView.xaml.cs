using SearchFittingsInParkingSpaces.ViewModels;

namespace SearchFittingsInParkingSpaces.Views
{
    public sealed partial class SearchFittingsInParkingSpacesView
    {
        public SearchFittingsInParkingSpacesView(SearchFittingsInParkingSpacesViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}