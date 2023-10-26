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
        public event EventHandler LoginButtonPressed;
        public void OnLoginButtonPressed()
        {
            LoginButtonPressed?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler AccountPhotoChanged;
        public void OnAccountPhotoChanged()
        {
            AccountPhotoChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}