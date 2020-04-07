using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// CsvCreateCtrl.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvCreateCtrl : UserControl
    {
        public ObservableCollection<MainWindow.Data> Table { get; private set; } = new ObservableCollection<MainWindow.Data>();

        /// <summary>
        /// プレイヤーと同期するチェックボックスがONのときにテキストボックスを更新するタイマー
        /// </summary>
        private DispatcherTimer timeTextboxTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };


        /// <summary>
        /// 削除したデータ（行）
        /// </summary>
        private Stack<IEnumerable<MainWindow.Data>> removedData = new Stack<IEnumerable<MainWindow.Data>>();

        public CsvCreateCtrl()
        {
            InitializeComponent();

            // CSVを開くボタンを押したらメインタブとこのタブの処理を行う
            openCsvButton.Click += MainWindow.Instance.CSVFileOpenButton_Click;
            openCsvButton.Click += OpenCsvButton_Click;

            // 表のソースを配列に設定
            dataGrid.ItemsSource = Table;
            // コレクションビューで時間でソート
            /*
            var cView = CollectionViewSource.GetDefaultView(Table);
            cView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Time", System.ComponentModel.ListSortDirection.Ascending));
            */
            // 音声プレイヤーの現在時間と挿入する新規データの時間を同期
            timeTextboxTimer.Tick += TimeTextboxTimer_Tick;
        }


        /// <summary>
        /// CSVをロードするためのボタンが押されたときのイベント。ロードしたCSVのデータをtableに格納
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenCsvButton_Click(object sender, RoutedEventArgs e)
        {
            Table = new ObservableCollection<MainWindow.Data>(MainWindow.Instance.dataList);
            dataGrid.ItemsSource = Table;
        }

        /// <summary>
        /// 列の自動生成イベント。ヘッダ名と並び順の指定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Time":
                    e.Column.Header = "時間(デシ秒）";
                    e.Column.DisplayIndex = 0;
                    break;
                case "Direction":
                    e.Column.Header = "方向(0 or 1)";
                    e.Column.DisplayIndex = 1;
                    break;
                case "Level":
                    e.Column.Header = "強度(0 - 100)";
                    e.Column.DisplayIndex = 2;
                    break;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            Table.Add(new MainWindow.Data(r.Next(), r.Next(), 1));
        }

        /// <summary>
        /// 増分保存してログに表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCSVButton_Click(object sender, RoutedEventArgs e)
        {
            saveCsvButton.IsEnabled = false;
            var newFilePath = getNewFilePath();

            SaveCsv(newFilePath);
            logTextbox.Text = $"CSVを以下のファイルに保存：{newFilePath}\n" + logTextbox.Text;
            saveCsvButton.IsEnabled = true;
        }

        /// <summary>
        /// 現在開いているCSVデータをファイルに保存
        /// </summary>
        /// <param name="newFilePath"></param>
        private void SaveCsv(string newFilePath)
        {
            using (var sw = new StreamWriter(newFilePath))
            {
                foreach (var row in Table.OrderBy(r => r.Time))
                {
                    sw.WriteLine(row.ToCSV());
                }
            }
            MainWindow.Instance.LoadCSV(newFilePath);   // グラフを読み込み直す
        }

        /// <summary>
        /// 現在開いているファイルの増分したファイル名を返す
        /// </summary>
        /// <returns></returns>
        private static string getNewFilePath()
        {
            string newPath = MainWindow.Instance.loadedCsvFullPath;
            if (string.IsNullOrWhiteSpace(newPath))
                newPath = "./data.csv";
            var f = new FileInfo(newPath);

            while (f.Exists)
            {
                var res = Regex.Match(newPath, @"(.*\.)(\d{4})\.csv$");
                if (res.Success && int.TryParse(res.Groups[2].Value, out int num))
                {
                    var n2 = (num + 1).ToString("D4");
                    newPath = res.Groups[1].Value + n2 + ".csv";
                }
                else
                {
                    newPath = Regex.Replace(newPath, @".csv$", "");
                    newPath += ".0001.csv";
                }
                f = new FileInfo(newPath);
            }
            return f.FullName;
        }

        /// <summary>
        /// データ追加ボタンを押したとき、Tableにデータを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDataButton_Click(object sender, RoutedEventArgs e)
        {
            string timeStr = timeTextBox01.Text;
            bool isReverse = isReverse_checkbox01.IsChecked == true;
            int power = (int)powerSlider01.Value;

            if (int.TryParse(timeStr, out int time))
            {
                var newData = new MainWindow.Data(time, isReverse ? 1 : 0, power);
                Table.Add(newData);
            }

            MainWindow.Instance.LoadEditedData();   // データとグラフを再読み込み
        }

        /// <summary>
        /// 新規データの時間とプレイヤーの再生時間を同期ON
        /// </summary>
        private void timeIsNowPlaytime_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            timeTextBox01.IsEnabled = false;
            timeTextboxTimer.Start();
        }

        /// <summary>
        /// 新規データの時間とプレイヤーの再生時間を同期OFF
        /// </summary>
        private void timeIsNowPlaytime_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            timeTextBox01.IsEnabled = true;
            timeTextboxTimer.Stop();
        }

        /// <summary>
        /// 音声プレイヤーの現在時間と挿入する新規データの時間を同期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeTextboxTimer_Tick(object sender, EventArgs e)
        {
            var position = MediaPlayer.player.Position;
            timeTextBox01.Text = Math.Round(position.TotalSeconds * 10, 0, MidpointRounding.AwayFromZero).ToString();
        }


        /// <summary>
        /// 選択中の行のデータを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveDataButton_Click(object sender, RoutedEventArgs e)
        {
            List<DataGridCellInfo> cells = new List<DataGridCellInfo>(dataGrid.SelectedCells);
            var selected = cells.Select(cell => cell.Item as MainWindow.Data).Where(d => d != null).Distinct();
            int n = selected.Count();
            if (n <= 0) return;
            removedData.Push(selected);
            foreach (var data in selected)
            {
                Table.Remove(data);
            }
            logTextbox.Text = logTextbox.Text.Insert(0, $"{DateTime.Now.ToShortTimeString()} : データを{n}件削除\n");
            MainWindow.Instance.LoadEditedData();   // データとグラフを再読み込み
        }

        /// <summary>
        /// 削除を取り消す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoRemoveDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (removedData.Count() <= 0)
                return;

            var removed = removedData.Pop();
            foreach (var data in removed)
            {
                Table.Add(data);
            }
            int n = removed.Count();
            logTextbox.Text = logTextbox.Text.Insert(0, $"{DateTime.Now.ToShortTimeString()} : {n}件の削除をもとに戻しました。\n");
        }
    }
}
