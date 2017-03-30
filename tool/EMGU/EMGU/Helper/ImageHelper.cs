using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace SpiralEdge.Helper
{
    public static class ImageHelper
    {
        private static Dictionary<string, ImageCodecInfo> encoders = null;
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            get
            {
                if (null == encoders)
                {
                    encoders = new Dictionary<string, ImageCodecInfo>();
                }
                if (0 == encoders.Count)
                {
                    foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                    {
                        encoders.Add(codec.MimeType.ToLower(), codec);
                    }
                }
                return encoders;
            }
        }

        public static ImageCodecInfo GetCodecInfo(string mimeType)
        {
            ImageCodecInfo codec = null;
            mimeType = string.Format("{0}", mimeType).ToLower();
            if (Encoders.ContainsKey(mimeType))
            {
                codec = Encoders[mimeType];
            }
            return codec;
        }

        public static void RotateImage(string orgPath, string desPath, string direction)
        {
            using (Image img_org = Image.FromFile(orgPath))
            {
                if ("left" == direction)
                {
                    img_org.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                if ("right" == direction)
                {
                    img_org.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                img_org.Save(desPath, img_org.RawFormat);
            }
        }

        public static void ResizeImage(string orgPath, string desPath, int width, int height)
        {
            #region For: Recalculate width & height
            Image img_cal = Image.FromFile(orgPath);
            double ratio =
                0 != width ? (double)width / img_cal.Width :
                0 != height ? (double)height / img_cal.Height : 1;
            width = (int)(img_cal.Width * ratio);
            height = (int)(img_cal.Height * ratio);
            if (null != img_cal) { img_cal.Dispose(); }
            #endregion
            #region For: Resize via width & height
            using (Image img_org = Image.FromFile(orgPath))
            {
                Image img_bmp = new Bitmap(width, height, img_org.PixelFormat);
                Graphics graphics = Graphics.FromImage(img_bmp);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Rectangle rectangle = new Rectangle(0, 0, width, height);
                graphics.DrawImage(img_org, rectangle);

                long compress_quality = 96;
                EncoderParameter quality_param = new EncoderParameter(Encoder.Quality, compress_quality);
                ImageCodecInfo jpeg_codec = GetCodecInfo("image/jpeg");
                EncoderParameters encoder_params = new EncoderParameters();
                encoder_params.Param[0] = quality_param;
                img_bmp.Save(desPath, jpeg_codec, encoder_params);
                if (null != img_bmp) { img_bmp.Dispose(); }
            }
            #endregion
        }

        public static void CropImage(string orgPath, string desPath, int x, int y, int width, int height)
        {
            #region For: Crop image
            byte[] bytes;
            using (Image img_org = Image.FromFile(orgPath))
            {
                using (Bitmap img_bmp = new Bitmap(width, height, img_org.PixelFormat))
                {
                    img_bmp.SetResolution(img_org.HorizontalResolution, img_org.VerticalResolution);
                    using (Graphics graphics = Graphics.FromImage(img_bmp))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(img_org, new Rectangle(0, 0, width, height), x, y, width, height, GraphicsUnit.Pixel);

                        MemoryStream ms = new MemoryStream();
                        img_bmp.Save(ms, img_org.RawFormat);
                        bytes = ms.GetBuffer();
                    }
                }
            }
            #endregion
            #region For: Save image
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                using (Image img = Image.FromStream(ms, true))
                {
                    img.Save(desPath, img.RawFormat);
                }
            }
            #endregion
        }
    }
}
