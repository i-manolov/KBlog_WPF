using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.Sql;
using System.Windows.Navigation;

namespace KBlog_WPF
{
    public class MediaSpeech
    {
        #region "Voice Variables"
        /// <summary>
        /// Format of Kinect audio stream samples.
        /// </summary>
        private const EncodingFormat AudioFormat = EncodingFormat.Pcm;

        /// <summary>
        /// Samples per second in Kinect audio stream.
        /// </summary>
        private const int AudioSamplesPerSecond = 16000;

        /// <summary>
        /// Bits per audio sample in Kinect audio stream.
        /// </summary>
        private const int AudioBitsPerSample = 16;

        /// <summary>
        /// Number of channels in Kinect audio stream.
        /// </summary>
        private const int AudioChannels = 1;

        /// <summary>
        /// Average bytes per second in Kinect audio stream
        /// </summary>
        private const int AudioAverageBytesPerSecond = 32000;

        /// <summary>
        /// Block alignment in Kinect audio stream.
        /// </summary>
        private const int AudioBlockAlign = 2;

        /// <summary>
        /// Amount of time (in milliseconds) for which we keep sound source angle data coming from Kinect sensor.
        /// </summary>
        private const int AngleRetentionPeriod = 1000;

        /// <summary>
        /// Default threshold value (in [0.0,1.0] interval) used to determine wether we'll propagate a speech
        /// event or drop it as if it had never happened.
        /// </summary>
        private const double DefaultConfidenceThreshold = 0.3;


        /// <summary>
        /// Name of speech grammar corresponding to file. Note that the name must be the same, it is case sensative
        /// </summary> 
 
        private const string saverule = "saverule";
        private const string newrule = "newrule";

        //isSave bool value 
        Nullable<bool> isSave = null;

        // private const string 
        /// <summary>
        /// Speech recognizer used to detect voice commands issued by application users.
        /// </summary>
        private SpeechRecognizer speechRecognizer;


        #endregion
        #region "Grammar Variables"
        /// <summary>
        /// Speech grammar used during Application.
        /// </summary>   
        private Grammar saveGrammar;
        private Grammar newGrammar;
        #endregion

        public KinectSensor sensor;

        #region "Voice Recognition"

        public Nullable<bool> startAudioThread(KinectSensor sensor)
        {
            try
            {
                //Check if the Sensor is Connected
                if (sensor.Status == KinectStatus.Connected)
                {
                    // Create and configure speech grammars and recognizer     
                    this.saveGrammar = CreateGrammar(saverule);
                    this.newGrammar = CreateGrammar(newrule);

                    this.speechRecognizer = SpeechRecognizer.Create(
                        new[] { 
                            saveGrammar,
                            newGrammar
                        });
                    if (null != speechRecognizer)
                    {
                        this.speechRecognizer.SpeechRecognized += SpeechRecognized;

                        this.speechRecognizer.Start(sensor.AudioSource);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

            }
            return isSave;

        }
        
        /// <summary>
        /// Create a grammar from grammar definition XML file.
        /// </summary>
        /// <param name="ruleName">
        /// Rule corresponding to grammar we want to use.
        /// </param>Tha
        /// <returns>
        /// New grammar object corresponding to specified rule.
        /// </returns>
        private Grammar CreateGrammar(string ruleName)
        {
            Grammar grammar;

            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammarMedia)))  //Access The Media Gramar File
            {
                grammar = new Grammar(memoryStream, ruleName);
            }

            return grammar;
        }

        private void SpeechRecognized(object sender, SpeechRecognizerEventArgs e)
        {
            const string saveruleCommand = "SAVE";
            const string newruleCommand = "NEW";


            if (null == e.SemanticValue)
            {
                return;
            }


            // Handle game mode control commands
            switch (e.SemanticValue)
            {

                case saveruleCommand:
                    isSaveMedia(saveruleCommand);
                    return;

                case newruleCommand:
                    isSaveMedia(newruleCommand);
                    return;

            }


            // We only handle speech commands with an associated sound source angle, so we can find the
            // associated player
            if (!e.SourceAngle.HasValue)
            {
                return;
            }
        }        
       #endregion

        #region RecognizedSpeechHandler
        private Nullable<bool> isSaveMedia(string result)
        {
            if (result == "SAVE") { isSave = true; }
            else if (result == "NEW") { isSave = false; }
            return isSave;
        }
        #endregion
    }
}

