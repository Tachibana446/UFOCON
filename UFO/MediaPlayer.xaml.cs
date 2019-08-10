﻿using System;
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
    /// MediaPlayer.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        public System.Windows.Media.MediaPlayer player = new System.Windows.Media.MediaPlayer();

        private DispatcherTimer mediaPositionTimer = null;
        private DispatcherTimer sliderPostionTimer = null;

        /// <summary>
        /// Slider_ValueChangedイベントがユーザーの手で起こされたものかコード上起こされたものか判別する
        /// </summary>
        private bool changeSliderByCode = false;

        public MediaPlayer()
        {
            InitializeComponent();

            slider.IsEnabled = false;
            player.MediaOpened += Player_MediaOpened;
        }

        /// <summary>
        /// ファイルの読み込みが完了したときスライダーをセットする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_MediaOpened(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                slider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
                slider.IsEnabled = true;
                sliderPostionTimer = new DispatcherTimer();
                sliderPostionTimer.Interval = TimeSpan.FromMilliseconds(100);
                sliderPostionTimer.Tick += (_s, _e) =>
                {
                    slider.Value = player.Position.TotalMilliseconds;
                    changeSliderByCode = true;
                };
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

                player.Open(new Uri(filePath.Text, UriKind.Absolute));

                // 稼働中のタイマーを停止
                mediaPositionTimer?.Stop();
                sliderPostionTimer?.Stop();
                // 再生時間表示タイマーを再起動
                mediaPositionTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                mediaPositionTimer.Tick += (_s, _e) => SetPlayTimeText(player.Position);
                mediaPositionTimer.Start();
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

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            player.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
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
