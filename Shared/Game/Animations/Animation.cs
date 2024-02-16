using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace iSketch.app.Shared.Game.Animations
{
    public class Animation : ComponentBase
    {
        public bool Visible = false;
        public TimeSpan Duration = TimeSpan.Zero;
        private bool Playing = false;
        public async void Play()
        {
            if (Playing) return;
            Visible = Playing = true;
            StateHasChanged();
            await Task.Delay(Duration);
            Visible = Playing = false;
            StateHasChanged();
        }
    }
}