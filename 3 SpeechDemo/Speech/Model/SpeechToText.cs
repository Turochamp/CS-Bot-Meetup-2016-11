﻿using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpeechPrototype.Model
{
    public class SpeechToText : IDisposable
    {
        private const string LanguagesList = "de-DE,zh-TW,zh-HK,ru-RU,es-ES,ja-JP,ar-EG,da-DK,en-GB,en-IN,fi-FI,nl-NL,en-US,pt-BR,pt-PT,ca-ES,fr-FR,ko-KR,en-NZ,nb-NO,it-IT,fr-CA,pl-PL,es-MX,zh-CN,en-AU,en-CA,sv-SE";

        public event EventHandler<SpeechToTextEventArgs> OnSttStatusUpdated;
        private MicrophoneRecognitionClient _micRecClient;
        private string _bingApiKey;
        private bool _isMicRecording = false;

        public SpeechToText(string bingApiKey)
        {
            _bingApiKey = bingApiKey;
        }

        public string[] GetLanguages()
        {
            return LanguagesList.Split(',');
        }

        private void OnPartialResponseReceived(object sender, PartialSpeechResponseEventArgs e)
        {
            Debug.WriteLine($"Partial response received: { e.PartialResult}");
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Debug.WriteLine($"Microphone status changed to recording: { e.Recording}");
        }

        private void RaiseSttStatusUpdated(SpeechToTextEventArgs e)
        {
            OnSttStatusUpdated?.Invoke(this, e);
        }

        private void SpeechToText_OnSttStatusUpdated(object sender, SpeechToTextEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnIntentReceived(object sender, SpeechIntentEventArgs e)
        {
            SpeechToTextEventArgs args = new
                SpeechToTextEventArgs(SttStatus.Success,
                $"Intent received: {e.Intent.ToString()}.\nPayload: { e.Payload }"); 

            RaiseSttStatusUpdated(args);
        }

        private void OnConversationErrorReceived(object sender,
        SpeechErrorEventArgs e)
        {
            if (_isMicRecording) StopMicRecording();

            string message = 
                $"Speech to text failed with status code: { e.SpeechErrorCode.ToString()}, and error message: { e.SpeechErrorText} ";
            SpeechToTextEventArgs args = new
                SpeechToTextEventArgs(SttStatus.Error, message);

            RaiseSttStatusUpdated(args);
        }

        private void StopMicRecording()
        {
            _micRecClient.EndMicAndRecognition();
            _isMicRecording = false;
        }

        private void OnResponseReceived(object sender, SpeechResponseEventArgs e)
        {
            if (_isMicRecording) StopMicRecording();

            RecognizedPhrase[] recognizedPhrases = e.PhraseResponse.Results;
            List<string> phrasesToDisplay = new List<string>();
            foreach (RecognizedPhrase phrase in recognizedPhrases)
            {
                phrasesToDisplay.Add(phrase.DisplayText);
            }

            SpeechToTextEventArgs args = new
            SpeechToTextEventArgs(SttStatus.Success,
                $"STT completed with status: { e.PhraseResponse.RecognitionStatus.ToString() }",
                phrasesToDisplay);

            RaiseSttStatusUpdated(args);
        }

        public void StartMicToText(SpeechRecognitionMode speechMode, string language)
        {
            _micRecClient = SpeechRecognitionServiceFactory.
                CreateMicrophoneClient(speechMode, language, _bingApiKey);

            _micRecClient.OnMicrophoneStatus += OnMicrophoneStatus;
            _micRecClient.OnPartialResponseReceived += OnPartialResponseReceived;
            _micRecClient.OnResponseReceived += OnResponseReceived;
            _micRecClient.OnConversationError += OnConversationErrorReceived;

            _micRecClient.StartMicAndRecognition();
            _isMicRecording = true;
        }

        public void Dispose()
        {
            if (_micRecClient != null)
            {
                _micRecClient.EndMicAndRecognition();
                _micRecClient.OnMicrophoneStatus -= OnMicrophoneStatus;
                _micRecClient.OnPartialResponseReceived -= OnPartialResponseReceived;
                _micRecClient.OnResponseReceived -= OnResponseReceived;
                _micRecClient.OnConversationError -= OnConversationErrorReceived;
                _micRecClient.Dispose();
                _micRecClient = null;
            }
        }
    }

    public enum SttStatus { Success, Error }

    public class SpeechToTextEventArgs : EventArgs
    {
        public SttStatus Status { get; private set; }
        public string Message { get; private set; }
        public List<string> Results { get; private set; }
        public SpeechToTextEventArgs(SttStatus status,
        string message, List<string> results = null)
        {
            Status = status;
            Message = message;
            Results = results;
        }
    }
}
