// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSessionService.cs" company="OpenSky">
// OpenSky project 2021
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
        /// Gets the refresh token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RefreshToken { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Date/Time of the expiration of the main JWT OpenSky API token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Expiration { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Date/Time of the refresh token expiration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? RefreshExpiration { get; private set; }

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
            // Restore tokens and expiration times from local store
            this.OpenSkyApiToken = await this.localStore.GetItemAsStringAsync("OpenSkyApiToken");
            this.Expiration = await this.localStore.GetItemAsync<DateTime?>("OpenSkyApiTokenExpiration");
            this.Username = await this.localStore.GetItemAsStringAsync("OpenSkyUser");
            this.RefreshToken = await this.localStore.GetItemAsStringAsync("OpenSkyApiRefreshToken");
            this.RefreshExpiration = await this.localStore.GetItemAsync<DateTime?>("OpenSkyApiRefreshTokenExpiration");

            await this.CheckExpiration();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check token expiration.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields true if the main JWT token needs to be refreshed, false if
        /// it is current or the refresh token is also expired (will trigger logout!).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<bool> CheckExpiration()
        {
            if (!string.IsNullOrEmpty(this.OpenSkyApiToken) && this.Expiration.HasValue && this.Expiration.Value > DateTime.UtcNow)
            {
                this.IsUserLoggedIn = true;
                return false;
            }
            
            if (!string.IsNullOrEmpty(this.RefreshToken) && this.RefreshExpiration.HasValue && this.RefreshExpiration.Value > DateTime.UtcNow)
            {
                this.IsUserLoggedIn = true;
                return true;
            }
                
            await this.Logout();
            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The tokens were refreshed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="refreshToken">
        /// The refresh token response model (contains new tokens).
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task RefreshedToken(RefreshTokenResponse refreshToken)
        {
            this.OpenSkyApiToken = refreshToken.Token;
            this.Expiration = refreshToken.Expiration.UtcDateTime;
            this.RefreshToken = refreshToken.RefreshToken;
            this.RefreshExpiration = refreshToken.RefreshTokenExpiration.UtcDateTime;
            await this.localStore.SetItemAsStringAsync("OpenSkyApiToken", refreshToken.Token);
            await this.localStore.SetItemAsync("OpenSkyApiTokenExpiration", refreshToken.Expiration.UtcDateTime);
            await this.localStore.SetItemAsStringAsync("OpenSkyApiRefreshToken", refreshToken.RefreshToken);
            await this.localStore.SetItemAsync("OpenSkyApiRefreshTokenExpiration", refreshToken.RefreshTokenExpiration.UtcDateTime);

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
                this.Expiration = login.Expiration.UtcDateTime;
                this.Username = login.Username;
                this.RefreshToken = login.RefreshToken;
                this.RefreshExpiration = login.RefreshTokenExpiration.UtcDateTime;
                await this.localStore.SetItemAsStringAsync("OpenSkyApiToken", login.Token);
                await this.localStore.SetItemAsync("OpenSkyApiTokenExpiration", login.Expiration.UtcDateTime);
                await this.localStore.SetItemAsStringAsync("OpenSkyUser", login.Username);
                await this.localStore.SetItemAsStringAsync("OpenSkyApiRefreshToken", login.RefreshToken);
                await this.localStore.SetItemAsync("OpenSkyApiRefreshTokenExpiration", login.RefreshTokenExpiration.UtcDateTime);
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
            this.Expiration = null;
            this.Username = null;
            this.RefreshToken = null;
            this.RefreshExpiration = null;
            await this.localStore.RemoveItemAsync("OpenSkyApiToken");
            await this.localStore.RemoveItemAsync("OpenSkyApiTokenExpiration");
            await this.localStore.RemoveItemAsync("OpenSkyUser");
            await this.localStore.RemoveItemAsync("OpenSkyApiRefreshToken");
            await this.localStore.RemoveItemAsync("OpenSkyApiRefreshTokenExpiration");
            if (this.NotifyUserChanged != null)
            {
                await this.NotifyUserChanged.Invoke(false);
            }
        }
    }
}