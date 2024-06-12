using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Paint += PictureBox1_Paint;
        }
        float ang = 0;
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var gr = e.Graphics;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.Clear(Color.White);
            gr.ResetTransform();
            gr.TranslateTransform(300, 300);
            gr.RotateTransform(ang++);

            gr.DrawEllipse(Pens.Black, -100, -25, 200, 50);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
