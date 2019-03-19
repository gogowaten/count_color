using System;
using System.Collections.Generic;
using System.Globalization;
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



//Q092.Binding.RelativeSource の使い方がよくわからない - 周回遅れのブルース
//http://d.hatena.ne.jp/hilapon/20130405/1365143758
//wpf – バインディングConverterParameter - コードログ
//https://codeday.me/jp/qa/20181201/36915.html
//"ConverterParameterプロパティは依存関係プロパティではないため、バインドできません。
//しかし、代替の解決策があります。通常のBindingの代わりにmulti-value converterを使用することができます："

namespace test幅を比率で2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += MainWindow_ContentRendered;
            
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            double actualWidth = MyListBox.ActualWidth;
            List<MyData> data = new List<MyData>() { new MyData(120, actualWidth,120), new MyData(50, actualWidth,120) };
            DataContext = data;
        }
    }

    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = (string)parameter;
            bool p = double.TryParse(param, out double x);
            double v = (double)value;
            var r = v * x / 100;
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return value;
        }
    }

    public class MyConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string param = (string)parameter;
            //bool p = double.TryParse(param, out double x);
            RelativeSource rs = (RelativeSource)parameter;
            Type at = rs.AncestorType;
            
            ListBox lb = (ListBox)parameter;
            double x = lb.ActualWidth;
            int v = (int)value;
            var r = v * x / 100;
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return value;
        }
    }

    public class MyData:DependencyObject
    {
        public double Value { get; set; }
        public double Rate { get; set; }
        public string Wariai { get; set; }
        public double Rate2 { get; set; }
        public MyData(double value,double listBoxWidth ,double maxValue)
        {
            Value = value;
            MyValue = value;
            Rate = (value /maxValue* listBoxWidth)-20;
            Rate2 = value / maxValue;
            double w =  value / maxValue;
            string wari = w.ToString();
            wari += "*";
            Wariai = wari;
        }


        public double MyValue
        {
            get { return (int)GetValue(MyValueProperty); }
            set { SetValue(MyValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyValueProperty =
            DependencyProperty.Register("MyValue", typeof(double), typeof(MyData));

        
    }

    public class MyMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double aWidth =(double) values[0];
            double value = (double)values[1];
            return aWidth * value-20;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
