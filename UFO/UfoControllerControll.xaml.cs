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

namespace UFO
{
    /// <summary>
    /// UfoControllerControll.xaml の相互作用ロジック
    /// </summary>
    public partial class UfoControllerControll : UserControl
    {
        Random rand = new Random();
        /// <summary>
        /// 動作パターン一覧
        /// </summary>
        List<Pattern> patterns = new List<Pattern>();
        /// <summary>
        /// 現在のパターン
        /// </summary>
        Pattern nowPattern = Pattern.rest;
        /// <summary>
        /// パターンが実行する最小の時間
        /// </summary>
        int minTimeSpan = 1000;
        int maxTimeSpan = 5000;
        int minPower = 0;
        int maxPower = 100;
        int nowPower = 0;
        /// <summary>
        /// 徐々に変化する系のパターンだったときの毎Tickの変化量
        /// </summary>
        double crescPower = 0;
        /// <summary>
        /// 最小の休憩時間(ms)
        /// </summary>
        int minBreakSpan = 1000;
        int maxBreakSpan = 3000;
        /// <summary>
        /// パターンを決めるタイマー
        /// </summary>
        DispatcherTimer setPatternTimer = new DispatcherTimer();
        /// <summary>
        /// UFOにデータを送信するタイマー
        /// </summary>
        DispatcherTimer sendUfoTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
        /// <summary>
        /// 現在停止中かどうか
        /// </summary>
        public bool IsPaused { get; private set; } = true;

        public UfoControllerControll()
        {
            var _patternsEnumerable = Enum.GetValues(typeof(Pattern)).OfType<Pattern>();
            patterns = new List<Pattern>(_patternsEnumerable);
            sendUfoTimer.Tick += SendUfoTimer_Tick;
            InitializeComponent();
        }

        /// <summary>
        /// スタートボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartRandom();
        }

        private void StartRandom()
        {
            if (patterns.Count() <= 1)
                return;
            nowPattern = Pattern.rest;
            setPatternTimer?.Stop();
            setPatternTimer = new DispatcherTimer();
            setPatternTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            setPatternTimer.Tick += RandTimerElapsed;
            setPatternTimer.Start();

            sendUfoTimer.Start();
            IsPaused = false;
        }

        /// <summary>
        /// 現在のパターンが終わったとき再設定される次のパターンを決める処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandTimerElapsed(object sender, EventArgs e)
        {
            var timer = sender as System.Windows.Threading.DispatcherTimer;
            timer?.Stop();
            if (patterns.Count() <= 1)
                return;
            // パターン決定
            Pattern nextPattern = nowPattern;
            while (nowPattern == nextPattern)
            { nextPattern = patterns.ElementAt(rand.Next(0, patterns.Count())); }
            nowPattern = nextPattern;
            // 時間決定
            int span;
            if (nowPattern == Pattern.rest)
                span = rand.Next(minBreakSpan, maxBreakSpan);
            else
                span = rand.Next(minTimeSpan, maxTimeSpan);
            // パワー決定
            nowPower = rand.Next(minPower, maxPower);
            // 徐々に変化する系だったときの変化量算出
            if (nowPattern == Pattern.cresc)
            {
                nowPower = minPower;
                crescPower = (maxPower - minPower) / (span / 50 /* sendUfoTimerのインターバル */);
            }
            else if (nowPattern == Pattern.decresc)
            {
                nowPower = maxPower;
                crescPower = (minPower - maxPower) / (span / 50);
            }
            // 表示
            viewNowPatternLabel.Text = $"{nowPattern}:{span / 1000.0}";
            // 指定の時間になったらまたパターンを決める
            if (timer != null)
                timer.Interval = TimeSpan.FromMilliseconds(span);
            timer?.Start();
        }

        DateTime stacLastChanged = DateTime.Now;
        int stacLastDirection = 0;

        /// <summary>
        /// 一定時間ごとに現在のパターンをUFOに送信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendUfoTimer_Tick(object sender, EventArgs e)
        {
            if (IsPaused)
                return;
            switch (nowPattern)
            {
                case Pattern.flat:
                    PortUtil.Instance.SendData(true, nowPower);
                    break;
                case Pattern.reverse:
                    PortUtil.Instance.SendData(false, nowPower);
                    break;
                case Pattern.cresc:
                    PortUtil.Instance.SendData(true, nowPower);
                    nowPower = (int)(nowPower + crescPower);
                    break;
                case Pattern.decresc:
                    PortUtil.Instance.SendData(false, nowPower);
                    nowPower = (int)(nowPower + crescPower);
                    break;
                case Pattern.stac:
                    var delta = (DateTime.Now - stacLastChanged).TotalMilliseconds;
                    if(delta >= 1000)
                    {
                        stacLastChanged = DateTime.Now;
                        if (stacLastDirection == 0)
                        {
                            stacLastDirection = 1;
                            PortUtil.Instance.SendData(true, nowPower);
                        }
                        else
                        {
                            PortUtil.Instance.SendStop();
                            stacLastDirection = 0;
                        }
                    }
                    break;
                case Pattern.rest:
                    PortUtil.Instance.SendStop();
                    break;
                default:
                    break;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        /// <summary>
        /// 全タイマー・UFOを停止
        /// </summary>
        public void Stop()
        {
            sendUfoTimer.Stop();
            setPatternTimer.Stop();
            PortUtil.Instance.SendStop();
            IsPaused = true;
        }
        /// <summary>
        /// 動きのパターン
        /// </summary>
        enum Pattern
        {
            /// <summary>
            /// 正回転
            /// </summary>
            flat,
            /// <summary>
            /// 逆回転
            /// </summary>
            reverse,
            /// <summary>
            /// 徐々に強く
            /// </summary>
            cresc,
            /// <summary>
            /// 徐々に弱く
            /// </summary>
            decresc,
            /// <summary>
            /// 小刻みに
            /// </summary>
            stac,
            /// <summary>
            /// 一時停止
            /// </summary>
            rest,
        }

    }
}
