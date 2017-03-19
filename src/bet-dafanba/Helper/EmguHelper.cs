using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace SpiralEdge.Helper
{
    public class EmguHelper
    {
        public static Image<Bgr, Byte> CVImgBgrByte(string path)
        {
            return new Image<Bgr, Byte>(path);
        }

        public static void TotalBGR(Image<Bgr, Byte> cvImgBgrByte, out double totalB, out double totalG, out double totalR)
        {
            totalB = 0; totalG = 0; totalR = 0;
            for (int x = 0; x < cvImgBgrByte.Size.Width; x++)
            {
                for (int y = 0; y < cvImgBgrByte.Size.Height; y++)
                {
                    Bgr bgr = cvImgBgrByte[y, x];
                    totalB += bgr.Blue; totalG += bgr.Green; totalR += bgr.Red;
                }
            }
        }

        public static CircleF[] CircleFs(Image<Bgr, Byte> cvImgBgrByte, double dp, double minDist, double circleCannyThreshold, double circleAccumlatorThreshold, int minRadius, int maxRadius)
        {
            UMat uimg = new UMat();
            CvInvoke.CvtColor(cvImgBgrByte, uimg, ColorConversion.Bgr2Gray);
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimg, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimg);
            return CvInvoke.HoughCircles(uimg, HoughType.Gradient, dp, minDist, circleCannyThreshold, circleAccumlatorThreshold, minRadius, maxRadius);
        }
    }
}
