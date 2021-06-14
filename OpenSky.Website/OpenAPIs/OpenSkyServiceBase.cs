// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyServiceBase.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;

    using OpenSky.Website.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky service client base class (implements auto-token refresh and JWT bearer
    /// authentication header inject)
    /// </summary>
    /// <remarks>
    /// sushi.at, 30/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyServiceBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The configuration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IConfiguration configuration;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The HTTP client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly HttpClient httpClient;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// JSON serializer settings.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Lazy<Newtonsoft.Json.JsonSerializerSettings> settings;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user session service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserSessionService userSession;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyServiceBase"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyServiceBase()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyServiceBase"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/05/2021.
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
        public OpenSkyServiceBase(IConfiguration configuration, HttpClient httpClient, UserSessionService userSession)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.userSession = userSession;

            this.settings = new Lazy<Newtonsoft.Json.JsonSerializerSettings>(CreateSerializerSettings);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the response as string should be read.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ReadResponseAsString { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the JSON serializer settings.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        protected Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings => this.settings.Value;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates OpenSky HTTP request message asynchronous - automatically adds authorization header and refreshes tokens.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the create HTTP request message.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            var msg = new HttpRequestMessage();

            // Check if the token needs to be refreshed
            if (await this.userSession.CheckExpiration())
            {
                try
                {
                    await this.RefreshToken(cancellationToken);
                }
                catch
                {
                    await this.userSession.Logout();
                    throw;
                }
            }

            // Add the JWT token to the authorization header
            if (this.userSession.IsUserLoggedIn)
            {
                msg.Headers.Add("Authorization", $"Bearer {this.userSession.OpenSkyApiToken}");
            }

            return msg;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Reads object response asynchronous.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// <exception cref="ApiException">
        /// Thrown when an API error condition occurs.
        /// </exception>
        /// <param name="response">
        /// The HTTP response.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the object response asynchronous.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
        {
            if (response?.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (this.ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText, this.JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    using var streamReader = new System.IO.StreamReader(responseStream);
                    using var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader);
                    var serializer = Newtonsoft.Json.JsonSerializer.Create(this.JsonSerializerSettings);
                    var typedBody = serializer.Deserialize<T>(jsonTextReader);
                    return new ObjectResponseResult<T>(typedBody, string.Empty);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Create json serializer settings.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/05/2021.
        /// </remarks>
        /// <returns>
        /// The new serializer settings.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static Newtonsoft.Json.JsonSerializerSettings CreateSerializerSettings()
        {
            return new();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Refresh current main JWT token using the refresh token.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// <exception cref="ApiException">
        /// Thrown when an API error condition occurs.
        /// </exception>
        /// <param name="cancellationToken">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task RefreshToken(CancellationToken cancellationToken)
        {
            var requestBody = new RefreshToken
            {
                Token = this.userSession.OpenSkyApiToken,
                Refresh = this.userSession.RefreshToken
            };

            var urlBuilder = new StringBuilder();
            var baseUrl = this.configuration["OpenSky:OpenSkyAPIUrl"];
            urlBuilder.Append(baseUrl != null ? baseUrl.TrimEnd('/') : "").Append("/Authentication/refreshToken");

            using var request = new HttpRequestMessage();
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, this.settings.Value));
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            request.Content = content;
            request.Method = new HttpMethod("POST");
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

            var url = urlBuilder.ToString();
            request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);

            var response = await this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            try
            {
                var headers = response.Headers.ToDictionary(h => h.Key, h => h.Value);
                if (response.Content is { Headers: { } })
                {
                    foreach (var (key, value) in response.Content.Headers)
                    {
                        headers[key] = value;
                    }
                }

                var status = (int)response.StatusCode;
                if (status == 200)
                {
                    var objectResponse = await this.ReadObjectResponseAsync<RefreshTokenResponseApiResponse>(response, headers, cancellationToken).ConfigureAwait(false);
                    if (objectResponse.Object == null)
                    {
                        throw new ApiException("Response was null which was not expected.", status, objectResponse.Text, headers, null);
                    }

                    if (!objectResponse.Object.IsError)
                    {
                        await this.userSession.RefreshedToken(objectResponse.Object.Data);
                    }
                    else
                    {
                        throw new ApiException($"Unable to refresh OpenSky token: {objectResponse.Object.Message}", 401, objectResponse.Text, headers, null);
                    }
                }
                else
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new ApiException("The HTTP status code of the response was not expected (" + status + ").", status, responseData, headers, null);
                }
            }
            finally
            {
                response.Dispose();
            }
        }

        // -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Encapsulates the result of an object response.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        protected readonly struct ObjectResponseResult<T>
        {
            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="OpenSkyServiceBase"/> class.
            /// </summary>
            /// <remarks>
            /// sushi.at, 01/06/2021.
            /// </remarks>
            /// <param name="responseObject">
            /// The response object.
            /// </param>
            /// <param name="responseText">
            /// The response text.
            /// </param>
            /// -------------------------------------------------------------------------------------------------
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets the object.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public T Object { get; }

            /// -------------------------------------------------------------------------------------------------
            /// <summary>
            /// Gets the text.
            /// </summary>
            /// -------------------------------------------------------------------------------------------------
            public string Text { get; }
        }
    }
}