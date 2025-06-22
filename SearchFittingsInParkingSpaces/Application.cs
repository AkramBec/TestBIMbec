using JetBrains.Annotations;
using Nice3point.Revit.Extensions;
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
            var panel = Application.CreatePanel("Машиноместо", "ТЗ Бешшар - АР");

            panel.AddPushButton<StartupCommand>("Поиск арматуры")
                .SetImage("/SearchFittingsInParkingSpaces;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/SearchFittingsInParkingSpaces;component/Resources/Icons/RibbonIcon32.png");
        }
    }
}