﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSessionService.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Website.Services
{
    using System;
    using System.Threading.Tasks;

    using Blazored.LocalStorage;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// User session service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 07/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class UserSessionService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The WASM local storage service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILocalStorageService localStore;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSessionService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/05/2021.
        /// </remarks>
        /// <param name="localStore">
        /// The WASM local storage service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public UserSessionService(ILocalStorageService localStore)
        {
            this.localStore = localStore;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the currently logged in user changed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event Func<bool, Task> NotifyUserChanged;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether there is a user currently logged in.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsUserLoggedIn { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current OpenSky API token, null if no token is available.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string OpenSkyApiToken { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the username (for display purposes only).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Username { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initialize the service.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/05/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task Initialize()
        {
            var existingToken = await this.localStore.GetItemAsStringAsync("OpenSkyApiToken");
            var expiration = await this.localStore.GetItemAsync<DateTime?>("OpenSkyApiTokenExpiration");
            var username = await this.localStore.GetItemAsStringAsync("OpenSkyUser");
            if (!string.IsNullOrEmpty(existingToken) && expiration.HasValue && expiration.Value > DateTime.Now)
            {
                this.IsUserLoggedIn = true;
                this.OpenSkyApiToken = existingToken;
                this.Username = username;
            }
            else
            {
                await this.Logout();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Informs the service that a user logged in.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/05/2021.
        /// </remarks>
        /// <param name="login">
        /// The login response received from the OpenSky API.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task LoggedIn(LoginResponse login)
        {
            if (!string.IsNullOrEmpty(login.Token))
            {
                this.IsUserLoggedIn = true;
                this.OpenSkyApiToken = login.Token;
                this.Username = login.Username;
                await this.localStore.SetItemAsync("OpenSkyApiToken", login.Token);
                await this.localStore.SetItemAsync("OpenSkyApiTokenExpiration", login.Expiration);
                await this.localStore.SetItemAsync("OpenSkyUser", login.Username);
                if (this.NotifyUserChanged != null)
                {
                    await this.NotifyUserChanged.Invoke(true);
                }
            }
            else
            {
                await this.Logout();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// User wants to log out.
        /// </summary>
        /// <remarks>
        /// sushi.at, 07/05/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task Logout()
        {
            this.IsUserLoggedIn = false;
            this.OpenSkyApiToken = null;
            this.Username = null;
            await this.localStore.RemoveItemAsync("OpenSkyApiToken");
            await this.localStore.RemoveItemAsync("OpenSkyApiTokenExpiration");
            await this.localStore.RemoveItemAsync("OpenSkyUser");
            if (this.NotifyUserChanged != null)
            {
                await this.NotifyUserChanged.Invoke(false);
            }
        }
    }
}