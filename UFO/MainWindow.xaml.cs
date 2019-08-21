﻿using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace UFO
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance = null;

        public MainWindow()
        {
            if (Instance == null) Instance = this;

            InitializeComponent();
            this.Closing += (s, e) => PortUtil.Instance.Close();
            stopButton.Click += (s, e) => PortUtil.Instance.SendData(true, 0);
            PortUtil.Instance.FindPort();
        }

        private void CSVFileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                LoadCSV(dialog.FileName);
            }
        }

        /// <summary>
        /// CSVデータ一覧
        /// </summary>
        public List<Data> dataList { get; private set; } = new List<Data>();
        /// <summary>
        /// 前回のタイマー実行時に参照した時間
        /// </summary>
        private double lastTime = 0;

        private System.Windows.Threading.DispatcherTimer csvTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };

        /// <summary>
        /// CSVを読み込む。形式は時間（1/10秒）,方向（1:正 0:負),強さ(0~100まで）
        /// </summary>
        /// <param name="filepath"></param>
        private void LoadCSV(string filepath)
        {
            dataList.Clear(); // 以前のデータをクリア
            graphControl.isSetGraph = false; // グラフを書き直す必要がある

            foreach (var line in File.ReadLines(filepath))
            {
                var cols = line.Split(',');
                if (cols.Length < 3)
                    continue;
                int time, direction, level;
                if (int.TryParse(cols[0], out time) && int.TryParse(cols[1], out direction) && int.TryParse(cols[2], out level))
                {
                    dataList.Add(new Data(time, direction, level));
                }
                else
                {
                    continue;
                }
            }
            // ファイル名の表示
            if (dataList.Count > 0)
            {
                CSVFilePathText.Text = System.IO.Path.GetFileName(filepath);
            }
            // 時間順にソート
            dataList.Sort((a, b) => a.Time - b.Time);
            // タイマーを起動
            csvTimer.Stop();
            csvTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
            csvTimer.Tick += CsvTimer_Tick;
            csvTimer.Start();
        }

        /// <summary>
        /// 50ミリ秒ごとに再生位置を参照し、前回から今回の間に指定されているコマンドのうち最後のものを実行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvTimer_Tick(object sender, EventArgs e)
        {
            double nowTime = MediaPlayer.player.Position.TotalSeconds * 10;
            // 巻き戻されていた場合何もしない
            if (lastTime > nowTime)
            {
                lastTime = nowTime;
                return;
            }
            Data data = dataList.LastOrDefault(d => d.Time > lastTime && d.Time <= nowTime);
            PortUtil.Instance.SendData(data.Direction == 1, data.Level);
            lastTime = nowTime;
        }

        /// <summary>
        /// 時間・方向・強さを表す構造体
        /// </summary>
        public struct Data
        {
            public int Time;
            public int Direction;
            public int Level;
            public Data(int time, int direction, int level)
            {
                Time = time;
                Direction = direction;
                Level = level;
            }
        }

        /// <summary>
        /// タブの変更イベント
        /// グラフのタブに切り替わったときグラフを読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabcontrol = sender as TabControl;
            // タブがグラフのタブかつまだ描画されてないとき
            if (!graphControl.isSetGraph && tabcontrol.SelectedIndex == 2)
                graphControl.SetGraph(dataList);
        }
    }
}
