using eTasks_server.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Layout
{
    public class MainLayoutBase : LayoutComponentBase
    {
        [Inject] protected IConfiguration? Configuration { get; set; }
        [Inject] protected UserState? UserState { get; set; }

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

        protected override void OnInitialized()
        {
            var culture = new System.Globalization.CultureInfo("pt-BR");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

            UserState!.OnChange += StateHasChanged;
        }

        public void Dispose()
        {
            UserState!.OnChange -= StateHasChanged;
        }
    }
}
