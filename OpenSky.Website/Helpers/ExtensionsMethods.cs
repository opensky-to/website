// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionsMethods.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Website.Helpers
{
    using System;
    using System.Collections.Specialized;
    using System.Web;

    using Microsoft.AspNetCore.Components;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class ExtensionsMethods
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A NavigationManager extension method that returns the query strings name/value collection.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="navigationManager">
        /// The navigationManager to act on.
        /// </param>
        /// <returns>
        /// The query strings name/value collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static NameValueCollection QueryString(this NavigationManager navigationManager)
        {
            return HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A NavigationManager extension method that returns the specified query string value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="navigationManager">
        /// The navigationManager to act on.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The specified query string value.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string QueryString(this NavigationManager navigationManager, string key)
        {
            return navigationManager.QueryString()[key];
        }
    }
}