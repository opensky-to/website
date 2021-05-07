// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Website
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Blazored.LocalStorage;

    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    using OpenSky.Website.Services;

    using OpenSkyApi;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// The Blazor WASM main program entry point.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Program
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Main entry-point for this application.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="args">
        /// An array of command-line argument strings.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AlertService>();
            builder.Services.AddScoped<UserSessionService>();
            var httpClient = new HttpClient();
            builder.Services.AddScoped(_ => httpClient);
            builder.Services.AddSingleton(new OpenSkyService(builder.Configuration["OpenSkyAPI:Url"], httpClient));

            var host = builder.Build();
            var userSessionService = host.Services.GetRequiredService<UserSessionService>();
            await userSessionService.Initialize();
            await host.RunAsync();
        }
    }
}