using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EMGU.HelloWorld
{
    public class HelloWorldMain
    {
        public HelloWorldMain()
        {
            string win1 = "Test Window";
            CvInvoke.NamedWindow(win1);

            Mat img = new Mat(200, 400, DepthType.Cv8U, 3);
            img.SetTo(new Bgr(255, 0, 0).MCvScalar);

            CvInvoke.PutText(img, "Hello, world", new System.Drawing.Point(10, 80), FontFace.HersheyComplex, 1.0, new Bgr(0, 255, 0).MCvScalar);

            CvInvoke.Imshow(win1, img);
            CvInvoke.WaitKey(0);
            CvInvoke.DestroyWindow(win1);
        }
    }
}
