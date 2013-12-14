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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Data;


namespace KBlog_WPF.Player
{
    /// <summary>
    /// Interaction logic for AudioPlayer.xaml
    /// </summary>
    public partial class AudioPlayer : Page
    {
        DispatcherTimer timer;

        public delegate void timerTick();
        timerTick tick;

        bool isDragging = false;
        bool fileIsPlaying = false;
        string sec, min, hours;

        public AudioPlayer()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            tick = new timerTick(changeStatus);

            //mediaElement.Source = new Uri(ApplicationState.GetValue<string>("temp_audio_path"));
            mediaElement.Source = new Uri(@"C:\KBlog_media\1\Audio\temp.wav");

            BitmapImage bi4 = new BitmapImage();
            bi4.BeginInit();
            bi4.UriSource = new Uri(@"Images\pause.png", UriKind.Relative);
            bi4.EndInit();

            pauseBtn.Background = new ImageBrush(bi4);
            pauseBtn.Height = 100;
            pauseBtn.Width = 100;

            BitmapImage bi5 = new BitmapImage();
            bi5.BeginInit();
            bi5.UriSource = new Uri(@"Images\play.png", UriKind.Relative);
            bi5.EndInit();

            playBtn.Background = new ImageBrush(bi5);
            playBtn.Height = 100;
            playBtn.Width = 100;

            BitmapImage bi6 = new BitmapImage();
            bi6.BeginInit();
            bi6.UriSource = new Uri(@"Images\stop.png", UriKind.Relative);
            bi6.EndInit();

            stopBtn.Background = new ImageBrush(bi6);
            stopBtn.Height = 100;
            stopBtn.Width = 100;
        }

         void timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(tick);
        }


        //visualize progressBar 
        void changeStatus()
        {
            if (fileIsPlaying)
            {

                #region customizeTime
                if (mediaElement.Position.Seconds < 10)
                    sec = "0" + mediaElement.Position.Seconds.ToString();
                else
                    sec = mediaElement.Position.Seconds.ToString();


                if (mediaElement.Position.Minutes < 10)
                    min = "0" + mediaElement.Position.Minutes.ToString();
                else
                    min = mediaElement.Position.Minutes.ToString();

                if (mediaElement.Position.Hours < 10)
                    hours = "0" + mediaElement.Position.Hours.ToString();
                else
                    hours = mediaElement.Position.Hours.ToString();

                #endregion customizeTime

                seekSlider.Value = mediaElement.Position.TotalMilliseconds;
                progressBar.Value = mediaElement.Position.TotalMilliseconds;

                if (mediaElement.Position.Hours == 0)
                {

                    currentTimeTextBlock.Text = min + ":" + sec;
                }
                else
                {
                    currentTimeTextBlock.Text = hours + ":" + min + ":" + sec;
                }
            }
        }


        //open the file
        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            Stream checkStream = null;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Supported File Types(*.mp3,*.wav,*.mpeg,*.wmv,*.avi)|*.mp3;*.wav;*.mpeg;*.wmv;*.avi";
            // Show open file dialog box 
            if ((bool)dlg.ShowDialog())
            {
                try
                {
                    if ((checkStream = dlg.OpenFile()) != null)
                    {
                        mediaElement.Source = new Uri(dlg.FileName);
                    }
                    Thread.Sleep(50);
                    mediaElement.Close();
                    mediaElement.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }


        //occurs when the file is opened
        public void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            timer.Start();
            fileIsPlaying = true;
            openMedia();
        }


        //opens media,adds file to playlist and gets file info
        public void openMedia()
        {
            InitializePropertyValues();
            try
            {
                #region customizeTime
                if (mediaElement.NaturalDuration.TimeSpan.Seconds < 10)
                    sec = "0" + mediaElement.Position.Seconds.ToString();
                else
                    sec = mediaElement.NaturalDuration.TimeSpan.Seconds.ToString();

                if (mediaElement.NaturalDuration.TimeSpan.Minutes < 10)
                    min = "0" + mediaElement.NaturalDuration.TimeSpan.Minutes.ToString();
                else
                    min = mediaElement.NaturalDuration.TimeSpan.Minutes.ToString();

                if (mediaElement.NaturalDuration.TimeSpan.Hours < 10)
                    hours = "0" + mediaElement.NaturalDuration.TimeSpan.Hours.ToString();
                else
                    hours = mediaElement.NaturalDuration.TimeSpan.Hours.ToString();

                if (mediaElement.NaturalDuration.TimeSpan.Hours == 0)
                {

                    endTimeTextBlock.Text = min + ":" + sec;
                }
                else
                {
                    endTimeTextBlock.Text = hours + ":" + min + ":" + sec;
                }

                #endregion customizeTime
            }
            catch { }
            string path = mediaElement.Source.LocalPath.ToString();

            double duration = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            seekSlider.Maximum = duration;
            progressBar.Maximum = duration;

            mediaElement.Volume = volumeSlider.Value;
            mediaElement.ScrubbingEnabled = true;

            volumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(volumeSlider_ValueChanged);
           
        }

        //occurs when the file is done playing
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            volumeSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(volumeSlider_ValueChanged);
            
        }


        //initialize properties of file
        void InitializePropertyValues()
        {
            mediaElement.Volume = (double)volumeSlider.Value;
        }


        //seek to desirable position of the file
        private void seekSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)seekSlider.Value);

            changePostion(ts);
        }


        //mouse down on slide bar in order to seek
        private void seekSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            fileIsPlaying = false;
        }


        private void seekSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)seekSlider.Value);
                changePostion(ts);
                fileIsPlaying = true;
            }
            isDragging = false;
        }


        //change position of the file
        void changePostion(TimeSpan ts)
        {
            mediaElement.Position = ts;
        }

        //play the file
        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            fileIsPlaying = true;
            mediaElement.Play();
            timer.Start();
        }

        //pause the file
        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            fileIsPlaying = false;
            mediaElement.Pause();
            timer.Stop();
        }


        //stop the file
        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            fileIsPlaying = false;
            timer.Stop();
            mediaElement.Stop();
            seekSlider.Value = 0;
            progressBar.Value = 0;
            currentTimeTextBlock.Text = "00:00";
        }


        //turn volume up-down
        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }

    }

    
}
