﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AlertType.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Website.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Alert type enumeration.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum AlertType
    {
        /// <summary>
        /// Success alert.
        /// </summary>
        Success,

        /// <summary>
        /// Error alert.
        /// </summary>
        Error,

        /// <summary>
        /// Info alert.
        /// </summary>
        Info,

        /// <summary>
        /// Warning alert.
        /// </summary>
        Warning
    }
}