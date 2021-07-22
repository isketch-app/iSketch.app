using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace iSketch.app.Data.Authentication
{
    public class iSketchAuthenticationHandler : IAuthenticationHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.Fail(":("));
            //throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}
