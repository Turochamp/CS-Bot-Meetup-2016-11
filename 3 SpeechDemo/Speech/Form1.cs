using Microsoft.CognitiveServices.SpeechRecognition;
using SpeechPrototype.Model;
using System;
using System.Text;
using System.Windows.Forms;

namespace SpeechPrototype
{
    public partial class Form1 : Form
    {
        private SpeechToText _sttClient;
        private Translator _translatorClient;
        private string _speechApiKey = "03f570ed51fb41ffbc0814f28da387c7";
        private string _translateApiKey = "32d6c6439b7646ae8a46a8ee12281567";

        public Form1()
        {
            InitializeComponent();

            _sttClient = new SpeechToText(_speechApiKey);
            _sttClient.OnSttStatusUpdated += OnSttStatusUpdated;

            _translatorClient = new Translator(_translateApiKey);

            fromComboBox.Items.AddRange(
                _sttClient.GetLanguages());
            fromComboBox.SelectedIndex = fromComboBox.Items.IndexOf("en-US");

            toComboBox.Items.AddRange(
                _translatorClient.GetLanguages());

            timer1.Interval = 1000;
            progressBar1.Maximum = 15;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value != progressBar1.Maximum)
            {
                progressBar1.Value++;
            }
            else
            {
                timer1.Stop();
            }
        }

        private void OnSttStatusUpdated(object sender, SpeechToTextEventArgs e)
        {
            timer1.Stop();

            StringBuilder sb = new StringBuilder();
            if (e.Status == SttStatus.Success)
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    sb.AppendFormat("Result message: {0}\n\n", e.Message);
                }

                if (e.Results != null && e.Results.Count != 0)
                {
                    sb.Append("Retrieved the following results:\n");
                    foreach (string sentence in e.Results)
                    {
                        sb.AppendFormat("{0}\n", sentence);

                        var toLanguage = GetSelectedText(toComboBox);
                        if (!string.IsNullOrEmpty(toLanguage))
                        {
                            var translated = Translate(sentence);
                            sb.AppendFormat("Translation ({0}): {1}\n\n", toLanguage, translated);

                            Speak(translated, toLanguage);
                        }
                    }
                }
            }
            else
            {
                sb.AppendFormat("Could not convert speech to text: {0}\n", e.Message);
            }

            sb.Append("\n");

            SetText(sb.ToString());
        }

        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.AppendText(text.Replace("\n", "\r\n"));
            }
        }

        private string Translate(string sourceText)
        {
            var fromLanguage = GetSelectedText(fromComboBox);
            var toLanguage = GetSelectedText(toComboBox);

            return _translatorClient.Translate(sourceText, fromLanguage, toLanguage);
        }

        private void Speak(string text, string toLanguage)
        {
            _translatorClient.Speak(text, toLanguage);
        }

        private string GetSelectedText(ComboBox comboBox)
        {
            string text = null;
            this.Invoke((Action)(() => text = comboBox.Text));
            return text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var language = fromComboBox.Text;
            _sttClient.StartMicToText(SpeechRecognitionMode.ShortPhrase, language);

            progressBar1.Value = 0;
            timer1.Start();
            timer1.Enabled = true;
        }

    }
}

