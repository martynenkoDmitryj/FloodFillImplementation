using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        List<Pixel> pixels;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Image file|*.jpg;*.png;*.bmp" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var image = (Bitmap)Bitmap.FromFile(ofd.FileName);

            }
            pictureBox1.Image = new Bitmap(ofd.FileName);
            pixels = new List<Pixel>();
        }

      

        private static bool ColorMatch(Color a, Color b)
        {
            if (a.GetBrightness() >= 0.05f)
                return Math.Abs(a.GetHue() - b.GetHue()) <= 45;
            else return false;
        }

        void FloodFill(Bitmap bmp, Point pt, Color targetColor)
        {
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(pt);
            while (q.Count > 0)
            {
                Point n = q.Dequeue();
                if (!ColorMatch(bmp.GetPixel(n.X, n.Y), targetColor))
                    continue;
                Point w = n, e = new Point(n.X + 1, n.Y);
                while ((w.X >= 0) && ColorMatch(bmp.GetPixel(w.X, w.Y), targetColor))
                {
                    pixels.Add(new Pixel() { x = w.X, y = w.Y, color = bmp.GetPixel(w.X, w.Y) });

                    bmp.SetPixel(w.X, w.Y, Color.White);
                    if ((w.Y > 0) && ColorMatch(bmp.GetPixel(w.X, w.Y - 1), targetColor))
                        q.Enqueue(new Point(w.X, w.Y - 1));
                    if ((w.Y < bmp.Height - 1) && ColorMatch(bmp.GetPixel(w.X, w.Y + 1), targetColor))
                        q.Enqueue(new Point(w.X, w.Y + 1));
                    w.X--;
                }

                while ((e.X <= bmp.Width - 1) && ColorMatch(bmp.GetPixel(e.X, e.Y), targetColor))
                {

                    pixels.Add(new Pixel() { x = e.X, y = e.Y, color = bmp.GetPixel(e.X, e.Y) });
                    bmp.SetPixel(e.X, e.Y, Color.White);

                    if ((e.Y > 0) && ColorMatch(bmp.GetPixel(e.X, e.Y - 1), targetColor))
                        q.Enqueue(new Point(e.X, e.Y - 1));
                    if ((e.Y < bmp.Height - 1) && ColorMatch(bmp.GetPixel(e.X, e.Y + 1), targetColor))
                        q.Enqueue(new Point(e.X, e.Y + 1));
                    e.X++;
                }
            }

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            pictureBox3.Image = new Bitmap(bmp.Width, bmp.Height);
            Bitmap bmp2 = new Bitmap(pictureBox3.Image);
            Thread t = new Thread(() =>
            {
                FloodFill(bmp, e.Location, bmp.GetPixel(e.X, e.Y));
            });
            
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            t.Start();
            t.Join(2000);
            t.Abort();
            stopWatch.Stop();
            if (stopWatch.ElapsedMilliseconds < 1000)
            {
                foreach (Pixel p in pixels)
                {
                    bmp2.SetPixel(p.x, p.y, p.color);
                }
                pictureBox3.Image = bmp2;
                if (pixels.Count > 0)
                {
                    string[] text = new string[4];
                    text[0] = "West: (" + pixels.OrderByDescending((x => x.x)).Last().x + ";" + pixels.OrderByDescending((x => x.x)).Last().y + ")";
                    text[1] = "East: (" + pixels.OrderByDescending((x => x.x)).First().x + ";" + pixels.OrderByDescending((x => x.x)).First().y + ")";
                    text[2] = "North: (" + pixels.OrderByDescending((x => x.y)).Last().x + ";" + pixels.OrderByDescending((x => x.y)).Last().y + ")";
                    text[3] = "South: (" + pixels.OrderByDescending((x => x.y)).First().x + ";" + pixels.OrderByDescending((x => x.y)).First().y + ")";
                    File.WriteAllLines(Application.StartupPath + @"/coord.txt", text);
                }
                pixels.Clear();
            }
        }
    }
    public struct Pixel
    {
        public int x;
        public int y;
        public Color color;
    }
}

   


        
    