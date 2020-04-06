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
        int minTimeSpan { get { return (int)(minSpanSlider.slider.Value * 1000); } }
        int maxTimeSpan { get { return (int)(maxSpanSlider.slider.Value * 1000); } }
        int nowPower = 0;
        /// <summary>
        /// 徐々に変化する系のパターンだったときの毎Tickの変化量
        /// </summary>
        double crescPower = 0;
        /// <summary>
        /// 最小の休憩時間(ms)
        /// </summary>
        int minBreakSpan { get { return (int)(minBreakSpanSlider.slider.Value * 1000); } }
        int maxBreakSpan { get { return (int)(maxBreakSpanSlider.slider.Value * 1000); } }
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

            minSpanSlider.slider.Value = 1;
            minSpanSlider.LabelText = "最小時間";
            maxSpanSlider.slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
            maxSpanSlider.slider.Value = 5;
            maxSpanSlider.LabelText = "最大時間";
            minBreakSpanSlider.slider.Value = 1;
            minBreakSpanSlider.LabelText = "最小停止時間";
            maxBreakSpanSlider.slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
            maxBreakSpanSlider.slider.Value = 3;
            maxBreakSpanSlider.LabelText = "最大停止時間";

            maxPowerSlider.slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.TopLeft;
            maxPowerSlider.slider.Maximum = 100; maxPowerSlider.slider.Value = 80;
            maxPowerSlider.slider.TickFrequency = 10; maxPowerSlider.slider.SmallChange = 10; maxPowerSlider.slider.LargeChange = 10;
            maxPowerSlider.LabelText = "最大パワー"; maxPowerSlider.unitLabel.Content = "％";
            minPowerSlider.slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
            minPowerSlider.slider.Maximum = 100; minPowerSlider.slider.Value = 0;
            minPowerSlider.slider.TickFrequency = 10; minPowerSlider.slider.SmallChange = 10; minPowerSlider.slider.LargeChange = 10;
            minPowerSlider.LabelText = "最小パワー"; minPowerSlider.unitLabel.Content = "％";
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
            onOffButton.Content = "■";
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
            if (IsPaused)
                return;
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
            int minPower = (int)minPowerSlider.slider.Value; int maxPower = (int)maxPowerSlider.slider.Value;
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
            // STACパターンの方向を決定
            stacDirection = rand.Next() % 2 == 0;
            // 表示
            viewNowPatternLabel.Text = $"パターン:{patternStr[(int)nowPattern]} - {span}秒";
            // 指定の時間になったらまたパターンを決める
            if (timer != null)
                timer.Interval = TimeSpan.FromMilliseconds(span);
            timer?.Start();
        }

        /// <summary>
        /// STACパターン用 最後に回転した時間
        /// </summary>
        DateTime stacLastChanged = DateTime.Now;
        /// <summary>
        /// STACパターン用 最後に停止だったか回転だったか
        /// </summary>
        int stacLastOnOff = 0;
        /// <summary>
        /// STACパターン用 ランダムな方向
        /// </summary>
        bool stacDirection;

        /// <summary>
        /// 一定時間ごとに現在のパターンをUFOに送信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendUfoTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("SendUfoTimer_Tick");
            if (IsPaused)
            {
                PortUtil.Instance.SendStop();
                var t = sender as DispatcherTimer;
                t?.Stop();
            }
            else
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
                        if (delta >= 200)
                        {
                            stacLastChanged = DateTime.Now;
                            if (stacLastOnOff == 0)
                            {
                                stacLastOnOff = 1;
                                PortUtil.Instance.SendData(true, nowPower);
                            }
                            else
                            {
                                PortUtil.Instance.SendStop();
                                stacLastOnOff = 0;
                            }
                        }
                        break;
                    case Pattern.rest:
                        PortUtil.Instance.SendStop();
                        break;
                    default:
                        break;
                }

            viewNowPowerLabel.Text = $"強さ：{nowPower}％";
        }

        /// <summary>
        /// 全タイマー・UFOを停止
        /// </summary>
        public void Stop()
        {
            nowPower = 0;
            nowPattern = Pattern.rest;
            viewNowPatternLabel.Text = "停止";
            IsPaused = true;
            PortUtil.Instance.SendStop();
            PortUtil.Instance.SendStop();
            PortUtil.Instance.SendStop();
            onOffButton.Content = "▶";
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

        string[] patternStr = new string[] { "正回転", "逆回転", "増幅", "減少", "小刻みに", "一時停止" };

        /// <summary>
        /// On/Offを切り替える
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOffButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPaused)
            {
                StartRandom();
            }
            else
            {
                Stop();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }
    }
}
