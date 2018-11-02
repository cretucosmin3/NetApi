using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace NetApi
{
    class Program
    {
        static ShowImage ImageCast = new ShowImage();
        static void Main(string[] args)
        {
            var result = MessageBox.Show("are you a client", "Question", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter Server IP : ", "Question", "192.168.0.1", -1, -1);
                Client cl = new Client(input, 555, "localhostClient");
                Task.Factory.StartNew(() =>
                {
                    {
                        while (true)
                        {

                            try
                            {
                                cl.client.Send("fisier", getScreen(4f));
                            //ImageCast.Invoke((MethodInvoker)(() => ImageCast.Picture.Image = getScreen(2f)));
                        }
                            catch (Exception ex)
                            {
                            //Console.WriteLine(ex.Message);
                        }

                            Thread.Sleep(20);
                        }
                    }
                });
            }
            else
            {
                Server sv = new Server(555);
                sv.OnDataReceived += Sv_OnDataReceived;
                ImageCast.ShowDialog();
            }
            Console.Read();
        }

        private static void Sv_OnDataReceived(string Message, object Data)
        {
            ImageCast.Invoke((MethodInvoker)(() => ImageCast.Picture.Image = (Bitmap)Data));
        }

        public static Bitmap getScreen(double ReducedSize)
        {

            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Console.WriteLine(bounds.ToString());
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

                    Image img = bitmap;
                    Bitmap resizedImg = new Bitmap((int)(Screen.GetBounds(Point.Empty).Width / ReducedSize), (int)(Screen.GetBounds(Point.Empty).Height / ReducedSize));
                    Console.WriteLine(resizedImg.Size);
                    double ratioX = (double)resizedImg.Width / (double)img.Width;
                    double ratioY = (double)resizedImg.Height / (double)img.Height;
                    double ratio = ratioX < ratioY ? ratioX : ratioY;

                    int newHeight = Convert.ToInt32(img.Height * ratio);
                    int newWidth = Convert.ToInt32(img.Width * ratio);

                    using (Graphics gp = Graphics.FromImage(resizedImg))
                    {
                        gp.DrawImage(img, 0, 0, newWidth, newHeight);
                    }
                    return resizedImg;
                }
            }
        }
    }
}
