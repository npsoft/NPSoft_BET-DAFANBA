using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;

namespace EMGU.SettingUp
{
    public partial class SettingUpFrm : Form
    {
        #region For: Ctors
        public SettingUpFrm()
        {
            InitializeComponent();
        }
        #endregion
        #region For: Events
        private void btnLoadImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_file = new OpenFileDialog();
            if (open_file.ShowDialog() == DialogResult.OK)
            {
                #region Comment: Example #1
                /* -: StringBuilder msg = new StringBuilder("RESULTs: ");

                Image<Bgr, Byte> my_img = new Image<Bgr, Byte>(open_file.FileName);
                
                Image<Gray, double> gray_img = my_img.Convert<Gray, Byte>().Convert<Gray, double>();*/

                /* -: UMat gray_img2 = new UMat();
                CvInvoke.CvtColor(my_img, gray_img2, ColorConversion.Bgr2Gray);
                UMat pyrDown = new UMat();
                CvInvoke.PyrDown(gray_img2, pyrDown);
                CvInvoke.PyrUp(pyrDown, gray_img2);
                pictureBoxNew.Image = gray_img2.Bitmap;*/

                /* -: my_img[0, 0] = new Bgr(Color.Red);
                gray_img[0, 0] = new Gray(200);*/

                /* -: Color R = Color.Red;
                my_img.Data[0, 0, 0] = R.B;
                my_img.Data[0, 0, 1] = R.G;
                my_img.Data[0, 0, 2] = R.R;
                gray_img[0, 0] = new Gray(200);*/

                /* -: Bgr my_bgr = my_img[0, 0];
                Gray my_gray = gray_img[0, 0];
                Color my_color = Color.FromArgb((int)my_bgr.Red, (int)my_bgr.Green, (int)my_bgr.Blue);
                int my_intensity = (int)my_gray.Intensity;*/

                /* -: Color my_color = Color.FromArgb(my_img.Data[0, 0, 2], my_img.Data[0, 0, 1], my_img.Data[0, 0, 0]);
                int my_intensity = (int)gray_img.Data[0, 0, 0];*/

                /* -: msg.Append(string.Format("[ B:{0}, G:{1}, R:{2}, Intensity:{3} ]", my_color.B, my_color.G, my_color.R, my_intensity));

                pictureBoxOrg.Image = my_img.ToBitmap();
                pictureBoxNew.Image = gray_img.ToBitmap();

                this.Text = msg.ToString();*/
                #endregion
                #region Comment: Example #2
                try
                {
                    StringBuilder msg = new StringBuilder("RESULTs: ");
                    Image<Bgr, Byte> img_org = new Image<Bgr, Byte>(open_file.FileName);
                    Image<Gray, double> img_gray = img_org.Convert<Gray, Byte>().Convert<Gray, double>();

                    List<Color> colors = new List<Color>();
                    for (int x = 0; x < img_org.Size.Width; x++)
                    {
                        for (int y = 0; y < img_org.Size.Height; y++)
                        {
                            Bgr bgr = img_org[y, x];
                            Color color = Color.FromArgb((int)bgr.Red, (int)bgr.Green, (int)bgr.Blue);
                            colors.Add(color);
                        }
                    }
                    int total = 255 * colors.Count;
                    double percent_b = (double)colors.Sum(x => x.B) / total;
                    double percent_g = (double)colors.Sum(x => x.G) / total;
                    double percent_r = (double)colors.Sum(x => x.R) / total;
                    msg.Append(string.Format("[ width:{0}, height:{1}, percent-b:{2:P}, percent-g:{3:P}, percent-r:{4:P} ]", img_org.Width, img_org.Height, percent_b, percent_g, percent_r));

                    pictureBoxOrg.Image = img_org.ToBitmap();
                    this.Text = msg.ToString();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(string.Format("Exeption\t:: {0}{1}", ex.Message, ex.StackTrace));
                }
                finally { }
                #endregion
            }
        }
        #endregion
    }
}
