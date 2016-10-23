using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using QRCoder;
using ThoughtWorks.QRCode.Codec;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using GDI = System.Drawing;

namespace ImageMerge
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.imgPreview.Source = makePicture(@"back.jpg");
          
        }

        private BitmapSource makePicture(string bgImagePath)
        {
            var qrCodeEncoder = new QRCodeEncoder
            {
                 
                QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
                QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q,
                QRCodeVersion = 4,
                QRCodeScale = 25,
                QRCodeBackgroundColor =System.Drawing.Color.White,
                QRCodeForegroundColor = System.Drawing.Color.FromArgb(0,3,63)

            };
            
            var img = qrCodeEncoder.Encode("asdffffffffffffffff", Encoding.Default);

            img.Save("bbb.jpg", ImageFormat.Png);

         
            BitmapSource headerImage = new BitmapImage(new Uri("bbb.jpg", UriKind.Relative));

            return headerImage;
        }
        //
        private BitmapSource makePicture(string bgImagePath,string iconpath,string signature)
        {

             
            BitmapSource bgImage = new BitmapImage(new Uri(bgImagePath, UriKind.Relative));
            
            BitmapSource headerImage = new BitmapImage(new Uri(iconpath, UriKind.Relative));

             
            RenderTargetBitmap composeImage = new RenderTargetBitmap(bgImage.PixelWidth, bgImage.PixelHeight, bgImage.DpiX, bgImage.DpiY, PixelFormats.Default);

          


            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(bgImage, new Rect(0, 0, bgImage.PixelWidth, bgImage.PixelHeight));

            
            //double x = (bgImage.Width / 2 - headerImage.Width) / 2;
            double x = 46+2;
            double y = bgImage.Height - 189;

            var circleCenterX = x + 63;
            var circleCenterY = y + 63;
            drawingContext.DrawImage(headerImage, new Rect(x, y, 120, 120));
            var pen1 = new Pen(Brushes.White, 1);

            drawingContext.DrawEllipse(Brushes.White, pen1, new Point(circleCenterX, circleCenterY), 21, 21);
            
            SolidColorBrush brush=new SolidColorBrush(Color.FromRgb(0,3,63));
            var pen = new Pen(brush, 1);
            drawingContext.DrawEllipse(brush,pen,new Point(circleCenterX,circleCenterY),20,20);


            FormattedText signatureTxt = new FormattedText(signature,
                                                 System.Globalization.CultureInfo.CurrentCulture,
                                                 System.Windows.FlowDirection.LeftToRight,
                                                 new Typeface(System.Windows.SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                                                 17,
                                                 System.Windows.Media.Brushes.White);
            
            double x2 = circleCenterX-signatureTxt.Width/2;
            double y2 = circleCenterY-signatureTxt.Height/2;
            drawingContext.DrawText(signatureTxt, new System.Windows.Point(x2, y2));
            drawingContext.Close();
            composeImage.Render(drawingVisual);

          
            JpegBitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            
            bitmapEncoder.Frames.Add(BitmapFrame.Create(composeImage));

            string savePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\merge.jpg";
            bitmapEncoder.Save(File.OpenWrite(Path.GetFileName(savePath)));
            return composeImage;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.imgPreview.Source = makePicture(@"back.jpg",@"bbb.jpg","A01");
        }

        private BitmapSource MakePictureGDI(string bgImagePath, string headerImagePath, string signature)
        {
            GDI.Image bgImage = GDI.Bitmap.FromFile(bgImagePath);
            GDI.Image headerImage = GDI.Bitmap.FromFile(headerImagePath);

            
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bgImage.Width, bgImage.Height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
       
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
       
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
       
            g.Clear(System.Drawing.Color.Transparent);

            //先在画板上面画底图
            g.DrawImage(bgImage, new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height));

            //再在画板上画头像
            int x = (bgImage.Width / 2 - 120) / 2;
            int y = (bgImage.Height - 120) / 2 - 100;
            g.DrawImage(headerImage, new GDI.Rectangle(x, y, 120, 120),
                                     new GDI.Rectangle(0, 0, headerImage.Width, headerImage.Height),
                                     GDI.GraphicsUnit.Pixel);



            //在画板上写文字
            using (GDI.Font f = new GDI.Font("Arial", 20, GDI.FontStyle.Bold))
            {
                using (GDI.Brush b = new GDI.SolidBrush(GDI.Color.White))
                {
                    float fontWidth = g.MeasureString(signature, f).Width;
                    float x2 = (bgImage.Width / 2 - fontWidth) / 2;
                    float y2 = y + headerImage.Height + 20;
                    g.DrawString(signature, f, b, x2, y2);
                }
            }

            try
            {
                string savePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\GDI+合成.jpg";
                bitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ToBitmapSource(bitmap);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                bgImage.Dispose();
                headerImage.Dispose();
                g.Dispose();
            }
        }

        #region GDI+ Image 转化成 BitmapSource
        [System.Runtime.InteropServices.DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        public BitmapSource ToBitmapSource(GDI.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();

            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ip, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);//释放对象
            return bitmapSource;
        }
        #endregion
    }
}
