using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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

        public static void RotateImg(string orgPath, string desPath, string direction)
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

        public static void ResizeImg(string orgPath, string desPath, int width, int height)
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

        public static void CropImg(string orgPath, string desPath, int x, int y, int width, int height)
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
        
        public static void AnalysisImg_AGIN_3840x2160(string filePath, out AGIN_3840x2160_Baccarat output)
        {
            #region For: DirectoryInfo.Create()
            DirectoryInfo dir_info = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)));
            if (dir_info.Exists) { dir_info.Delete(true); }
            dir_info.Create();
            #endregion
            #region For: Crop and Save image
            Image img_org = Image.FromFile(filePath);
            int maxx = 2, maxy = 4;
            int x = 0, y = 0, w = 1638, h = 471, mgl = 355, mgt = 170;
            output = new AGIN_3840x2160_Baccarat(maxx, maxy);
            using (Bitmap img_bmp = new Bitmap(w, h, img_org.PixelFormat))
            {
                for (int ix = 0; ix < maxx; ix++)
                {
                    for (int iy = 0; iy < maxy; iy++)
                    {
                        if (!(0 == ix && 0 == iy || 0 == ix && 3 == iy || 1 == ix && 3 == iy))
                        {
                            x = mgl + (w + 49) * ix;
                            y = mgt + (h + 29) * iy;
                            string file_name = string.Format("baccarat-{0}-{1}{2}", ix, iy, Path.GetExtension(filePath));
                            string file_path = Path.Combine(dir_info.FullName, file_name);
                            img_bmp.SetResolution(img_org.HorizontalResolution, img_org.VerticalResolution);
                            using (Graphics graphics = Graphics.FromImage(img_bmp))
                            {
                                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graphics.DrawImage(img_org, new Rectangle(0, 0, w, h), x, y, w, h, GraphicsUnit.Pixel);
                                img_bmp.Save(file_path, img_org.RawFormat);
                            }
                            AnalysisImg_AGIN_3840x2160_Step2(file_path, out output.Tbls[ix][iy]);
                        }
                    }
                }
            }
            dir_info.Delete(true);
            #endregion
            #region For: Image.Dispose()
            if (null != img_org) { img_org.Dispose(); }
            #endregion
        }

        public static void AnalysisImg_AGIN_3840x2160_Step2(string filePath, out AGIN_3840x2160_Baccarat_TblLevel1 output)
        {
            #region For: DirectoryInfo.Create()
            DirectoryInfo dir_info = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)));
            dir_info.Create();
            #endregion
            #region For: Crop and Save image
            Image img_org = Image.FromFile(filePath);
            int x = 778, y = 90, w = 846, h = 152;
            using (Bitmap img_bmp = new Bitmap(w, h, img_org.PixelFormat))
            {
                string file_name = string.Format("table-level-1{0}", Path.GetExtension(filePath));
                string file_path = Path.Combine(dir_info.FullName, file_name);
                img_bmp.SetResolution(img_org.HorizontalResolution, img_org.VerticalResolution);
                using (Graphics graphics = Graphics.FromImage(img_bmp))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(img_org, new Rectangle(0, 0, w, h), x, y, w, h, GraphicsUnit.Pixel);
                    img_bmp.Save(file_path, img_org.RawFormat);
                }
                AnalysisImg_AGIN_3840x2160_Step3(file_path, out output);
            }
            #endregion
            #region For: Image.Dispose()
            if (null != img_org) { img_org.Dispose(); }
            #endregion
        }
        
        public static void AnalysisImg_AGIN_3840x2160_Step3(string filePath, out AGIN_3840x2160_Baccarat_TblLevel1 output)
        {
            #region For: DirectoryInfo.Create()
            DirectoryInfo dir_info = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)));
            dir_info.Create();
            #endregion
            #region For: Crop and Save image
            Image img_org = Image.FromFile(filePath);
            int maxx = 34, maxy = 6;
            int x = 0, y = 0, w = 22, h = 22, mgl = 3, mgt = 3;
            output = new AGIN_3840x2160_Baccarat_TblLevel1(maxx, maxy);
            for (int ix = 0; ix < maxx; ix++)
            {
                for (int iy = 0; iy < maxy; iy++)
                {
                    x = mgl + (w + 3) * ix - (ix - 1) / 5;
                    y = mgt + (h + 3) * iy;
                    string file_name = string.Format("cell-{0}-{1}{2}", ix, iy, Path.GetExtension(filePath));
                    string file_path = Path.Combine(dir_info.FullName, file_name);
                    using (Bitmap img_bmp = new Bitmap(w, h, img_org.PixelFormat))
                    {
                        img_bmp.SetResolution(img_org.HorizontalResolution, img_org.VerticalResolution);
                        using (Graphics graphics = Graphics.FromImage(img_bmp))
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(img_org, new Rectangle(0, 0, w, h), x, y, w, h, GraphicsUnit.Pixel);
                            img_bmp.Save(file_path, img_org.RawFormat);

                            var cell = new AGIN_3840x2160_Baccarat_TblLevel1_Cell(file_path);
                            output.AddCell(cell, ix, iy);
                        }
                    }
                }
            }
            #endregion
            #region For: Image.Dispose()
            if (null != img_org) { img_org.Dispose(); }
            #endregion
        }
    }

    public class AGIN_3840x2160_Baccarat
    {
        public AGIN_3840x2160_Baccarat_TblLevel1[][] Tbls { get; set; }

        public AGIN_3840x2160_Baccarat() { }

        public AGIN_3840x2160_Baccarat(int maxX, int maxY)
        {
            Tbls = new AGIN_3840x2160_Baccarat_TblLevel1[maxX][];
            for(int i = 0; i < maxX; i++)
            {
                Tbls[i] = new AGIN_3840x2160_Baccarat_TblLevel1[maxY];
            }
        }

        public void AddTbl(AGIN_3840x2160_Baccarat_TblLevel1 tbl, int idxX, int idxY)
        {
            Tbls[idxX][idxY] = tbl;
        }
    }

    public class AGIN_3840x2160_Baccarat_TblLevel1
    {
        public AGIN_3840x2160_Baccarat_TblLevel1_Cell[][] Cells { get; set; }
        
        public AGIN_3840x2160_Baccarat_TblLevel1() { }

        public AGIN_3840x2160_Baccarat_TblLevel1(int maxX, int maxY)
        {
            Cells = new AGIN_3840x2160_Baccarat_TblLevel1_Cell[maxX][];
            for (int i = 0; i < maxX; i++)
            {
                Cells[i] = new AGIN_3840x2160_Baccarat_TblLevel1_Cell[maxY];
            }
        }

        public void AddCell(AGIN_3840x2160_Baccarat_TblLevel1_Cell cell, int idxX, int idxY)
        {
            Cells[idxX][idxY] = cell;
        }
    }

    public class AGIN_3840x2160_Baccarat_TblLevel1_Cell
    {
        private int Width { get; set; }
        private int Height { get; set; }
        private double TotalB { get; set; }
        private double TotalG { get; set; }
        private double TotalR { get; set; }
        public List<string> Matches { get; set; }
        public double PercentB { get { return Math.Round((TotalB / (Width * Height * 255)) * 10000) / 10000; } }
        public double PercentG { get { return Math.Round((TotalG / (Width * Height * 255)) * 10000) / 10000; } }
        public double PercentR { get { return Math.Round((TotalR / (Width * Height * 255)) * 10000) / 10000; } }
        
        private void Analysis(string path)
        {
            #region For: Width/Height and TotalB/TotalG/TotalR
            var img = EmguHelper.CVImgBgrByte(path);
            double totalB, totalG, totalR;
            EmguHelper.TotalBGR(img, out totalB, out totalG, out totalR);
            Width = img.Size.Width; Height = img.Size.Height;
            TotalB = totalB; TotalG = totalG; TotalR = totalR;
            #endregion
            #region For: Matches [bg-white, slash-green, circle-blue, circle-red, number-black]
            Matches = new List<string>();
            #region For: bg-white
            if (1 > PercentB && PercentB > 0.96 &&
                1 > PercentG && PercentG > 0.96 &&
                1 > PercentR && PercentR > 0.96)
            {
                Matches.AddRange(new List<string>() { "bg-white" });
            }
            #endregion
            #region For: bg-white, slash-green
            if (0.75 > PercentB && PercentB > 0.70 &&
                0.90 > PercentG && PercentG > 0.85 &&
                0.80 > PercentR && PercentR > 0.75)
            {
                Matches.AddRange(new List<string>() { "bg-white", "slash-green" });
            }
            #endregion
            #region For: bg-white, circle-blue
            if (0.80 > PercentB && PercentB > 0.70 &&
                0.65 > PercentG && PercentG > 0.55 &&
                0.65 > PercentR && PercentR > 0.55)
            {
                Matches.AddRange(new List<string>() { "bg-white", "circle-blue" });
            }
            #endregion
            #region For: bg-white, circle-red
            if (0.65 > PercentB && PercentB > 0.55 &&
                0.65 > PercentG && PercentG > 0.60 &&
                0.85 > PercentR && PercentR > 0.80)
            {
                Matches.AddRange(new List<string>() { "bg-white", "circle-red" });
            }
            #endregion
            if (0 == Matches.Count)
            {
                #region For: Circles detection
                var circles = EmguHelper.CircleFs(img, 1.0, 22.0, 250.0, 7, 8, 12);
                if (0 == circles.Length) { circles = EmguHelper.CircleFs(img, 1.0, 22.0, 250.0, 7, 7, 12); }
                #endregion
                #region For: Slash's percentage | no need :-s
                /* -: string file_name_rotate = string.Format("{0}-rotate{1}", Path.GetFileNameWithoutExtension(path), Path.GetExtension(path));
                string file_path_rotate = Path.Combine(Path.GetDirectoryName(path), file_name_rotate);
                string file_name_crop = string.Format("{0}-crop{1}", Path.GetFileNameWithoutExtension(path), Path.GetExtension(path));
                string file_path_crop = Path.Combine(Path.GetDirectoryName(path), file_name_crop);
                if (File.Exists(file_path_rotate)) { File.Delete(file_path_rotate); }
                if (File.Exists(file_path_crop)) { File.Delete(file_path_crop); }
                Emgu.CV.Image<Emgu.CV.Structure.Bgr, Byte> img_rotate = img.Rotate(45, new Emgu.CV.Structure.Bgr(Color.White), false);
                img_rotate.Save(file_path_rotate);
                ImageHelper.CropImg(file_path_rotate, file_path_crop, 0, 14, 31, 4);
                Emgu.CV.Image<Emgu.CV.Structure.Bgr, Byte> img_crop = new Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>(file_path_crop);
                if (File.Exists(file_path_rotate)) { File.Delete(file_path_rotate); }
                if (File.Exists(file_path_crop)) { File.Delete(file_path_crop); }
                double totalB_slash = 0, totalG_slash = 0, totalR_slash = 0;
                for (int x = 0; x < img_crop.Size.Width; x++)
                {
                    for (int y = 0; y < img_crop.Size.Height; y++)
                    {
                        Emgu.CV.Structure.Bgr bgr = img_crop[y, x];
                        totalB_slash += bgr.Blue; totalG_slash += bgr.Green; totalR_slash += bgr.Red;
                    }
                }
                double total_slash = img_crop.Size.Width * img_crop.Size.Height * 255;
                double percentB_slash = totalB_slash / total_slash, percentG_slash = totalG_slash / total_slash, percentR_slash = totalR_slash / total_slash;*/
                #endregion
                if (0 != circles.Length)
                {
                    #region For: bg-white, circle-blue, slash-green
                    if (0.65 > PercentB && PercentB > 0.55 &&
                        0.60 > PercentG && PercentG > 0.50 &&
                        0.50 > PercentR && PercentR > 0.45)
                    {
                        Matches.AddRange(new List<string>() { "bg-white", "circle-blue", "slash-green" });
                    }
                    #endregion
                    #region For: bg-white, circle-red, slash-green
                    if (0.45 > PercentB && PercentB > 0.40 &&
                        0.60 > PercentG && PercentG > 0.55 &&
                        0.70 > PercentR && PercentR > 0.65)
                    {
                        Matches.AddRange(new List<string>() { "bg-white", "circle-red", "slash-green" });
                    }
                    #endregion
                    #region For: bg-white, circle-blue, slash-green, number-black
                    if (0.55 > PercentB && PercentB > 0.50 &&
                        0.50 > PercentG && PercentG > 0.45 &&
                        0.45 > PercentR && PercentR > 0.35)
                    {
                        Matches.AddRange(new List<string>() { "bg-white", "circle-blue", "slash-green", "number-black" });
                    }
                    #endregion
                    #region For: bg-white, circle-red, slash-green, number-black
                    if (0.40 > PercentB && PercentB > 0.30 &&
                        0.55 > PercentG && PercentG > 0.45 &&
                        0.65 > PercentR && PercentR > 0.55)
                    {
                        Matches.AddRange(new List<string>() { "bg-white", "circle-red", "slash-green", "number-black" });
                    }
                    #endregion
                }
            }
            #endregion
        }

        public AGIN_3840x2160_Baccarat_TblLevel1_Cell() { }

        public AGIN_3840x2160_Baccarat_TblLevel1_Cell(string path)
        {
            Analysis(path);
        }
    }
}
