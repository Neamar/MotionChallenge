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
        /// <summary>
        /// Apply an anti-aliasing algorithm on a Bitmap object
        /// </summary>
        /// <param name="_input">A Bitmap to anti-alias</param>
        /// <returns>The anti-aliased Bitmap</returns>
        public static Bitmap AntiAliasing(Bitmap _input)
        {
            Image<Bgra, Byte> input;
            Image<Bgra, Byte> outputGaussian;
            Image<Bgra, Byte> outputGaussianResized;
            Image<Bgra, Byte> outputThreshold;
            Image<Bgra, Byte> outputThresholdResized;

            // Image loading
            input = new Image<Bgra, Byte>(_input);

            // Gaussian filter
            outputGaussian = input.Clone();
            for (int i = 1; i < 10; i = i + 2)
            {
                CvInvoke.cvSmooth(input, outputGaussian, SMOOTH_TYPE.CV_GAUSSIAN, i, i, 0, 0);
            }

            // Resize the image after the Gaussian filter (2 times bigger)
            outputGaussianResized = new Image<Bgra, Byte>(outputGaussian.Cols * 2, outputGaussian.Rows * 2);
            CvInvoke.cvResize(outputGaussian, outputGaussianResized, INTER.CV_INTER_CUBIC);

            // Threshold
            outputThreshold = outputGaussianResized.Clone();
            CvInvoke.cvThreshold(outputGaussianResized, outputThreshold, 127, 255, THRESH.CV_THRESH_BINARY);

            // Resize the image after the threshold (2 times smaller)
            outputThresholdResized = new Image<Bgra, Byte>(outputThreshold.Cols / 2, outputThreshold.Rows / 2);
            CvInvoke.cvResize(outputThreshold, outputThresholdResized, INTER.CV_INTER_CUBIC);

            // Return the processed image
            return outputThresholdResized.Bitmap;
        }

        /// <summary>
        /// Convert a BitmapSource object to a Bitmap one
        /// </summary>
        /// <param name="source">The BitmapSource object to convert</param>
        /// <returns>The Bitmap object corresponding to the BitmapSource parameter</returns>
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

        /// <summary>
        /// Convert a Bitmap object to a BitmapSource one
        /// </summary>
        /// <param name="bitmap">The Bitmap object to convert</param>
        /// <returns>The BitmapSource object corresponding to the Bitmap parameter</returns>
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
