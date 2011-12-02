using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows;

namespace MotionChallenge
{
    class Util
    {
        public static Bitmap AntiAliasing(Bitmap _input)
        {
            Image<Bgra, Byte> input;
            Image<Bgra, Byte> outputGaussian;
            Image<Bgra, Byte> outputGaussianResized;
            Image<Bgra, Byte> outputThreshold;
            Image<Bgra, Byte> outputThresholdResized;

            // Chargement de l'image
            input = new Image<Bgra, Byte>(_input);

            // Filtre gaussien
            outputGaussian = input.Clone();
            for (int i = 1; i < 10; i = i + 2)
            {
                CvInvoke.cvSmooth(input, outputGaussian, SMOOTH_TYPE.CV_GAUSSIAN, i, i, 0, 0);
            }

            // Redimensionnment de l'image apres flitre Gaussien
            outputGaussianResized = new Image<Bgra, Byte>(outputGaussian.Cols * 2, outputGaussian.Rows * 2);
            CvInvoke.cvResize(outputGaussian, outputGaussianResized, INTER.CV_INTER_CUBIC);

            // Application du seuil
            outputThreshold = outputGaussianResized.Clone();
            CvInvoke.cvThreshold(outputGaussianResized, outputThreshold, 127, 255, THRESH.CV_THRESH_BINARY);

            // Redimensionnment de l'image apres seuillage
            outputThresholdResized = new Image<Bgra, Byte>(outputThreshold.Cols / 2, outputThreshold.Rows / 2);
            CvInvoke.cvResize(outputThreshold, outputThresholdResized, INTER.CV_INTER_CUBIC);

            // Retour de l'image
            return outputThresholdResized.Bitmap;
        }

        public static Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
            );
        }
    }
}
