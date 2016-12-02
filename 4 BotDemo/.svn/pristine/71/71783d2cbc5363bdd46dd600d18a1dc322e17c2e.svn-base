using System;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;

namespace SpeechPrototype.Model
{
    public class AzureAccessToken
    {
        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
        private const string ServiceUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";

        private string apiKey;
        private string token;
        private Timer accessTokenRenewer;
        //Access token expires every 10 minutes. Renew it every 9 minutes only.
        private const int RefreshTokenDuration = 9;

        public AzureAccessToken(string apiKey)
        {
            this.apiKey = apiKey;
            this.token = null;

            //renew the token every specfied minutes
            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback), this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
        }

        public string GetAccessToken()
        {
            if (token == null)
            {
                this.token = GetAccessToken(apiKey);
            }

            return this.token;
        }

        private void RenewAccessToken()
        {
            var newToken = GetAccessToken(apiKey);

            //swap the new token with old one
            //Note: the swap is thread unsafe
            this.token = newToken;
            Debug.WriteLine(string.Format("Renewed token for key: {0}", this.apiKey));
        }

        private void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                RenewAccessToken();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }
            }
        }

        public string GetAccessToken(string subscriptionSecret)
        {
            var task = GetAccessTokenAsync(subscriptionSecret);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Gets a token for the specified subscription.
        /// </summary>
        /// <param name="subscriptionSecret">Subscription secret key.</param>
        /// <returns>The encoded JWT token.</returns>
        public async Task<string> GetAccessTokenAsync(string subscriptionSecret)

        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(ServiceUrl);
                request.Content = new StringContent(string.Empty);
                request.Headers.TryAddWithoutValidation(OcpApimSubscriptionKeyHeader, subscriptionSecret);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var token = await response.Content.ReadAsStringAsync();

                return token;
            }
        }
    }
}
