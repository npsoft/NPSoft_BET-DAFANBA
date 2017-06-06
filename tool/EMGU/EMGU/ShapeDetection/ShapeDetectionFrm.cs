using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EMGU
{
    public partial class ShapDetectionFrm : Form
    {
        #region For: Ctors
        public ShapDetectionFrm()
        {
            InitializeComponent();
            fileNameTextBox.Text = @"D:\NPSoft_BET-DAFANBA\doc\log-production\agin-170606-222722-492\baccarat-1-2\table-level-1\cell-15-1.png";
        }
        #endregion
        #region For: Events
        private void textBox1_TextChange(object sender, EventArgs e)
        {
            PerformShapeDetection();
        }

        private void loadImgageButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                fileNameTextBox.Text = openFileDialog1.FileName;
            }
        }
        #endregion
        #region For: Methods
        private void PerformShapeDetection()
        {
            if (!string.IsNullOrEmpty(fileNameTextBox.Text))
            {
                //StringBuilder msgBuilder = new StringBuilder("Performance: ");

                //Image<Bgr, Byte> img = new Image<Bgr, Byte>(fileNameTextBox.Text)/*.Resize(400, 400, Inter.Linear, true)*/;
                //originalImageBox.Image = img;

                //UMat uimage = new UMat();
                //CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

                //UMat pyrDown = new UMat();
                //CvInvoke.PyrDown(uimage, pyrDown);
                //CvInvoke.PyrUp(pyrDown, uimage);
                //triangleRectangleImageBox.Image = uimage;

                //Stopwatch watch = Stopwatch.StartNew();
                //double cannyThreshold = 300.0;
                //double circleAccumulatorThreshold = 12;
                //CircleF[] circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 1.0, 20.0, cannyThreshold, circleAccumulatorThreshold, 5, 15);
                //watch.Stop();
                //msgBuilder.Append(string.Format("Hough circles - {0} ms; Count - {1}", watch.ElapsedMilliseconds, circles.Length));

                ///* -: Mat circleImage = new Mat(img.Size, DepthType.Cv8U, 3);
                //circleImage.SetTo(new MCvScalar(0));
                //foreach (CircleF circle in circles)
                //    CvInvoke.Circle(circleImage, Point.Round(circle.Center), (int)circle.Radius, new Bgr(Color.Brown).MCvScalar, 2);
                //circleImageBox.Image = circleImage;*/
                //foreach (CircleF circle in circles)
                //    img.Draw(circle, new Bgr(Color.Yellow), 2);
                //circleImageBox.Image = img;

                //this.Text = msgBuilder.ToString();

                #region For: Line Detection
                //StringBuilder sb_msg = new StringBuilder("Performance: ");
                //Image<Bgr, Byte> img = new Image<Bgr, Byte>(fileNameTextBox.Text).Resize(400, 400, Inter.Linear, true);
                //originalImageBox.Image = img;

                //UMat uimage = new UMat();
                //CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);
                //triangleRectangleImageBox.Image = uimage;

                //UMat pyrDown = new UMat();
                //CvInvoke.PyrDown(uimage, pyrDown);
                //CvInvoke.PyrUp(pyrDown, uimage);
                //circleImageBox.Image = uimage;

                //Stopwatch watch = Stopwatch.StartNew();
                //double cannyThreshold = 180.0;
                //double cannyThresholdLinking = 100.0/*120.0*/;
                //UMat cannyEdges = new UMat();
                //CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

                //LineSegment2D[] lines = CvInvoke.HoughLinesP(cannyEdges, 1, Math.PI/45.0, 20, 30, 10);
                //watch.Stop();
                //sb_msg.Append(string.Format("Canny & Hough lines - {0} ms; ", watch.ElapsedMilliseconds));

                //Mat line_img = new Mat(img.Size, DepthType.Cv8U, 3);
                //line_img.SetTo(new MCvScalar(0));
                //foreach (LineSegment2D line in lines)
                //{
                //    CvInvoke.Line(line_img, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 2);
                //}
                //lineImageBox.Image = line_img;

                //this.Text = sb_msg.ToString();
                #endregion
                #region For: Circle Detection
                StringBuilder sb = new StringBuilder("Performance: ");

                Image<Bgr, Byte> img = new Image<Bgr, Byte>(fileNameTextBox.Text);
                originalImageBox.Image = img;

                UMat uimg = new UMat();
                CvInvoke.CvtColor(img, uimg, ColorConversion.Bgr2Gray);

                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(uimg, pyrDown);
                CvInvoke.PyrUp(pyrDown, uimg);
                triangleRectangleImageBox.Image = uimg;

                Stopwatch watch = Stopwatch.StartNew();
                CircleF[] circles = CvInvoke.HoughCircles(uimg, HoughType.Gradient, 1.0, 22.0, 200.0, 6.5, 8, 12);
                System.Diagnostics.Debug.Print(string.Format("== 1.0 - 22.0 - 200.0 - 6.5 - 8 - 12"));
                if (0 == circles.Length)
                {
                    circles = CvInvoke.HoughCircles(uimg, HoughType.Gradient, 1.0, 22.0, 200.0, 6.5, 7, 12);
                    System.Diagnostics.Debug.Print(string.Format("== 1.0 - 22.0 - 200.0 - 6.5 - 7 - 12"));
                }
                if (0 == circles.Length)
                {
                    circles = CvInvoke.HoughCircles(uimg, HoughType.Gradient, 1.5, 22.0, 200.0, 20.0, 7, 12);
                    System.Diagnostics.Debug.Print(string.Format("== 1.5 - 22.0 - 250.0 - 20.0 - 7 - 12"));
                }
                watch.Stop();
                sb.Append(string.Format(" | Hough circles - {0} ms; Count - {1}", watch.ElapsedMilliseconds, circles.Length));
                
                Mat circleImg = new Mat(img.Size, DepthType.Cv8U, 3);
                circleImg.SetTo(new MCvScalar(0));
                foreach (CircleF circle in circles)
                {
                    img.Draw(circle, new Bgr(Color.Yellow), 2);
                }
                circleImageBox.Image = img;

                /* -: string file_name_rotate = string.Format("{0}-rotate{1}", Path.GetFileNameWithoutExtension(fileNameTextBox.Text), Path.GetExtension(fileNameTextBox.Text));
                string file_path_rotate = Path.Combine(Path.GetDirectoryName(fileNameTextBox.Text), file_name_rotate);
                string file_name_crop = string.Format("{0}-crop{1}", Path.GetFileNameWithoutExtension(file_path_rotate), Path.GetExtension(file_path_rotate));
                string file_path_crop = Path.Combine(Path.GetDirectoryName(file_path_rotate), file_name_crop);
                if (File.Exists(file_path_rotate)) { File.Delete(file_path_rotate); }
                if (File.Exists(file_path_crop)) { File.Delete(file_path_crop); }
                Image<Bgr, Byte> img_rotate = img.Rotate(45, new Bgr(Color.White), false);
                img_rotate.Save(file_path_rotate);
                SpiralEdge.Helper.ImageHelper.CropImage(file_path_rotate, file_path_crop, 0, 14, 31, 4);
                Image<Bgr, Byte> img_crop = new Image<Bgr, Byte>(file_path_crop);
                circleImageBox.Image = new Image<Bgr, Byte>(file_path_crop);
                // -: if (File.Exists(file_path_rotate)) { File.Delete(file_path_rotate); }
                // -: if (File.Exists(file_path_crop)) { File.Delete(file_path_crop); }*/

                this.Text = sb.ToString();
                #endregion
            }
        }
        #endregion
    }
}
