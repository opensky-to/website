using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSky.Website.Services
{
    using Blazored.LocalStorage;

    public class UserSessionService
    {
        public bool IsUserLoggedIn { get; private set; }

        public string OpenSkyApiToken { get; private set; }

        private readonly ILocalStorageService localStore;

        public async Task Initialize()
        {
            var existingToken = await this.localStore.GetItemAsStringAsync("OpenSkyApiToken");
            if (!string.IsNullOrEmpty(existingToken))
            {
                this.IsUserLoggedIn = true;
                this.OpenSkyApiToken = existingToken;
            }
        }

        public event Func<string, Task> NotifyUserChanged;

        public UserSessionService(ILocalStorageService localStore)
        {
            this.localStore = localStore;
        }

        public async Task LoggedIn(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                this.IsUserLoggedIn = true;
                this.OpenSkyApiToken = token;
                await this.localStore.SetItemAsync("OpenSkyApiToken", token);
                if (this.NotifyUserChanged != null)
                {
                    await this.NotifyUserChanged.Invoke(token);
                }
            }
            else
            {
                await this.Logout();
            }
        }

        public async Task Logout()
        {
            this.IsUserLoggedIn = false;
            this.OpenSkyApiToken = null;
            await this.localStore.RemoveItemAsync("OpenSkyApiToken");
            if (this.NotifyUserChanged != null)
            {
                await this.NotifyUserChanged.Invoke(null);
            }
        }
    }
}
