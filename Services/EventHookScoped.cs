using System;

namespace iSketch.app.Services
{
    public class EventHookScoped
    {
        public event EventHandler LoginLogoutStatusChanged;
        public void OnLoginLogoutStatusChanged()
        {
            LoginLogoutStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
