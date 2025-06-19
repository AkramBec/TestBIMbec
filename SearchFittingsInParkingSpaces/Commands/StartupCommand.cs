using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using SearchFittingsInParkingSpaces.ViewModels;
using SearchFittingsInParkingSpaces.Views;

namespace SearchFittingsInParkingSpaces.Commands
{
    /// <summary>
    ///     External command entry point
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class StartupCommand : ExternalCommand
    {
        public override void Execute()
        {
            var viewModel = new SearchFittingsInParkingSpacesViewModel(UiDocument);
            var view = new SearchFittingsInParkingSpacesView(viewModel);
            view.Show(UiApplication.MainWindowHandle);
        }
    }
}