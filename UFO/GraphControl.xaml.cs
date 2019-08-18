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

        private Polyline polyline = new Polyline() { Stroke = Brushes.Pink, StrokeThickness = 2 };
        private List<Line> gridLines = new List<Line>();

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

            // 横方向の倍率 10: 1pxあたり1sec  1:1pxあたり0.1sec   0.1: 1pxあたり0.01sec
            double horizonRange = 1;
            // 縦方向の倍率
            double verticalRange = -1.8;
            double offset = 100 * verticalRange;

            Point prev = new Point();
            bool first = true;
            foreach (var d in data)
            {
                double x = d.Time / horizonRange;
                double y = d.Level * (d.Direction == 0 ? 1 : -1) * verticalRange - offset;
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
            SetGridLines(verticalRange, prev.X + 5);
        }

        /// <summary>
        /// グリッド線の追加
        /// </summary>
        /// <param name="vertialRange">縦倍率</param>
        /// <param name="width">Canvasの幅</param>
        /// <param name="Split">何本出すか</param>
        private void SetGridLines(double vertialRange, double width, int Split = 4)
        {
            foreach (var old in gridLines)
            {
                canvas.Children.Remove(old);
            }
            gridLines.Clear();

            double offset = 100 * vertialRange;
            DoubleCollection lineDash = new DoubleCollection(new double[] { 2, 1 }); // 破線の設定
            for (int i = 0; i < Split + 1; i++)
            {
                double rawLev = 200.0 / Split * i - 100;
                double y = rawLev * vertialRange - offset;
                var l = new Line() { X1 = 0, Y1 = y, X2 = width, Y2 = y, Stroke = Brushes.Gray, StrokeThickness = 1, StrokeDashArray = lineDash };
                gridLines.Add(l);
            }

            gridLines.ForEach(l => canvas.Children.Add(l));
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SetGraph(MainWindow.Instance.dataList);
        }
    }
}
