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

namespace UFO
{
    /// <summary>
    /// GraphControl.xaml の相互作用ロジック
    /// </summary>
    public partial class GraphControl : UserControl
    {
        public GraphControl()
        {
            InitializeComponent();
            canvas.Children.Add(polyline);
        }

        private Polyline polyline = new Polyline() { Stroke = Brushes.Red, StrokeThickness = 2 };
        private List<Line> gridLines = new List<Line>();
        /// <summary>
        /// 時間目盛りの線
        /// </summary>
        private List<Line> timeGridLines = new List<Line>();
        /// <summary>
        /// 時間目盛りの文字
        /// </summary>
        private List<TextBlock> timeGridTexts = new List<TextBlock>();

        #region グラフの大きさに関わる変数
        /// <summary>
        /// 横方向の倍率 
        /// 10: 1pxあたり1sec  
        /// 1:1pxあたり0.1sec   
        /// 0.1: 1pxあたり0.01sec
        /// </summary>
        private double horizonRange
        {
            get { return _horizonRange; }
            set
            {
                if (value != _horizonRange)
                {
                    _horizonRange = value;
                    horizonRangeSlider.Value = value;
                }
            }
        }
        private double _horizonRange = 1;
        /// <summary>
        /// 縦方向の倍率
        /// </summary>
        private double verticalRange
        {
            get
            {
                return _verticalRange;
            }
            set
            {
                if (value != _verticalRange)
                {
                    _verticalRange = value;
                    verticalRangeSlider.Value = Math.Abs(value);
                }
            }
        }
        private double _verticalRange = -1.7;
        /// <summary>
        /// グラフ描画用の縦の位置オフセット
        /// </summary>
        private double offset { get { return 100 * Math.Abs(verticalRange); } }
        #endregion

        /// <summary>
        /// 一度グラフを描画したか　してないときMainWindows側で描画SetGraphを呼び出す
        /// </summary>
        public bool isSetGraph = false;

        /// <summary>
        /// グラフを生成
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isSorted"></param>
        public void SetGraph(List<MainWindow.Data> data, bool isSorted = true)
        {
            isSetGraph = true;
            // 未ソートならソート
            if (!isSorted)
            {
                data.Sort((a, b) => a.Time - b.Time);
            }

            polyline.Points.Clear();

            Point prev = new Point();
            bool first = true;
            foreach (var d in data)
            {
                double x = d.Time / horizonRange;
                double y = d.Level * (d.Direction == 0 ? 1 : -1) * verticalRange + offset;
                var p1 = new Point(x, y);

                if (first)
                    first = false;
                // デジタルなグラフなので1個前の高さの点を追加
                else
                {
                    var p2 = new Point(x, prev.Y);
                    polyline.Points.Add(p2);
                }
                polyline.Points.Add(p1);
                prev = p1;
            }
            canvas.Width = prev.X + 5;
            SetGridLines(prev.X + 5);
            SetTimeGrid();
            // 現在時の線の高さ
            positionLine.Y1 = 5;
            positionLine.Y2 = Math.Abs(verticalRange) * 200 - 5;
        }

        /// <summary>
        /// 現在時刻を更新し、その位置に線を表示する
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(TimeSpan position, bool playerIsPaused)
        {
            double nowTime = position.TotalSeconds * 10;
            double x = nowTime / horizonRange;
            positionLine.X1 = positionLine.X2 = x;
            // 再生中は線の位置まで自動でスクロール
            if (!playerIsPaused && !double.IsNaN(canvas.Width))
            {
                double nowRatio = x / canvas.Width;
                scrollViewer.ScrollToHorizontalOffset(nowRatio * scrollViewer.ScrollableWidth);
            }
        }

        /// <summary>
        /// グリッド線の追加
        /// </summary>
        /// <param name="width">Canvasの幅</param>
        /// <param name="Split">何本出すか(偶数じゃないとセンターラインがなくなる）</param>
        private void SetGridLines(double width, int Split = 4)
        {
            foreach (var old in gridLines)
            {
                canvas.Children.Remove(old);
            }
            gridLines.Clear();

            double offset = 100 * Math.Abs(verticalRange);
            var lineDash = new DoubleCollection(new double[] { 3, 2 }); // 破線の設定
            for (int i = 0; i < Split + 1; i++)
            {
                double rawLev = 200.0 / Split * i - 100;
                double y = rawLev * verticalRange + offset;

                var l = new Line() { X1 = 0, Y1 = y, X2 = width, Y2 = y, Stroke = Brushes.DarkGray, StrokeThickness = 1 };

                // 破線と実線
                var dash = lineDash;
                if ((Split / 2) % 2 != i % 2)
                    l.StrokeDashArray = lineDash;
                gridLines.Add(l);
            }

            gridLines.ForEach(l => canvas.Children.Add(l));
        }

        /// <summary>
        /// 時間軸の目盛り(10s刻み)を表示する
        /// </summary>
        private void SetTimeGrid()
        {
            timeGridLines.ForEach(l => canvas.Children.Remove(l));
            timeGridLines.Clear();
            timeGridTexts.ForEach(t => canvas.Children.Remove(t));
            timeGridTexts.Clear();

            double y = Math.Abs(verticalRange) * 200 + 5;
            canvas.Height = y + 23;

            // 何pxごとに数字を表示すればいいか
            double interval = 100 / horizonRange;

            for (int i = 0; i < canvas.Width / interval; i++)
            {
                var l = new Line() { X1 = interval * i, X2 = interval * i, Y1 = y, Y2 = y + 5, Stroke = Brushes.DarkGray, StrokeThickness = 2 };
                var t = new TextBlock() { Text = formatTime(i * 10), FontSize = 12, TextAlignment = TextAlignment.Center };
                Canvas.SetLeft(t, interval * i - 22);
                Canvas.SetTop(t, y + 6);
                canvas.Children.Add(l);
                canvas.Children.Add(t);
                timeGridLines.Add(l);
                timeGridTexts.Add(t);
            }
        }

        static string formatTime(int second)
        {
            TimeSpan span = TimeSpan.FromSeconds(second);
            return span.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// ユーザーがグラフの再描画ボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // 値を更新
            verticalRange = -1 * verticalRangeSlider.Value;
            horizonRange = horizonRangeSlider.Value;
            // 描画
            SetGraph(MainWindow.Instance.dataList);
        }
    }
}
