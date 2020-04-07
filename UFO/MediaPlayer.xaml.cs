using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace UFO
{
    /// <summary>
    /// 音声再生用のUI
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        public static System.Windows.Media.MediaPlayer player = new System.Windows.Media.MediaPlayer();

        /// <summary>
        /// 現在停止中かどうか
        /// </summary>
        public static bool isPaused { get; private set; } = true;

        /// <summary>
        /// 再生時刻を表示するタイマー
        /// </summary>
        private DispatcherTimer mediaPositionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        /// <summary>
        /// スライダーの位置を更新するタイマー
        /// </summary>
        private DispatcherTimer sliderPostionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        /// <summary>
        /// グラフの位置を更新するタイマー
        /// </summary>
        private static DispatcherTimer graphPositionTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };

        /// <summary>
        /// Slider_ValueChangedイベントがユーザーの手で起こされたものかコード上起こされたものか判別する
        /// </summary>
        private bool changeSliderByCode = false;

        static MediaPlayer()
        {
            graphPositionTimer.Tick += GraphPositionTimer_Tick;
            graphPositionTimer.Start();
        }

        public MediaPlayer()
        {
            InitializeComponent();

            slider.IsEnabled = false;
            player.MediaOpened += this.Player_MediaOpened;
            mediaPositionTimer.Tick += (_s, _e) => SetPlayTimeText(player.Position);
            sliderPostionTimer.Tick += (_s, _e) =>
            {
                changeSliderByCode = true;
                slider.Value = player.Position.TotalMilliseconds;
            };
            // すでにファイルが開かれている（2つ目以降のプレイヤー）だったら
            if (player.Source != null)
            {
                filePath.Text = player.Source.AbsoluteUri;
                Player_MediaOpened(this, null); // 各種表示用タイマーも開始
            }
        }

        /// <summary>
        /// グラフ更新用のタイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GraphPositionTimer_Tick(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                MainWindow.Instance.graphControl.SetPosition(player.Position, isPaused);
            }
        }


        /// <summary>
        /// ファイルの読み込みが完了したときその音声に合わせてスライダーをセットする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_MediaOpened(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                slider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
                slider.IsEnabled = true;

                // 稼働中のタイマーを停止
                mediaPositionTimer?.Stop();
                sliderPostionTimer?.Stop();
                // 再生時間表示タイマーを再起動
                mediaPositionTimer.Start();
                sliderPostionTimer.Start();
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "wav files (*.wav)|*.wav|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                filePath.Text = dialog.FileName;

                if (player.Source != null)
                    player.Close();
                player.Open(new Uri(filePath.Text, UriKind.Absolute));

            }
        }

        /// <summary>
        /// 現在の再生時間を表示
        /// </summary>
        /// <param name="position"></param>
        private void SetPlayTimeText(TimeSpan position)
        {
            string text = position.ToString(@"hh\:mm\:ss\.ff") + " (" + Math.Round(position.TotalSeconds * 10, 0, MidpointRounding.AwayFromZero).ToString() + ")";
            playTimeText.Text = text;
        }

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            player.Play();
            isPaused = false;
        }

        /// <summary>
        /// 外部から止めたいとき
        /// </summary>
        public void Stop()
        {
            changeSliderByCode = true;
            PauseButton_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// 一時停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
            isPaused = true;
            PortUtil.Instance.SendStop();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!changeSliderByCode)
            {
                TimeSpan newPosition = TimeSpan.FromMilliseconds(slider.Value);
                player.Position = newPosition;
            }
            changeSliderByCode = false;
        }

        /// <summary>
        /// 10秒進む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add10secButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan newPosition = TimeSpan.FromSeconds(player.Position.TotalSeconds + 10);
            player.Position = newPosition;
        }

        private void Back5secButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan newPosition = TimeSpan.FromSeconds(player.Position.TotalSeconds - 5);
            player.Position = newPosition;
        }
    }
}
