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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        private const string Onerule = "onerule";
        private const string tworule = "tworule";
        private const string threerule = "threerule";
        private const string fourrule = "fourrule";
        private const string fiverule = "fiverule";
        private const string sixrule = "sixrule";
        private const string sevenrule = "sevenrule";
        private const string eightrule = "eightrule";
        private const string ninerule = "ninerule";
        private const string zerorule = "zerorule";
        private const string deleterule = "deleterule";
        private const string clearrule = "clearrule";
        private const string finishrule = "finishrule";

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

        ///Numbers  
        private Grammar OneruleGrammar;
        private Grammar tworuleGrammar;
        private Grammar threeruleGrammar;
        private Grammar fourruleGrammar;
        private Grammar fiveruleGrammar;
        private Grammar sixruleGrammar;
        private Grammar sevenruleGrammar;
        private Grammar eightruleGrammar;
        private Grammar nineruleGrammar;
        private Grammar zeroruleGrammar; 
        //delete Command
        private Grammar deleteGrammar;
        private Grammar clearGrammar;
        private Grammar finishGrammar;
        #endregion
        #region "Voice Recognition" 
        private void SpeechRecognized(object sender, SpeechRecognizerEventArgs e)
        { 
            const string OneruleCommand = "ONE";
            const string tworuleCommand = "TWO";
            const string threeruleCommand = "THREE";
            const string fourruleCommand = "FOUR";
            const string fiveruleCommand = "FIVE";
            const string sixruleCommand = "SIX";
            const string sevenruleCommand = "SEVEN";
            const string eightruleCommand = "EIGHT";
            const string nineruleCommand = "NINE";
            const string zeroruleCommand = "ZERO";
             
            //Delete 
            const string deleteruleComand = "DELETE";
            const string clearruleCommand = "CLEAR";
            const string finishruleCommand="FINISH";
              
            if (null == e.SemanticValue)
            {
                return;
            }


            // Handle game mode control commands
            switch (e.SemanticValue)
            {

                case OneruleCommand:

                    DisplayWords(OneruleCommand);
                    return;

                case tworuleCommand:

                    DisplayWords(tworuleCommand);
                    return;

                case threeruleCommand:
                    DisplayWords(threeruleCommand);
                    return;

                case fourruleCommand:
                    DisplayWords(fourruleCommand);
                    return;

                case fiveruleCommand:
                    DisplayWords(fiveruleCommand);
                    return;

                case sixruleCommand:
                    DisplayWords(sixruleCommand);
                    return;

                case sevenruleCommand:
                    DisplayWords(sevenruleCommand);
                    return;

                case eightruleCommand:
                    DisplayWords(eightruleCommand);
                    return;

                case nineruleCommand:
                    DisplayWords(nineruleCommand);
                    return;

                case zeroruleCommand:
                    DisplayWords(zeroruleCommand);
                    return;


                case deleteruleComand:
                    DisplayWords(deleteruleComand);
                    return; 

                case clearruleCommand:
                    DisplayWords(clearruleCommand);
                    return;

                case finishruleCommand:
                    DisplayWords(finishruleCommand);
                    return;

            }

            // We only handle speech commands with an associated sound source angle, so we can find the
            // associated player
            if (!e.SourceAngle.HasValue)
            {
                return;
            }
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

            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))  //Access a Gramar File
            {
                grammar = new Grammar(memoryStream, ruleName);
            }

            return grammar;
        }
         

        #endregion
        #region  "Kinect Variables"
        KinectSensor sensor = null;
        #endregion
        #region "WPF"  
        public MainWindow()
        { 
            InitializeComponent();
            Loaded += VoiceKeyBoard_Loaded;
        } 
        void VoiceKeyBoard_Loaded(object sender, RoutedEventArgs e)
        {

            if (KinectSensor.KinectSensors.Count == 0)
            {
                return;
            }
            sensor = KinectSensor.KinectSensors[0];
           
            try
            {
                //Check if the Sensor is Connected
                if (sensor.Status == KinectStatus.Connected)
                {
                    
                    sensor.Start();
                    
                    // Create and configure speech grammars and recognizer     
                    this.OneruleGrammar = CreateGrammar(Onerule);
                    this.tworuleGrammar = CreateGrammar(tworule);
                    this.threeruleGrammar = CreateGrammar(threerule);
                    this.fourruleGrammar = CreateGrammar(fourrule);
                    this.fiveruleGrammar = CreateGrammar(fiverule);
                    this.sixruleGrammar = CreateGrammar(sixrule);
                    this.sevenruleGrammar = CreateGrammar(sevenrule);
                    this.eightruleGrammar = CreateGrammar(eightrule);
                    this.nineruleGrammar = CreateGrammar(ninerule);
                    this.zeroruleGrammar = CreateGrammar(zerorule);

                    //Delete 
                    this.deleteGrammar = CreateGrammar(deleterule);
                    this.clearGrammar = CreateGrammar(clearrule);
                    this.finishGrammar=CreateGrammar(finishrule);
                    //Submits 

                    //recognize the speech
                    this.speechRecognizer = SpeechRecognizer.Create(
                        new[] { 
                             OneruleGrammar,
                            tworuleGrammar,
                            threeruleGrammar,
                            fourruleGrammar,
                            fiveruleGrammar,
                            sixruleGrammar,
                            sevenruleGrammar,
                            eightruleGrammar,
                            nineruleGrammar,
                            zeroruleGrammar,
                            deleteGrammar,
                            clearGrammar,
                            finishGrammar
                         });

                    if (null != speechRecognizer)
                    {
                        this.speechRecognizer.SpeechRecognized += SpeechRecognized;

                        this.speechRecognizer.Start(sensor.AudioSource);
                    }
                }
                else if (sensor.Status == KinectStatus.Disconnected)
                {
                    //nice message with Colors to alert you if your sensor is working or not
                    Message.Content = "Kinect Sensor is not Connected";
                    Message.Background = new SolidColorBrush(Colors.Orange);
                    Message.Foreground = new SolidColorBrush(Colors.Black);

                }
                else if (sensor.Status == KinectStatus.NotPowered)
                {//nice message with Colors to alert you if your sensor is working or not
                    Message.Content = "Kinect Sensor is not Powered";
                    Message.Background = new SolidColorBrush(Colors.Red);
                    Message.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (sensor.Status == KinectStatus.NotReady)
                {//nice message with Colors to alert you if your sensor is working or not
                    Message.Content = "Kinect Sensor is not Ready";
                    Message.Background = new SolidColorBrush(Colors.Red);
                    Message.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

            }
        }

        // Close Wuindow and Release Resources
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sensor = null;
        }

        /// <summary>
        /// Displays the words.
        /// </summary>
        /// <param name="result">The result.</param>
        private void DisplayWords(string result)
        {
            StringBuilder sb = new StringBuilder();

            if (txtUserGUID.Text.Trim() == "Please say your User GUID" | txtUserGUID.Text.Trim()== "Wrong User GUID. Please Repeat" )
            {
                sb.Append(string.Format("{0}", GenericFunctions.ConvertWordsToNumber(result)));
                txtUserGUID.Text = string.Empty;
                txtUserGUID.Text = sb.ToString();
            }
            else if (result == "DELETE")
            {
                if (txtUserGUID.Text.Length > 0)
                {
                    txtUserGUID.Text = txtUserGUID.Text.Substring(0, txtUserGUID.Text.Length - 1);
                }
            }
            else if (result == "CLEAR")
            {
                if (txtUserGUID.Text.Length > 0)
                {
                    txtUserGUID.Text = "  Please say your User GUID";
                }
            }
            else if (result =="FINISH")
            {
                User_DB_Query db= new User_DB_Query();
                bool correct_guid=db.validate_guid(txtUserGUID.Text.Trim());
                if (correct_guid)
                {
                    Loaded -= VoiceKeyBoard_Loaded;
                    sensor.Stop();
                    sensor = null;

                    //save media location path
                    ApplicationState.SetValue("kblog_media", @"C:\KBlog_Media");

                    HomeScreen homeScreen = new HomeScreen();
                    homeScreen.Show();
                    //Window1 wnd = new Window1();
                    //wnd.Show();
                    //AudioRecord ar = new AudioRecord();
                    //ar.Show();
                    this.Close();

                }
                else
                {
                    txtUserGUID.Text="Wrong User GUID. Please Repeat";
                }
            }
            else
            {
                sb.Append(txtUserGUID.Text);
                sb.Append(GenericFunctions.ConvertWordsToNumber(result));
                txtUserGUID.Text = sb.ToString();
            }
        }
        #endregion
  
    }
}
