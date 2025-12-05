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
    [ToolboxBitmap(typeof(SingleTrack), "Dangui.ico")]
    public partial class SingleTrack : UserControl
    {
        #region 变量
        public string ID,ch365_position;
        public int flag_zt,cuxi=2;
        public string Rlocation;
        public IntPtr pwaram = new IntPtr(0);  //用在获取窗体句柄，获取到的窗体句柄就是以IntPtr类型保存的
        Bitmap bmp;
        Pen p_white, p_blue, p_red;
        #endregion

        #region 属性
        //属性1：ID位置
        public enum IDweizhi
        { 
            上,
            下
        }
        IDweizhi idweizhi = IDweizhi.上;
        [Browsable(true), Category("专用属性")] //是否将该属性加入属性栏，并分在某一栏目下
        public IDweizhi ID位置
        {
            get { return idweizhi; }
            set 
            {
                idweizhi = value;
                flag_zt = 3;
                Drawpic();
            }
        }

        //属性2：实际位置
        [Browsable(true), Category("专用属性")]
        public string 实际位置
        {
            get { return Rlocation; }
            set 
            {
                Rlocation = value;
            }
        }

        //属性3：单轨ID
        [Browsable(true), Category("专用属性")]
        public string 单轨ID
        {
            get { return ID; }
            set 
            {
                ID = value;
            }
        }

        //属性4：轨道粗细
        [Browsable(true), Category("专用属性")]
        public int 轨道粗细
        {
            get { return cuxi; }
            set
            {
                cuxi = value;
                flag_zt = 3;
                Initial();
                Drawpic();
            }
        }

        //属性5：Zlocation，即控件位于窗口前端还是后端 
        public enum Weizhi
        {
            置顶,
            置底
        }
        public Weizhi weizhi;
        [Browsable(true), Category("专用属性")]
        public Weizhi Zlocation
        {
            get { return weizhi; }
            set 
            {
                weizhi = value;
                switch (weizhi)
                { 
                    case Weizhi.置底:
                        this.SendToBack();break;
                    case Weizhi.置顶:
                        this.BringToFront();break;
                }
            }
        }

        //属性6：板卡位置
        [Browsable(true), Category("专用属性")]
        public string 板卡位置
        {
            get { return ch365_position; }
            set 
            {
                ch365_position = value;
            }
        }
        #endregion

        /// <summary>
        /// 控件初始化
        /// </summary>
        public SingleTrack()
        {
            InitializeComponent();
            Initial();
        }
        private void Dangui_Load(object sender, EventArgs e)
        {
            flag_zt = 3;
            Drawpic();
        }

        private void Initial()
        {
            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Location = new Point(0, 0);  //初始位置

            p_white = new Pen(new SolidBrush(Color.White), cuxi);  //画笔类，前参数为颜色，后参数为颜色宽度
            p_blue = new Pen(new SolidBrush(Color.MediumTurquoise), cuxi);
            p_red = new Pen(new SolidBrush(Color.Red), cuxi);
            //jyj_white = new Pen(new SolidBrush(Color.White), 2);
        }

        /// <summary>
        /// 1是占用绘制信息，2是锁闭绘制信息，3是未锁闭未占用
        /// </summary>
        public void Drawpic()
        {
            pwaram = new IntPtr(flag_zt);
            Initial();
            Point p1, p2;
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height); //新实例初始化 Bitmap 类具有指定大小
            pictureBox1.Image = bmp;

            Graphics g = Graphics.FromImage(pictureBox1.Image);  //创建画板工具
            p1 = new Point(0, pictureBox1.Height / 2);  //起点坐标
            p2 = new Point(pictureBox1.Width, pictureBox1.Height / 2);  //终点坐标
            switch (flag_zt)
            {
                case 1:
                    g.DrawLine(p_red, p1, p2); break;
                case 2:
                    g.DrawLine(p_white, p1, p2); break;
                case 3:
                    g.DrawLine(p_blue, p1, p2); break;
            }
        }

        /// <summary>
        /// 鼠标在控件上显示手形，离开后显示箭头形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void 置为白光带区段加锁ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_zt = 2;
            Drawpic();
        }
        private void 去除白光带区段解锁ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_zt = 3;
            Drawpic();
        }
        private void 去除红光带区段占用解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_zt = 3;
            Drawpic();
        }
        private void 置为红光带区段占用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag_zt = 1;
            Drawpic();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Drawpic();
        }
    }   
}
