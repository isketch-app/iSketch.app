using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace iSketch.app.Shared.Game.Animations
{
    public class Animation() : ComponentBase
    {
        public bool Visible = false;
        public TimeSpan Duration = TimeSpan.Zero;
        public string TitleText;
        public string ThemeColor;
        [Inject]
        private Services.Header HeaderService { get; set; }
        private bool Playing = false;
        public async void Play()
        {
            if (Playing) return;
            Visible = Playing = true;
            
            string originalTitleText = HeaderService.TitleText;
            string originalThemeColor = HeaderService.ThemeColor;
            if (TitleText != null) HeaderService.TitleText = TitleText;
            if (ThemeColor != null) HeaderService.ThemeColor = ThemeColor;
            StateHasChanged();
            await Task.Delay(Duration);
            Visible = Playing = false;
            if (TitleText != null) HeaderService.TitleText = originalTitleText;
            if (ThemeColor != null) HeaderService.ThemeColor = originalThemeColor;
            StateHasChanged();
        }
    }
}