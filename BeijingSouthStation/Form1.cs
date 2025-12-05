using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConLib;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace NanJingNanStation
{
    public partial class 南京南站 : Form
    {
        const string TCCM = "1";
        const string POSTION = "7";
        const string PLAN = "2";
        const string DELETE = "3";
        const string TIME = "9";
        const string ACCIDENT = "10";
        const string DC = "12";
        const string STOPSTATION = "NJ";
        const string DOWNINTEVAL_1 = "X4";   // ？
        const string UPINTEVAL_1 = "S4";     // ？
        const string DOWNINTEVAL_2 = "X5";   // ？
        const string UPINTEVAL_2 = "S5";     // ？
        //const string UP_ARRIVE_BLOCK = "S5_11";
        const string UP_ARRIVE_BLOCK = "S5_12";           //S5段的第12节，从右往左数
        const string UP_ARRIVE_BLOCK_FX = "X5_2";//FX  //X5段的第2节，从左往右数
        //const string DOWN_ARRIVE_BLOCK = "X4_17";
        const string DOWN_ARRIVE_BLOCK = "X4_18";      //X4段的第18节，从左往右数
        const string DOWN_ARRIVE_BLOCK_FX = "S4_2";//FX  //S4段的第2节，从右往左数
        public Dictionary<string, string> CHUFA = new Dictionary<string, string>();
        public Dictionary<string, string> TRAIN_ROUTE = new Dictionary<string, string>();

        public Dictionary<string, DeparturePortInfo> departurePortDict = new Dictionary<string, DeparturePortInfo>(); // 发车口信息字典

        const string ARRIVE = "Ar";
        const string DEPARTURE = "De";
        const string DOWN_DIRECTION = "下";
        const string UP_DIRECTION = "上";
        const string SEPARATOR = "$";
        const char CSEPARATOR = '$';
        const string IP = "127.0.0.1";
        const int PORT = 1029;
        const string RBCIP = "192.168.10.5";
        const int RBCPORT = 180;
        const string CTCIP = "192.168.10.4";
        const int CTCPORT = 180;
        const string DOWNDIRECTION = "北京 --> 上海";
        const string UPDIRECTION = "上海 --> 北京";
        const int AHEAD = 3;
        const int ID_LENGTH = 4;
        int buttonvalue = 0;
        public string mode_interlock = "normal";
        Hashtable stationTracks = new Hashtable();
        Hashtable singleTracks = new Hashtable();
        Hashtable intervalTracks = new Hashtable();
        Hashtable switches = new Hashtable();
        Hashtable signals = new Hashtable();
        Hashtable interlocks = new Hashtable();
        Hashtable trains = new Hashtable();
        Hashtable trainToTrack = new Hashtable();
        Hashtable upDownState = new Hashtable();
        Hashtable timeToInterlock = new Hashtable();
        Hashtable trainScheduleInfo = new Hashtable();
        Hashtable trainToInterlock = new Hashtable();
        Hashtable codeOrders = new Hashtable();
        List<string> routes = new List<string>();
        static Socket server;
        private SynchronizationContext maincontext;
        int upBlockNum_1 = 0, downBlockNum_1 = 0;
        int upBlockNum_2 = 0, downBlockNum_2 = 0;
        int dataGridId = 1;
        bool Zhijietongguo = false;
        public 南京南站()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            initializeInterface();
            readInterlockTable();
            maincontext = SynchronizationContext.Current;
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));
            //new Thread(receivevMessage).Start();


            new Thread(new ThreadStart(receivevMessage)).Start();
            new Thread(new ThreadStart(sendMessage)).Start();

            //Thread Re_Msg = new Thread(new ThreadStart(receivevMessage));
            //Thread Se_Msg = new Thread(new ThreadStart(sendMessage));
            //Re_Msg.Start();
            //Se_Msg.Start();

            /*
            Thread Re_Msg = new Thread(receivevMessage);
            Re_Msg.IsBackground = true;
            Re_Msg.Start();
            Thread Se_Msg = new Thread(sendMessage);
            Se_Msg.IsBackground = true;
            Se_Msg.Start();
            */
        }
        private void initializeInterface()
        {
            interlocks.Clear();
            singleTracks.Clear();
            stationTracks.Clear();
            switches.Clear();
            initializeDataGrid();
            addControllers();
            addCodeOrders();
            addStateRount();

        }
        private void readInterlockTable()
        {
            string key;
            string value;
            StreamReader sr = null;
            interlocks.Clear();
            try
            {
                sr = new StreamReader(@"..\..\interlock.txt");
                while (!sr.EndOfStream)
                {
                    key = sr.ReadLine();
                    if (key.StartsWith("//"))
                    {
                        continue;
                    }
                    value = sr.ReadLine();
                    interlocks.Add(key, value);
                }
            }
            catch (System.Exception ex)
            {
                Console.Write("读取文件出错");
            }
            finally
            {
                sr.Close();
            }
        }
        private void initializeDataGrid()
        {
            dataGridView1.ColumnCount = 11;
            dataGridView1.ColumnHeadersVisible = true;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Columns[i].Width = dataGridView1.Width / 11;
            }
            dataGridView1.Columns[0].Name = "序号";
            dataGridView1.Columns[1].Name = "车次";
            dataGridView1.Columns[2].Name = "计划时间";
            dataGridView1.Columns[3].Name = "触发条件";
            dataGridView1.Columns[4].Name = "股道";
            dataGridView1.Columns[5].Name = "进路类型";
            dataGridView1.Columns[6].Name = "进路名称";
            dataGridView1.Columns[7].Name = "触发方式";
            dataGridView1.Columns[8].Name = "方向";
            dataGridView1.Columns[9].Name = "状态";
            dataGridView1.Columns[10].Name = "未执行的作业";
        }
        private void addControllers()
        {
            foreach (Control con in this.Controls)
            {
                if (con.Name.Contains("StationTrack"))
                {
                    SingleTrack_WithIJ stationTrack = (SingleTrack_WithIJ)con;
                    stationTrack.ID = con.Name.Split('_')[1];
                    stationTracks.Add(con.Name, stationTrack.ID);
                    Console.WriteLine(con.Name);
                    Console.WriteLine(stationTrack.ID);

                }
                else if (con.Name.Contains("SingleTrack"))
                {
                    Console.WriteLine(con.Name);
                    SingleTrack_WithIJ singleTrack = (SingleTrack_WithIJ)con;
                    singleTrack.ID = con.Name.Split('_')[1];
                    singleTracks.Add(con.Name, singleTrack);
                }
                else if (con.Name.Contains("QujianTrack"))
                {
                    SingleTrack_WithIJ singleTrack = (SingleTrack_WithIJ)con;
                    singleTrack.ID = con.Name.Split('_')[1] + "_" + con.Name.Split('_')[2];
                    switch (con.Name.Split('_')[1])
                    {
                        case UPINTEVAL_2:
                            upBlockNum_2++;
                            break;
                        case DOWNINTEVAL_2:
                            downBlockNum_2++;
                            break;
                        case DOWNINTEVAL_1:
                            downBlockNum_1++;
                            break;
                        case UPINTEVAL_1:
                            upBlockNum_1++;
                            break;
                    }
                    intervalTracks.Add(con.Name, singleTrack);
                }
                else if (con.Name.Contains("Switch_3"))
                {
                    Console.WriteLine(con.Name);
                    Switch_3 switch_3 = (Switch_3)con;
                    string[] str = con.Name.Split('_');
                    switch_3.道岔ID号 = str[1];
                    switches.Add(con.Name, switch_3);
                }
                else if (con.Name.Contains("Switch_2"))
                {
                    // Console.WriteLine(con.Name);
                    Switch_2 switch_2 = (Switch_2)con;
                    string[] str = con.Name.Split('_');
                    switches.Add(con.Name, switch_2);
                }
                else if (con.Name.Contains("Xinhaoji"))
                {
                    // Console.WriteLine(con.Name);
                    Signal xinhaoji = (Signal)con;
                    signals.Add(con.Name, xinhaoji);
                }
            }
            foreach (var item in stationTracks)
            {
                // stationTracks是个哈希表，打印一下它的内容
                DictionaryEntry entry = (DictionaryEntry)item;
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }
        /// <summary>
        /// 为所有区间轨道添加码序显示控件（CodeOrder）
        /// 码序是铁路信号系统中用于显示区间轨道占用状态的信号显示，遵循"红黄-黄-黄绿-绿"的码序规则
        /// 该函数在界面初始化时调用，为每个区间轨道创建并配置对应的码序显示控件
        /// </summary>
        private void addCodeOrders()
        {
            // 遍历所有区间轨道控件（intervalTracks字典中存储的所有区间轨道）
            // 区间轨道名称格式：QujianTrack_方向_编号，例如：QujianTrack_S5_12、QujianTrack_X4_18
            foreach (string name in intervalTracks.Keys)
            {
                // 解析区间轨道名称，按'_'分割
                // str[0] = "QujianTrack"
                // str[1] = 方向标识（"S5"上行区间2、"X5"下行区间2、"S4"上行区间1、"X4"下行区间1）
                // str[2] = 轨道编号（从1开始的序号）
                string[] str = name.Split('_');
                
                // 创建码序显示控件实例
                CodeOrder co = new CodeOrder();
                
                // 设置码序控件的位置：
                // X坐标：区间轨道控件的中心位置（轨道X坐标 + 轨道宽度的一半 - 码序控件宽度的一半）
                // Y坐标：区间轨道控件的上方3像素处（轨道Y坐标 - 3）
                // 这样码序显示控件会显示在对应区间轨道的正上方居中位置
                co.Location = new System.Drawing.Point(
                    Convert.ToInt32(((SingleTrack_WithIJ)intervalTracks[name]).Location.X) +
                    ((SingleTrack_WithIJ)intervalTracks[name]).Width / 2 - co.Width / 2,
                    Convert.ToInt32(((SingleTrack_WithIJ)intervalTracks[name]).Location.Y) - 3);
                
                // 根据区间轨道的方向标识，设置码序控件的方向和初始显示状态
                switch (str[1])
                {
                    case UPINTEVAL_2:  // 上行区间2（S5）
                        // 设置码序方向为上行
                        co.上下行方向 = CodeOrder.Direction.上行;
                        
                        // 根据轨道编号与区间总轨道数的关系，设置码序显示状态
                        // 码序规则：从车站向外，依次为"红黄-黄-黄绿-绿"
                        // 最接近车站的轨道（编号最大，等于upBlockNum_2）显示"红黄"
                        if (Convert.ToInt32(str[2]) == upBlockNum_2)
                        {
                            co.显示状态 = CodeOrder.CodeView.红黄;  // 最接近车站，显示红黄码
                        }
                        // 倒数第二个轨道（upBlockNum_2 - 1）显示"黄"
                        else if (Convert.ToInt32(str[2]) == (upBlockNum_2 - 1))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄;  // 第二接近车站，显示黄码
                        }
                        // 倒数第三个轨道（upBlockNum_2 - 2）显示"黄绿"
                        else if (Convert.ToInt32(str[2]) == (upBlockNum_2 - 2))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄绿;  // 第三接近车站，显示黄绿码
                        }
                        // 其他更远的轨道显示"绿"
                        else
                        {
                            co.显示状态 = CodeOrder.CodeView.绿;  // 远离车站，显示绿码
                        }
                        break;
                        
                    case DOWNINTEVAL_2:  // 下行区间2（X5）
                        // 设置码序方向为下行
                        co.上下行方向 = CodeOrder.Direction.下行;
                        
                        // 下行区间2的码序显示逻辑与上行区间2相同
                        // 最接近车站的轨道（编号最大，等于downBlockNum_2）显示"红黄"
                        if (Convert.ToInt32(str[2]) == downBlockNum_2)
                        {
                            co.显示状态 = CodeOrder.CodeView.红黄;
                        }
                        // 倒数第二个轨道显示"黄"
                        else if (Convert.ToInt32(str[2]) == (downBlockNum_2 - 1))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄;
                        }
                        // 倒数第三个轨道显示"黄绿"
                        else if (Convert.ToInt32(str[2]) == (downBlockNum_2 - 2))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄绿;
                        }
                        // 其他更远的轨道显示"绿"
                        else
                        {
                            co.显示状态 = CodeOrder.CodeView.绿;
                        }
                        break;
                        
                    case UPINTEVAL_1:  // 上行区间1（S4）
                        // 设置码序方向为上行
                        co.上下行方向 = CodeOrder.Direction.上行;
                        
                        // 上行区间1的码序显示逻辑
                        // 最接近车站的轨道（编号最大，等于upBlockNum_1）显示"红黄"
                        if (Convert.ToInt32(str[2]) == upBlockNum_1)
                        {
                            co.显示状态 = CodeOrder.CodeView.红黄;
                        }
                        // 倒数第二个轨道显示"黄"
                        else if (Convert.ToInt32(str[2]) == (upBlockNum_1 - 1))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄;
                        }
                        // 倒数第三个轨道显示"黄绿"
                        else if (Convert.ToInt32(str[2]) == (upBlockNum_1 - 2))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄绿;
                        }
                        // 其他更远的轨道显示"绿"
                        else
                        {
                            co.显示状态 = CodeOrder.CodeView.绿;
                        }
                        break;
                        
                    case DOWNINTEVAL_1:  // 下行区间1（X4）
                        // 设置码序方向为下行
                        co.上下行方向 = CodeOrder.Direction.下行;
                        
                        // 下行区间1的码序显示逻辑
                        // 最接近车站的轨道（编号最大，等于downBlockNum_1）显示"红黄"
                        if (Convert.ToInt32(str[2]) == downBlockNum_1)
                        {
                            co.显示状态 = CodeOrder.CodeView.红黄;
                        }
                        // 倒数第二个轨道显示"黄"
                        else if (Convert.ToInt32(str[2]) == (downBlockNum_1 - 1))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄;
                        }
                        // 倒数第三个轨道显示"黄绿"
                        else if (Convert.ToInt32(str[2]) == (downBlockNum_1 - 2))
                        {
                            co.显示状态 = CodeOrder.CodeView.黄绿;
                        }
                        // 其他更远的轨道显示"绿"
                        else
                        {
                            co.显示状态 = CodeOrder.CodeView.绿;
                        }
                        break;
                }
                
                // 将码序控件添加到窗体控件集合中，使其在界面上可见
                this.Controls.Add(co);
                
                // 将码序控件置于最前层，确保其显示在其他控件之上，不会被遮挡
                co.BringToFront();
                
                // 将码序控件添加到codeOrders字典中，键为区间轨道名称，值为码序控件实例
                // 这样后续可以通过区间轨道名称快速找到对应的码序控件，用于动态更新码序显示状态
                codeOrders.Add(name, co);
            }
        }

        private void addStateRount()
        {
            // 对qf_summary_dict进行赋值，添加key为各发车口，value为"空闲"，默认状态为空闲
            // FACHEKOU.Clear();
            // 假设发车口为G1-G6，对应股道编号可根据实际情况调整

            // 定义一个发车口信息类，包含发车口占用锁闭信息、区间占用状态信息、允许或禁止发车信息
            // public class DeparturePortInfo
            // {
            //     // 发车口占用锁闭信息（如"空闲"、"占用"、"锁闭"等）
            //     public string PortOccupyLockStatus { get; set; }
            //     // 区间占用状态信息（如"空闲"、"占用"）
            //     public string SectionOccupyStatus { get; set; }
            //     // 允许或禁止发车信息（如"允许发车"、"禁止发车"）
            //     public string AllowDepartureStatus { get; set; }
            // }

            // 定义一个字典，键为发车口名称，值为DeparturePortInfo对象

            // 示例：初始化一个发车口（如"发车口1"）
            // departurePortDict["发车口1"] = new DeparturePortInfo
            // {
            //     PortOccupyLockStatus = "空闲",
            //     SectionOccupyStatus = "空闲",
            //     AllowDepartureStatus = "允许发车"
            // };

            //添加发车口1 到 发车口8的初始化
            for (int i = 1; i <= 8; i++)
            {
                string portName = "发车口" + i;
                departurePortDict[portName] = new DeparturePortInfo
                {
                    PortOccupyLockStatus = "空闲",
                    SectionOccupyStatus = "空闲",
                    AllowDepartureStatus = "允许发车",
                    DepartureDirection = "无方向",

                };
            }
            // 打印departurePortDict的状态
            foreach (var kvp in departurePortDict)
            {
                var info = kvp.Value;
                Console.WriteLine($"发车口: {kvp.Key}, 占用锁闭状态: {info.PortOccupyLockStatus}, 区间占用状态: {info.SectionOccupyStatus}, 允许发车状态: {info.AllowDepartureStatus}");
            }


            // FACHEKOU.Add("G1X", "空闲");
            // FACHEKOU.Add("G2X", "空闲");
            // FACHEKOU.Add("G3X", "空闲");
            // FACHEKOU.Add("G4X", "空闲");
            // FACHEKOU.Add("G1S", "空闲");
            // FACHEKOU.Add("G2S", "空闲");
            // FACHEKOU.Add("G3S", "空闲");
            // FACHEKOU.Add("G4S", "空闲");
            // FACHEKOU.Clear();
            // 假设发车口为G1-G6，对应股道编号可根据实际情况调整
            // FACHEQUDUAN.Add("G1X", "空闲");
            // FACHEQUDUAN.Add("G2X", "空闲");
            // FACHEQUDUAN.Add("G3X", "空闲");
            // FACHEQUDUAN.Add("G4X", "空闲");
            // FACHEQUDUAN.Add("G1S", "空闲");
            // FACHEQUDUAN.Add("G2S", "空闲");
            // FACHEQUDUAN.Add("G3S", "空闲");
            // FACHEQUDUAN.Add("G4S", "空闲");
        }
        /// <summary>
        /// 锁闭进路函数：根据进路信息（Interlock_Info）建立并锁闭一条完整的进路
        /// 该函数会执行以下操作：
        /// 1. 从进路表中获取进路包含的所有设备（轨道、道岔等）
        /// 2. 设置信号机显示状态（开放信号）
        /// 3. 锁闭进路中的所有轨道（flag_zt = 2）
        /// 4. 设置道岔的定反位并锁闭道岔
        /// 5. 将进路添加到已建立进路列表（routes）中
        /// </summary>
        /// <param name="Interlock_Info">
        /// 进路标识字符串，格式为："StationTrack_G{股道号}&SingleTrack_{信号类型}"
        /// 
        /// 可能的取值（根据interlock.txt和handlePlan函数）：
        /// 
        /// 【下行方向进路】
        /// 1. 下行接车进路（正向）：
        ///    - "StationTrack_G1&SingleTrack_XJC"  (G1股道下行接车)
        ///    - "StationTrack_G2&SingleTrack_XJC"  (G2股道下行接车)
        ///    - "StationTrack_G3&SingleTrack_XJC"  (G3股道下行接车)
        ///    - "StationTrack_G4&SingleTrack_XJC"  (G4股道下行接车)
        ///    - "StationTrack_G5&SingleTrack_XJC"  (G5股道下行接车)
        ///    - "StationTrack_G6&SingleTrack_XJC"  (G6股道下行接车)
        /// 
        /// 2. 下行发车进路（正向）：
        ///    - "StationTrack_G1&SingleTrack_XFC"  (G1股道下行发车)
        ///    - "StationTrack_G2&SingleTrack_XFC"  (G2股道下行发车)
        ///    - "StationTrack_G3&SingleTrack_XFC"  (G3股道下行发车)
        ///    - "StationTrack_G4&SingleTrack_XFC"  (G4股道下行发车)
        ///    - "StationTrack_G5&SingleTrack_XFC"  (G5股道下行发车)
        ///    - "StationTrack_G6&SingleTrack_XFC"  (G6股道下行发车)
        /// 
        /// 3. 下行接车进路（反向，通过上行线接车）：
        ///    - "StationTrack_G1&SingleTrack_SFC"  (G1股道通过上行线反向接车)
        ///    - "StationTrack_G2&SingleTrack_SFC"  (G2股道通过上行线反向接车)
        ///    - "StationTrack_G3&SingleTrack_SFC"  (G3股道通过上行线反向接车)
        ///    - "StationTrack_G4&SingleTrack_SFC"  (G4股道通过上行线反向接车)
        ///    - "StationTrack_G5&SingleTrack_SFC"  (G5股道通过上行线反向接车)
        ///    - "StationTrack_G6&SingleTrack_SFC"  (G6股道通过上行线反向接车)
        /// 
        /// 4. 下行发车进路（反向，通过上行线发车）：
        ///    - "StationTrack_G1&SingleTrack_SJC"  (G1股道通过上行线反向发车)
        ///    - "StationTrack_G2&SingleTrack_SJC"  (G2股道通过上行线反向发车)
        ///    - "StationTrack_G3&SingleTrack_SJC"  (G3股道通过上行线反向发车)
        ///    - "StationTrack_G4&SingleTrack_SJC"  (G4股道通过上行线反向发车)
        ///    - "StationTrack_G5&SingleTrack_SJC"  (G5股道通过上行线反向发车)
        ///    - "StationTrack_G6&SingleTrack_SJC"  (G6股道通过上行线反向发车)
        /// 
        /// 【上行方向进路】
        /// 5. 上行接车进路（正向）：
        ///    - "StationTrack_G1&SingleTrack_SJC"  (G1股道上行接车)
        ///    - "StationTrack_G2&SingleTrack_SJC"  (G2股道上行接车)
        ///    - "StationTrack_G3&SingleTrack_SJC"  (G3股道上行接车)
        ///    - "StationTrack_G4&SingleTrack_SJC"  (G4股道上行接车)
        ///    - "StationTrack_G5&SingleTrack_SJC"  (G5股道上行接车)
        ///    - "StationTrack_G6&SingleTrack_SJC"  (G6股道上行接车)
        /// 
        /// 6. 上行发车进路（正向）：
        ///    - "StationTrack_G1&SingleTrack_SFC"  (G1股道上行发车)
        ///    - "StationTrack_G2&SingleTrack_SFC"  (G2股道上行发车)
        ///    - "StationTrack_G3&SingleTrack_SFC"  (G3股道上行发车)
        ///    - "StationTrack_G4&SingleTrack_SFC"  (G4股道上行发车)
        ///    - "StationTrack_G5&SingleTrack_SFC"  (G5股道上行发车)
        ///    - "StationTrack_G6&SingleTrack_SFC"  (G6股道上行发车)
        /// 
        /// 7. 上行接车进路（反向，通过下行线接车）：
        ///    - "StationTrack_G1&SingleTrack_XFC"  (G1股道通过下行线反向接车)
        ///    - "StationTrack_G2&SingleTrack_XFC"  (G2股道通过下行线反向接车)
        ///    - "StationTrack_G3&SingleTrack_XFC"  (G3股道通过下行线反向接车)
        ///    - "StationTrack_G4&SingleTrack_XFC"  (G4股道通过下行线反向接车)
        ///    - "StationTrack_G5&SingleTrack_XFC"  (G5股道通过下行线反向接车)
        ///    - "StationTrack_G6&SingleTrack_XFC"  (G6股道通过下行线反向接车)
        /// 
        /// 8. 上行发车进路（反向，通过下行线发车）：
        ///    - "StationTrack_G1&SingleTrack_XJC"  (G1股道通过下行线反向发车)
        ///    - "StationTrack_G2&SingleTrack_XJC"  (G2股道通过下行线反向发车)
        ///    - "StationTrack_G3&SingleTrack_XJC"  (G3股道通过下行线反向发车)
        ///    - "StationTrack_G4&SingleTrack_XJC"  (G4股道通过下行线反向发车)
        ///    - "StationTrack_G5&SingleTrack_XJC"  (G5股道通过下行线反向发车)
        ///    - "StationTrack_G6&SingleTrack_XJC"  (G6股道通过下行线反向发车)
        /// 
        /// 信号类型说明：
        /// - XJC: 下行接车（下行方向接车进路）
        /// - XFC: 下行发车（下行方向发车进路）
        /// - SJC: 上行接车（上行方向接车进路）
        /// - SFC: 上行发车（上行方向发车进路）
        /// </param>
        private void lockRoute(string Interlock_Info)
        {
            // 检查进路信息是否为空
            if (Interlock_Info != null)
            {
                // 从interlocks字典中获取该进路对应的进路表字符串
                // 进路表字符串格式：用'&'分隔的多个设备信息，每个设备信息格式为"设备名#状态"或"设备名#状态%方向"
                // 例如："StationTrack_G1#FREE&SingleTrack_L1#FREE&Switch_3_G13_L#D&Switch_2_G12_L#F%~&SingleTrack_SFC#FREE"
                string Interlock_str = (string)interlocks[Interlock_Info];
                string[] str;
                
                // 如果找到了对应的进路表
                if (Interlock_str != null)
                {
                    // 将进路表字符串按'&'分割，得到进路中包含的所有设备信息数组
                    str = Interlock_str.Split('&');
                    
                    // 将进路标识字符串按'&'分割，得到进路的起点和终点
                    // side[0] = 起点股道（如"StationTrack_G1"）
                    // side[1] = 终点信号机类型（如"SingleTrack_SFC"）
                    string[] side = Interlock_Info.Split('&');
                    
                    // 将股道名称转换为信号机名称（用于查找信号机控件）
                    // 例如："StationTrack_G1" -> "Xinhaoji_G1"
                    side[0] = side[0].Replace("StationTrack", "Xinhaoji");
                    
                    // 将信号机类型名称转换为信号机名称（用于查找信号机控件）
                    // 例如："SingleTrack_SFC" -> "Xinhaoji_SFC"
                    side[1] = side[1].Replace("SingleTrack", "Xinhaoji");
                    
                    // 提取进路标识字符串的最后3个字符，用于判断进路类型（SFC、SJC、XFC、XJC）
                    // 这3个字符表示进路的类型：上行发车、上行接车、下行发车、下行接车
                    string JFC_state = Interlock_Info.Substring(Interlock_Info.Length - 3, 3);
                    
                    // 根据进路类型设置信号机的显示状态
                    switch (JFC_state)
                    {
                        // case "L17":  // 下行接车（下行方向接车进路）
                        case "XJC":  // 下行接车（下行方向接车进路）   左上接车
                            // 判断是否为G1或G2股道
                            if (side[0].Contains("1") || side[0].Contains("2"))
                            {
                                // 如果是直通通过，显示开放信号（状态3）
                                // 否则显示黄灯（状态1，表示接车进路开放但需要减速）
                                if (Zhijietongguo)
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 3;  // 开放信号
                                }
                                else
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 1;  // 黄灯（减速）
                                }
                            }
                            else
                            {
                                // 其他股道的下行接车进路显示双黄灯（状态2）
                                ((Signal)signals[side[1]]).信号灯状态 = 2;  // 双黄灯
                            }
                            break;
                        
                        case "XFFC":  // 下行反向发车 左上发车
                        // case "L35": 

                            // 为股道信号机名称添加"_S"后缀，表示上行方向
                            // 例如："Xinhaoji_G1" -> "Xinhaoji_G1_S"
                            side[0] = side[0] + "_S";
                            // 设置股道信号机状态为3（开放信号，允许发车）
                            ((Signal)signals[side[0]]).信号灯状态 = 3;
                            break;

                        case "SFC":  // 上行发车（上行方向发车进路） 左下发车
                        // case "L35":  // 上行发车（上行方向发车进路）

                            // 为股道信号机名称添加"_S"后缀，表示上行方向
                            // 例如："Xinhaoji_G1" -> "Xinhaoji_G1_S"
                            side[0] = side[0] + "_S";
                            // 设置股道信号机状态为3（开放信号，允许发车）
                            ((Signal)signals[side[0]]).信号灯状态 = 3;
                            break;

                        case "SFJC":  // 上行反向接车            左下接车
                            side[1] = side[1].Replace("SFJC", "SFC");
                            if (side[0].Contains("1") || side[0].Contains("2"))
                            {
                                // 如果是直通通过，显示开放信号（状态3）
                                // 否则显示黄灯（状态1，表示接车进路开放但需要减速）
                                if (Zhijietongguo)
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 3;  // 开放信号
                                }
                                else
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 1;  // 黄灯（减速）
                                }
                            }
                            else
                            {
                                // 其他股道的下行接车进路显示双黄灯（状态2）
                                ((Signal)signals[side[1]]).信号灯状态 = 2;  // 双黄灯
                            }
                            break;
                        
                        case "XFJC":  // 下行反向接车             右上接车         
                            side[1] = side[1].Replace("XFJC", "XFC");
                            if (side[0].Contains("1") || side[0].Contains("2"))
                            {
                                // 如果是直通通过，显示开放信号（状态3）
                                // 否则显示黄灯（状态1，表示接车进路开放但需要减速）
                                if (Zhijietongguo)
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 3;  // 开放信号
                                }
                                else
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 1;  // 黄灯（减速）
                                }
                            }
                            else
                            {
                                // 其他股道的下行接车进路显示双黄灯（状态2）
                                ((Signal)signals[side[1]]).信号灯状态 = 2;  // 双黄灯
                            }
                            break;

                            // case "L28":  // 下行发车（下行方向发车进路）
                        case "XFC":  // 下行发车（下行方向发车进路）  右上发车
                            // 为股道信号机名称添加"_X"后缀，表示下行方向
                            // 例如："Xinhaoji_G1" -> "Xinhaoji_G1_X"
                            side[0] = side[0] + "_X";
                            // 设置股道信号机状态为3（开放信号，允许发车）
                            ((Signal)signals[side[0]]).信号灯状态 = 3;
                            break;
                        
                        

                        case "SJC":  // 上行接车 右下接车
                        // case "L46":  // 上行接车（上行方向接车进路）
                            // 判断是否为G1或G2股道（这些股道可能有特殊的信号显示逻辑）
                            if (side[0].Contains("1") || side[0].Contains("2"))
                            {
                                // 如果是直通通过（Zhijietongguo = true），显示开放信号（状态3）
                                // 否则显示黄灯（状态1，表示接车进路开放但需要减速）
                                if (Zhijietongguo)
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 3;  // 开放信号
                                }
                                else
                                {
                                    ((Signal)signals[side[1]]).信号灯状态 = 1;  // 黄灯（减速）
                                }
                            }
                            else
                            {
                                // 其他股道的上行接车进路显示双黄灯（状态2，表示接车进路开放）
                                ((Signal)signals[side[1]]).信号灯状态 = 2;  // 双黄灯
                            }
                            break;
                            


                        case "SFFC":  // 上行反向发车 右下发车
                            // 为股道信号机名称添加"_X"后缀，表示下行方向
                            // 例如："Xinhaoji_G1" -> "Xinhaoji_G1_X"
                            side[0] = side[0] + "_X";
                            // 设置股道信号机状态为3（开放信号，允许发车）
                            ((Signal)signals[side[0]]).信号灯状态 = 3;
                            break;
                            
                        
                       
                        
                    
                    }
                    
                    // 遍历进路表中的所有设备，对每个设备进行锁闭操作
                    foreach (string tempName in str)
                    {
                        // 提取设备名称（去掉状态信息）
                        // tempName格式可能是："StationTrack_G1#FREE" 或 "Switch_3_G13_L#D" 或 "Switch_2_G12_L#F%~"
                        // name = "StationTrack_G1" 或 "Switch_3_G13_L" 或 "Switch_2_G12_L"
                        string name;
                        name = tempName.Split('#')[0];
                        
                        // 处理站内股道（StationTrack）
                        if (name.Contains("StationTrack"))
                        {
                            // 检查股道控件是否存在
                            if (((SingleTrack_WithIJ)stationTracks[name]) != null)
                            {
                                // 如果股道当前不是被列车占用状态（flag_zt != 1），则将其锁闭（flag_zt = 2）
                                // flag_zt = 1 表示被列车占用，不能改变状态
                                // flag_zt = 2 表示被进路锁闭
                                // flag_zt = 3 表示空闲
                                if (((SingleTrack_WithIJ)stationTracks[name]).flag_zt != 1)
                                {
                                    // 设置股道状态为锁闭（flag_zt = 2）
                                    ((SingleTrack_WithIJ)stationTracks[name]).flag_zt = 2;
                                    // 重绘股道控件，更新显示状态
                                    ((SingleTrack_WithIJ)stationTracks[name]).Drawpic();
                                }
                            }
                        }
                        // 处理单线轨道（SingleTrack，如连接线、信号机间的轨道）
                        else if (name.Contains("SingleTrack"))
                        {
                            // 检查单线轨道控件是否存在
                            if (((SingleTrack_WithIJ)singleTracks[name]) != null)
                            {
                                // 直接设置单线轨道状态为锁闭（flag_zt = 2）
                                ((SingleTrack_WithIJ)singleTracks[name]).flag_zt = 2;
                                // 重绘单线轨道控件，更新显示状态
                                ((SingleTrack_WithIJ)singleTracks[name]).Drawpic();
                            }
                        }
                        // 处理三开道岔（Switch_3）
                        else if (name.Contains("Switch_3"))
                        {
                            // 检查三开道岔控件是否存在
                            if (((Switch_3)switches[name]) != null)
                            {
                                // 从tempName中提取道岔位置信息
                                // tempName格式："Switch_3_G13_L#D" 或 "Switch_3_G13_L#F"
                                // tempName.Split('#')[1] = "D"（定位）或 "F"（反位）
                                bool dir = tempName.Split('#')[1] == "D";
                                
                                // 根据进路要求设置道岔的定反位
                                if (dir)
                                {
                                    // 设置为定位
                                    ((Switch_3)switches[name]).定反位 = Switch_3.DingFan.定位;
                                }
                                else
                                {
                                    // 设置为反位
                                    ((Switch_3)switches[name]).定反位 = Switch_3.DingFan.反位;
                                }
                                
                                // 锁闭道岔（防止道岔在进路使用期间被操作）
                                ((Switch_3)switches[name]).锁闭状态 = Switch_3.STATE.锁闭;
                                
                                // 将道岔控件置于最底层，确保轨道显示在道岔之上
                                ((Switch_3)switches[name]).SendToBack();
                                
                                dir = false;  // 重置临时变量
                            }
                        }
                        // 处理双开道岔（Switch_2）
                        else
                        {
                            // 从tempName中提取道岔的方向和位置信息
                            // tempName格式："Switch_2_G12_L#F%~" 或 "Switch_2_G12_L#D%S" 或 "Switch_2_G12_L#D%X"
                            // direction = "~"（无方向）或 "S"（上行）或 "X"（下行）
                            // information = "F%~" 或 "D%S" 或 "D%X"
                            string direction;
                            string information;
                            direction = tempName.Split('%')[1];  // 提取方向信息
                            information = tempName.Split('#')[1];  // 提取位置和方向组合信息
                            
                            // 检查双开道岔控件是否存在
                            if (((Switch_2)switches[name]) != null)
                            {
                                // 判断道岔是否为反位（information.Split('%')[0] == "F"）
                                if (information.Split('%')[0] == "F")
                                {
                                    // 设置为反位
                                    ((Switch_2)switches[name]).定反位 = ConLib.Switch_2.DingFan.反位;
                                    // 锁闭道岔的反位
                                    ((Switch_2)switches[name]).反位锁闭状态 = ConLib.Switch_2.STATE.锁闭;
                                }
                                else
                                {
                                    // 设置为定位
                                    ((Switch_2)switches[name]).定反位 = ConLib.Switch_2.DingFan.定位;
                                    
                                    // 根据进路方向锁闭道岔的相应方向
                                    if (direction == "S")
                                    {
                                        // 锁闭道岔的上行方向
                                        ((Switch_2)switches[name]).锁闭状态上 = ConLib.Switch_2.STATE.锁闭;
                                    }
                                    else
                                    {
                                        // 锁闭道岔的下行方向
                                        ((Switch_2)switches[name]).锁闭状态下 = ConLib.Switch_2.STATE.锁闭;
                                    }
                                }
                            }
                        }
                    }
                    
                    // 将已建立的进路添加到routes列表中，用于后续的进路管理和释放
                    routes.Add(Interlock_Info);
                }
            }
        }
        private void lockRoute_multi(string Interlock_Info)
        {
            if (Interlock_Info != null)
            {
                string Interlock_str = (string)interlocks[Interlock_Info];
                string[] str;
                if (Interlock_str != null)
                {
                    str = Interlock_str.Split('&');
                    foreach (string tempName in str)
                    {
                        string name;
                        name = tempName.Split('#')[0];
                        if (name.Contains("StationTrack"))
                        {
                            if (((SingleTrack_WithIJ)stationTracks[name]) == null)
                            {
                                MessageBox.Show("这里没有:" + name);
                            }

                        }
                        else if (name.Contains("SingleTrack"))
                        {
                            if (((SingleTrack_WithIJ)singleTracks[name]) == null)
                            {
                                MessageBox.Show("这里没有:" + name);
                            }
                        }
                        else if (name.Contains("Switch_3"))
                        {
                            if (((Switch_3)switches[name]) != null)
                            {
                                bool dir = tempName.Split('#')[1] == "D";
                                if (dir)
                                {
                                    ((Switch_3)switches[name]).定反位 = Switch_3.DingFan.定位;
                                }
                                else
                                {
                                    ((Switch_3)switches[name]).定反位 = Switch_3.DingFan.反位;
                                }
                                ((Switch_3)switches[name]).SendToBack();
                                dir = false;
                            }
                            else
                            {
                                MessageBox.Show("这里没有:" + name);
                            }

                        }
                        else
                        {
                            string direction;
                            string information;
                            direction = tempName.Split('%')[1];
                            information = tempName.Split('#')[1];
                            if (((Switch_2)switches[name]) != null)
                            {
                                if (information.Split('%')[0] == "F")
                                {
                                    ((Switch_2)switches[name]).定反位 = ConLib.Switch_2.DingFan.反位;
                                }
                                else
                                {
                                    ((Switch_2)switches[name]).定反位 = ConLib.Switch_2.DingFan.定位;
                                }
                            }
                            else
                            {
                                MessageBox.Show("这里没有:" + name);
                            }
                        }

                    }
                    routes.Add(Interlock_Info);
                }
            }

        }
        private void clearRoute(string Interlock_Info)
        {
            if (Interlock_Info != null)
            {
                string Interlock_str = (string)interlocks[Interlock_Info];
                string[] str;
                if (Interlock_str != null)
                {
                    str = Interlock_str.Split('&');
                    foreach (string tempName in str)
                    {
                        string name;
                        name = tempName.Split('#')[0];
                        if (name.Contains("StationTrack"))
                        {
                            if (((SingleTrack_WithIJ)stationTracks[name]) != null)
                            {
                                if (((SingleTrack_WithIJ)stationTracks[name]).flag_zt == 2)
                                {
                                    string s = name.Replace("StationTrack", "Xinhaoji");
                                    if (Interlock_Info.Split('&')[1].Split('_')[1] == "SFC" ||
                                        Interlock_Info.Split('&')[1].Split('_')[1] == "SJC")
                                    {
                                        s = s + "_S";
                                    }
                                    else
                                    {
                                        s = s + "_X";
                                    }
                                    ((Signal)signals[s]).信号灯状态 = 5;
                                }
                                if (((SingleTrack_WithIJ)stationTracks[name]).flag_zt != 1)
                                {
                                    ((SingleTrack_WithIJ)stationTracks[name]).flag_zt = 3;
                                    ((SingleTrack_WithIJ)stationTracks[name]).Drawpic();
                                }
                            }

                        }
                        else if (name.Contains("SingleTrack"))
                        {
                            if (((SingleTrack_WithIJ)singleTracks[name]) != null)
                            {
                                ((SingleTrack_WithIJ)singleTracks[name]).flag_zt = 3;
                                ((SingleTrack_WithIJ)singleTracks[name]).Drawpic();
                                if (name.Split('_')[1].Contains("XFC") || name.Split('_')[1].Contains("XJC")
                                    || name.Split('_')[1].Contains("SFC") || name.Split('_')[1].Contains("SJC"))
                                {
                                    string s = name.Replace("SingleTrack", "Xinhaoji");
                                    ((Signal)signals[s]).信号灯状态 = 5;
                                }
                            }
                        }
                        else if (name.Contains("Switch_3"))
                        {
                            ((Switch_3)switches[name]).定反位 = Switch_3.DingFan.定位;
                            ((Switch_3)switches[name]).锁闭状态 = Switch_3.STATE.空闲;
                        }
                        else
                        {
                            string direction;
                            string information;
                            direction = tempName.Split('%')[1];
                            information = tempName.Split('#')[1];

                            if (((Switch_2)switches[name]) != null)
                            {
                                if (information.Split('%')[0] == "F")
                                {
                                    ((Switch_2)switches[name]).反位锁闭状态 = ConLib.Switch_2.STATE.空闲;
                                }
                                else
                                {
                                    if (direction == "S")
                                    {
                                        ((Switch_2)switches[name]).锁闭状态上 = ConLib.Switch_2.STATE.空闲;
                                    }
                                    else
                                    {
                                        ((Switch_2)switches[name]).锁闭状态下 = ConLib.Switch_2.STATE.空闲;
                                    }
                                }
                            }
                        }
                    }
                    routes.Remove(Interlock_Info);
                }
            }
        }
        /// <summary>
        /// 检查进路冲突函数：判断要建立的进路是否与现有已锁闭的进路存在冲突
        /// 该函数遍历待建立进路中的所有设备（轨道、道岔），检查每个设备的状态是否空闲
        /// 如果任何设备被占用或锁闭，说明存在冲突，返回false；如果所有设备都空闲，返回true
        /// </summary>
        /// <param name="key">
        /// 进路标识键，格式为："{车次号}{车站代码}{到发类型}{方向}"
        /// 格式说明：
        ///   - 车次号：列车车次，如 "G1234"
        ///   - 车站代码：STOPSTATION常量，值为 "NJ"（南京南站）
        ///   - 到发类型：ARRIVE常量（"Ar"）表示接车，DEPARTURE常量（"De"）表示发车
        ///   - 方向：DOWN_DIRECTION常量（"下"）表示下行，UP_DIRECTION常量（"上"）表示上行
        /// 
        /// 示例：
        ///   - "G1234NJAr下"  - G1234次列车在南京南站下行接车进路
        ///   - "G1234NJDe上"  - G1234次列车在南京南站上行发车进路
        ///   - "G5678NJAr下"  - G5678次列车在南京南站下行接车进路
        /// 
        /// 该参数主要用于判断进路类型（接车还是发车），对接车进路有特殊的冲突检查逻辑
        /// </param>
        /// <param name="Interlock_Info">
        /// 进路信息标识字符串，格式为："StationTrack_G{股道号}&SingleTrack_{信号类型}"
        /// 格式说明：
        ///   - StationTrack_G{股道号}：起点股道，如 "StationTrack_G1"、"StationTrack_G3"
        ///   - SingleTrack_{信号类型}：终点信号机类型，表示进路类型
        ///     - "SingleTrack_XJC"：下行接车（下行方向接车进路）
        ///     - "SingleTrack_XFC"：下行发车（下行方向发车进路）
        ///     - "SingleTrack_SJC"：上行接车（上行方向接车进路）
        ///     - "SingleTrack_SFC"：上行发车（上行方向发车进路）
        ///     - "SingleTrack_L17"、"SingleTrack_L28"、"SingleTrack_L35"、"SingleTrack_L46"：连接线轨道
        /// 
        /// 示例：
        ///   - "StationTrack_G1&SingleTrack_XJC"  - G1股道下行接车进路
        ///   - "StationTrack_G3&SingleTrack_SFC"  - G3股道上行发车进路
        ///   - "StationTrack_G2&SingleTrack_L17"  - G2股道到L17连接线的进路
        ///   - "StationTrack_G4&SingleTrack_XFC"  - G4股道下行发车进路
        /// 
        /// 该参数用于从interlocks字典中查找对应的进路表，获取进路中包含的所有设备信息
        /// </param>
        /// <returns>
        /// bool类型返回值：
        ///   - true：进路无冲突，可以建立（所有设备都空闲）
        ///   - false：进路存在冲突，不能建立（至少有一个设备被占用或锁闭）
        /// </returns>
        private bool isConflict(string key, string Interlock_Info) //key G1234NJAr下
        {
            // 初始化冲突检查结果为"允许建立"（true表示无冲突）
            bool Admit = true;
            // Interlock_Info表示 输入到interlocks（txt）字典中的键值对 StationTrack_G1&SingleTrack_XJC
            // 检查进路信息是否为空
            if (Interlock_Info != null)
            {
                // 从interlocks字典中获取该进路对应的进路表字符串
                // 进路表字符串格式：用'&'分隔的多个设备信息
                // 例如："StationTrack_G1#FREE&SingleTrack_L1#FREE&Switch_3_G13_L#D&Switch_2_G12_L#F%~&SingleTrack_SFC#FREE"
                string Interlock_str = (string)interlocks[Interlock_Info];
                string[] str;
                
                // 如果找到了对应的进路表
                if (Interlock_str != null)
                {
                    // 将进路表字符串按'&'分割，得到进路中包含的所有设备信息数组
                    str = Interlock_str.Split('&');
                    
                    // 遍历进路表中的每个设备，检查其状态是否空闲
                    foreach (string tempName in str)
                    {
                        // 提取设备名称（去掉状态信息）
                        // tempName格式："StationTrack_G1#FREE" 或 "Switch_3_G13_L#D"
                        // name = "StationTrack_G1" 或 "Switch_3_G13_L"
                        string name;
                        name = tempName.Split('#')[0];
                        
                        // 检查站内股道（StationTrack）
                        if (name.Contains("StationTrack"))
                        {
                            // 检查股道控件是否存在
                            if (((SingleTrack_WithIJ)stationTracks[name]) != null)
                            {
                                // 如果股道状态不是空闲（flag_zt != 3）
                                // flag_zt = 1 表示被列车占用
                                // flag_zt = 2 表示被进路锁闭
                                // flag_zt = 3 表示空闲
                                if (((SingleTrack_WithIJ)stationTracks[name]).flag_zt != 3)
                                {
                                    // 如果是接车进路（key中包含"Ar"），则判定为冲突
                                    // 接车进路对股道占用更敏感，即使股道被占用也不能建立接车进路
                                    if (key.Contains(ARRIVE))
                                    {
                                        Admit = false;  // 发现冲突，标记为不允许建立
                                        break;          // 立即退出循环，不再检查其他设备
                                    }
                                    // 注意：发车进路（DEPARTURE）如果股道被占用，可能是正常的（列车在股道上准备发车）
                                    // 所以发车进路不在这里判定冲突，允许在占用股道上建立发车进路
                                }
                            }
                        }
                        // 检查单线轨道（SingleTrack，如连接线、信号机间的轨道）
                        else if (name.Contains("SingleTrack"))
                        {
                            // 检查单线轨道控件是否存在
                            if (((SingleTrack_WithIJ)singleTracks[name]) != null)
                            {
                                // 如果单线轨道状态不是空闲（flag_zt != 3），判定为冲突
                                // 单线轨道无论是接车还是发车进路，都必须空闲才能使用
                                if (((SingleTrack_WithIJ)singleTracks[name]).flag_zt != 3)
                                {
                                    Admit = false;  // 发现冲突
                                    break;          // 立即退出
                                }
                            }
                        }
                        // 检查三开道岔（Switch_3）
                        else if (name.Contains("Switch_3"))
                        {
                            // 检查三开道岔控件是否存在
                            if (((Switch_3)switches[name]) != null)
                            {
                                // 如果道岔锁闭状态不是空闲，判定为冲突
                                // 道岔必须完全空闲（未被任何进路锁闭）才能用于新进路
                                if (((Switch_3)switches[name]).锁闭状态 != Switch_3.STATE.空闲)
                                {
                                    Admit = false;  // 发现冲突
                                    break;          // 立即退出
                                }
                            }
                        }
                        // 检查双开道岔（Switch_2）
                        else
                        {
                            // 从tempName中提取道岔的方向和位置信息
                            // tempName格式："Switch_2_G12_L#F%~" 或 "Switch_2_G12_L#D%S" 或 "Switch_2_G12_L#D%X"
                            // direction = "~"（无方向）或 "S"（上行）或 "X"（下行）
                            // information = "F%~" 或 "D%S" 或 "D%X"
                            string direction;
                            string information;
                            direction = tempName.Split('%')[1];      // 提取方向信息
                            information = tempName.Split('#')[1];    // 提取位置和方向组合信息
                            
                            // 检查双开道岔控件是否存在
                            if (((Switch_2)switches[name]) != null)
                            {
                                // 判断道岔是否为反位（information.Split('%')[0] == "F"）
                                if (information.Split('%')[0] == "F")
                                {
                                    // 如果进路要求道岔在反位，检查道岔的所有锁闭状态
                                    // 双开道岔在反位时，需要检查反位锁闭状态、上行锁闭状态、下行锁闭状态
                                    // 只要有一个方向被锁闭，就不能用于新进路
                                    if (((Switch_2)switches[name]).反位锁闭状态 != Switch_2.STATE.空闲 ||
                                        ((Switch_2)switches[name]).锁闭状态上 != Switch_2.STATE.空闲 ||
                                        ((Switch_2)switches[name]).锁闭状态下 != Switch_2.STATE.空闲)
                                    {
                                        Admit = false;  // 发现冲突
                                        break;          // 立即退出
                                    }
                                }
                                else
                                {
                                    // 如果进路要求道岔在定位，根据进路方向检查相应方向的锁闭状态
                                    if (direction == "S")
                                    {
                                        // 上行方向进路，检查道岔的上行锁闭状态
                                        if (((Switch_2)switches[name]).锁闭状态上 != ConLib.Switch_2.STATE.空闲)
                                        {
                                            Admit = false;  // 发现冲突
                                            break;          // 立即退出
                                        }
                                    }
                                    else
                                    {
                                        // 下行方向进路，检查道岔的下行锁闭状态
                                        if (((Switch_2)switches[name]).锁闭状态下 != ConLib.Switch_2.STATE.空闲)
                                        {
                                            Admit = false;  // 发现冲突
                                            break;          // 立即退出
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // 返回冲突检查结果
            // true：无冲突，可以建立进路
            // false：有冲突，不能建立进路
            return Admit;
        }
        /// <summary>
        /// 该线程函数用于循环监听并接收UDP消息：
        /// - 每当成功收到一条消息（server.ReceiveFrom返回数据），会输出"aws"，然后将消息分发到主线程处理（maincontext.Post）。
        /// - 如果没有收到消息（即ReceiveFrom阻塞在等待），则不会输出"aws"，更不会处理任何消息。
        /// - 只有接收到消息时，才会依次执行打印、解析与分发逻辑。收不到消息时本线程会一直处于阻塞等待状态，不会有任何输出和动作。
        /// </summary>
        public void receivevMessage()
        {
            while (true)
            {
                try
                {                   
                     Console.WriteLine("w");

                    EndPoint point = new IPEndPoint(IPAddress.Any, 0); // 保存发送方的ip和端口号
                    byte[] buffer = new byte[1024 * 1024];
                    int length = server.ReceiveFrom(buffer, ref point); // 阻塞等待接收数据报
                    if (length == 0)
                    {
                        break;
                    }
                    string message = Encoding.ASCII.GetString(buffer, 0, length);
                    if (message != null)
                    {
                        // 这里maincontext.Post会将onConnect方法委托到主线程（UI线程）执行，这样做的好处是可以安全操作UI控件，避免多线程同时访问界面带来的线程安全问题。
                        // 如果你直接用 new Thread(onConnect).Start(message)，onConnect会在子线程里执行，若在onConnect中访问或更新了窗体控件，会导致程序异常（如“线程间操作无效”）。
                        // 因此，maincontext.Post传到主线程是必须的，只要onConnect里会处理UI相关的内容。
                        maincontext.Post(new SendOrPostCallback(onConnect), message);  // 返回主线程运行onConnect，保障UI线程安全
                    }
                }
                catch (Exception ex)
                { }
            }
        }
        private void sendMessage()
        {

            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("as");


                    // string message = "1|TCC|G1234|K+1500|1|14:30:00";
                    // server.SendTo(Encoding.UTF8.GetBytes(message), point);

                    // 消息格式："10|Train_ID|Train_ID|STOPSTATION|2|7|1"
                    // 消息含义说明：
                    //   - "10"：消息类型标识，表示进路许可消息
                    //   - Train_ID（第1个）：车次号，如 "G1234"
                    //   - Train_ID（第2个）：车次号重复（协议要求）
                    //   - STOPSTATION：车站代码，值为 "NJ"（南京南站）
                    //   - "2"：进路类型标识
                    //     * "0" = 定时进路（按计划时间触发）
                    //     * "1" = 接车进路（列车到达进路）
                    //     * "2" = 发车进路（列车出发进路）← 当前消息类型
                    //   - "7"：授权级别，表示正常授权（最高安全级别）
                    //   - "1"：状态标识，表示消息有效/进路已建立
                    
                    // sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                    //                 "|" + Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));

                    if (TRAIN_ROUTE.Count == 1)
                    {
                        // 取出TRAIN_ROUTE字典中的唯一键值对
                        foreach (var kvp in TRAIN_ROUTE)
                        {
                            var TRAIN_ID = kvp.Key;
                            var JINLUSTTE = kvp.Value;
                            // 这里可以根据需求进一步处理onlyKey和onlyValue
                            break; // 只取第一个即可
                        }
                    }

                    if (JINLUSTTE == '0')
                    {
                        sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + TRAIN_ID +
                                    "|" + TRAIN_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                    }
                    else
                    {
                        sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + TRAIN_ID +
                                    "|" + TRAIN_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                    }
                    // sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + "G1234" + "|" +
                    //                  "G1234" + "|" + "NJ" + "|" + "2" + "|" + "7" + "|" + "1"));

                }
                catch (Exception ex)
                { }
            }
        }
        private void onConnect(object mess)
        {
            string message = (string)mess;
            string messType = message.Split('|')[0];
            switch (messType)
            {
                case TCCM:
                    tccHandle(message.Split('|')[1]);
                    break;
                case POSTION:
                    positionHandle(message);
                    break;
                case PLAN:
                    handlePlan(message);
                    break;
                case DELETE:  // 消息类型 "3"：处理删除列车消息
                    // 调用 handledelete() 函数从系统中移除列车及其所有相关信息
                    // 消息格式："3|车次号"
                    handledelete(message);
                    break;
                case TIME:
                    handleTime(message);
                    break;
                case ACCIDENT:  // 消息类型 "10"：处理事故/紧急情况下的进路建立
                    // 调用 handleAccident() 函数立即建立进路，不等待时间条件
                    // 消息格式："10|进路路径|车次+方向|车次号|进路类型|时间信息"
                    handleAccident(message);
                    break;
                case DC:
                    if (mode_interlock == "normal")
                    {
                        mode_interlock = "novel";
                        textBox1.Text = "多车";
                    }
                    else if (mode_interlock == "novel")
                    {
                        mode_interlock = "normal";
                        textBox1.Text = "单车";
                    }
                    break;
            }
        }


        private void tccHandle(string message)
        {
            string m;
            Console.WriteLine("TCC RECEVIE!!!");

            m = DataConvert.CBIToTCC(message);

            // m变量的结构: "Train_ID|Train_Direction|qf_summary|qb_summary|sp_summary|xr_summary|oi_summary|qd_summary"
            // 这里我们提取qf_summary（m的第3段）并存入字典变量，再做更新（如需）。
            // Dictionary<string, string> qf_summary_dict = new Dictionary<string, string>();
            // try
            // {
            //     var ms = m.Split('|');
            //     if (ms.Length > 2)
            //     {
            //         string qf_summary = ms[2];
            //         // 按照每一位是一个设备编号的状态，假设设备编号为索引号（"0", "1", ...）
            //         for (int i = 0; i < qf_summary.Length; i++)
            //         {
            //             // 设备编号为字符串i，值为该位字符
            //             qf_summary_dict[i.ToString()] = qf_summary[i].ToString();
            //         }
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine("解析qf_summary失败: " + ex.Message);
            // }


            // for (int i = 0; i < 16; i++)
            // {
            //     string portName = "发车口" + i;
            //     departurePortDict[portName] = new DeparturePortInfo
            //     {
            //         PortOccupyLockStatus = "空闲",
            //         SectionOccupyStatus = "空闲",
            //         AllowDepartureStatus = "允许发车"
            //     };
            // }
            // var 是C#中的隐式类型变量声明关键字，编译器会自动根据等号右边表达式的结果类型推断出ms的实际类型为string[]
            string[] ms = m.Split('|');
            if (ms.Length > 2)
            {
                string qf_summary = ms[2];

                // 循环读取8个发车口的状态，每次循环读取qf_summary的五个字符串长度的值
                for (int i = 1; i <= 8; i++)
                {
                    string portName = "发车口" + i;
                    int startIdx = (i - 1) * 5;
                    if (startIdx + 5 <= qf_summary.Length)
                    {
                        // 这句话的作用是：从qf_summary字符串中提取当前发车口的5位状态编码（每个发车口信息长度为5），用于后续解析该发车口的状态
                        string segment = qf_summary.Substring(startIdx, 5);

                        // 这句话的作用是：取出segment的第一个字符，表示该发车口的允许或禁止发车状态码
                        char firstChar = segment[0];
                        if (departurePortDict.ContainsKey(portName))
                        {
                            if (firstChar == '1')
                            {
                                departurePortDict[portName].AllowDepartureStatus = "允许发车";
                            }
                            else if (firstChar == '0')
                            {
                                departurePortDict[portName].AllowDepartureStatus = "禁止发车";
                            }

                            if (segment[1] == '1')
                            {
                                departurePortDict[portName].SectionOccupyStatus = "空闲";
                            }
                            else if (segment[1] == '0')
                            {
                                departurePortDict[portName].SectionOccupyStatus = "占用";
                            }

                            if (segment[2] == '1')
                            {
                                departurePortDict[portName].PortOccupyLockStatus = "空闲";
                            }
                            else if (segment[2] == '0')
                            {
                                departurePortDict[portName].PortOccupyLockStatus = "占用";
                            }

                            if (segment[3] == '1')
                            {
                                departurePortDict[portName].DepartureDirection = "接车";
                            }
                            else if (segment[3] == '0')
                            {
                                departurePortDict[portName].DepartureDirection = "发车";
                            }


                        }
                    }
                }
                string qb_summary = ms[3];
                // 循环读取qb_summary的8位字符，并运行与上述类似的解析及赋值逻辑
                for (int i = 0; i <= 8; i += 1)
                {
                    string segment = qb_summary.Substring(i, 1); // 取8位
                    // 可以参考上文的模式进行解析
                    // 假设有一个类似的字典用于保存解析后的状态，例如trackSectionDict，且key可以生成
                    string sectionName;
                    switch (i)
                    {
                        case 4:
                            sectionName = "singleTracks_L17";
                            break;
                        case 5:
                            sectionName = "singleTracks_L28";
                            break;
                        case 6:
                            sectionName = "singleTracks_L35";
                            break;
                        case 7:
                            sectionName = "singleTracks_L46";
                            break;
                        default:
                            sectionName = "StationTrack_G"+i.ToString();
                            break;
                    }
                    switch (segment)
                    {
                        case "0":
                            ((SingleTrack_WithIJ)singleTracks[sectionName]).flag_zt = 3;
                            ((SingleTrack_WithIJ)singleTracks[sectionName]).Drawpic();                            
                            break;
                        case "1":
                            ((SingleTrack_WithIJ)singleTracks[sectionName]).flag_zt = 1;
                            ((SingleTrack_WithIJ)singleTracks[sectionName]).Drawpic();
                            break;
                    }
                    
                    
                }

            }

            //从代码中读取出所有发车口的状态，发车区段的状态
            //自己定义逻辑内部的变量值用以保存


            //定义控件，将每个控件的状态读取对应该状态，写入控件。  
            //读取出所有轨道电路的状态。


            Console.WriteLine(m);

        }
        private void positionHandle(string message)
        {
            string Dir;
            //获取列车车次号、位置以及方向
            string Train_ID = message.Split('|')[2];
            string Train_Position = message.Split('|')[3];
            string Train_Direction = message.Split('|')[4];
            string Train_position = Train_Position;
            
            if (!TRAIN_ROUTE.ContainsKey(Train_ID))
            {
                TRAIN_ROUTE.Add(Train_ID, '0');
            }

            //如果是一辆新注册列车，默认为发车
            if (!upDownState.ContainsKey(Train_ID))
            {
                upDownState.Add(Train_ID, DEPARTURE);
            }
            //判断列车上下行方向
            if (Train_Direction == "1")
            {
                Dir = DOWN_DIRECTION;
            }
            else
            {
                Dir = UP_DIRECTION;
            }
            //判断是在区间还是车站内
            if (Train_Position.Contains("K"))    //公里坐标（Kxxx+yyy）或邻站区间块号
            {
                //获取列车目前在车站的具体坐标
                string[] str = Train_Position.Split('+');
                int dis = Convert.ToInt32(str[1]);
                string route = null;
                //遍历所有进路
                foreach (string key in trainToInterlock.Keys) // 遍历所有进路表的值
                {
                    //判断具体属于哪一个进路  初始默认是发车进路
                    if (key == Train_ID + STOPSTATION + upDownState[Train_ID] + Dir)
                    {
                        //获取列车所在进路的进路表
                        //trainToInterlock["G1234NJAr下"] = "StationTrack_G3&SingleTrack_XJC"
                        // trainToInterlock["G1234NJDe下"] = "StationTrack_G3&SingleTrack_XFC"
                        route = ((string)interlocks[(string)trainToInterlock[key]]);
                        string[] str1 = route.Split('&');

                        //遍历进路表中所有轨道控件，判断当前列车具体所在轨道 判断控件的位置和dis位置比较
                        foreach (string s in str1)
                        {
                            string controlname = s.Split('#')[0];
                            if (controlname.Contains("Switch_2"))
                            {
                                string loc = ((Switch_2)switches[controlname]).实际位置;
                                int zuo = Convert.ToInt32(loc.Split(',')[0]);
                                int you = Convert.ToInt32(loc.Split(',')[1]);
                                if (dis >= zuo && dis <= you)
                                {
                                    Train_position = ((Switch_2)switches[controlname]).Name;
                                    break;
                                }
                            }
                            else if (controlname.Contains("Switch_3"))
                            {
                                string loc = ((Switch_3)switches[controlname]).实际位置;
                                int zuo = Convert.ToInt32(loc.Split(',')[0]);
                                int you = Convert.ToInt32(loc.Split(',')[1]);
                                if (dis >= zuo && dis <= you)
                                {
                                    Train_position = ((Switch_3)switches[controlname]).Name;
                                    break;
                                }
                            }
                            else if (controlname.Contains("SingleTrack"))
                            {
                                string loc = ((SingleTrack_WithIJ)singleTracks[controlname]).实际位置;
                                int zuo = Convert.ToInt32(loc.Split(',')[0]);
                                int you = Convert.ToInt32(loc.Split(',')[1]);
                                if (dis >= zuo && dis <= you)
                                {
                                    Train_position = ((SingleTrack_WithIJ)singleTracks[controlname]).Name;
                                    break;
                                }
                            }
                            else
                            {
                                string loc = ((SingleTrack_WithIJ)stationTracks[controlname]).实际位置;
                                int zuo = Convert.ToInt32(loc.Split(',')[0]);
                                int you = Convert.ToInt32(loc.Split(',')[1]);
                                if (dis >= zuo && dis <= you)
                                {
                                    Train_position = ((SingleTrack_WithIJ)stationTracks[controlname]).Name;
                                    break;
                                }
                            }
                        }
                        //移动列车
                        updateTrainPosition(Train_ID, Train_position, Train_Direction);
                        string stationtrack;
                        //判断是否为新注册列车
                        if (((Train)trains[Train_ID]).Train_Location != null)
                        {
                            // //比较上次和这次的位置判断是否到站
                            // ((Train)trains[Train_ID]).Train_Location 保存上一次的控件名（比如 SingleTrack_L17）。
                            // 当新位置 Train_position 包含 StationTrack（即列车刚进入站内股道）而旧位置不在股道里，视为“到站完成”：
                            if (!((Train)trains[Train_ID]).Train_Location.Contains("StationTrack") &&
                                Train_position.Contains("StationTrack"))
                            {
                                //clearRoute((string)trainToInterlock[key]);
                                trainToInterlock.Remove(key); //移除旧进路
                                // 用 trainScheduleInfo 取出计划到达时间和股道，拼装 subCTCToCTC(...) 上报“到站时刻”给 CTC；
                                stationtrack = ((string)trainScheduleInfo[key]).Split(CSEPARATOR)[1].Split('&')[0].Split('_')[1]; //获取站内股道号
                                sendMessageToCTC(DataConvert.subCTCToCTC(Train_ID, "0", STOPSTATION,
                                    Convert.ToDateTime(((string)trainScheduleInfo[key]).Split(CSEPARATOR)[0]), DateTime.Now,
                                    Convert.ToInt32(stationtrack.Substring(1, stationtrack.Length - 1))));
                                upDownState[Train_ID] = DEPARTURE; //更新车次状态为发车
                            }
                            //判断是否出站
                            // 当旧位置在股道、新位置不在（列车刚发车离站）时
                            if (((Train)trains[Train_ID]).Train_Location.Contains("StationTrack") &&
                                !Train_position.Contains("StationTrack"))
                            {
                                upDownState[Train_ID] = DEPARTURE;
                                string s = ((Train)trains[Train_ID]).Train_Location.Replace("StationTrack", "Xinhaoji");
                                if (Convert.ToInt32(Train_Direction) == 1)
                                {
                                    s = s + "_X";
                                }
                                else
                                {
                                    s = s + "_S";
                                }
                                ((Signal)signals[s]).信号灯状态 = 5; //将对应股道信号机（StationTrack -> Xinhaoji_*）恢复为灭灯状态； 

                                stationtrack = ((string)trainScheduleInfo[key]).Split(CSEPARATOR)[1].Split('&')[0].Split('_')[1];
                                sendMessageToCTC(DataConvert.subCTCToCTC(Train_ID, "1", STOPSTATION,
                                    Convert.ToDateTime(((string)trainScheduleInfo[key]).Split(CSEPARATOR)[0]), DateTime.Now,
                                    Convert.ToInt32(stationtrack.Substring(1, stationtrack.Length - 1))));
                            }
                        }
                        ((Train)trains[Train_ID]).Train_Location = Train_position;
                        break;
                    }
                }
            }
            else
            {
                upDownState[Train_ID] = ARRIVE;
                string key;
                string key1;
                string r;
                //CHUFA[Train_ID] DOWN_ARRIVE_BLOCK = "X4_18" DOWN_ARRIVE_BLOCK_FX = "S4_2";
                // UP_ARRIVE_BLOCK = "S5_12" UP_ARRIVE_BLOCK_FX = "X5_2"
                if (Train_Position == CHUFA[Train_ID])
                {   
                    // 到达区间触发的位置
                    key = Train_ID + STOPSTATION + ARRIVE + DOWN_DIRECTION; //到达的进路
                    key1 = Train_ID + STOPSTATION + DEPARTURE + DOWN_DIRECTION; //发车的进路
                    if (mode_interlock == "normal")  //单车
                    {
                        if (timeToInterlock.ContainsKey(key))
                        {
                            //  timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                            //     arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        
                            r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1]; //获取进路表中的进路arriveRoute
                            string r1 = ((string)timeToInterlock[key]).Split(CSEPARATOR)[0];
                            
                            // 只有在发车进路存在时才比较时间，避免空引用异常
                            if (timeToInterlock.ContainsKey(key1))
                            {
                                string r2 = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0];
                                if (r1 == r2) //比较接车进路和发车进路的时间，如果一致直接通过
                                {
                                    Zhijietongguo = true;
                                }
                            }
                            
                            if (isConflict(key, r)) //判断进路是否冲突
                            {
                                lockRoute(r);
                                TRAIN_ROUTE[Train_ID] = "1";

                                sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                    "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
                                if (timeToInterlock.ContainsKey(key1)) //判断计划是否有发车的进路
                                {
                                    // 只有当接车时间和发车时间相同时，才在到达触发位置时建立发车进路（直通通过场景）
                                    // 如果时间不同（有停车时间），发车进路会保留在timeToInterlock中，等待定时器在发车时间到达时建立
                                    if (((string)timeToInterlock[key]).Split(CSEPARATOR)[0] ==
                                        ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0])
                                    {
                                        r = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[1];
                                        if (isConflict(key1, r))
                                        {
                                            lockRoute(r);
                                            TRAIN_ROUTE[Train_ID] = "1";

                                            timeToInterlock.Remove(key1);
                                            // ===== 向RBC系统发送发车进路许可消息 =====
                                            // 消息格式："10|Train_ID|Train_ID|STOPSTATION|2|7|1"
                                            // 消息含义说明：
                                            //   - "10"：消息类型标识，表示进路许可消息
                                            //   - Train_ID（第1个）：车次号，如 "G1234"
                                            //   - Train_ID（第2个）：车次号重复（协议要求）
                                            //   - STOPSTATION：车站代码，值为 "NJ"（南京南站）
                                            //   - "2"：进路类型标识
                                            //     * "0" = 定时进路（按计划时间触发）
                                            //     * "1" = 接车进路（列车到达进路）
                                            //     * "2" = 发车进路（列车出发进路）← 当前消息类型
                                            //   - "7"：授权级别，表示正常授权（最高安全级别）
                                            //   - "1"：状态标识，表示消息有效/进路已建立
                                            // 
                                            // 发送场景：
                                            //   1. 列车到达触发块，接车进路已成功建立并发送接车许可（"1"）
                                            //   2. 检查到该列车有发车进路计划（key1存在）
                                            //   3. 接车和发车计划时间相同（r1 == r2），说明是"直通通过"（不停车）
                                            //   4. 发车进路无冲突，成功建立（lockRoute成功）
                                            //   5. 向RBC发送发车进路许可，告知RBC该列车可以发车
                                            // 
                                            // 业务意义：
                                            //   - 通知RBC系统：该列车的发车进路已建立，可以授权列车发车
                                            //   - RBC收到此消息后，会向列车发送移动授权，允许列车从车站出发
                                            //   - 这是列车直通通过场景：列车到达后不停车，直接发车离开
                                            // 
                                            // 示例：
                                            //   假设 G1234 次列车在南京南站直通通过：
                                            //   输入消息："10|G1234|G1234|NJ|2|7|1"
                                            //   含义：G1234次列车在南京南站的发车进路已建立，授权级别7，状态有效
                                            sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID + "|" +
                                                Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                                        }
                                        else
                                        {
                                            MessageBox.Show("进路冲突");
                                            TRAIN_ROUTE[Train_ID] = "0";

                                        }
                                    }
                                    // 注意：如果接车时间和发车时间不同（有停车时间），上面的if块不会执行
                                    // 此时发车进路（key1）仍保留在timeToInterlock中，不会在这里建立
                                    // 发车进路将由定时器函数currentTime_Tick()在发车时间到达时自动建立（见2479-2505行）
                                }
                                timeToInterlock.Remove(key);
                            }
                            else
                            {
                                MessageBox.Show("进路冲突");
                                TRAIN_ROUTE[Train_ID] = "0";

                            }
                        }
                        Zhijietongguo = false;
                    }
                    else
                    {
                        if (timeToInterlock.ContainsKey(key))
                        {
                            r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1];
                            lockRoute_multi(r);
                            sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                    "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
                            if (timeToInterlock.ContainsKey(key1))
                            {
                                string r1 = ((string)timeToInterlock[key]).Split(CSEPARATOR)[0];
                                string r2 = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0];
                                if (r1 == r2)
                                {
                                    r = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[1];
                                    lockRoute_multi(r);
                                    timeToInterlock.Remove(key1);
                                    sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID + "|" +
                                                Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                                }
                            }
                            timeToInterlock.Remove(key);
                        }
                    }
                }
                #region
                switch (Train_Position)  //同理用位置再触发一遍判断是否到进路位置
                {
                    case DOWN_ARRIVE_BLOCK:
                        key = Train_ID + STOPSTATION + ARRIVE + DOWN_DIRECTION;
                        key1 = Train_ID + STOPSTATION + DEPARTURE + DOWN_DIRECTION;
                        if (mode_interlock == "normal")
                        {
                            if (timeToInterlock.ContainsKey(key))
                            {
                                r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1];
                                string r1 = ((string)timeToInterlock[key]).Split(CSEPARATOR)[0];
                                string r2 = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0];
                                if (r1 == r2)
                                {
                                    Zhijietongguo = true;
                                }
                                if (isConflict(key, r))
                                {
                                    lockRoute(r);
                                    sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                        "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
                                    if (timeToInterlock.ContainsKey(key1))
                                    {
                                        if (((string)timeToInterlock[key]).Split(CSEPARATOR)[0] ==
                                            ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0])
                                        {
                                            r = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[1];
                                            if (isConflict(key1, r))
                                            {
                                                lockRoute(r);
                                                timeToInterlock.Remove(key1);
                                                sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID + "|" +
                                                    Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                                            }
                                            else
                                            {
                                                MessageBox.Show("进路冲突");
                                            }
                                        }
                                    }
                                    timeToInterlock.Remove(key);
                                }
                                else
                                {
                                    MessageBox.Show("进路冲突");
                                }
                            }
                            Zhijietongguo = false;
                        }
                        else
                        {
                            if (timeToInterlock.ContainsKey(key))
                            {
                                r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1];
                                lockRoute_multi(r);
                                sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                        "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
                                if (timeToInterlock.ContainsKey(key1))
                                {
                                    string r1 = ((string)timeToInterlock[key]).Split(CSEPARATOR)[0];
                                    string r2 = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0];
                                    if (r1 == r2)
                                    {
                                        r = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[1];
                                        lockRoute_multi(r);
                                        sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                        "|" + Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                                        timeToInterlock.Remove(key1);
                                    }
                                }
                                timeToInterlock.Remove(key);
                            }
                        }
                        break;
                    case UP_ARRIVE_BLOCK:
                        key = Train_ID + STOPSTATION + ARRIVE + UP_DIRECTION;
                        key1 = Train_ID + STOPSTATION + DEPARTURE + UP_DIRECTION;
                        if (mode_interlock == "normal")
                        {
                            if (timeToInterlock.ContainsKey(key))
                            {
                                r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1];
                                string r1 = ((string)timeToInterlock[key]).Split(CSEPARATOR)[0];
                                string r2 = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0];
                                if (r1 == r2)
                                {
                                    Zhijietongguo = true;
                                }
                                if (isConflict(key, r))
                                {
                                    lockRoute(r);
                                    sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" +
                                        Train_ID + "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
                                    if (timeToInterlock.ContainsKey(key1))
                                    {
                                        if (((string)timeToInterlock[key]).Split(CSEPARATOR)[0] ==
                                            ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0])
                                        {
                                            r = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[1];
                                            if (isConflict(key1, r))
                                            {
                                                lockRoute(r);
                                                timeToInterlock.Remove(key1);
                                                sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                                    "|" + Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                                            }
                                            else
                                            {
                                                MessageBox.Show("进路冲突");
                                            }
                                        }
                                    }
                                    timeToInterlock.Remove(key);
                                }
                                else
                                {
                                    MessageBox.Show("进路冲突");
                                }
                            }
                            Zhijietongguo = false;
                        }
                        else
                        {
                            if (timeToInterlock.ContainsKey(key))
                            {
                                r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1];
                                lockRoute_multi(r);
                                sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                        "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
                                if (timeToInterlock.ContainsKey(key1))
                                {
                                    string r1 = ((string)timeToInterlock[key]).Split(CSEPARATOR)[0];
                                    string r2 = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[0];
                                    if (r1 == r2)
                                    {
                                        r = ((string)timeToInterlock[key1]).Split(CSEPARATOR)[1];
                                        lockRoute_multi(r);
                                        sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                        "|" + Train_ID + "|" + STOPSTATION + "|" + "2" + "|" + "7" + "|" + "1"));
                                        timeToInterlock.Remove(key1);
                                    }
                                }
                                timeToInterlock.Remove(key);
                            }
                        }
                        break;
                }
                #endregion
                
                // ===== 处理列车在区间一般位置的情况 =====
                // 当列车位置不是特定的到达触发块（DOWN_ARRIVE_BLOCK、UP_ARRIVE_BLOCK）或CHUFA位置时，
                // 说明列车在区间的普通位置（如 X4_15、S5_8 等区间轨道块）
                // 这些位置标识格式为：方向标识_编号，例如 "X4_15"（下行区间1的第15块）、"S5_8"（上行区间2的第8块）
                
                // 将区间位置标识转换为区间轨道控件名称
                // Train_Position 格式：如 "X4_15"、"S5_8"、"X5_12"、"S4_18" 等
                // Train_position 格式：如 "QujianTrack_X4_15"、"QujianTrack_S5_8" 等
                // 这样可以在 intervalTracks 字典中查找对应的区间轨道控件
                Train_position = "QujianTrack_" + Train_Position;
                
                // 更新列车在界面上的位置显示
                // 该函数会：
                // 1. 将列车图标移动到对应的区间轨道控件位置
                // 2. 更新区间轨道的占用状态（flag_zt = 1，表示被占用）
                // 3. 更新码序显示（根据列车位置更新前方区间的码序颜色）
                // 4. 释放上一个位置的占用状态
                updateTrainPosition(Train_ID, Train_position, Train_Direction);
                
                // 检查列车是否刚从车站内进入区间
                // 这个检查用于清理发车进路记录，因为列车一旦离开车站进入区间，发车进路就应该被清除
                if (((Train)trains[Train_ID]).Train_Location != null)
                {
                    // 如果上一次位置不在区间（即上一次在车站内或连接线上），而当前在区间
                    // 说明列车刚刚从车站出发进入区间，需要清除发车进路记录
                    // 上一个位置不包含区间，目前在区间所以删除出战进路
                    if (!((Train)trains[Train_ID]).Train_Location.Contains("QujianTrack_"))
                    {
                        // 构造发车进路的键值
                        // 格式："{车次}{车站代码}De{方向}"
                        // 例如："G1234NJDe下" 表示 G1234 次列车在南京南站下行发车进路
                        
                        // 注释掉的代码：原本会调用 clearRoute 释放发车进路
                        // 但这里只移除记录，不释放进路，可能是因为列车已经离开，进路会自动释放
                        //clearRoute((string)trainToInterlock[Train_ID + STOPSTATION + DEPARTURE + Dir]);
                        
                        // 从 trainToInterlock 字典中移除发车进路记录
                        // 因为列车已经离开车站，发车进路不再需要，可以清除记录
                        // 这样可以避免后续误判或重复处理
                        trainToInterlock.Remove(Train_ID + STOPSTATION + DEPARTURE + Dir);
                    }
                }
                
                // 更新列车的当前位置记录
                // 将当前计算出的位置（区间轨道控件名称）保存到列车的 Train_Location 属性中
                // 这样下次位置更新时，可以通过比较新旧位置判断列车的移动状态
                // 例如：判断是否到站、是否出站、是否进入区间等
                ((Train)trains[Train_ID]).Train_Location = Train_position;
            }
        }
        private void ADDTRAIN_TRAIN(Train tr)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate { ADDTRAIN_TRAIN(tr); }));
                return;
            }
            this.Controls.Add(tr);
        }
        private void updateTrainPosition(string Train_ID, string Train_Position, string Train_Direction)
        {
            Train train = null;
            string last_positon = "";
            if (!trainToTrack.ContainsKey(Train_ID))  //车号和控件位置的名字 trainToTrack["G1234"] = "StationTrack_G3"
            {   //如果车号和控件位置的名字不存在，则创建新的列车
                train = new ConLib.Train();
                trainToTrack.Add(Train_ID, Train_Position);
                train.Name = Train_ID;
                train.车次号 = Train_ID;
                if (Convert.ToInt32(Train_Direction) == 1)  //指定列车方向
                {
                    train.Fangxiang = ConLib.Train.FangXiang.XiaXing;
                }
                else
                {
                    train.Fangxiang = ConLib.Train.FangXiang.ShangXing;
                }
                train.BackColor = System.Drawing.SystemColors.Desktop;      //初始化列车各项参数
                train.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
                train.Size = new System.Drawing.Size(70, 20);
                train.TabIndex = 42;
                train.Train_State = ConLib.Train.Train_state.late;
                train.晚点时间 = "";
                ADDTRAIN_TRAIN(train);
                //this.Controls.Add(train);
                train.Zlocation = ConLib.Train.Weizhi.置顶;
                train.Visible = true;
                trains.Add(Train_ID, train);
            }
            else
            {   //如果车号和控件位置的名字存在，则判断是否走过了上一个控件位置，如果走过了则释放上一个控件位置的进路
                if ((string)trainToTrack[Train_ID] != Train_Position)  //判断已经走过了上一个控件位置，则释放上一个控件位置的进路
                {
                    last_positon = (string)trainToTrack[Train_ID];
                    //判断是否是区间的 区间轨道释放为空闲 X4 S5 X5 S4
                    if (last_positon.Contains(DOWNINTEVAL_1 + "_") || last_positon.Contains(UPINTEVAL_1 + "_") ||
                        last_positon.Contains(DOWNINTEVAL_2 + "_") || last_positon.Contains(UPINTEVAL_2 + "_"))
                    {
                        ((SingleTrack_WithIJ)intervalTracks[last_positon]).flag_zt = 3;
                        ((SingleTrack_WithIJ)intervalTracks[last_positon]).Drawpic();
                        ((CodeOrder)codeOrders[last_positon]).Visible = true;
                        if (Train_Position.Contains("SingleTrack"))  //更改进路信号机状态应该是？
                        {
                            string s = Train_Position.Replace("SingleTrack", "Xinhaoji");
                            if (((Signal)signals[s]).信号灯状态 == 3)
                            {
                                ((Signal)signals[s]).信号灯状态 = 5;
                            }
                        }
                    }
                    else if (last_positon.Contains("Switch_2"))  //道岔释放为空闲
                    {
                        if (((Switch_2)switches[last_positon]).定反位 == ConLib.Switch_2.DingFan.定位)
                        {
                            if (((Switch_2)switches[last_positon]).锁闭状态上 == ConLib.Switch_2.STATE.占用)
                            {
                                ((Switch_2)switches[last_positon]).锁闭状态上 = ConLib.Switch_2.STATE.空闲;
                            }
                            else if (((Switch_2)switches[last_positon]).锁闭状态下 == ConLib.Switch_2.STATE.占用)
                            {
                                ((Switch_2)switches[last_positon]).锁闭状态下 = ConLib.Switch_2.STATE.空闲;
                            }
                        }
                        else if (((Switch_2)switches[last_positon]).定反位 == ConLib.Switch_2.DingFan.反位)
                        {
                            ((Switch_2)switches[last_positon]).反位锁闭状态 = ConLib.Switch_2.STATE.空闲;
                        }
                    }
                    else if (last_positon.Contains("Switch_3"))  //三开道岔释放为空闲
                    {
                        ((Switch_3)switches[last_positon]).锁闭状态 = Switch_3.STATE.空闲;
                    }
                    else if (last_positon.Contains("StationTrack"))  //车站轨道释放为空闲
                    {
                        ((SingleTrack_WithIJ)stationTracks[last_positon]).flag_zt = 3;
                        ((SingleTrack_WithIJ)stationTracks[last_positon]).Drawpic();
                    }
                    else if (last_positon.Contains("SingleTrack"))  //单线轨道释放为空闲
                    {
                        ((SingleTrack_WithIJ)singleTracks[last_positon]).flag_zt = 3;
                        ((SingleTrack_WithIJ)singleTracks[last_positon]).Drawpic();
                    }
                    trainToTrack.Remove(Train_ID);
                    trainToTrack.Add(Train_ID, Train_Position);  //释放完后更新新的位置
                }
            }
            //判断是否是区间的 区间轨道占用
            if (Train_Position.Contains(DOWNINTEVAL_1 + "_") || Train_Position.Contains(UPINTEVAL_1 + "_") ||
                Train_Position.Contains(DOWNINTEVAL_2 + "_") || Train_Position.Contains(UPINTEVAL_2 + "_"))
            {
                ((SingleTrack_WithIJ)intervalTracks[Train_Position]).flag_zt = 1;
                ((SingleTrack_WithIJ)intervalTracks[Train_Position]).Drawpic();
                ((Train)trains[Train_ID]).Location =
                    new System.Drawing.Point(Convert.ToInt32(((SingleTrack_WithIJ)intervalTracks[Train_Position]).Location.X) - 10,
                    Convert.ToInt32(((SingleTrack_WithIJ)intervalTracks[Train_Position]).Location.Y) - 23);
                ((CodeOrder)codeOrders[Train_Position]).Visible = false;
                ((Train)trains[Train_ID]).Visible = true;
                if (mode_interlock == "normal")
                {
                    int num;
                    string c;
                    num = Convert.ToInt32(Train_Position.Split('_')[2]);
                    for (int i = num - 1; i > 0; i--)           //码序颜色的改变
                    {
                        c = "QujianTrack_" + Train_Position.Split('_')[1] + "_" + Convert.ToString(i);
                        if (((SingleTrack_WithIJ)intervalTracks[c]).flag_zt == 3)
                        {
                            switch (num - i)
                            {
                                case 1:
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.红黄;
                                    break;
                                case 2:
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄;
                                    break;
                                case 3:
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄绿;
                                    break;
                                default:
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.绿;
                                    break;
                            }
                        }
                        else
                            break;
                    }
                }
            }
            else if (Train_Position.Contains("Switch_2"))  //道岔占用
            {
                ((Train)trains[Train_ID]).Visible = false;
                if (((Switch_2)switches[Train_Position]).定反位 == ConLib.Switch_2.DingFan.定位)
                {
                    if (((Switch_2)switches[Train_Position]).锁闭状态上 == ConLib.Switch_2.STATE.锁闭 &&
                        ((Train)trains[Train_ID]).Fangxiang == ConLib.Train.FangXiang.ShangXing)
                    {
                        ((Switch_2)switches[Train_Position]).锁闭状态上 = ConLib.Switch_2.STATE.占用;
                    }
                    else if (((Switch_2)switches[Train_Position]).锁闭状态下 == ConLib.Switch_2.STATE.锁闭 &&
                        ((Train)trains[Train_ID]).Fangxiang == ConLib.Train.FangXiang.XiaXing)
                    {
                        ((Switch_2)switches[Train_Position]).锁闭状态下 = ConLib.Switch_2.STATE.占用;
                    }
                }
                else if (((Switch_2)switches[Train_Position]).定反位 == ConLib.Switch_2.DingFan.反位)
                {
                    ((Switch_2)switches[Train_Position]).反位锁闭状态 = ConLib.Switch_2.STATE.占用;
                }
                if (mode_interlock == "normal")
                {
                    if (last_positon.Contains(DOWNINTEVAL_1 + "_") || last_positon.Contains(UPINTEVAL_1 + "_") ||
                    last_positon.Contains(DOWNINTEVAL_2 + "_") || last_positon.Contains(UPINTEVAL_2 + "_"))
                    {

                        int num;
                        string c;
                        num = Convert.ToInt32(last_positon.Split('_')[2]) + 1;
                        for (int i = num - 1; i > 0; i--)           //码序颜色的改变
                        {
                            c = "QujianTrack_" + last_positon.Split('_')[1] + "_" + Convert.ToString(i);
                            if (((SingleTrack_WithIJ)intervalTracks[c]).flag_zt == 3)
                            {
                                switch (num - i)
                                {
                                    case 1:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.红黄;
                                        break;
                                    case 2:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄;
                                        break;
                                    case 3:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄绿;
                                        break;
                                    default:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.绿;
                                        break;
                                }
                            }
                            else
                                break;
                        }
                    }
                }
                else
                {
                    ((Switch_2)switches[Train_Position]).锁闭状态下 = ConLib.Switch_2.STATE.占用;
                }
            }
            else if (Train_Position.Contains("Switch_3"))  //三开道岔占用
            {
                ((Train)trains[Train_ID]).Visible = false;
                ((Switch_3)switches[Train_Position]).锁闭状态 = Switch_3.STATE.占用;
                if (mode_interlock == "normal")
                {
                    if (last_positon.Contains(DOWNINTEVAL_1 + "_") || last_positon.Contains(UPINTEVAL_1 + "_") ||
                    last_positon.Contains(DOWNINTEVAL_2 + "_") || last_positon.Contains(UPINTEVAL_2 + "_"))
                    {
                        int num;
                        string c;
                        num = Convert.ToInt32(last_positon.Split('_')[2]) + 1;
                        for (int i = num - 1; i > 0; i--)           //码序颜色的改变
                        {
                            c = "QujianTrack_" + last_positon.Split('_')[1] + "_" + Convert.ToString(i);
                            if (((SingleTrack_WithIJ)intervalTracks[c]).flag_zt == 3)
                            {
                                switch (num - i)
                                {
                                    case 1:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.红黄;
                                        break;
                                    case 2:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄;
                                        break;
                                    case 3:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄绿;
                                        break;
                                    default:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.绿;
                                        break;
                                }
                            }
                            else
                                break;
                        }
                    }
                }
            }
            else if (Train_Position.Contains("StationTrack"))  //车站轨道占用
            {
                ((Train)trains[Train_ID]).Visible = true;
                ((SingleTrack_WithIJ)stationTracks[Train_Position]).flag_zt = 1;
                ((SingleTrack_WithIJ)stationTracks[Train_Position]).Drawpic();
                ((Train)trains[Train_ID]).Location = new System.Drawing.Point
                    (Convert.ToInt32(((SingleTrack_WithIJ)stationTracks[Train_Position]).Location.X),
                    Convert.ToInt32(((SingleTrack_WithIJ)stationTracks[Train_Position]).Location.Y) - 5);
            }
            else if (Train_Position.Contains("SingleTrack"))  //单线轨道占用
            {
                ((Train)trains[Train_ID]).Visible = true;
                ((SingleTrack_WithIJ)singleTracks[Train_Position]).flag_zt = 1;
                ((SingleTrack_WithIJ)singleTracks[Train_Position]).Drawpic();
                if (Train_Position.Split('_')[1].Contains("XJC") || Train_Position.Split('_')[1].Contains("SJC"))
                {
                    string s = Train_Position.Replace("SingleTrack", "Xinhaoji");
                    ((Signal)signals[s]).信号灯状态 = 5;
                }
                ((Train)trains[Train_ID]).Location =
                    new System.Drawing.Point(Convert.ToInt32(((SingleTrack_WithIJ)singleTracks[Train_Position]).Location.X),
                    Convert.ToInt32(((SingleTrack_WithIJ)singleTracks[Train_Position]).Location.Y) - 22);
                if (mode_interlock == "normal")
                {
                    if (last_positon.Contains(DOWNINTEVAL_1 + "_") || last_positon.Contains(UPINTEVAL_1 + "_") ||
                        last_positon.Contains(DOWNINTEVAL_2 + "_") || last_positon.Contains(UPINTEVAL_2 + "_"))
                    {
                        int num;
                        string c;
                        num = Convert.ToInt32(last_positon.Split('_')[2]) + 1;
                        for (int i = num - 1; i > 0; i--)           //码序颜色的改变
                        {
                            c = "QujianTrack_" + last_positon.Split('_')[1] + "_" + Convert.ToString(i);
                            if (((SingleTrack_WithIJ)intervalTracks[c]).flag_zt == 3)
                            {
                                switch (num - i)
                                {
                                    case 1:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.红黄;
                                        break;
                                    case 2:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄;
                                        break;
                                    case 3:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄绿;
                                        break;
                                    default:
                                        ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.绿;
                                        break;
                                }
                            }
                            else
                                break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 处理 CTC/计划系统下发的运行计划（message）
        /// message 字符串按“|”分隔，主要字段格式：
        ///   [0] = PLAN 标识（外层 switch 已处理）
        ///   [1] = key：列车车次
        ///   [2] = ??? (保留，未使用)
        ///   [3] = dir：方向 1=下行 0=上行
        ///   [4] = departureTime：计划发车时间
        ///   [5] = arriveTime：计划到达时间
        ///   [6] = temp[0]：股道号（如 G1 的 “1”）
        ///   [7] = DIR_IN：进站方向 0=正向 1=反向
        ///   [8] = DIR_OUT：出站方向 0=正向 1=反向
        /// 函数职责：
        ///   - 根据方向/股道/进出站方向拼接进路标识（Interlock_Info）
        ///   - 将到发计划写入 timeToInterlock / trainToInterlock / trainScheduleInfo
        ///   - 在界面 DataGridView 中同步显示计划
        /// </summary>
        private void handlePlan(string message)
        {
            // temp[0]：股道号；temp[1]：到达时间；temp[2]：发车时间；temp[3]：备用
            string[] temp = new string[4];
            string arriveRoute = "";
            string departureRoute = "";
            string arriveTime;
            string departureTime;
            string key = message.Split('|')[1];
            string dir = message.Split('|')[3];
            string direction;

            if (dir == "1")
            {
                dir = DOWN_DIRECTION;
                direction = DOWNDIRECTION;
            }
            else
            {
                dir = UP_DIRECTION;
                direction = UPDIRECTION;
            }

            temp[0] = message.Split('|')[6]; // 股道号（不含"G"），如 "1"
            temp[1] = message.Split('|')[5]; // 到达时间字符串
            temp[2] = message.Split('|')[4]; // 发车时间字符串
            temp[3] = "";
            //  if (dir == DOWN_DIRECTION)
            //  {
            //      arriveRoute = "StationTrack_G" + temp[0] + "&SingleTrack_XJC";
            //      departureRoute = "StationTrack_G" + temp[0] + "&SingleTrack_XFC";
            //  }
            //  else
            //  {
            //      arriveRoute = "StationTrack_G" + temp[0] + "&SingleTrack_SJC";
            //      departureRoute = "StationTrack_G" + temp[0] + "&SingleTrack_SFC";
            //  }
            string DIR_IN = message.Split('|')[7];  // 进站方向：0=正向，1=反向
            string DIR_OUT = message.Split('|')[8]; // 出站方向：0=正向，1=反向
            // CHUFA 字典保存列车在邻站的出发（区间）位置，用于 later positionHandle
            if (CHUFA.ContainsKey(key))
            {
                CHUFA.Remove(key);
            }

            // CHUFA：Dictionary<string,string>，键是列车车次，值是该列车在邻站（区间）端的到达触发区段 ID。
            // handlePlan 在解析计划时，根据方向/邻站进站方式写入区段（如 DOWN_ARRIVE_BLOCK = "X4_18" 表示下行临近区间的第 18 段）。
            // 例：列车 G1234 下行正向进站 → CHUFA["G1234"] = "X4_18"，
            // 用于之后 positionHandle 判断列车进入区间时触发该列车的到站进路。


            //只有进站的时候要添加CHUFA

            if (dir == DOWN_DIRECTION)   //下行车，从左向右行驶 靠左侧铁轨
            {
                if (DIR_IN == "0")       //下行正向进站即为 从左向右行驶 靠左侧铁轨
                {
                    arriveRoute = "StationTrack_G" + temp[0] + "&SingleTrack_XJC";
                    CHUFA.Add(key, DOWN_ARRIVE_BLOCK);
                }
                else                     //下行反向进站即为 从左向右行驶 靠右侧铁轨
                {
                    arriveRoute = "StationTrack_G" + temp[0] + "&SingleTrack_SFJC";
                    CHUFA.Add(key, DOWN_ARRIVE_BLOCK_FX);
                }
                if (DIR_OUT == "0")      //下行正向出站即为 从左向右行驶 靠左侧铁轨
                {
                    departureRoute = "StationTrack_G" + temp[0] + "&SingleTrack_XFC";
                }
                else                     //下行反向出站即为 从左向右行驶 靠右侧铁轨
                {
                    departureRoute = "StationTrack_G" + temp[0] + "&SingleTrack_SFFC";
                }
            }
            else                     //上行行车，从右向左行驶 靠左侧铁轨
            {
                if (DIR_IN == "0")     //上行正向进站即为 从右向左行驶 靠左侧铁轨
                {
                    arriveRoute = "StationTrack_G" + temp[0] + "&SingleTrack_SJC";
                    CHUFA.Add(key, UP_ARRIVE_BLOCK);
                }
                else                     //上行反向进站即为 从右向左行驶 靠右侧铁轨
                {
                    arriveRoute = "StationTrack_G" + temp[0] + "&SingleTrack_XFJC";
                    CHUFA.Add(key, UP_ARRIVE_BLOCK_FX);
                }
                if (DIR_OUT == "0")      //上行正向出站即为 从右向左行驶 靠左侧铁轨
                {
                    departureRoute = "StationTrack_G" + temp[0] + "&SingleTrack_SFC";
                }
                else                     //上行反向出站即为 从右向左行驶 靠右侧铁轨
                {
                    departureRoute = "StationTrack_G" + temp[0] + "&SingleTrack_XFFC";
                }
            }
            // arriveRoute
            // departureRoute
            arriveTime = temp[1];
            departureTime = temp[2];

            // === 写入到站计划 ===
            if (arriveTime != "")
            {
                // 判断当前时间是否早于到站时间，如果是，则进行后续到站计划的写入操作
                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(arriveTime)) < 0)
                {
                    if (timeToInterlock.ContainsKey(key + STOPSTATION + ARRIVE + dir) &&
                        trainToInterlock.ContainsKey(key + STOPSTATION + ARRIVE + dir))
                    {
                        timeToInterlock.Remove(key + STOPSTATION + ARRIVE + dir);
                        trainToInterlock.Remove(key + STOPSTATION + ARRIVE + dir);
                        
                        //将区间的触发条件通过 CHUFA 变量写入timeToInterlock
                        if (dir == DOWN_DIRECTION)
                        {
                            timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                                arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        }
                        else
                        {
                            timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                                arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        }


                        //添加车进站的进路条件到trainToInterlock
                        //trainToInterlock["G1234NJAr下"] = "StationTrack_G3&SingleTrack_XJC"
                        // trainToInterlock["G1234NJDe下"] = "StationTrack_G3&SingleTrack_XFC"
                        trainToInterlock.Add(key + STOPSTATION + ARRIVE + dir, arriveRoute);

                        trainScheduleInfo.Remove(key + STOPSTATION + ARRIVE + dir);
                        //将到站计划写入trainScheduleInfo
                        // Key:  "G1234NJAr下"
                        // Value: "2025-11-25 10:00:00$StationTrack_G3&SingleTrack_XJC"
                        // trainScheduleInfo["G1234NJAr下"] = "2025-11-25 10:00:00$StationTrack_G3&SingleTrack_XJC"
                        trainScheduleInfo.Add(key + STOPSTATION + ARRIVE + dir, arriveTime + SEPARATOR + arriveRoute);
                    }
                    else if (!timeToInterlock.ContainsKey(key + STOPSTATION + ARRIVE + dir) &&
                        trainToInterlock.ContainsKey(key + STOPSTATION + ARRIVE + dir))
                    {
                        //clearRoute((string)trainToInterlock[key + STOPSTATION + ARRIVE + dir]);
                        trainToInterlock.Remove(key + STOPSTATION + ARRIVE + dir);
                        if (dir == DOWN_DIRECTION)
                        {
                            timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                                arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        }
                        else
                        {
                            timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                                arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        }
                        trainToInterlock.Add(key + STOPSTATION + ARRIVE + dir, arriveRoute);

                        trainScheduleInfo.Remove(key + STOPSTATION + ARRIVE + dir);
                        trainScheduleInfo.Add(key + STOPSTATION + ARRIVE + dir, arriveTime + SEPARATOR + arriveRoute);
                    }
                    else if (!timeToInterlock.ContainsKey(key + STOPSTATION + ARRIVE + dir) &&
                        !trainToInterlock.ContainsKey(key + STOPSTATION + ARRIVE + dir))
                    {
                        if (dir == DOWN_DIRECTION)
                        {
                            timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                                arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        }
                        else
                        {
                            timeToInterlock.Add(key + STOPSTATION + ARRIVE + dir,
                                arriveTime + SEPARATOR + arriveRoute + SEPARATOR + "QujianTrack_" + CHUFA[key]);
                        }
                        trainToInterlock.Add(key + STOPSTATION + ARRIVE + dir, arriveRoute);

                        trainScheduleInfo.Add(key + STOPSTATION + ARRIVE + dir, arriveTime + SEPARATOR + arriveRoute);
                    }

                    for (int j = 0; j < dataGridView1.Rows.Count - 1; j++)
                    {
                        if ((string)dataGridView1.Rows[j].Cells[1].Value == key &&
                            (string)dataGridView1.Rows[j].Cells[5].Value == "接")
                        {
                            dataGridView1.Rows.Remove(dataGridView1.Rows[j]);
                            break;
                        }
                    }
                    // 在计划列表中加入“接车”记录
                    dataGridView1.Rows.Add(dataGridId, key,
                        arriveTime, "邻站", temp[0], "接", arriveRoute, "自动", direction, "等候中...", "无");

                }
            }

            // === 写入发车计划 ===
            if (departureTime != "")
            {
                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(departureTime)) < 0)
                {
                    if (timeToInterlock.ContainsKey(key + STOPSTATION + DEPARTURE + dir) &&
                        trainToInterlock.ContainsKey(key + STOPSTATION + DEPARTURE + dir))
                    {
                        timeToInterlock.Remove(key + STOPSTATION + DEPARTURE + dir);
                        trainToInterlock.Remove(key + STOPSTATION + DEPARTURE + dir);
                        timeToInterlock.Add(key + STOPSTATION + DEPARTURE + dir,
                            departureTime + SEPARATOR + departureRoute + SEPARATOR + departureRoute.Split('&')[0]);
                        trainToInterlock.Add(key + STOPSTATION + DEPARTURE + dir, departureRoute);

                        trainScheduleInfo.Remove(key + STOPSTATION + DEPARTURE + dir);
                        trainScheduleInfo.Add(key + STOPSTATION + DEPARTURE + dir, arriveTime + SEPARATOR + arriveRoute);
                    }
                    else if (!timeToInterlock.ContainsKey(key + STOPSTATION + DEPARTURE + dir) &&
                        trainToInterlock.ContainsKey(key + STOPSTATION + DEPARTURE + dir))
                    {
                        //clearRoute((string)trainToInterlock[key + STOPSTATION + DEPARTURE + dir]);
                        trainToInterlock.Remove(key + STOPSTATION + DEPARTURE + dir);
                        timeToInterlock.Add(key + STOPSTATION + DEPARTURE + dir,
                            departureTime + SEPARATOR + departureRoute + SEPARATOR + departureRoute.Split('&')[0]);
                        trainToInterlock.Add(key + STOPSTATION + DEPARTURE + dir, departureRoute);

                        trainScheduleInfo.Remove(key + STOPSTATION + DEPARTURE + dir);
                        trainScheduleInfo.Add(key + STOPSTATION + DEPARTURE + dir, arriveTime + SEPARATOR + arriveRoute);
                    }
                    else if (!timeToInterlock.ContainsKey(key + STOPSTATION + DEPARTURE + dir) &&
                        !trainToInterlock.ContainsKey(key + STOPSTATION + DEPARTURE + dir))
                    {
                        timeToInterlock.Add(key + STOPSTATION + DEPARTURE + dir,
                            departureTime + SEPARATOR + departureRoute + SEPARATOR + departureRoute.Split('&')[0]);
                        trainToInterlock.Add(key + STOPSTATION + DEPARTURE + dir, departureRoute);

                        trainScheduleInfo.Add(key + STOPSTATION + DEPARTURE + dir, arriveTime + SEPARATOR + arriveRoute);
                    }

                    for (int j = 0; j < dataGridView1.Rows.Count - 1; j++)
                    {
                        if ((string)dataGridView1.Rows[j].Cells[1].Value == key &&
                            (string)dataGridView1.Rows[j].Cells[5].Value == "发")
                        {
                            dataGridView1.Rows.Remove(dataGridView1.Rows[j]);
                            break;
                        }
                    }
                    // 在计划列表中加入“发车”记录，提前5分钟触发
                    dataGridView1.Rows.Add(dataGridId, key, departureTime,
                        Convert.ToDateTime(departureTime).AddMinutes(-5).ToShortTimeString(),
                        temp[0], "发", departureRoute, "自动", direction, "等候中...", "无");

                }
            }
        }
        /// <summary>
        /// 处理删除列车消息 - 从系统中移除列车及其所有相关信息
        /// 
        /// 【功能说明】
        /// 此函数用于处理列车删除消息，当列车离开车站管辖范围或需要从系统中移除时调用。
        /// 函数会清理与该列车相关的所有数据结构和界面元素，包括：
        /// - 列车对象和界面控件
        /// - 列车位置信息（trainToTrack）
        /// - 列车状态信息（upDownState）
        /// - 列车计划信息（trainScheduleInfo）
        /// - 释放轨道资源（将轨道状态设置为空闲）
        /// - 更新码序显示（恢复区间码序颜色）
        /// 
        /// 【调用时机和位置】
        /// 1. 调用位置：onConnect() 函数中的消息分发逻辑（第1155-1156行）
        /// 2. 触发条件：接收到消息类型为 "3"（DELETE）的消息时调用
        /// 3. 消息来源：通过UDP套接字接收，消息类型标识为常量 DELETE = "3"
        /// 
        /// 【消息格式】
        /// 消息格式：用 "|" 分隔的字符串
        /// "3|车次号"
        /// 
        /// 示例消息：
        /// "3|G1234"
        /// 
        /// 消息字段说明：
        /// - [0] = "3"：消息类型标识（DELETE常量）
        /// - [1] = 车次号：要删除的列车车次号（如 "G1234"）
        /// 
        /// 【执行的主要操作】
        /// 1. 检查列车是否存在（trainToTrack字典中是否有该车次）
        /// 2. 删除列车状态信息（upDownState）
        /// 3. 释放列车占用的轨道资源（将轨道状态设置为空闲flag_zt=3）
        /// 4. 更新区间码序显示（恢复码序颜色：红黄→黄→黄绿→绿）
        /// 5. 删除列车位置信息（trainToTrack）
        /// 6. 删除列车对象（trains字典和界面控件）
        /// 7. 删除列车计划信息（trainScheduleInfo中所有包含该车次的条目）
        /// 
        /// 【业务场景】
        /// 适用情况：
        /// - 列车离开车站管辖范围，需要从系统中注销
        /// - 列车运行计划取消，需要清理相关数据
        /// - 系统维护或调试时手动删除列车
        /// - 列车信息错误需要重新注册
        /// 
        /// 【示例场景】
        /// 场景：G1234次列车离开车站，需要从系统中删除
        /// - 接收消息："3|G1234"
        /// - 执行流程：
        ///   ① 检查列车是否存在：trainToTrack.Contains("G1234") → true
        ///   ② 获取列车最后位置：假设为 "QujianTrack_X4_15"（X4区间第15段）
        ///   ③ 释放轨道资源：
        ///      - intervalTracks["QujianTrack_X4_15"].flag_zt = 3（设置为空闲）
        ///      - 重新绘制轨道显示
        ///   ④ 更新码序显示：
        ///      - 将码序顺序恢复（第15段→第14段→第13段...）
        ///      - 码序颜色：红黄 → 黄 → 黄绿 → 绿
        ///   ⑤ 删除数据记录：
        ///      - trainToTrack.Remove("G1234")
        ///      - upDownState.Remove("G1234")
        ///      - trains.Remove("G1234")
        ///      - trainScheduleInfo.Remove("G1234NJAr下")
        ///      - trainScheduleInfo.Remove("G1234NJDe下")
        ///   ⑥ 删除界面控件：this.Controls.Remove(train)
        /// 
        /// 【注意事项】
        /// - 只有当列车存在于 trainToTrack 中时才会执行删除操作
        /// - 如果列车在区间轨道上，会更新码序显示
        /// - 会删除 trainScheduleInfo 中所有包含该车次号的条目（包括接车和发车计划）
        /// - 代码中有注释掉的 dataGridView1 删除逻辑（可能由其他机制处理）
        /// </summary>
        /// <param name="message">删除消息字符串，格式："3|车次号"</param>
        private void handledelete(string message)
        {
            // ===== 步骤1：检查列车是否存在 =====
            // 检查 trainToTrack 字典中是否包含该车次号
            // 只有当列车已注册到系统中时，才执行删除操作
            // 注意：Hashtable的Contains方法检查的是Value，这里应该用ContainsKey检查Key
            // 但从上下文看，message.Split('|')[1] 是车次号，应该是作为Key使用
            // message.Split('|')[1] 示例："G1234"
            if (trainToTrack.Contains(message.Split('|')[1]))
            {
                Train train = new ConLib.Train();  // 临时变量，用于存储列车对象引用

                // ===== 步骤2：删除列车状态信息 =====
                // 从 upDownState 字典中移除列车的上下行状态记录
                // 示例：upDownState.Remove("G1234")
                upDownState.Remove(message.Split('|')[1]);

                // ===== 步骤3：释放列车占用的轨道资源 =====
                // 获取列车当前占用的轨道位置，并将其状态设置为空闲（flag_zt = 3）
                // trainToTrack["G1234"] 示例："QujianTrack_X4_15"
                // 将轨道的占用状态标志设置为3（空闲状态）
                ((SingleTrack_WithIJ)intervalTracks[trainToTrack[message.Split('|')[1]]]).flag_zt = 3;
                // 重新绘制轨道控件，更新界面显示
                ((SingleTrack_WithIJ)intervalTracks[trainToTrack[message.Split('|')[1]]]).Drawpic();

                // ===== 步骤4：保存列车最后位置，用于后续处理 =====
                string last_positon = (string)trainToTrack[message.Split('|')[1]];

                // ===== 步骤5：处理区间轨道的码序显示恢复 =====
                // 如果列车最后位置在区间轨道上（X4、S5、X5、S4），需要恢复码序显示
                // 码序：列车在区间运行时显示的信号指示，表示前方可用轨道数量
                if (last_positon.Contains(DOWNINTEVAL_1 + "_") || last_positon.Contains(UPINTEVAL_1 + "_")
                    || last_positon.Contains(DOWNINTEVAL_2 + "_") || last_positon.Contains(UPINTEVAL_2 + "_"))
                {
                    int num;    // 当前轨道段编号
                    string c;   // 轨道名称

                    // 计算当前轨道段编号
                    // 示例：last_positon = "QujianTrack_X4_15" → Split('_')[2] = "15" → num = 16
                    num = Convert.ToInt32(last_positon.Split('_')[2]) + 1;

                    // 获取当前轨道段的码序控件名称
                    // 示例：c = "QujianTrack_X4_15"
                    c = "QujianTrack_" + last_positon.Split('_')[1] + "_" + Convert.ToString(num - 1);
                    
                    // 显示码序控件（Visible = true）
                    ((CodeOrder)codeOrders[c]).Visible = true;

                    // 从当前轨道段向前遍历，恢复码序颜色显示
                    // 码序颜色含义：
                    // - 红黄：1个轨道段可用（紧邻）
                    // - 黄：2个轨道段可用
                    // - 黄绿：3个轨道段可用
                    // - 绿：4个或更多轨道段可用
                    for (int i = num - 1; i > 0; i--)           //码序颜色的改变
                    {
                        // 构造轨道段名称
                        // 示例：i=14 → c = "QujianTrack_X4_14"
                        c = "QujianTrack_" + last_positon.Split('_')[1] + "_" + Convert.ToString(i);
                        
                        // 检查轨道段是否为空闲状态（flag_zt == 3）
                        if (((SingleTrack_WithIJ)intervalTracks[c]).flag_zt == 3)
                        {
                            // 根据距离当前轨道的段数，设置不同的码序颜色
                            // num - i 表示距离：1=紧邻，2=隔1段，3=隔2段，4+=更远
                            switch (num - i)
                            {
                                case 1:
                                    // 紧邻段：显示红黄码序（最紧急）
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.红黄;
                                    break;
                                case 2:
                                    // 隔1段：显示黄码序
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄;
                                    break;
                                case 3:
                                    // 隔2段：显示黄绿码序
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.黄绿;
                                    break;
                                default:
                                    // 更远的段：显示绿码序（正常）
                                    ((CodeOrder)codeOrders[c]).显示状态 = CodeOrder.CodeView.绿;
                                    break;
                            }
                        }
                        else
                            // 如果轨道段不是空闲状态，停止向前遍历
                            break;
                    }
                }

                // ===== 步骤6：删除列车位置信息 =====
                // 从 trainToTrack 字典中移除列车的车次号-位置映射
                // 示例：trainToTrack.Remove("G1234")
                trainToTrack.Remove(message.Split('|')[1]);

                // ===== 步骤7：删除列车对象 =====
                // 获取列车对象引用
                train = (Train)trains[message.Split('|')[1]];
                
                // 从 trains 字典中移除列车对象
                trains.Remove(message.Split('|')[1]);
                
                // 从界面控件集合中移除列车控件（从界面上删除列车显示）
                this.Controls.Remove(train);

                // ===== 步骤8：删除列车计划信息 =====
                // trainScheduleInfo 中可能包含多条记录（接车计划、发车计划等）
                // 需要查找所有包含该车次号的键，然后统一删除
                List<string> l = new List<string>();  // 临时列表，存储需要删除的键

                // 遍历 trainScheduleInfo 字典，找出所有包含该车次号的键
                // 示例键："G1234NJAr下"、"G1234NJDe下"
                foreach (string key in trainScheduleInfo.Keys)
                {
                    // 如果键中包含该车次号，添加到删除列表
                    // 示例：key = "G1234NJAr下"，message.Split('|')[1] = "G1234" → 匹配
                    if (key.Contains(message.Split('|')[1]))
                    {
                        l.Add(key);
                    }
                }

                // 统一删除所有相关的计划信息
                foreach (string key in l)
                {
                    trainScheduleInfo.Remove(key);
                }

                // ===== 注释掉的代码：删除数据表格中的显示 =====
                // 以下代码被注释掉，可能由其他机制处理数据表格的更新
                // 或者在其他地方统一处理表格显示
                //for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                //{
                //    if ((string)dataGridView1.Rows[i].Cells[1].Value == message.Split('|')[1])
                //    {
                //        dataGridView1.Rows.Remove(dataGridView1.Rows[i]);
                //        break;
                //    }
                //}
            }
            // 如果列车不存在于 trainToTrack 中，则不执行任何操作（列车可能已被删除或从未注册）
        }
        private void handleTime(string message)
        {
            DateTime t = DateTime.Parse(message.Split('|')[1]);
            TimeUtil.SYSTEMTIME st = new TimeUtil.SYSTEMTIME();
            st.FromDateTime(t);
            TimeUtil.Win32API.SetLocalTime(ref st);
        }
        /// <summary>
        /// 处理事故/紧急情况下的进路建立
        /// 
        /// 【功能说明】
        /// 此函数用于处理紧急情况下（如事故、故障等）需要立即建立进路并关闭信号机的场景。
        /// 与正常的计划进路建立不同，事故进路会立即执行，不等待时间条件或位置条件，属于紧急响应机制。
        /// 
        /// 【调用时机和位置】
        /// 1. 调用位置：onConnect() 函数中的消息分发逻辑（第1161-1162行）
        /// 2. 触发条件：接收到消息类型为 "10"（ACCIDENT）的消息时调用
        /// 3. 消息来源：通过UDP套接字接收，消息类型标识为常量 ACCIDENT = "10"
        /// 
        /// 【消息格式】
        /// 消息格式：用 "|" 分隔的字符串
        /// "10|进路路径|车次+方向标识|车次号|进路类型|时间信息"
        /// 
        /// 示例消息：
        /// "10|StationTrack_G3&SingleTrack_XFC|G12341|G1234|De|2025-01-01 10:00:00"
        /// 
        /// 消息字段说明：
        /// - [0] = "10"：消息类型标识（ACCIDENT常量）
        /// - [1] = 进路路径：如 "StationTrack_G3&SingleTrack_XFC"（股道和区间轨道路径，用&连接）
        /// - [2] = 车次+方向标识：如 "G12341"（最后一位是方向，"1"=下行，"0"=上行）
        /// - [3] = 车次号：如 "G1234"
        /// - [4] = 进路类型：如 "De"（发车）或 "Ar"（接车）
        /// - [5] = 时间信息：如 "2025-01-01 10:00:00"（用于记录到trainScheduleInfo）
        /// 
        /// 【执行的主要操作】
        /// 1. 立即建立进路（lockRoute）- 不检查时间条件，立即锁闭道岔、占用轨道
        /// 2. 更新 trainToInterlock 字典 - 记录列车与进路的映射关系
        /// 3. 删除 timeToInterlock 中的计划（如果存在）- 避免定时器重复处理
        /// 4. 更新 trainScheduleInfo - 记录列车计划信息
        /// 5. 更新 upDownState - 记录列车的上下行状态
        /// 6. 关闭相关信号机 - 将信号机状态设置为5（灭灯状态）
        /// 
        /// 【业务场景】
        /// 适用情况：
        /// - 紧急情况下需要立即建立进路（不等待计划时间）
        /// - 系统故障或人工干预时强制建立进路
        /// - 事故处理时需要立即为列车建立进路
        /// - 临时调整运行计划
        /// 
        /// 【示例场景】
        /// 场景1：G1234次列车需要在南京南站紧急下行发车
        /// - 接收消息："10|StationTrack_G3&SingleTrack_XFC|G12341|G1234|De|2025-01-01 10:00:00"
        /// - 执行操作：
        ///   ① 立即建立进路：lockRoute("StationTrack_G3&SingleTrack_XFC")
        ///   ② 记录进路映射：trainToInterlock["G1234NJDe下"] = "StationTrack_G3&SingleTrack_XFC"
        ///   ③ 删除定时计划：timeToInterlock.Remove("G1234NJDe下")（如果存在）
        ///   ④ 更新计划信息：trainScheduleInfo["G1234NJDe下"] = "2025-01-01 10:00:00$StationTrack_G3&SingleTrack_XFC"
        ///   ⑤ 更新状态：upDownState["G1234"] = "De"
        ///   ⑥ 关闭信号机：signals["Xinhaoji_G3_X"].信号灯状态 = 5（G3股道下行信号机灭灯）
        /// 
        /// 场景2：G5678次列车需要紧急上行接车
        /// - 接收消息："10|StationTrack_G5&SingleTrack_SJC|G56780|G5678|Ar|2025-01-01 11:00:00"
        /// - 执行操作：类似上述流程，但关闭的是接车信号机
        /// </summary>
        /// <param name="message">事故消息字符串，格式："10|进路路径|车次+方向|车次号|进路类型|时间"</param>
        private void handleAccident(string message)
        {
            // ===== 步骤1：立即建立进路 =====
            // 从消息中提取进路路径（第2个字段），立即建立进路
            // 注意：事故进路不检查时间条件、位置条件或冲突，直接建立（紧急响应）
            // message.Split('|')[1] 示例："StationTrack_G3&SingleTrack_XFC"
            lockRoute(message.Split('|')[1]);

            // ===== 步骤2：解析方向和方向标识 =====
            string s = "";      // 方向字符串（"下" 或 "上"）
            string s3 = "";     // 方向字母标识（"X"=下行 或 "S"=上行）
            
            // 从消息第3个字段（车次+方向）的最后一位提取方向标识
            // "1" = 下行方向，"0" = 上行方向
            // 示例："G12341" → 最后一位是 "1" → 下行
            if (message.Split('|')[2].Substring(message.Split('|')[2].Length - 1, 1) == "1")
            {
                s = DOWN_DIRECTION;   // "下"
                s3 = "X";             // 下行方向字母标识
            }
            else
            {
                s = UP_DIRECTION;     // "上"
                s3 = "S";             // 上行方向字母标识
            }

            // ===== 步骤3：构造进路键值 =====
            // 格式：车次号 + 车站代码 + 进路类型 + 方向
            // 示例：从 "G12341" 提取车次号 "G1234"，加上方向 "下" → "G1234NJDe下"
            // 用于在 trainToInterlock、timeToInterlock 等字典中标识进路
            string s1 = message.Split('|')[2].Substring(0, message.Split('|')[2].Length - 1) + s;

            // ===== 步骤4：更新 trainToInterlock 字典 =====
            // 记录列车车次与进路路径的映射关系
            // 如果该进路不存在，则添加新的映射
            // 示例：trainToInterlock["G1234NJDe下"] = "StationTrack_G3&SingleTrack_XFC"
            if (!trainToInterlock.ContainsKey(s1))
            {
                trainToInterlock.Add(s1, message.Split('|')[1]);
            }

            // ===== 步骤5：删除 timeToInterlock 中的计划（如果存在）=====
            // 如果该进路在 timeToInterlock 中有定时计划，需要删除
            // 原因：事故进路已经立即建立，不需要等待定时器触发
            if (timeToInterlock.ContainsKey(s1))
            {
                timeToInterlock.Remove(s1);
            }

            // ===== 步骤6：更新 trainScheduleInfo 字典 =====
            // 记录列车的计划信息（时间和进路路径）
            // 格式：时间 + SEPARATOR($) + 进路路径
            // 示例：trainScheduleInfo["G1234NJDe下"] = "2025-01-01 10:00:00$StationTrack_G3&SingleTrack_XFC"
            if (!trainScheduleInfo.ContainsKey(s1))
            {
                trainScheduleInfo.Add(s1, message.Split('|')[5] + SEPARATOR + message.Split('|')[1]);
            }

            // ===== 步骤7：更新 upDownState 字典 =====
            // 记录列车的上下行状态（接车Ar或发车De）
            // 示例：upDownState["G1234"] = "De"（发车状态）
            if (!upDownState.ContainsKey(message.Split('|')[3]))
            {
                upDownState.Add(message.Split('|')[3], message.Split('|')[4]);
            }

            // ===== 步骤8：关闭相关信号机 =====
            // 根据进路类型（接车或发车）构造信号机名称，并将其状态设置为5（灭灯状态）
            string s2 = "Xinhaoji_";  // 信号机名称前缀
            
            if (message.Split('|')[4] == DEPARTURE)
            {
                // 发车进路：从进路路径的第一个轨道（股道）提取信号机名称
                // 示例：进路 "StationTrack_G3&SingleTrack_XFC" 
                //       → Split('&')[0] = "StationTrack_G3"
                //       → Split('_')[1] = "G3"
                //       → 信号机名称："Xinhaoji_G3_X"（G3股道下行发车信号机）
                s2 = s2 + message.Split('|')[1].Split('&')[0].Split('_')[1] + "_" + s3;
            }
            else
            {
                // 接车进路：从进路路径的第二个轨道（区间轨道）提取信号机名称
                // 示例：进路 "StationTrack_G3&SingleTrack_SJC"
                //       → Split('&')[1] = "SingleTrack_SJC"
                //       → Split('_')[1] = "SJC"
                //       → 信号机名称："Xinhaoji_SJC"（上行接车信号机）
                s2 = s2 + message.Split('|')[1].Split('&')[1].Split('_')[1];
            }
            
            // 将信号机状态设置为5（灭灯状态）
            // 状态5通常表示信号机关闭，不显示任何信号
            ((Signal)signals[s2]).信号灯状态 = 5;
        }
        /// <summary>
        /// 定时器回调函数 - 定时检查并自动建立进路
        /// 
        /// 【定时器配置说明】
        /// - 定时器名称：currentTime（在 Form1.Designer.cs 中定义）
        /// - 定时器类型：System.Windows.Forms.Timer
        /// - 触发间隔：500 毫秒（0.5秒，即每半秒触发一次）
        /// - 启用状态：Enabled = true（窗体加载后自动启动）
        /// - 事件绑定：在 Form1.Designer.cs 第389行绑定 Tick 事件
        ///   this.currentTime.Tick += new System.EventHandler(this.currentTime_Tick);
        /// - 运行机制：定时器一旦启用，会持续自动运行，直到程序关闭或手动停止
        /// 
        /// 
        /// // 第35行：定时器对象声明
        /// this.currentTime = new System.Windows.Forms.Timer(this.components);
        /// 窗体加载后（Form1_Load 执行时）自动启动
        /// 无需手动调用 Start()，因为 Enabled = true
         /// // 第385-389行：定时器配置
        /// this.currentTime.Enabled = true;        // 默认启用，自动运行
        /// this.currentTime.Interval = 500;        // 触发间隔：500毫秒（0.5秒）
        /// this.currentTime.Tick += new System.EventHandler(this.currentTime_Tick);  // 绑定事件
        /// 【调用时机】
        /// 1. 程序启动：窗体加载后（Form1_Load），定时器自动开始运行
        /// 2. 持续运行：每 500 毫秒（0.5秒）自动触发一次
        /// 3. 停止时机：程序关闭或窗体销毁时自动停止
        /// 
        /// 【功能说明】
        /// 此函数由定时器每 0.5 秒自动调用一次，用于检查 timeToInterlock 字典中的进路计划，
        /// 当满足时间条件和位置条件时，自动建立进路（锁闭道岔、设置信号机等）。
        /// 
        /// 【主要处理两种类型的进路】
        /// 1. 发车进路（DEPARTURE）：需要列车在指定股道上，且在计划发车时间前3秒建立
        /// 2. 其他定时进路（loc == "NO"）：仅根据时间触发，不需要位置条件
        /// 
        /// 【数据格式说明】
        /// timeToInterlock 字典结构：
        /// - Key: "G1234NJDe下"  (车次号 + 车站代码 + 进路类型 + 方向)
        /// - Value: "2025-01-01 10:00:00$StationTrack_G3&SingleTrack_XFC$StationTrack_G3"
        ///   格式：计划时间$进路路径$位置条件（用 $ 分隔）
        ///   - 第1部分：计划时间（DateTime格式字符串）
        ///   - 第2部分：进路路径（如 "StationTrack_G3&SingleTrack_XFC"）
        ///   - 第3部分：位置条件（如 "StationTrack_G3" 表示列车必须在G3股道；"NO" 表示无位置限制）
        /// 
        /// 【触发条件】
        /// - 时间条件：当前时间 + AHEAD(3秒) >= 计划时间
        /// - 位置条件（仅发车进路）：列车当前位置 == 指定的股道位置
        /// 
        /// 【示例场景】
        /// 
        /// 示例1：G1234次列车在南京南站下行发车（有位置限制的发车进路）
        /// ┌─────────────────────────────────────────────────────────────────┐
        /// │ 初始状态：                                                        │
        /// │ - timeToInterlock["G1234NJDe下"] =                               │
        /// │   "2025-01-01 10:00:00$StationTrack_G3&SingleTrack_XFC$StationTrack_G3" │
        /// │ - trainToTrack["G123"] = "StationTrack_G3" （列车在G3股道）      │
        /// │ - 当前时间：2025-01-01 09:59:56                                  │
        /// └─────────────────────────────────────────────────────────────────┘
        /// 
        /// 执行流程：
        /// ① 定时器每 0.5 秒（500毫秒）自动调用一次 currentTime_Tick()
        /// ② 遍历 timeToInterlock，找到 "G1234NJDe下"
        /// ③ 解析 Value：
        ///    - Time = 2025-01-01 10:00:00（计划发车时间）
        ///    - r = "StationTrack_G3&SingleTrack_XFC"（进路路径）
        ///    - loc = "StationTrack_G3"（位置条件）
        /// ④ 判断条件：
        ///    - loc != "NO" → 进入位置限制分支
        ///    - DateTime.Now.AddSeconds(3) = 09:59:59 < 10:00:00 → 时间条件不满足
        ///    → 本次不建立进路，继续等待
        /// 
        /// ⑤ 1秒后，当前时间：2025-01-01 09:59:57
        ///    - DateTime.Now.AddSeconds(3) = 10:00:00 >= 10:00:00 → 时间条件满足 ✓
        ///    - trainToTrack["G123"] = "StationTrack_G3" == loc → 位置条件满足 ✓
        ///    - key.Contains("De") → 是发车进路 ✓
        /// ⑥ 检查冲突：isConflict("G1234NJDe下", "StationTrack_G3&SingleTrack_XFC")
        /// ⑦ 建立进路：lockRoute("StationTrack_G3&SingleTrack_XFC")
        ///    - 锁闭相关道岔
        ///    - 设置信号机状态
        ///    - 占用相关轨道
        /// ⑧ 发送RBC消息："10|G123|G123|NJ|0|7|1"（告知RBC进路已建立）
        /// ⑨ 记录删除：Deletestring[0] = "G1234NJDe下"
        /// ⑩ 清理计划：timeToInterlock.Remove("G1234NJDe下")
        /// 
        /// ┌─────────────────────────────────────────────────────────────────┐
        /// │ 示例2：G5678次列车定时进路（无位置限制）                         │
        /// │ - Key: "G5678NJAr上"                                             │
        /// │ - Value: "2025-01-01 11:00:00$StationTrack_G5&SingleTrack_SJC$NO" │
        /// │ - 触发条件：当前时间 >= 2025-01-01 10:59:57（提前3秒）          │
        /// │ - 执行操作：直接建立进路，无需位置验证                           │
        /// └─────────────────────────────────────────────────────────────────┘
        /// 
        /// ┌─────────────────────────────────────────────────────────────────┐
        /// │ 示例3：时间未到，进路暂不建立                                    │
        /// │ - 计划时间：2025-01-01 15:00:00                                 │
        /// │ - 当前时间：2025-01-01 14:00:00                                 │
        /// │ - 计算：14:00:00 + 3秒 = 14:00:03 < 15:00:00                    │
        /// │ - 结果：时间条件不满足，进路保留在 timeToInterlock 中继续等待  │
        /// └─────────────────────────────────────────────────────────────────┘
        /// 
        /// ┌─────────────────────────────────────────────────────────────────┐
        /// │ 示例4：位置不匹配，进路暂不建立                                  │
        /// │ - 计划要求：列车必须在 "StationTrack_G3"                        │
        /// │ - 实际位置：列车在 "StationTrack_G5"                            │
        /// │ - 结果：位置条件不满足，即使时间到了也不建立进路               │
        /// └─────────────────────────────────────────────────────────────────┘
        /// </summary>
        /// <param name="sender">事件发送者（定时器对象）</param>
        /// <param name="e">事件参数</param>
        private void currentTime_Tick(object sender, EventArgs e)
        {
            DateTime Time;                              // 从 timeToInterlock 中解析出的计划时间
            string[] Deletestring = new string[10];    // 用于记录需要删除的进路计划（已成功建立进路的计划）
            int i = 0;                                 // Deletestring 数组的索引
            string r;                                  // 进路路径（如 "StationTrack_G3&SingleTrack_XFC"）
            string loc;                                // 位置条件（如 "StationTrack_G3" 或 "NO"）

            if (!TRAIN_ROUTE.ContainsKey(Train_ID))
            {
                TRAIN_ROUTE.Add(Train_ID, '0');
            }

            // 遍历 timeToInterlock 字典中的所有进路计划
            // Key 示例："G1234NJDe下"（车次号4位 + 车站代码2位 + 进路类型 + 方向）
            // Value 示例："2025-01-01 10:00:00$StationTrack_G3&SingleTrack_XFC$StationTrack_G3"
            foreach (string key in timeToInterlock.Keys)
            {
                // ===== 步骤1：解析 timeToInterlock 中的进路信息 =====
                // 使用 $ 分隔符拆分 Value，获取三个部分：
                // [0] = 计划时间（如 "2025-01-01 10:00:00"）
                // [1] = 进路路径（如 "StationTrack_G3&SingleTrack_XFC"）
                // [2] = 位置条件（如 "StationTrack_G3" 或 "NO"）
                Time = Convert.ToDateTime(((string)timeToInterlock[key]).Split(CSEPARATOR)[0]);
                r = ((string)timeToInterlock[key]).Split(CSEPARATOR)[1];
                loc = ((string)timeToInterlock[key]).Split(CSEPARATOR)[2];

                // ===== 步骤2：判断位置条件类型 =====
                if (loc != "NO")
                {
                    // 【情况A：有位置限制的进路（主要是发车进路）】
                    // 需要同时满足时间条件和位置条件才能建立进路
                    
                    // 触发条件1：时间条件 - 当前时间 + 3秒 >= 计划时间（提前3秒触发）
                    // 触发条件2：位置条件 - 列车必须在指定的股道上
                    // 触发条件3：进路类型 - 必须是发车进路（DEPARTURE）
                    // 
                    // key.Substring(0, ID_LENGTH) 提取车次号（前4位，如 "G123"）
                    // trainToTrack["G1234"] 获取该列车的当前位置（如 "StationTrack_G3"）
                    // 
                    // 示例：如果计划时间是 10:00:00，当前时间是 09:59:57，则满足时间条件
                    //       如果列车G1234在 "StationTrack_G3"，且 loc = "StationTrack_G3"，则满足位置条件
                    if (DateTime.Compare(DateTime.Now.AddSeconds(AHEAD), Time) > 0 &&
                        (string)trainToTrack[key.Substring(0, ID_LENGTH)] == loc && key.Contains(DEPARTURE))
                    {
                        // ===== 步骤3A：建立发车进路 =====
                        // 根据联锁模式选择不同的进路建立方式
                        if (mode_interlock == "normal")
                        {
                            // 【单车模式】一次只允许一列车占用进路
                            // 检查进路是否与已建立的进路冲突（检查道岔、轨道是否被占用）
                            if (isConflict(key, r))
                            {
                                // 建立进路：锁闭相关道岔、设置信号机状态、占用相关轨道
                                lockRoute(r);
                                TRAIN_ROUTE[Train_ID] = "1";

                                
                                // 记录需要删除的进路计划（已成功建立，不再需要保留在 timeToInterlock 中）
                                Deletestring[i] = key;
                                i++;
                                
                                // ===== 向RBC系统发送进路许可消息 =====
                                // 消息格式："10|Train_ID|Train_ID|STATION|进路类型|授权级别|状态"
                                // - "10"：消息类型标识（进路许可消息）
                                // - Train_ID（第1个）：车次号，从key的前4位提取（如 "G123"）
                                // - Train_ID（第2个）：车次号重复（协议要求）
                                // - STATION：车站代码，从key的第5-6位提取（如 "NJ"）
                                // - "0"：进路类型（"0"=定时进路，按计划时间触发）
                                // - "7"：授权级别（最高安全级别）
                                // - "1"：状态标识（进路已建立，状态有效）
                                // 
                                // 示例：G1234次列车在南京南站下行发车
                                //       输入消息："10|G123|G123|NJ|0|7|1"
                                //       含义：G1234次列车在南京南站的发车进路已建立，可以授权列车发车
                                sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + key.Substring(0, ID_LENGTH) + "|" +
                                    key.Substring(0, ID_LENGTH) + "|" + key.Substring(ID_LENGTH, 2) + "|" + "0" + "|" + "7" + "|" + "1"));
                            }
                            else
                            {
                                // 进路冲突：道岔或轨道已被其他列车占用，无法建立进路
                                MessageBox.Show("进路冲突");
                            }
                        }
                        else
                        {
                            // 【多车模式】允许多列车共用进路（适用于追踪运行）
                            // 直接建立进路，不检查冲突（多车模式下的冲突处理由其他机制保证）
                            lockRoute_multi(r);
                            
                            // 向RBC发送进路许可消息
                            sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + key.Substring(0, ID_LENGTH) + "|" +
                                    key.Substring(0, ID_LENGTH) + "|" + key.Substring(ID_LENGTH, 2) + "|" + "0" + "|" + "7" + "|" + "1"));
                            
                            // 记录需要删除的进路计划
                            Deletestring[i] = key;
                            i++;
                        }
                    }
                    // 注意：如果时间或位置条件不满足，该进路计划会继续保留在 timeToInterlock 中，等待下次定时检查
                }
                else
                {
                    // 【情况B：无位置限制的定时进路（loc == "NO"）】
                    // 仅根据时间条件触发，不需要验证列车位置
                    // 适用于不需要位置验证的定时进路计划
                    
                    // 触发条件：当前时间 + 3秒 >= 计划时间（提前3秒触发）
                    if (DateTime.Compare(DateTime.Now.AddSeconds(AHEAD), Time) > 0)
                    {
                        // ===== 步骤3B：建立定时进路 =====
                        if (mode_interlock == "normal")
                        {
                            // 【单车模式】检查冲突后再建立
                            if (isConflict(key, r))
                            {
                                // 建立进路
                                lockRoute(r);
                                
                                // 记录需要删除的进路计划
                                Deletestring[i] = key;
                                i++;
                                // 注意：无位置限制的定时进路通常不发送RBC消息（可能已通过其他方式处理）
                            }
                            else
                            {
                                // 进路冲突
                                MessageBox.Show("进路冲突");
                            }
                        }
                        else
                        {
                            // 【多车模式】直接建立进路
                            lockRoute_multi(r);
                            
                            // 记录需要删除的进路计划
                            Deletestring[i] = key;
                            i++;
                        }
                    }
                }
            }
            
            // ===== 步骤4：清理已成功建立的进路计划 =====
            // 删除已成功建立进路的计划项，避免重复处理
            // 注意：使用延迟删除机制，先记录到 Deletestring 数组，最后统一删除
            // 这样可以在遍历集合时安全地删除元素（避免在遍历过程中修改集合导致异常）
            for (int j = 0; j < Deletestring.Length; j++)
            {
                if (Deletestring[j] != null)
                {
                    timeToInterlock.Remove(Deletestring[j]);
                }
            }
        }
        public void sendMessage(object obj)
        {
            EndPoint point = new IPEndPoint(IPAddress.Parse("192.168.1.7"), 178);
            string message = (string)obj;
            server.SendTo(Encoding.UTF8.GetBytes(message), point);
        }
        public void sendMessageToCTC(object obj)
        {
            EndPoint point = new IPEndPoint(IPAddress.Parse(CTCIP), CTCPORT);
            string message = (string)obj;
            server.SendTo(Encoding.UTF8.GetBytes(message), point);
        }
        public void sendMessageToRBC(object obj)
        {
            EndPoint point = new IPEndPoint(IPAddress.Parse(RBCIP), RBCPORT);
            string message = (string)obj;
            server.SendTo(Encoding.UTF8.GetBytes(message), point);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            lockRoute(textBox1.Text);
            //string Train_ID = '111';

           // sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
                                   // "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
        }

        private void 南京南站_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Signal Signal_CLosing in signals.Values)
            {
                Signal_CLosing.Signal_Shapan_Control_Close(Signal_CLosing.信号灯状态);
            }
        }

        private void label267_Click(object sender, EventArgs e)
        {

        }

        private void label265_Click(object sender, EventArgs e)
        {

        }

        private void label275_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label82_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void Switch_2_G13_Load(object sender, EventArgs e)
        {

        }

        private void Switch_2_G24_Load(object sender, EventArgs e)
        {

        }

        private void Switch_3_G7_Load(object sender, EventArgs e)
        {

        }


        //private void button2_Click(object sender, EventArgs e)
        //{
        // clearRoute(textBox2.Text);
        //if (buttonvalue == 0)
        //{
        //    Switch_3_G13_L.定反位 = Switch_3.DingFan.定位;
        //    buttonvalue = 1;
        // }
        //else
        //{
        //    Switch_3_G13_L.定反位 = Switch_3.DingFan.反位;
        //    buttonvalue = 0;
        //}
        //}


        private void button2_Click(object sender, EventArgs e)
        {
            // clearRoute(textBox2.Text);
            //if (buttonvalue == 0)
            //{
            //    foreach (Control con in this.Controls)
            //    {

            //        // Switch_2_G12_L.定反位 = Switch_2.DingFan.定位;

            //        textBox1.Text = "定位";
            //        if (con.Name.Contains("Switch_3"))
            //        {
            //            Switch_3 switch_3 = (Switch_3)con;
            //            switch_3.定反位 = Switch_3.DingFan.定位;
            //        }
            //        else if (con.Name.Contains("Switch_2"))
            //        {
            //            Switch_2 switch_2 = (Switch_2)con;
            //            switch_2.定反位 = Switch_2.DingFan.定位;
            //        }

            //    }
            //    buttonvalue = 1;
            //}
            //else
            //{
            //    // Switch_2_G12_L.定反位 = Switch_2.DingFan.反位;
            //    buttonvalue = 0;
            //    foreach (Control con in this.Controls)
            //    {
            //        textBox1.Text = "反位";
            //        if (con.Name.Contains("Switch_3"))
            //        {
            //            Switch_3 switch_3 = (Switch_3)con;
            //            switch_3.定反位 = Switch_3.DingFan.反位;
            //        }
            //        else if (con.Name.Contains("Switch_2"))
            //        {
            //            Switch_2 switch_2 = (Switch_2)con;
            //            switch_2.定反位 = Switch_2.DingFan.反位;
            //        }
            //    }
            //}

            /*
            if (buttonvalue == 0)
            {
                Switch_3_G35_L.定反位 = Switch_3.DingFan.定位;
                //Switch_3_G24_L.定反位 = Switch_3.DingFan.定位;
                //Switch_3_G13_L.定反位 = Switch_3.DingFan.定位;
                buttonvalue = 1;
                textBox1.Text = "定位";
            }
            else
            {
                Switch_3_G35_L.定反位 = Switch_3.DingFan.反位;
                //Switch_3_G24_L.定反位 = Switch_3.DingFan.反位;
               // Switch_3_G13_L.定反位 = Switch_3.DingFan.反位;
                buttonvalue = 0;
                textBox1.Text = "反位";
            }
            */
        }


        private void button3_Click(object sender, EventArgs e)
        {
            sendMessage(textBox3.Text);
        }
    }
}
