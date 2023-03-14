using System;

namespace iSketch.app.Services
{
    public class EventHookGlobal
    {
        public event EventHandler AdminWordsChanged;
        public void OnAdminWordsChanged()
        {
            AdminWordsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
