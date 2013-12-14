using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Kinect;
using System.Windows.Threading;
using System.Windows.Navigation;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;

namespace KBlog_WPF
{
    /// <summary>
    /// Interaction logic for AudioRecord.xaml
    /// </summary>
    public partial class AudioRecord : Window
    {

        #region Audio Vars
        /// <summary>
        /// Number of milliseconds between each read of audio data from the stream.
        /// Faster polling (few tens of ms) ensures a smoother audio stream visualization.
        /// </summary>
        private const int AudioPollingInterval = 50;

        /// <summary>
        /// Number of samples captured from Kinect audio stream each millisecond.
        /// </summary>
        private const int SamplesPerMillisecond = 16;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample.
        /// </summary>
        private const int BytesPerSample = 2;

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        private const int SamplesPerColumn = 40;

        /// <summary>
        /// Width of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapWidth = 780;

        /// <summary>
        /// Height of bitmap that stores audio stream energy data ready for visualization.
        /// </summary>
        private const int EnergyBitmapHeight = 195;

        /// <summary>
        /// Bitmap that contains constructed visualization for audio stream energy, ready to
        /// be displayed. It is a 2-color bitmap with white as background color and blue as
        /// foreground color.
        /// </summary>
        private readonly WriteableBitmap energyBitmap;

        /// <summary>
        /// Rectangle representing the entire energy bitmap area. Used when drawing background
        /// for energy visualization.
        /// </summary>
        private readonly Int32Rect fullEnergyRect = new Int32Rect(0, 0, EnergyBitmapWidth, EnergyBitmapHeight);

        /// <summary>
        /// Array of background-color pixels corresponding to an area equal to the size of whole energy bitmap.
        /// </summary>
        private readonly byte[] backgroundPixels = new byte[EnergyBitmapWidth * EnergyBitmapHeight];

        /// <summary>
        /// Buffer used to hold audio data read from audio stream.
        /// </summary>
        private readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

        /// <summary>
        /// Buffer used to store audio stream energy data as we read audio.
        /// 
        /// We store 25% more energy values than we strictly need for visualization to allow for a smoother
        /// stream animation effect, since rendering happens on a different schedule with respect to audio
        /// capture.
        /// </summary>
        private readonly double[] energy = new double[(uint)(EnergyBitmapWidth * 1.25)];

        /// <summary>
        /// Object for locking energy buffer to synchronize threads.
        /// </summary>
        private readonly object energyLock = new object();

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Stream of audio being captured by Kinect sensor.
        /// </summary>
        private Stream audioStream;

        /// <summary>
        /// <code>true</code> if audio is currently being read from Kinect stream, <code>false</code> otherwise.
        /// </summary>
        private bool reading;

        /// <summary>
        /// Thread that is reading audio from Kinect stream.
        /// </summary>
        private Thread recordingThread;


        /// <summary>
        /// Array of foreground-color pixels corresponding to a line as long as the energy bitmap is tall.
        /// This gets re-used while constructing the energy visualization.
        /// </summary>
        private byte[] foregroundPixels;

        /// <summary>
        /// Sum of squares of audio samples being accumulated to compute the next energy value.
        /// </summary>
        private double accumulatedSquareSum;

        /// <summary>
        /// Number of audio samples accumulated so far to compute the next energy value.
        /// </summary>
        private int accumulatedSampleCount;

        /// <summary>
        /// Index of next element available in audio energy buffer.
        /// </summary>
        private int energyIndex;

        /// <summary>
        /// Number of newly calculated audio stream energy values that have not yet been
        /// displayed.
        /// </summary>
        private int newEnergyAvailable;

        /// <summary>
        /// Error between time slice we wanted to display and time slice that we ended up
        /// displaying, given that we have to display in integer pixels.
        /// </summary>
        private double energyError;

        /// <summary>
        /// Last time energy visualization was rendered to screen.
        /// </summary>
        private DateTime? lastEnergyRefreshTime;

        /// <summary>
        /// Index of first energy element that has never (yet) been displayed to screen.
        /// </summary>
        private int energyRefreshIndex;

        //filestream to save audio data
        FileStream fileStream2;

        private readonly KinectSensorChooser sensorChooser;


        #endregion
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public AudioRecord()
        {
            InitializeComponent();

            this.energyBitmap = new WriteableBitmap(EnergyBitmapWidth, EnergyBitmapHeight, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Colors.White, (Color)this.Resources["KinectPurpleColor"] }));

            // Dynamically set background images for Buttons
            #region Buttons
            BitmapImage bi1 = new BitmapImage();
            bi1.BeginInit();
            bi1.UriSource = new Uri(@"Images\exit2.png", UriKind.Relative);
            bi1.EndInit();

            exitBtn.Background = new ImageBrush(bi1);
            exitBtn.Height = 100;
            exitBtn.Width = 100;
            exitBtn.HorizontalAlignment = HorizontalAlignment.Right;
            exitBtn.VerticalAlignment = VerticalAlignment.Bottom;
            exitBtn.ToolTip = "Press to exit the application";

            BitmapImage bi2 = new BitmapImage();
            bi2.BeginInit();
            bi2.UriSource = new Uri(@"Images\Home-icon.png", UriKind.Relative);
            bi2.EndInit();

            homeBtn.Background = new ImageBrush(bi2);
            homeBtn.Height = 100;
            homeBtn.Width = 100;
            homeBtn.HorizontalAlignment = HorizontalAlignment.Left;
            homeBtn.VerticalAlignment = VerticalAlignment.Bottom;
            homeBtn.ToolTip = "Press to return to Home Screen";

            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(@"Images\record2.png", UriKind.Relative);
            bi3.EndInit();

            recordBtn.Background = new ImageBrush(bi3);
            recordBtn.Height = 100;
            recordBtn.Width = 100;
            recordBtn.HorizontalAlignment = HorizontalAlignment.Center;
            recordBtn.VerticalAlignment = VerticalAlignment.Bottom;
            recordBtn.ToolTip = "Press to record audio";

            BitmapImage bi7 = new BitmapImage();
            bi7.BeginInit();
            bi7.UriSource = new Uri(@"Images\save.png", UriKind.Relative);
            bi7.EndInit();

            saveBtn.Background = new ImageBrush(bi7);
            saveBtn.Height = 100;
            saveBtn.Width = 100;
            saveBtn.Visibility = Visibility.Hidden;

            BitmapImage bi8 = new BitmapImage();
            bi8.BeginInit();
            bi8.UriSource = new Uri(@"Images\delete.png", UriKind.Relative);
            bi8.EndInit();

            cancelBtn.Background = new ImageBrush(bi8);
            cancelBtn.Height = 100;
            cancelBtn.Width = 100;
            cancelBtn.Visibility = Visibility.Hidden;
            #endregion

            #region KinectChooser
            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
            #endregion
        }

        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        /// <summary>
        /// Execute initialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                //this.statusBarText.Text = Properties.Resources.NoKinectReady;
                return;
            }

            // Initialize foreground pixels
            this.foregroundPixels = new byte[EnergyBitmapHeight];
            for (int i = 0; i < this.foregroundPixels.Length; ++i)
            {
                this.foregroundPixels[i] = 0xff;
            }

            this.waveDisplay.Source = this.energyBitmap;

            CompositionTarget.Rendering += UpdateEnergy;



            // Start streaming audio!
            this.audioStream = this.sensor.AudioSource.Start();

            // Use a separate thread for capturing audio because audio stream read operations
            // will block, and we don't want to block main UI thread.
            this.reading = true;
            this.recordingThread = new Thread(AudioRecordingThread);
            this.recordingThread.Priority = ThreadPriority.Highest;
            //this.readingThread.Start();

            //this.recordingThread = new Thread(RecordingThread);
            //this.recordingThread.Priority = ThreadPriority.Highest;

            
        }

        /// <summary>
        /// A bare bones WAV file header writer
        /// </summary>        
        static void WriteWavHeader(Stream stream, int dataLength)
        {
            //We need to use a memory stream because the BinaryWriter will close the underlying stream when it is closed
            using (MemoryStream memStream = new MemoryStream(64))
            {
                int cbFormat = 18; //sizeof(WAVEFORMATEX)
                WAVEFORMATEX format = new WAVEFORMATEX()
                {
                    //nAvrgBytesPerSec= nSamplesPerSec * nBlockAlign
                    wFormatTag = 1,
                    nChannels = 1,
                    nSamplesPerSec = 16000,
                    nAvgBytesPerSec = 32000,
                    nBlockAlign = 2,
                    wBitsPerSample = 16,
                    cbSize = 0
                };

                using (var bw = new BinaryWriter(memStream))
                {
                    //RIFF header
                    WriteString(memStream, "RIFF");
                    bw.Write(dataLength + cbFormat + 4); //File size - 8
                    WriteString(memStream, "WAVE");
                    WriteString(memStream, "fmt ");
                    bw.Write(cbFormat);

                    //WAVEFORMATEX
                    bw.Write(format.wFormatTag);
                    bw.Write(format.nChannels);
                    bw.Write(format.nSamplesPerSec);
                    bw.Write(format.nAvgBytesPerSec);
                    bw.Write(format.nBlockAlign);
                    bw.Write(format.wBitsPerSample);
                    bw.Write(format.cbSize);

                    //data header
                    WriteString(memStream, "data");
                    bw.Write(dataLength);
                    memStream.WriteTo(stream);
                }
            }
        }

        static void WriteString(Stream stream, string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        /// <summary>
        /// Execute uninitialization tasks.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            // Tell audio reading thread to stop and wait for it to finish.
            this.reading = false;
            if (null != recordingThread && recordingThread.IsAlive)
            {
                bool test = recordingThread.IsAlive;

                recordingThread.Join();
            }

            if (null != this.sensor)
            {
                CompositionTarget.Rendering -= UpdateEnergy;
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }
        }

  

        /// <summary>
        /// Handles polling audio stream and updating visualization every tick.
        /// </summary>
        private void AudioRecordingThread()
        {
            // Bottom portion of computed energy signal that will be discarded as noise.
            // Only portion of signal above noise floor will be displayed.
            const double EnergyNoiseFloor = 0.2;

            while (this.reading)
            {
                int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);

                fileStream2.Write(audioBuffer, 0, readCount);
           
                // Calculate energy corresponding to captured audio.
                // In a computationally intensive application, do all the processing like
                // computing energy, filtering, etc. in a separate thread.
                lock (this.energyLock)
                {
                    for (int i = 0; i < readCount; i += 2)
                    {
                        // compute the sum of squares of audio samples that will get accumulated
                        // into a single energy value.
                        short audioSample = BitConverter.ToInt16(audioBuffer, i);
                        this.accumulatedSquareSum += audioSample * audioSample;
                        ++this.accumulatedSampleCount;

                        if (this.accumulatedSampleCount < SamplesPerColumn)
                        {
                            continue;
                        }

                        // Each energy value will represent the logarithm of the mean of the
                        // sum of squares of a group of audio samples.
                        double meanSquare = this.accumulatedSquareSum / SamplesPerColumn;
                        double amplitude = Math.Log(meanSquare) / Math.Log(int.MaxValue);

                        // Renormalize signal above noise floor to [0,1] range.
                        this.energy[this.energyIndex] = Math.Max(0, amplitude - EnergyNoiseFloor) / (1 - EnergyNoiseFloor);
                        this.energyIndex = (this.energyIndex + 1) % this.energy.Length;

                        this.accumulatedSquareSum = 0;
                        this.accumulatedSampleCount = 0;
                        ++this.newEnergyAvailable;
                    }
                }
            }
        }

        /// <summary>
        /// Handles rendering energy visualization into a bitmap.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void UpdateEnergy(object sender, EventArgs e)
        {
            lock (this.energyLock)
            {
                // Calculate how many energy samples we need to advance since the last update in order to
                // have a smooth animation effect
                DateTime now = DateTime.UtcNow;
                DateTime? previousRefreshTime = this.lastEnergyRefreshTime;
                this.lastEnergyRefreshTime = now;

                // No need to refresh if there is no new energy available to render
                if (this.newEnergyAvailable <= 0)
                {
                    return;
                }

                if (previousRefreshTime != null)
                {
                    double energyToAdvance = this.energyError + (((now - previousRefreshTime.Value).TotalMilliseconds * SamplesPerMillisecond) / SamplesPerColumn);
                    int energySamplesToAdvance = Math.Min(this.newEnergyAvailable, (int)Math.Round(energyToAdvance));
                    this.energyError = energyToAdvance - energySamplesToAdvance;
                    this.energyRefreshIndex = (this.energyRefreshIndex + energySamplesToAdvance) % this.energy.Length;
                    this.newEnergyAvailable -= energySamplesToAdvance;
                }

                // clear background of energy visualization area
                this.energyBitmap.WritePixels(fullEnergyRect, this.backgroundPixels, EnergyBitmapWidth, 0);

                // Draw each energy sample as a centered vertical bar, where the length of each bar is
                // proportional to the amount of energy it represents.
                // Time advances from left to right, with current time represented by the rightmost bar.
                int baseIndex = (this.energyRefreshIndex + this.energy.Length - EnergyBitmapWidth) % this.energy.Length;
                for (int i = 0; i < EnergyBitmapWidth; ++i)
                {
                    const int HalfImageHeight = EnergyBitmapHeight / 2;

                    // Each bar has a minimum height of 1 (to get a steady signal down the middle) and a maximum height
                    // equal to the bitmap height.
                    int barHeight = (int)Math.Max(1.0, (this.energy[(baseIndex + i) % this.energy.Length] * EnergyBitmapHeight));

                    // Center bar vertically on image
                    var barRect = new Int32Rect(i, HalfImageHeight - (barHeight / 2), 1, barHeight);

                    // Draw bar in foreground color
                    this.energyBitmap.WritePixels(barRect, foregroundPixels, 1, 0);
                }
            }
        }

        private void recordBtn_Click(object sender, RoutedEventArgs e)
        {

            this.recordingThread.Start();

            string temp_file_name = "temp.wav";
            string mediaPath = ApplicationState.GetValue<string>("kblog_media");
            string user_audio_path = Path.Combine(mediaPath, ApplicationState.GetValue<string>("user_id"), "Audio");
            //string user_audio_path = @"C:\KBlog_media\1\Audio";
            if (!Directory.Exists(user_audio_path))
            {
                Directory.CreateDirectory(user_audio_path);
            }

            string file_name = Guid.NewGuid().ToString() + ".wav";
            string audio_path = Path.Combine(user_audio_path, file_name);
            ApplicationState.SetValue("audio_file_name", audio_path);

            fileStream2 = new FileStream(audio_path, FileMode.Create);

            int rec_time = (int)30 * 2 * 16000;//30 sec
            WriteWavHeader(fileStream2, rec_time);

            recordBtn.Visibility = Visibility.Hidden;
            countDownFrame.Content = new Timers.AudioTimer();
            countDownFrame.Visibility = Visibility.Visible;

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            DispatcherTimer dispatcherTimerSender = sender as DispatcherTimer;
            dispatcherTimerSender.Stop(); // execute only once;
            if (dispatcherTimerSender.IsEnabled == true)
            {
                string test = "STILL Ticking, Tick Tock";
            }
            else
            {

               // File Saved as temp.wav already. Replay now!
                saveLbl.Visibility = Visibility.Visible;
                saveBtn.Visibility = Visibility.Visible;
                cancelBtn.Visibility=Visibility.Visible;
            }
        }

        private void UnregisterEvent()
        {
            // Tell audio reading thread to stop and wait for it to finish.
            if (null != this.sensor)
            {
                CompositionTarget.Rendering -= UpdateEnergy;
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.reloadWindow();
        }

        private void reloadWindow() 
        {
            UnregisterEvent();
            AudioRecord ar = new AudioRecord();
            ar.Show();
            this.Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            //save val to db
            Media_DB_Query query = new Media_DB_Query();
            query.saveMedia(ApplicationState.GetValue<string>("user_id"), "Audio", ApplicationState.GetValue<string>("audio_file_name"));
            MessageBox.Show("Audio successfully saved");
            reloadWindow();


        }

        private void homeBtn_Click(object sender, RoutedEventArgs e)
        {
            UnregisterEvent();
            HomeScreen home = new HomeScreen();
            home.Show();
            this.Close();

        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            UnregisterEvent();
            this.Close();
        }
    }
}


