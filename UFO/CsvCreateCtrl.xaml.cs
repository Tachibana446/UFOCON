using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// CsvCreateCtrl.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvCreateCtrl : UserControl
    {
        ObservableCollection<MainWindow.Data> table = new ObservableCollection<MainWindow.Data>();


        public CsvCreateCtrl()
        {
            InitializeComponent();

            openCsvButton.Click += MainWindow.Instance.CSVFileOpenButton_Click;
            openCsvButton.Click += OpenCsvButton_Click;
            
            for (int i = 0; i < 10; i++)
            {
                table.Add(new MainWindow.Data(i, 0, 0));
            }

            // 表のソースを配列に設定
            dataGrid.ItemsSource = table;
            // コレクションビューで時間でソート
            var cView = CollectionViewSource.GetDefaultView(table);
            cView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Time", System.ComponentModel.ListSortDirection.Ascending));

        }

        private void OpenCsvButton_Click(object sender, RoutedEventArgs e)
        {
            table = new ObservableCollection<MainWindow.Data>(MainWindow.Instance.dataList);
            dataGrid.ItemsSource = table;
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
            table.Add(new MainWindow.Data(r.Next(), r.Next(), 1));
        }
    }
}
