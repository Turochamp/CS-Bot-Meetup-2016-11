﻿using System.IO;
using System.Media;
using System.Net;
using System.ServiceModel;
using System.Web;

namespace SpeechPrototype.Model
{
    public class Translator
    {
        private const string LangugagesList = "ar,ar-eg,ca,ca-es,da,da-dk,de,de-de,en,en-au,en-ca,en-gb,en-in,en-us,es,es-es,es-mx,fi,fi-fi,fr,fr-ca,fr-fr,hi,hi-in,it,it-it,ja,ja-jp,ko,ko-kr,nb-no,nl,nl-nl,no,pl,pl-pl,pt,pt-br,pt-pt,ru,ru-ru,sv,sv-se,yue,zh-chs,zh-cht,zh-cn,zh-hk,zh-tw";
        private AzureAccessToken _azureAccessToken;

        public Translator(string translateApiKey)
        {
            _azureAccessToken = new AzureAccessToken(translateApiKey);
        }

        public string[] GetLanguages()
        {
            return LangugagesList.Split(',');
        }

        public string Translate(string sourceText, string fromLanguauge, string toLanguage)
        {
            TranslatorService.LanguageServiceClient client = new TranslatorService.LanguageServiceClient();

            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                var token = _azureAccessToken.GetAccessToken();
                return client.Translate("Bearer " + token, sourceText, fromLanguauge, toLanguage, "text/plain", "", "");
            }
        }

        /// <remarks>
        /// Use Http as WebService and binary stream don't mix.
        /// </remarks>
        public void Speak(string text, string language)
        {
            var token = _azureAccessToken.GetAccessToken();

            string uri = string.Format("http://api.microsofttranslator.com/v2/Http.svc/Speak?text={0}&language={1}&format=" + 
                HttpUtility.UrlEncode("audio/wav") + "&options=MaxQuality", text, language);

            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Headers.Add("Authorization", "Bearer " + token);
            WebResponse response = null;
            try
            {
                response = webRequest.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    using (SoundPlayer player = new SoundPlayer(stream))
                    {
                        player.PlaySync();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }
    }
}
