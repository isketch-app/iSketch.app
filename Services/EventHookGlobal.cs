using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
