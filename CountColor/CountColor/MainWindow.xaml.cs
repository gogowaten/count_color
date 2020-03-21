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
using System.Globalization;
using MyHSV;//HSV.dllを参照に追加

namespace CountColor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyImageByte;
        BitmapSource MyBitmapSource;
        int MyBitmapPixelsCount;
        MyColorCountData MyDataDescend;
        MyColorCountData MyDataAscend;
        Dictionary<uint, int> MyTable;
        ImageBrush MyItimatu;
        //Brush MyBrushTransparent;

        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            Drop += MainWindow_Drop;
            MyTable = new Dictionary<uint, int>();
            MyItimatu= MakeTileBrush(MakeCheckeredPattern(10, Colors.LightGray));
            BorderTransparent.Background = MyItimatu;
            MyImageGrid.Background = MyItimatu;
            //MyScrollViewer.Background = MakeTileBrush(MakeCheckeredPattern(10, Colors.LightGray));

            Button1.Click += Button1_Click;
            ButtonTest1.Click += ButtonTest1_Click;
            MyListBox.SelectionChanged += MyListBox_SelectionChanged;
            MyToggleSort.Click += MyToggleSort_Click;
            ButtonBGColor.Click += ButtonBGColor_Click;
            ButtonImageStretch.Click += ButtonImageStretch_Click;
            ButtonGetClipboardImage.Click += ButtonGetClipboardImage_Click;
            ButtonGetClipboardImage2.Click += ButtonGetClipboardImage2_Click;
            ButtonItimatu.Click += ButtonItimatu_Click;

            //listbox.itemtemplate.datatemplate
            MyListBox.ItemTemplate = CreateDataTemplateForListBox();

        }

        private void ButtonItimatu_Click(object sender, RoutedEventArgs e)
        {
            if(MyImageGrid.Background == null)
            {
                MyImageGrid.Background = MyItimatu;
            }
            else
            {
                MyImageGrid.Background = null;
            }
        }

        //クリップボードから画像取得2、エクセルの図形用
        private void ButtonGetClipboardImage2_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromClipboard2();
            GetClipboardImageInitialize(bitmap);
        }
        //クリップボードから画像取得1
        private void ButtonGetClipboardImage_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetBitmapFromClipboard();
            GetClipboardImageInitialize(bitmap);
        }
        //クリップボードから画像取得時の初期化
        private void GetClipboardImageInitialize(BitmapSource bitmap)
        {
            if (bitmap == null) return;
            MyBitmapSource = bitmap;
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 32 / 8;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            MyImageByte = pixels;
            //表示の初期化
            MyInitialize();
        }

        private void ButtonImageStretch_Click(object sender, RoutedEventArgs e)
        {
            if (MyImageByte is null) { return; }
            if (MyScrollViewer.Content == null)
            {
                //MyDockPanel.Children.Remove(MyImage);
                MyImageGrid.Children.Remove(MyImage);
                MyScrollViewer.Content = MyImage;
                MyImage.Stretch = Stretch.None;
            }
            else
            {
                MyScrollViewer.Content = null;
                //MyDockPanel.Children.Add(MyImage);
                MyImageGrid.Children.Add(MyImage);
                MyImage.Stretch = Stretch.Uniform;

            }

        }

        private void ButtonBGColor_Click(object sender, RoutedEventArgs e)
        {
            Brush bgColor = MyListBox.Background;
            Brush black = Brushes.Black;
            Brush white = Brushes.White;
            Brush gray = Brushes.Gray;
            if (bgColor == black)
            {
                MyListBox.Background = white;
                MyListBox.Foreground = black;
            }
            else if (bgColor == gray)
            {
                MyListBox.Background = black;
                MyListBox.Foreground = white;
            }
            else
            {
                MyListBox.Background = gray;
            }
        }



        private void MyToggleSort_Click(object sender, RoutedEventArgs e)
        {
            if (MyTable is null) { return; }
            if (MyToggleSort.IsChecked == false)
            {
                DataContext = MyDataDescend.Data;
            }
            else
            {
                DataContext = MyDataAscend.Data;
            }
        }

        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sis = MyListBox.SelectedItems;
            var si = MyListBox.SelectedItem;
            var sv = MyListBox.SelectedValue;
            var sin = MyListBox.SelectedIndex;


        }

        private void ButtonTest1_Click(object sender, RoutedEventArgs e)
        {
            if (MyDataAscend is null) { return; }
            //MyListBox.Background = MyBrushTransparent;

            //var low = MyDescendSorteTable.Skip(MyDescendSorteTable.Count() - 10);
            //var max = low.Max(x => x.Value);
            //var dd = new MyColorCountData(low, max, MyBitmapPixelsCount);
            //DataContext = dd.Data;
            //MyDataAscend.Data[0].Color = Colors.Red;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (MyImageByte is null) { return; }
            MyToggleSort.IsEnabled = true;
            MyTable = Count32bit(MyImageByte);
            TextBlock2.Text = $"使用色数{MyTable.Count:#,0}(32bit)";
            //降順でソート
            IOrderedEnumerable<KeyValuePair<uint, int>> descend = MyTable.OrderByDescending((data) => data.Value);
            //上位128色
            IEnumerable<KeyValuePair<uint, int>> top = descend.Take(128);
            var maxValue = top.Max(x => x.Value);//最大値
            MyDataDescend = new MyColorCountData(top, maxValue, MyBitmapPixelsCount);
            DataContext = MyDataDescend.Data;


            var ascend = MyTable.OrderBy((x) => x.Value).Take(128);
            MyDataAscend = new MyColorCountData(ascend, ascend.Max(x => x.Value), MyBitmapPixelsCount);
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
                //表示の初期化
                MyInitialize();
            }

        }
        //表示の初期化
        private void MyInitialize()
        {
            int count = Count24bit(MyImageByte);
            TextBlock1.Text = $"使用色数{count:#,0}(24bit)";
            Button1.IsEnabled = true;
            MyToggleSort.IsEnabled = false;
            MyToggleSort.IsChecked = false;
            MyImage.Source = MyBitmapSource;

            TextBlock2.Text = "";
            DataContext = null;

            int pw = MyBitmapSource.PixelWidth;
            int ph = MyBitmapSource.PixelHeight;
            MyBitmapPixelsCount = pw * ph;
            TextBlockPixelsCount.Text = $"{pw} x {ph} = {MyBitmapPixelsCount:#,0} Pixels";

        }



        //DataTemplate作成
        private DataTemplate CreateDataTemplateForListBox()
        {
            //市松模様ブラシ作成
            ImageBrush myPattern = MakeTileBrush(MakeCheckeredPattern(8, Colors.Gray));


            //StackPanel  RootPanel
            //┣Grid
            //┃ ┣Border 市松模様
            //┃ ┗Border 色表示
            //┣TextBlock 値表示
            //┗Border    色表示、棒グラフ

            //市松模様表示用にBorder作成
            var eBorderPattern = new FrameworkElementFactory(typeof(Border));
            eBorderPattern.SetValue(Border.BackgroundProperty, myPattern);//背景市松模様
            eBorderPattern.SetValue(WidthProperty, 24.0);
            //eBorder.SetValue(HeightProperty, 20.0);
            //色表示用
            var eBorderColor = new FrameworkElementFactory(typeof(Border));
            //eBorderColor.SetBinding(Border.BackgroundProperty, new Binding("Brush"));
            eBorderColor.SetBinding(Border.BackgroundProperty, new Binding(nameof(MyColor.Brush)));
            eBorderColor.SetValue(WidthProperty, 24.0);

            var fGrid = new FrameworkElementFactory(typeof(Grid));
            //fGrid.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            fGrid.AppendChild(eBorderPattern);
            fGrid.AppendChild(eBorderColor);


            //値表示用にTextBlock作成
            var eTextBlock = new FrameworkElementFactory(typeof(TextBlock));
            //表示する値はBindingしたものにする設定
            //eTextBlock.SetBinding(TextBlock.TextProperty, new Binding("Count"));
            eTextBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(MyColor.Count)));
            eTextBlock.SetValue(WidthProperty, 50.0);
            eTextBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            eTextBlock.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 4, 0));
            //eTextBlock.SetValue(TextBlock.BackgroundProperty, myPattern);//背景市松模様

            //Border
            var borderBarGraph = new FrameworkElementFactory(typeof(Border));
            borderBarGraph.SetValue(HeightProperty, 10.0);
            //borderBarGraph.SetBinding(Border.BackgroundProperty, new Binding("Brush"));
            borderBarGraph.SetBinding(Border.BackgroundProperty, new Binding(nameof(MyColor.Brush)));
            var bind1 = new Binding();
            bind1.Source = MyListBox;
            bind1.Path = new PropertyPath(ListBox.ActualWidthProperty);
            //var bind2 = new Binding("Rate");
            var bind2 = new Binding(nameof(MyColor.Rate));
            var mBind = new MultiBinding();
            mBind.Bindings.Add(bind1);
            mBind.Bindings.Add(bind2);
            mBind.Converter = new MyMultiConverter();
            borderBarGraph.SetBinding(Border.WidthProperty, mBind);


            //RootStackPanel作成、スタック方向はHorizontal
            var rootPanel = new FrameworkElementFactory(typeof(StackPanel));
            rootPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            rootPanel.AppendChild(fGrid);
            rootPanel.AppendChild(eTextBlock);
            rootPanel.AppendChild(borderBarGraph);

            //DataTemplate作成、VisualTreeに上のStackPanelを指定で完成
            var dt = new DataTemplate();
            dt.VisualTree = rootPanel;
            return dt;
        }



        #region 市松模様ブラシ作成
        //無限の透明市松模様をWriteableBitmapとImageBrushのタイル表示で作成(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
        //https://blogs.yahoo.co.jp/gogowaten/15917385.html

        /// <summary>
        /// 市松模様画像作成
        /// </summary>
        /// <param name="cellSize">タイル1辺のサイズ</param>
        /// <param name="gray">白じゃない方の色指定</param>
        /// <returns></returns>
        private WriteableBitmap MakeCheckeredPattern(int cellSize, Color gray)
        {
            int width = cellSize * 2;
            int height = cellSize * 2;
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.Format.BitsPerPixel / 8 * width;
            byte[] pixels = new byte[stride * height];
            int p = 0;
            Color iro;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((y < cellSize & x < cellSize) | (y >= cellSize & x >= cellSize))
                    {
                        iro = Colors.White;
                    }
                    else { iro = gray; }

                    p = y * stride + x * 3;
                    pixels[p] = iro.R;
                    pixels[p + 1] = iro.G;
                    pixels[p + 2] = iro.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }

        //        方法: TileBrush のタイル サイズを設定する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/graphics-multimedia/how-to-set-the-tile-size-for-a-tilebrush
        /// <summary>
        /// BitmapからImageBrush作成
        /// 引き伸ばし無しでタイル状に敷き詰め
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private ImageBrush MakeTileBrush(BitmapSource bitmap)
        {
            var imgBrush = new ImageBrush(bitmap);
            imgBrush.Stretch = Stretch.Uniform;//これは必要ないかも
            //タイルモード、タイル
            imgBrush.TileMode = TileMode.Tile;
            //タイルサイズは元画像のサイズ
            imgBrush.Viewport = new Rect(0, 0, bitmap.Width, bitmap.Height);
            //タイルサイズ指定方法は絶対値、これで引き伸ばされない
            imgBrush.ViewportUnits = BrushMappingMode.Absolute;
            return imgBrush;
        }
        #endregion


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

        #region クリップボードから画像取得
        private BitmapSource GetBitmapFromClipboard()
        {
            BitmapSource bitmap = null;
            if (!Clipboard.ContainsImage()) return bitmap;

            bitmap = Clipboard.GetImage();
            
            //エクセルのデータ、もしくは全ピクセルのAlphaが全部0なら255にする
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 32 / 8;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            if (IsAlphaAll0(pixels) || IsExcelCell())
            {
                ToAlpha255(pixels);
                bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
                return bitmap;
            }
            return bitmap;
        }

        //エクセルの図形用？
        private BitmapSource GetBitmapFromClipboard2()
        {
            BitmapSource bitmap = null;
            if (!Clipboard.ContainsImage()) return bitmap;

            //var meta = Clipboard.GetDataObject().GetData(DataFormats.EnhancedMetafile);//これを画像で取得できればいいけどできない
            //var metapict= Clipboard.GetDataObject().GetData(DataFormats.MetafilePicture);//セルのコピーでエラーになる

            //エクセルのセル以外のコピー
            var stream = (System.IO.MemoryStream)Clipboard.GetDataObject().GetData("PNG");
            //var stream = (System.IO.MemoryStream)Clipboard.GetDataObject().GetData("GIF");
            if (stream != null)
            {
                bitmap = BitmapFrame.Create(stream);
                //bitmap = ToDpi96(bitmap);//dpiを96にする
                return bitmap;
            }
            return bitmap;
        }


        //未使用
        private BitmapSource ToDpi96(BitmapSource bitmap)
        {
            //dpiを96にする
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 32 / 8;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            return BitmapSource.Create(w, h, 96, 96, bitmap.Format, null, pixels, stride);
        }


        //alphaを255にする
        private void ToAlpha255(byte[] pixels)
        {
            for (int i = 3; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;
            }
        }


        //Alphaが全部0ならtrue、1つでも0以外があるならfalse
        private bool IsAlphaAll0(byte[] pixels)
        {
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }
        //エクセル判定、データの中にEnhancedMetafile形式があればエクセルと判定trueを返す
        private bool IsExcelCell()
        {
            string[] formats = Clipboard.GetDataObject().GetFormats();
            foreach (var item in formats)
            {
                if (item == "EnhancedMetafile")
                {
                    return true;
                }
            }
            return false;
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
        public double Rate { get; set; }//最多ピクセルからみた割合
        public double Rate2 { get; set; }//総ピクセル数からみた割合
        public Color Color { get; set; }
        public SolidColorBrush Brush { get; set; }

        public MyColor(int count, double rate, double rate2, Color color, SolidColorBrush brush)
        {
            Count = count;
            Rate = rate;
            Rate2 = rate2;
            Color = color;
            Brush = brush;
        }
    }


    public class MyColorCountData
    {
        public ObservableCollection<MyColor> Data { get; set; }

        public MyColorCountData(IEnumerable<KeyValuePair<uint, int>> keyValues, double maxValue, double pixelsCount)
        {
            Data = new ObservableCollection<MyColor>();
            foreach (var item in keyValues)
            {
                uint ui = item.Key;
                byte b = (byte)(ui & 0x000000FF);
                byte g = (byte)((ui >> 8) & 0x0000FF);
                byte r = (byte)((ui >> 16) & 0x00FF);
                byte a = (byte)(ui >> 24);
                Color c = Color.FromArgb(a, r, g, b);
                double rate = item.Value / maxValue;
                double rate2 = item.Value / pixelsCount;
                //data.Add(new MyColor() { Color = c, Count = item.Value, Brush = new SolidColorBrush(c), Rate = rate });
                Data.Add(new MyColor(item.Value, rate, rate2, c, new SolidColorBrush(c)));
            }
        }
    }

    public class MyMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double width = ((double)values[0] - 120) * (double)values[1];
            return width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            string rgb = $"ARGB = ({c.A}, {c.R}, {c.G}, {c.B})";
            return rgb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MyConverterColorToHSV : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            HSV hSV = HSV.Color2HSV(c);
            return $"HSV = {hSV.ToString100()}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
