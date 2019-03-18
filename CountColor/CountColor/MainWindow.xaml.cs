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
using System.Collections.ObjectModel;

namespace CountColor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyImageByte;
        BitmapSource MyBitmapSource;
        MyColorCountData MyData;
        //ObservableCollection<MyStruct> MyData;
        //ObservableCollection<MyColor> MyData;
        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            Drop += MainWindow_Drop;
            Button1.Click += Button1_Click;

        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (MyImageByte is null) { return; }
            Dictionary<uint, int> table = Count32bit(MyImageByte);
            TextBlock2.Text = $"使用色数{table.Count:#,0}(32bit)";
            //降順でソート
            IOrderedEnumerable<KeyValuePair<uint, int>> sorted = table.OrderByDescending((data) => data.Value);
            //上位10色
            //MyData = sorted.Take(10);
            IEnumerable<KeyValuePair<uint, int>> top = sorted.Take(10);
            //下位10色
            IEnumerable<KeyValuePair<uint, int>> bottom = sorted.Skip(sorted.Count() - 10);
            //KeyValuePair<uint, int>[] neko = top.ToArray();

            MyData = new MyColorCountData(top);
            //MyData = new ObservableCollection<MyStruct>();
            //MyData = new ObservableCollection<MyColor>();
            foreach (var item in top)
            {
                //MyData.Add(new MyStruct() { Color = Colors.Red, Count = item.Value });
                //MyData.Add(new MyColor { Count = item.Value, Value = item.Key });
            }

            DataContext = MyData.data;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);

            (MyImageByte, MyBitmapSource) = MakeByteArrayAndSourceFromImageFile(filePath[0], PixelFormats.Bgra32, 96, 96);
            if (MyBitmapSource == null)
            {
                MessageBox.Show("画像ファイルじゃないみたい");
                Button1.IsEnabled = false;
            }
            else
            {
                int count = Count24bit(MyImageByte);
                TextBlock1.Text = $"使用色数{count:#,0}(24bit)";
                Button1.IsEnabled = true;
                MyImage.Source = MyBitmapSource;

                TextBlock2.Text = "";
                DataContext = null;
            }

        }




        #region カウント

        //24bitだけ対応、bool型の配列で色の有無だけ確認、速い
        private int Count24bit(byte[] pixels)
        {
            bool[] bColor = new bool[256 * 256 * 256];
            //RGBをintに変換して、そのインデックスの値をTrueにする
            for (int i = 0; i < pixels.Length; i += 4)
            {
                bColor[pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16)] = true;//bgr
            }
            //Trueの数をカウント
            int count = 0;
            for (int i = 0; i < bColor.Length; i++)
            {
                if (bColor[i] == true) { count++; }
            }

            //LINQでTrueの数をカウント、↑より1～2割遅い
            //int neko = bColor.Where(saru => saru == true).Count();
            //Whereは省略してcountメソッドだけでもカウントできるけど、もっと遅い
            //int inu = bColor.Count(saru => saru);
            return count;
        }

        //32bit、色数と各色カウント
        private Dictionary<uint, int> Count32bit(byte[] pixels)
        {
            //Keyには色のインデックス、Valueはカウント数
            var table = new Dictionary<uint, int>();//uint
            uint key;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                key = (uint)(pixels[i] | (pixels[i + 1] << 8) | (pixels[i + 2] << 16) | (pixels[i + 3] << 24));
                //はじめての色なら要素を追加、Valueは1
                if (table.ContainsKey(key) == false) { table.Add(key, 1); }
                //2回目以降はValueに1を足していく
                else { table[key] = table[key] + 1; }//value += 1
            }
            return table;
        }


        #endregion



        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を取得、ピクセルフォーマットを指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] array, BitmapSource source) MakeByteArrayAndSourceFromImageFile(string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;
            BitmapSource source = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(fs);

                    var convertedBitmap = new FormatConvertedBitmap(bf, pixelFormat, null, 0);
                    int w = convertedBitmap.PixelWidth;
                    int h = convertedBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    pixels = new byte[h * stride];
                    convertedBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = bf.DpiX; }
                    if (dpiY == 0) { dpiY = bf.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        convertedBitmap.Format,
                        convertedBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {
            }
            return (pixels, source);
        }
    }



    public class MyColor
    {
        public int Count { get; set; }
        public uint Value { get; set; }
        public Color Color { get; set; }
        public SolidColorBrush Brush { get; set; }
    }

    public struct MyStruct
    {
        public Color Color;
        public int Count;

    }
    public class MyColorCountData
    {
        public ObservableCollection<MyColor> data { get; set; }
        public MyColorCountData(IEnumerable<KeyValuePair<uint, int>> keyValues)
        {
            data = new ObservableCollection<MyColor>();
            foreach (var item in keyValues)
            {
                uint ui = item.Key;
                byte b = (byte)(ui & 0x000000FF);
                byte g = (byte)(ui >> 8 & 0x0000FF);
                byte r = (byte)(ui >> 16 & 0x00FF);
                byte a = (byte)(ui >> 24);
                Color c = Color.FromArgb(a, r, g, b);
                data.Add(new MyColor() { Color = c, Count = item.Value, Brush = new SolidColorBrush(c) });
            }
        }
    }
}
