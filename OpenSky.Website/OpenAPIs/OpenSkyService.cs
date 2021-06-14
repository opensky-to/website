// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyService.cs" company="OpenSky">
// OpenSky project 2021
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
        public OpenSkyService(IConfiguration configuration, HttpClient httpClient, UserSessionService userSession) : base(configuration, httpClient, userSession)
        {
            this.BaseUrl = configuration["OpenSky:OpenSkyAPIUrl"];
            this._httpClient = httpClient;
            this._settings = new Lazy<Newtonsoft.Json.JsonSerializerSettings>(this.CreateSerializerSettings);
        }
    }
}