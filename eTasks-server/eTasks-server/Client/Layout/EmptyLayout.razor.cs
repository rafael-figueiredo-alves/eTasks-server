using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Layout
{
    public class EmptyLayoutBase : LayoutComponentBase
    {
        protected MudTheme MyCustomTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Blue.Default,
                Secondary = Colors.Green.Accent4,
                AppbarBackground = Colors.Indigo.Default,
            },
            PaletteDark = new PaletteDark()
            {
                Primary = Colors.Blue.Lighten1
            },

            LayoutProperties = new LayoutProperties()
            {
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "300px"
            }
        };
    }
}
