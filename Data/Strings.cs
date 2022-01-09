using System.Collections.Generic;

namespace iSketch.app.Data
{
    public static class Strings
    {
        public static Dictionary<string, string> PageErrors = new()
        {
            { "OpenID_idp-does-not-exist", "The selected idP is disabled or no longer exists." },
            { "OpenID_code-missing", "The idP did not respond with a code." },
            { "OpenID_jwt-missing", "The idP did not respond with a jSON Web Token." },
            { "OpenID_jwt-invalid", "The idP responded with a malformed jSON Web Token." },
            { "Account_set-bio-failed", "Something went wrong when setting your profile status. The status may have been too long or contained illegal characters." }
        };
    }
}