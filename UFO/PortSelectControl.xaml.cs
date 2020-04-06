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
    /// PortSelectControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PortSelectControl : UserControl
    {
        public PortSelectControl()
        {
            InitializeComponent();
            SetPortNameTextBlock();
        }

        /// <summary>
        /// ポート一覧を更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadPortListButton_Click(object sender, RoutedEventArgs e)
        {
            SetPortNameTextBlock();
            foreach (var portname in PortUtil.GetPorts())
            {
                portListCombobox.Items.Add(portname);
            }
            if (portListCombobox.Items.Count > 0)
                portListCombobox.SelectedIndex = 0;
        }

        /// <summary>
        /// ポートを手動で設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetPortButton_Click(object sender, RoutedEventArgs e)
        {
            PortUtil.Instance.SetPort(portListCombobox.SelectedItem?.ToString().Trim());
            SetPortNameTextBlock();
        }

        /// <summary>
        /// ポートを再探索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindPortButton_Click(object sender, RoutedEventArgs e)
        {
            PortUtil.Instance.FindPort();
            SetPortNameTextBlock();
        }

        /// <summary>
        /// ポート名を表示
        /// </summary>
        private void SetPortNameTextBlock()
        {
            portNameTextBlock.Text = "現在のポート：" + PortUtil.Instance.portName;
        }
    }
}
