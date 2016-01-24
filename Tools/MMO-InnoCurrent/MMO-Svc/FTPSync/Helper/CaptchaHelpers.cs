using MODI;

namespace ABSoft.Photobookmart.FTPSync.Helper
{
    public class CaptchaHelpers
    {
        public string ReadTextImg(string path)
        {
            Document document = new Document();
            document.Create(path);
            document.OCR(MiLANGUAGES.miLANG_ENGLISH, true, true);
            Image img = document.Images[0] as Image;
            string text = img.Layout.Text;
            document.Close();
            return text;
        }

        public CaptchaHelpers() { }
    }
}
