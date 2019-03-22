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

namespace _20190322レイアウト
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyImage.Source = new BitmapImage(new Uri(@"D:\ブログ用\チェック用2\NEC_6154_2019_01_18_午後わてん.jpg"));
            List<int> vs = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                vs.Add(i);
            }
            DataContext = vs;
        }
    }
}
