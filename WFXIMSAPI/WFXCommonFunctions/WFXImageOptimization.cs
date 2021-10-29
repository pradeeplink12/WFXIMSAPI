using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace WFXIMSAPI.WFXCommonFunctions
{
    public class WFXImageOptimization
    {
        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="canvasWidth">resize width.</param>
        /// <param name="canvasHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>      
        public void ProcessImage(Bitmap image, int canvasWidth, int canvasHeight, int quality, string filePath)
        {
            int newWidth = 0, newHeight = 0;
            getAspectRatio(image, canvasWidth, canvasHeight, out newWidth, out newHeight);
            Bitmap ResizedImage = Resizeimage(image, newWidth, newHeight);
            CompressAndSaveImage(ResizedImage, quality, filePath);
        }

        private void getAspectRatio(Bitmap image, int canvasWidth, int canvasHeight, out int newWidth, out int newHeight)
        {
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)canvasWidth / (float)originalWidth;
            float ratioY = (float)canvasHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);
            if (ratio > 1)
            {
                newWidth = originalWidth;
                newHeight = originalHeight;
            }
            else
            {
                // New width and height based on aspect ratio
                newWidth = (int)(originalWidth * ratio);
                newHeight = (int)(originalHeight * ratio);
            }
        }

        private Bitmap Resizeimage(Bitmap image, int newWidth, int newHeight)
        {
            Bitmap ResizedImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(ResizedImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.Clear(Color.White);
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return ResizedImage;
        }

        private void CompressAndSaveImage(Bitmap image, int quality, string filePath)
        {
            // Get an ImageCodecInfo object that represents the JPEG codec.
            ImageCodecInfo imageCodecInfo = this.GetEncoderInfo(ImageFormat.Jpeg);

            // Create an EncoderParameters object. 
            EncoderParameters encoderParameters = new EncoderParameters(1);

            // Save the image as a JPEG file with quality level.
            EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, quality);
            encoderParameters.Param[0] = encoderParameter;
            image.SetResolution(250, 250);
            image.Save(filePath, imageCodecInfo, encoderParameters);
        }

        private ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }
    }
}
