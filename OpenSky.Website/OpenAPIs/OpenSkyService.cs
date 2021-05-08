// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyService.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using System;
    using System.Net.Http;

    using Microsoft.Extensions.Configuration;

    using OpenSky.Website.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The OpenSky API service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public partial class OpenSkyService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user session service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserSessionService userSession;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// <param name="userSession">
        /// The user session service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyService(IConfiguration configuration, HttpClient httpClient, UserSessionService userSession)
        {
            this.BaseUrl = configuration["OpenSkyAPI:Url"];
            this._httpClient = httpClient;
            this._settings = new Lazy<Newtonsoft.Json.JsonSerializerSettings>(this.CreateSerializerSettings);
            this.userSession = userSession;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Prepare API request.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="client">
        /// The HTTP client.
        /// </param>
        /// <param name="request">
        /// The request message.
        /// </param>
        /// <param name="url">
        /// URL of the resource.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            if (this.userSession.IsUserLoggedIn)
            {
                request.Headers.Add("Authorization", $"Bearer {this.userSession.OpenSkyApiToken}");
            }
        }
    }
}