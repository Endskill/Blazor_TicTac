using Blazor_TicTac_Mvvm.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security;

namespace Blazor_TicTac_Mvvm.Authentication
{
    public class AuthProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly DatabaseService _dbService;
        private readonly LocalStorageService _storageService;

        public AuthProvider(DatabaseService dbService, LocalStorageService storageService)
        {
            _dbService = dbService;
            _storageService = storageService;
        }

        public async Task<bool> TryLoginAsync(string username)
        {
            var user = await _dbService.QueryUser(username);
            if (user is null)
                return false;

            await _storageService.SetUserAsync(user);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(ClaimFromUser(user))));

            return true;
        }

        [Authorize]
        public void Logout()
        {

        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();
            var user = await _storageService.GetUserAsync();

            if (user != null)
            {
                principal = ClaimFromUser(user ?? throw new NullReferenceException("Compiler mag das nicht."));
            }

            return new AuthenticationState(principal);
        }

        public void Dispose()
        {
        }

        private ClaimsPrincipal ClaimFromUser(UserData userModel)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, userModel.Username));
            claims.Add(new Claim(ClaimTypes.Sid, userModel.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, "User"));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie"));
        }
    }
}
