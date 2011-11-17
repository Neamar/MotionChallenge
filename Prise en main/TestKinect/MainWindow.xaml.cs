using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;

namespace TestKinect
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Runtime nui;
        private void image1_Loaded(object sender, RoutedEventArgs e)
        {
            nui = Runtime.Kinects[0];
            
            nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
            nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            
         //   nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
         //   nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.Depth);
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
            image1.Source = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgr32,
                null, image.Bits, image.Width * PixelFormats.Bgr32.BitsPerPixel / 8);
        }

        void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
            image1.Source = BitmapSource.Create( image.Width, image.Height, 96, 96, PixelFormats.Bgr32,
                null, image.Bits, image.Width * image.BytesPerPixel);
        }

        private void image1_Unloaded(object sender, RoutedEventArgs e)
        {
            nui.Uninitialize();
        }
    }
}
