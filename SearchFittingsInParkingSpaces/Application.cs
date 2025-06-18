using Nice3point.Revit.Toolkit.External;
using SearchFittingsInParkingSpaces.Commands;

namespace SearchFittingsInParkingSpaces
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            CreateRibbon();
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Commands", "SearchFittingsInParkingSpaces");

            panel.AddPushButton<StartupCommand>("Execute")
                .SetImage("/SearchFittingsInParkingSpaces;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/SearchFittingsInParkingSpaces;component/Resources/Icons/RibbonIcon32.png");
        }
    }
}