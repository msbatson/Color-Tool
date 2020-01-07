/*
The MIT License (MIT)

Copyright (c) 2017 FlakTheMighty

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorPicker
{
    public partial class MainForm : Form
    {
        string version = "1.3";

        //dll imports
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        //end dll imports

        public static int WM_HOTKEY = 0x312;

        //initializations and assignments
        public byte r = 0, g = 0, b = 0, a = 255;
        public string hex;
        Random randomizer = new Random();
        AboutForm about;
        //end initializations and assignments

        public MainForm()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, (int)Keys.F1, 0, (uint)Keys.F1);

            //don't want to be boring, now do we?
            generateRandomColor();
        }

        //textboxes
        private void hexDisplay_TextChanged(object sender, EventArgs e)
        {
            hex = hexDisplay.Text;

            //prevents crash on launch from method being called as soon as the random color is created
            if (hexDisplay.Focused)
            {
                convertHexToRGB();
            }
        }

        private void rDisplay_TextChanged(object sender, EventArgs e)
        {
            byte.TryParse(rDisplay.Text, out r);
            rSlider.Value = r;
            convertRGBToHex();
        }

        private void gDisplay_TextChanged(object sender, EventArgs e)
        {
            byte.TryParse(gDisplay.Text, out g);
            gSlider.Value = g;
            convertRGBToHex();
        }

        private void bDisplay_TextChanged(object sender, EventArgs e)
        {
            byte.TryParse(bDisplay.Text, out b);
            bSlider.Value = b;
            convertRGBToHex();
        }
        //no more textboxes

        //buttons
        private void randomColor_Click(object sender, EventArgs e)
        {
            generateRandomColor();
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            about = new AboutForm(version);
            about.Show();
        }

        private void openColorDialog_Click(object sender, EventArgs e)
        {
            colorDialog.Color = Color.FromArgb(r, g, b);

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                colorPreview.BackColor = colorDialog.Color;

                r = colorDialog.Color.R;
                g = colorDialog.Color.G;
                b = colorDialog.Color.B;
                rDisplay.Text = colorDialog.Color.R.ToString();
                gDisplay.Text = colorDialog.Color.G.ToString();
                bDisplay.Text = colorDialog.Color.B.ToString();
                rSlider.Value = r;
                gSlider.Value = g;
                bSlider.Value = b;

                convertRGBToHex();
            }
        }

        private void saveColor_Click(object sender, EventArgs e)
        {
            string filepath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Saved Colors.txt";

            if (File.Exists(filepath))
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("R: " + r);
                    sw.WriteLine("G: " + g);
                    sw.WriteLine("B: " + b);
                    sw.WriteLine("Hex: #" + hex);
                    sw.WriteLine("================");
                    sw.Close();
                }
            }
            else
            {
                var savefile = File.Create(filepath);
                savefile.Close();

                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("R: " + r);
                    sw.WriteLine("G: " + g);
                    sw.WriteLine("B: " + b);
                    sw.WriteLine("Hex: #" + hex);
                    sw.WriteLine("================");
                    sw.Close();
                }
            }
        }
        //no more buttons

        //converts the RGB values to hex, updates the textbox if it's not focused, and displays the color
        public void convertRGBToHex()
        {
            hex = string.Format("{0:X}{1:X}{2:X}", r, g, b);

            if (!hexDisplay.Focused)
            {
                hexDisplay.Text = hex;
            }

            colorPreview.BackColor = Color.FromArgb(r, g, b);
        }

        //converts hex to RGB, updates the textboxes if they aren't focused, and displays the color
        public void convertHexToRGB()
        {
            try
            {
                int num = (int)long.Parse(hex, NumberStyles.HexNumber);
                r = Convert.ToByte((num & 0xFF0000) >> 16);
                g = Convert.ToByte((num & 0xFF00) >> 8);
                b = Convert.ToByte(num & 0xFF);
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
            }

            if (!rDisplay.Focused || !gDisplay.Focused || !bDisplay.Focused)
            {
                rDisplay.Text = r.ToString();
                gDisplay.Text = g.ToString();
                bDisplay.Text = b.ToString();
            }

            colorPreview.BackColor = Color.FromArgb(r, g, b);
        }

        //generates a random color and displays it
        public void generateRandomColor()
        {
            r = Convert.ToByte(randomizer.Next(255));
            g = Convert.ToByte(randomizer.Next(255));
            b = Convert.ToByte(randomizer.Next(255));

            rDisplay.Text = r.ToString();
            gDisplay.Text = g.ToString();
            bDisplay.Text = b.ToString();

            rSlider.Value = r;
            gSlider.Value = g;
            bSlider.Value = b;

            convertRGBToHex();
        }

        //setting hotkeys
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_HOTKEY)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);

                if (key == Keys.F1)
                {
                    Point cursor = new Point();
                    GetCursorPos(ref cursor);

                    r = GetColorAt(cursor).R;
                    b = GetColorAt(cursor).B;
                    g = GetColorAt(cursor).G;

                    rDisplay.Text = r.ToString();
                    gDisplay.Text = g.ToString();
                    bDisplay.Text = b.ToString();

                    rSlider.Value = r;
                    gSlider.Value = g;
                    bSlider.Value = b;

                    convertRGBToHex();
                }
            }
        }

        private void MouseMoveTimer_Tick(object sender, EventArgs e)
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);

            var c = GetColorAt(cursor);
            this.BackColor = c;
        }

        public Color GetColorAt(Point location)
        {
            Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

            //take an image of the screen and wherever the cursor is, save the color
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            //return the color of whatever pixel the cursor was at (the bitmap is 1x1 so 0,0)
            return screenPixel.GetPixel(0, 0);
        }
        //end cursor color selection

        private void rSlider_Scroll(object sender, EventArgs e)
        {
            rDisplay.Text = rSlider.Value.ToString();
        }

        private void gSlider_Scroll(object sender, EventArgs e)
        {
            gDisplay.Text = gSlider.Value.ToString();
        }

        private void bSlider_Scroll(object sender, EventArgs e)
        {
            bDisplay.Text = bSlider.Value.ToString();
        }
    }
}