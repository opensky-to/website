using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace OpenSkyApi
{
    using System.Net.Http;
    using System.Threading;

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
        /// The user session service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserSessionService userSession;

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

        private Lazy<Newtonsoft.Json.JsonSerializerSettings> settings;

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

        private async Task RefreshToken(CancellationToken cancellationToken)
        {
            var requestBody = new RefreshToken
            {
                Token = this.userSession.OpenSkyApiToken,
                Refresh = this.userSession.RefreshToken
            };

            var urlBuilder = new System.Text.StringBuilder();
            var baseUrl = this.configuration["OpenSkyAPI:Url"];
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
                    foreach (var item in response.Content.Headers)
                    {
                        headers[item.Key] = item.Value;
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

        protected Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get { return this.settings.Value; } }

        public bool ReadResponseAsString { get; set; }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
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
                    using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    using (var streamReader = new System.IO.StreamReader(responseStream))
                    using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader))
                    {
                        var serializer = Newtonsoft.Json.JsonSerializer.Create(this.JsonSerializerSettings);
                        var typedBody = serializer.Deserialize<T>(jsonTextReader);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }
    }
}
