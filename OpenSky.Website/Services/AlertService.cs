// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AlertService.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Website.Services
{
    using System;

    using OpenSky.Website.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Alert service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AlertService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The default alert ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const string DefaultId = "default-alert";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when an alert is raised.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public event Action<Alert> OnAlert;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Raises the specified alert.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="alert">
        /// The alert.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Alert(Alert alert)
        {
            alert.Id ??= DefaultId;
            this.OnAlert?.Invoke(alert);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the specified alert ID.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="id">
        /// (Optional) The identifier.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Clear(string id = DefaultId)
        {
            this.OnAlert?.Invoke(new Alert { Id = id });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Display an error alert.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="keepAfterRouteChange">
        /// (Optional) True to keep the alert after a route change.
        /// </param>
        /// <param name="autoClose">
        /// (Optional) True to automatically close the alert.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Error(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(
                new Alert
                {
                    Type = AlertType.Error,
                    Message = message,
                    KeepAfterRouteChange = keepAfterRouteChange,
                    AutoClose = autoClose
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Display an info alert.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="keepAfterRouteChange">
        /// (Optional) True to keep the alert after a route change.
        /// </param>
        /// <param name="autoClose">
        /// (Optional) True to automatically close the alert.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Info(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(
                new Alert
                {
                    Type = AlertType.Info,
                    Message = message,
                    KeepAfterRouteChange = keepAfterRouteChange,
                    AutoClose = autoClose
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Display a success alert.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="keepAfterRouteChange">
        /// (Optional) True to keep the alert after a route change.
        /// </param>
        /// <param name="autoClose">
        /// (Optional) True to automatically close the alert.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Success(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(
                new Alert
                {
                    Type = AlertType.Success,
                    Message = message,
                    KeepAfterRouteChange = keepAfterRouteChange,
                    AutoClose = autoClose
                });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Display a warning alert.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="keepAfterRouteChange">
        /// (Optional) True to keep the alert after a route change.
        /// </param>
        /// <param name="autoClose">
        /// (Optional) True to automatically close the alert.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Warn(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(
                new Alert
                {
                    Type = AlertType.Warning,
                    Message = message,
                    KeepAfterRouteChange = keepAfterRouteChange,
                    AutoClose = autoClose
                });
        }
    }
}