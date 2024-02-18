using Microsoft.AspNetCore.Components;

namespace iSketch.app.Services
{
    public class Header
    {
        private string _TitleText = "iSketch.app";
        public string TitleText { 
            get
            {
                return _TitleText;
            }
            set
            {
                _TitleText = value;
                StateHasChanged.InvokeAsync();
            }
        }
        private string _ThemeColor = "#ffffff";
        public string ThemeColor
        {
            get
            {
                return _ThemeColor;
            }
            set
            {
                _ThemeColor = value;
                StateHasChanged.InvokeAsync();
            }
        }
        public EventCallback StateHasChanged;
    }
}