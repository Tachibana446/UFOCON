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
    /// SliderAndTextCtrl.xaml の相互作用ロジック
    /// </summary>
    public partial class SliderAndTextCtrl : UserControl
    {
        public int min = 0;
        public int max = 10;

        public string LabelText
        {
            set { label.Content = value; }
        }

        public Slider Slider
        {
            get { return slider; }
        }

        public TextBox TextBox
        {
            get { return textBox; }
        }

        public SliderAndTextCtrl()
        {
            InitializeComponent();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(textBox.Text, out double num))
            {
                if (slider.Value != num && slider.Minimum <= num && num <= slider.Maximum)
                {
                    slider.Value = num;
                    return;
                }
            }
            else
            {
                textBox.Text = slider.Value.ToString();
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (textBox == null) return;
            textBox.Text = slider.Value.ToString();
        }
    }
}
