using Blazor_TicTac_Mvvm.Data.Database.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;

namespace Blazor_TicTac_Mvvm.Services
{
    public class LocalStorageService
    {
        private readonly ProtectedSessionStorage _localStorage;

        private const string UserKey = "user";

        public LocalStorageService(ProtectedSessionStorage localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SetGameData()
        {

        }

        public async Task SetUserAsync(UserData data)
        {
            var dataString = JsonConvert.SerializeObject(data);
            await _localStorage.SetAsync(UserKey, dataString);
            return;
        }

        public async Task<UserData?> GetUserAsync()
        {
            try
            {
                var storedResult = await _localStorage.GetAsync<string>(UserKey);
                if (storedResult.Success && !string.IsNullOrEmpty(storedResult.Value))
                {
                    return JsonConvert.DeserializeObject<UserData>(storedResult.Value);
                }
            }
            catch (InvalidOperationException)
            { }

            return null;
        }
    }

    public readonly record struct UserData(string Username, int Id)
    {
        public static implicit operator UserData(UserModel model)
        {
            return new UserData(model.Name, model.PlayerId);
        }
    }
}
