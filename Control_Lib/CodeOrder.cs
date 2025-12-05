using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConLib
{
    [ToolboxBitmap(typeof(SingleButton), "Maxu.ico")]
    public partial class CodeOrder : UserControl
    {
        #region 变量
        Bitmap bmp;
        Point[] point1 = new Point[4];
        Point[] point2 = new Point[3];


        public enum CodeView
        {
            绿,
            黄绿,
            黄,
            红黄,
            红
        }
        #endregion

        //属性：显示红黄绿
        [Browsable(true), Category("专用属性")]
        public CodeView cv = CodeView.绿;
        public CodeView 显示状态
        {
            get { return cv; }
            set
            {
                cv = value;
                Drawpic(cv,fangxiang);
            }
        }

        public enum Direction
        {
           上行,
           下行
        }

        //属性：上下行方向
        [Browsable(true), Category("专用属性")]
        public Direction fangxiang = Direction.下行;

        public Direction 上下行方向
        {
            get { return fangxiang; }
            set
            {
                fangxiang = value;
                Drawpic(cv,fangxiang);
            }
        }

        public CodeOrder()
        {
            InitializeComponent();
            Initial();
        }
        public void Initial()
        {
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);
            point1[0] = new Point(0, 0);
            point1[1] = new Point(0, pictureBox1.Height);
            point1[3] = new Point(pictureBox1.Width / 2, pictureBox1.Height / 4);
            point1[2] = new Point(pictureBox1.Width / 2, pictureBox1.Height * 3 / 4);
            point2[0] = new Point(pictureBox1.Width / 2, pictureBox1.Height / 4);
            point2[1] = new Point(pictureBox1.Width / 2, pictureBox1.Height * 3 / 4);
            point2[2] = new Point(pictureBox1.Width, pictureBox1.Height / 2);
        }

        /// <summary>
        /// 绘制红黄绿灯规则
        /// </summary>
        public void Drawpic(CodeView t,Direction d)
        {
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);  //创建bmp对象

            Graphics g = Graphics.FromImage(bmp); //创建Graphics对象，相当于准备画板
            g.Clear(Color.Black); //清空画板并以某一特定颜色填充

            System.Drawing.Rectangle rt;
            

            if (t == CodeView.绿)
            {
                g.FillPolygon(Brushes.Green, point1);
                g.FillPolygon(Brushes.Green, point2);
            }

            else if (t == CodeView.红)
            {
                g.FillPolygon(Brushes.Red, point1);
                g.FillPolygon(Brushes.Red, point2);
            }

            else if (t == CodeView.黄)
            {
                g.FillPolygon(Brushes.Yellow, point1);
                g.FillPolygon(Brushes.Yellow, point2);
            }

            else if (t == CodeView.黄绿)
            {
                g.FillPolygon(Brushes.Yellow, point1);
                g.FillPolygon(Brushes.Green, point2);
            }
            else if (t == CodeView.红黄)
            {
                g.FillPolygon(Brushes.Red, point1);
                g.FillPolygon(Brushes.Yellow, point2);
            }
            if (fangxiang == Direction.上行)
            {
                bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
            }
            g.Save();
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic(cv,fangxiang);
        }

        private void Danniu_Paint(object sender, PaintEventArgs e)
        {
            Initial();
            Drawpic(cv,fangxiang);
        }
    }
}