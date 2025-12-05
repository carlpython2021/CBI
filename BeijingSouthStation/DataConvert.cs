using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanJingNanStation
{
    class DataConvert
    {
        public static string[] station_type = { "BJ", "DZ", "JN", "ZZ", "NJ", "SH", "DC" };
        /// <summary>
        /// 将CBI内部消息格式转换为RBC系统可识别的二进制通信协议格式
        /// 用于向RBC发送行车许可、进路状态等关键安全信息
        /// </summary>
        /// <param name="cbi">CBI内部消息，格式："消息类型|车次|车次|车站代码|进路类型|授权级别|状态"</param>
        /// <returns>176位二进制字符串，符合RBC通信协议标准</returns>
        /// <remarks>
        /// 输入消息格式说明：
        /// - cbi.Split('|')[0]: 消息类型标识 (通常为"10")
        /// - cbi.Split('|')[1]: 车次号 (如"G1234", "D5678", "C2020")
        /// - cbi.Split('|')[2]: 车次号重复 (与字段1相同)
        /// - cbi.Split('|')[3]: 车站代码 ("BJ", "DZ", "JN", "ZZ", "NJ", "SH", "DC")
        /// - cbi.Split('|')[4]: 进路类型 ("0"=定时, "1"=接车, "2"=发车)
        /// - cbi.Split('|')[5]: 授权级别 (通常为"7"=正常授权)
        /// - cbi.Split('|')[6]: 状态标识 (通常为"1"=有效)
        /// 
        /// 输出二进制格式：HEAD(128位) + OBJECT(48位) = 176位
        /// </remarks>
        /// <example>
        /// 输入: "10|G1234|G1234|BJ|1|7|1"
        /// 输出: 176位二进制字符串表示接车进路许可
        /// </example>
        public static string CBIToRBC(string cbi)
        {
            // === 声明所有二进制字段变量 ===
            // HEAD部分字段 (消息头，128位)
            string TYP;              // 消息类型标识 (16位)
            string UV;               // 发送方标识 (32位)  
            string SI;               // 车站标识 (32位)
            string SV;               // 版本信息 (32位)
            string NOO;              // 对象数量 (16位)
            string BAI;              // 填充字段 (16位)
            string HEAD;             // 消息头总和 (128位)

            // OBJECT部分字段 (消息体，48位)
            string OID;              // 对象标识/车次号 (16位)
            string OLEN;             // 对象长度 (16位)
            string SA_TYPE;          // 进路类型 (8位)
            string SA_STATE;         // 进路状态 (8位)
            string DEGRADATION;      // 降级标志 (8位)
            string SA_ID;            // 区段标识 (8位)
            string PROTECT_AREA;     // 保护区域 (8位)
            string SLIP;             // 滑动标志 (8位)
            string OBJECT;           // 消息体总和 (48位)

            string MESSAGE;          // 最终消息 (176位)

            // === 构造消息头 HEAD (128位) ===

            // TYP: 消息类型标识 (16位) = 0x9201
            // 固定值，表示CBI到RBC的行车许可消息类型
            // 十进制37377 → 二进制"1001001000000001"
            TYP = Convert.ToString(0X9201, 2).PadLeft(16, '0');

            // UV: 发送方标识 (32位)
            // 可能取值: 1=RBC系统, 2=CBI系统
            // 这里固定为2，表示消息来源是CBI系统
            UV = Convert.ToString(2, 2).PadLeft(32, '0');

            // SI: 车站标识 (32位)
            // 根据车站代码转换为对应的数字编码
            // cbi.Split('|')[3]: 车站代码字段
            switch (cbi.Split('|')[3])
            {
                case "DC":  // 调度中心
                    // 车站编码: 0
                    SI = Convert.ToString(0, 2).PadLeft(32, '0');
                    break;
                case "BJ":  // 北京南站
                    // 车站编码: 1
                    SI = Convert.ToString(1, 2).PadLeft(32, '0');
                    break;
                case "DZ":  // 德州东站
                    // 车站编码: 2
                    SI = Convert.ToString(2, 2).PadLeft(32, '0');
                    break;
                case "JN":  // 济南西站
                    // 车站编码: 3
                    SI = Convert.ToString(3, 2).PadLeft(32, '0');
                    break;
                case "ZZ":  // 郑州东站
                    // 车站编码: 4
                    SI = Convert.ToString(4, 2).PadLeft(32, '0');
                    break;
                case "NJ":  // 南京南站
                    // 车站编码: 5
                    SI = Convert.ToString(5, 2).PadLeft(32, '0');
                    break;
                case "SH":  // 上海虹桥站
                    // 车站编码: 6
                    SI = Convert.ToString(6, 2).PadLeft(32, '0');
                    break;
                default:   // 未知车站
                    // 默认编码: 10
                    SI = Convert.ToString(10, 2).PadLeft(32, '0');
                    break;
            }

            // SV: 版本信息 (32位) = 0xFFFFFFFF
            // 固定值4294967295 (全1)，表示协议版本
            SV = Convert.ToString(4294967295, 2).PadLeft(32, '0');

            // NOO: 对象数量 (16位) = 1
            // 固定值，表示消息包含1个对象
            NOO = Convert.ToString(1, 2).PadLeft(16, '0');

            // BAI: 填充字段 (16位) = 0xFFFF
            // 固定值65535 (全1)，用于字节对齐
            BAI = Convert.ToString(0XFFFF, 2).PadLeft(16, '0');

            // 组合消息头 (总计128位)
            HEAD = TYP + UV + SI + SV + NOO + BAI;

            // === 构造消息体 OBJECT (48位) ===

            // OID: 对象标识/车次号数字部分 (16位)
            // 从车次号中提取数字部分 (去掉字母前缀)
            // 例: "G1234" → 1234, "D5678" → 5678, "C2020" → 2020
            // cbi.Split('|')[1]: 完整车次号
            // .Substring(1, length-1): 去掉第一个字符(字母)
            // 可能取值: 1-65535 (16位整数范围)
            OID = Convert.ToString(Convert.ToInt32(cbi.Split('|')[1].Substring(1, cbi.Split('|')[1].Length - 1)), 2).PadLeft(16, '0');

            // OLEN: 对象长度 (16位) = 6
            // 固定值，表示对象数据长度为6字节
            // 注释说明: CBI系统固定为6, RBC系统为10
            OLEN = Convert.ToString(6, 2).PadLeft(16, '0');

            // SA_TYPE: 进路类型 (8位)
            // 根据进路类型字段转换为对应编码
            // cbi.Split('|')[4]: 进路类型字段
            switch (cbi.Split('|')[4])
            {
                case "0":  // 定时进路 (按时间自动触发)
                    // 编码值: 3
                    SA_TYPE = Convert.ToString(3, 2).PadLeft(8, '0');
                    break;
                case "1":  // 接车进路 (列车进站)
                    // 编码值: 1
                    SA_TYPE = Convert.ToString(1, 2).PadLeft(8, '0');
                    break;
                case "2":  // 发车进路 (列车出站) 
                    // 编码值: 2
                    SA_TYPE = Convert.ToString(2, 2).PadLeft(8, '0');
                    break;
                default:   // 未知类型
                    // 默认编码: 0
                    SA_TYPE = Convert.ToString(0, 2).PadLeft(8, '0');
                    break;
            }

            // SA_STATE: 进路状态 (8位) = 3
            // 固定值，表示这是列车进路状态
            // 可能取值: 3=列车进路 (正常运营状态)
            SA_STATE = Convert.ToString(3, 2).PadLeft(8, '0');

            // DEGRADATION: 降级标志 (8位) = 0
            // 固定值，表示系统运行正常，无降级
            // 可能取值: 0=正常, 1=降级运行
            DEGRADATION = Convert.ToString(0, 2).PadLeft(8, '0');

            // SA_ID: 区段标识 (8位) = 0
            // 固定值，表示区段编号
            // 在当前实现中为固定值0
            SA_ID = Convert.ToString(0, 2).PadLeft(8, '0');

            // PROTECT_AREA: 保护区域 (8位) = 0
            // 固定值，表示保护区域编号
            // 在当前实现中为固定值0
            PROTECT_AREA = Convert.ToString(0, 2).PadLeft(8, '0');

            // SLIP: 滑动标志 (8位) = 1
            // 固定值，可能与列车防滑功能相关
            // 可能取值: 0=关闭, 1=开启
            SLIP = Convert.ToString(1, 2).PadLeft(8, '0');

            // 组合消息体 (总计48位)
            OBJECT = OID + OLEN + SA_TYPE + SA_STATE + DEGRADATION + SA_ID + PROTECT_AREA + SLIP;

            // === 组合最终消息 (176位) ===
            MESSAGE = HEAD + OBJECT;

            // 注释掉的转换为十六进制的代码
            // 原本可能用于将二进制转换为十六进制格式
            // MESSAGE = string.Format("{0:X}",Convert.ToInt32(MESSAGE, 2));

            // 返回完整的176位二进制字符串
            return MESSAGE;
        }

        /// <summary>
        /// 将RBC系统发送的二进制位置信息转换为CBI系统可识别的标准消息格式
        /// 用于接收来自RBC的列车实时位置更新，实现列车跟踪和位置监控
        /// </summary>
        /// <param name="rbc">RBC发送的十六进制编码字符串，包含列车位置、方向、区域等信息</param>
        /// <returns>CBI标准位置消息，格式："7|车次|车次|位置|方向"</returns>
        /// <remarks>
        /// RBC数据包字段解析：
        /// - rbc.Substring(36, 4): 车次编号字段 (4个十六进制字符)
        /// - rbc.Substring(52, 4): 位置数值字段 (4个十六进制字符)  
        /// - rbc.Substring(12, 8): 区域编号字段 (8个十六进制字符)
        /// 
        /// 输出格式：
        /// - 消息类型: "7" (位置消息标识)
        /// - 车次: "G" + 数字 (如G1234, G5678)
        /// - 位置: 区间格式"S1_16"或车站格式"K+1500"
        /// - 方向: "0"=上行, "1"=下行
        /// </remarks>
        /// <example>
        /// 输入: RBC十六进制字符串
        /// 输出: "7|G1234|G1234|S1_16|0"
        /// </example>
        public static string RBCToCBI(string rbc)
        {
            // === 从RBC数据包中提取车次号信息 ===
            // 从位置36开始的4个字符转换为车次号
            // rbc.Substring(36, 4): 提取4个十六进制字符
            // "0x" + 字符串: 构造十六进制数字格式
            // Convert.ToInt32("0x...", 16): 十六进制转十进制
            // 示例: "04D2" → 0x04D2 → 1234 → "G1234"
            // 可能取值: G1-G65535 (16位整数范围内的车次号)
            string Train_ID = "G" + Convert.ToString(Convert.ToInt32("0x" + rbc.Substring(36, 4), 16));

            // === 解析列车运行方向 ===
            // 根据车次号的奇偶性判断运行方向
            // 车次号数值提取: 同上面的转换过程
            string Train_Direction;
            int trainNumber = Convert.ToInt32("0x" + rbc.Substring(36, 4), 16);
            if (trainNumber % 2 == 0)
            {
                // 偶数车次 = 上行方向 (如G1234, G5678)
                // 可能取值: "0" (上行，通常指向北京方向)
                Train_Direction = "0";
            }
            else
            {
                // 奇数车次 = 下行方向 (如G1235, G5679)
                // 可能取值: "1" (下行，通常指向上海方向)
                Train_Direction = "1";
            }

            // === 提取位置和区域信息 ===
            // train_position: 列车在当前区域内的具体位置数值
            // rbc.Substring(52, 4): 位置字段 (4个十六进制字符)
            // 可能取值: 0-65535 (16位无符号整数)
            int train_position = Convert.ToUInt16("0x" + rbc.Substring(52, 4), 16);

            // train_area: 列车所在的区域编号
            // rbc.Substring(12, 8): 区域字段 (8个十六进制字符)
            // 可能取值: 0=区间, 1-6=车站内不同区域
            int train_area = Convert.ToInt32("0x" + rbc.Substring(12, 8), 16);

            // Train_Position: 最终的位置描述字符串
            // 初始值为位置数值的字符串形式，后续根据区域和方向重新计算
            string Train_Position = Convert.ToString(Convert.ToInt32("0x" + rbc.Substring(52, 4), 16));

            // === 位置编码转换 ===
            if (train_area == 0)  // 区间内位置
            {
                // === 上行方向区间位置编码 ===
                if (Train_Direction == "0" && train_position > 44)
                {
                    // S1区间: 位置45及以上
                    // 格式: "S1_相对位置"
                    // 示例: position=60 → "S1_16"
                    // 可能取值: "S1_1" 到 "S1_N" (N取决于区间长度)
                    Train_Position = "S1_" + Convert.ToString(train_position - 44);
                }
                else if (Train_Direction == "0" && train_position > 38 && train_position <= 44)
                {
                    // S2区间: 位置39-44
                    // 格式: "S2_相对位置"
                    // 示例: position=42 → "S2_4"  
                    // 可能取值: "S2_1" 到 "S2_6"
                    Train_Position = "S2_" + Convert.ToString(train_position - 38);
                }
                else if (Train_Direction == "0" && train_position > 25 && train_position <= 38)
                {
                    // S3区间: 位置26-38
                    // 格式: "S3_相对位置"
                    // 示例: position=30 → "S3_5"
                    // 可能取值: "S3_1" 到 "S3_13"
                    Train_Position = "S3_" + Convert.ToString(train_position - 25);
                }
                else if (Train_Direction == "0" && train_position > 11 && train_position <= 25)
                {
                    // S4区间: 位置12-25
                    // 格式: "S4_相对位置"
                    // 示例: position=20 → "S4_9"
                    // 可能取值: "S4_1" 到 "S4_14"
                    Train_Position = "S4_" + Convert.ToString(train_position - 11);
                }
                else if (Train_Direction == "0" && train_position <= 11)
                {
                    // S5区间: 位置1-11
                    // 格式: "S5_绝对位置"
                    // 示例: position=8 → "S5_8"
                    // 可能取值: "S5_1" 到 "S5_11"
                    Train_Position = "S5_" + Convert.ToString(train_position);
                }

                // === 下行方向区间位置编码 ===
                else if (Train_Direction == "1" && train_position > 52)
                {
                    // X5区间: 位置53及以上
                    // 格式: "X5_相对位置"
                    // 示例: position=60 → "X5_8"
                    // 可能取值: "X5_1" 到 "X5_N"
                    Train_Position = "X5_" + Convert.ToString(train_position - 52);
                }
                else if (Train_Direction == "1" && train_position > 38 && train_position <= 52)
                {
                    // X4区间: 位置39-52
                    // 格式: "X4_相对位置"
                    // 示例: position=45 → "X4_7"
                    // 可能取值: "X4_1" 到 "X4_14"
                    Train_Position = "X4_" + Convert.ToString(train_position - 38);
                }
                else if (Train_Direction == "1" && train_position > 25 && train_position <= 38)
                {
                    // X3区间: 位置26-38
                    // 格式: "X3_相对位置"
                    // 示例: position=30 → "X3_5"
                    // 可能取值: "X3_1" 到 "X3_13"
                    Train_Position = "X3_" + Convert.ToString(train_position - 25);
                }
                else if (Train_Direction == "1" && train_position > 19 && train_position <= 25)
                {
                    // X2区间: 位置20-25
                    // 格式: "X2_相对位置"
                    // 示例: position=23 → "X2_4"
                    // 可能取值: "X2_1" 到 "X2_6"
                    Train_Position = "X2_" + Convert.ToString(train_position - 19);
                }
                else if (Train_Direction == "1" && train_position <= 19)
                {
                    // X1区间: 位置1-19
                    // 格式: "X1_绝对位置"
                    // 示例: position=15 → "X1_15"
                    // 可能取值: "X1_1" 到 "X1_19"
                    Train_Position = "X1_" + Convert.ToString(train_position);
                }
            }
            else  // 车站内位置 (train_area != 0)
            {
                // === 车站内位置编码 ===
                // 根据区域编号和运行方向计算绝对坐标
                switch (train_area)
                {
                    case 1:  // 区域1: 坐标范围0-2400米
                        if (Train_Direction == "0")  // 上行方向
                        {
                            // 上行: 起始坐标 + 位置偏移
                            // 可能取值: 0-2400
                            Train_Position = Convert.ToString(0 + train_position);
                        }
                        else  // 下行方向
                        {
                            // 下行: 终止坐标 - 位置偏移 (反向计算)
                            // 可能取值: 0-2400
                            Train_Position = Convert.ToString(2400 - train_position);
                        }
                        // 添加车站内位置标识
                        // 最终格式: "K+坐标" (K表示公里标)
                        Train_Position = "K+" + Train_Position;
                        break;

                    case 2:  // 区域2: 坐标范围2400-4800米
                        if (Train_Direction == "0")
                        {
                            // 可能取值: 2400-4800
                            Train_Position = Convert.ToString(2400 + train_position);
                        }
                        else
                        {
                            // 可能取值: 2400-4800
                            Train_Position = Convert.ToString(4800 - train_position);
                        }
                        Train_Position = "K+" + Train_Position;
                        break;

                    case 3:  // 区域3: 坐标范围4800-7000米
                        if (Train_Direction == "0")
                        {
                            // 可能取值: 4800-7000
                            Train_Position = Convert.ToString(4800 + train_position);
                        }
                        else
                        {
                            // 可能取值: 4800-7000
                            Train_Position = Convert.ToString(7000 - train_position);
                        }
                        Train_Position = "K+" + Train_Position;
                        break;

                    case 4:  // 区域4: 坐标范围7000-9200米
                        if (Train_Direction == "0")
                        {
                            // 可能取值: 7000-9200
                            Train_Position = Convert.ToString(7000 + train_position);
                        }
                        else
                        {
                            // 可能取值: 7000-9200
                            Train_Position = Convert.ToString(9200 - train_position);
                        }
                        Train_Position = "K+" + Train_Position;
                        break;

                    case 5:  // 区域5: 坐标范围9200-11600米
                        if (Train_Direction == "0")
                        {
                            // 可能取值: 9200-11600
                            Train_Position = Convert.ToString(9200 + train_position);
                        }
                        else
                        {
                            // 可能取值: 9200-11600
                            Train_Position = Convert.ToString(11600 - train_position);
                        }
                        Train_Position = "K+" + Train_Position;
                        break;

                    case 6:  // 区域6: 坐标范围11600-14000米
                        if (Train_Direction == "0")
                        {
                            // 可能取值: 11600-14000
                            Train_Position = Convert.ToString(11600 + train_position);
                        }
                        else
                        {
                            // 可能取值: 11600-14000
                            Train_Position = Convert.ToString(14000 - train_position);
                        }
                        Train_Position = "K+" + Train_Position;
                        break;
                }
            }

            // === 构造并返回CBI标准位置消息 ===
            // 格式: "消息类型|车次|车次重复|位置|方向"
            // 示例: "7|G1234|G1234|S1_16|0"
            // 字段说明:
            // - "7": 位置消息类型标识 (固定值)
            // - Train_ID: 车次号 (如"G1234", "G5678")
            // - Train_ID: 车次号重复 (用于校验)
            // - Train_Position: 位置信息 (区间格式或车站格式)
            // - Train_Direction: 运行方向 ("0"=上行, "1"=下行)
            return "7" + "|" + Train_ID + "|" + Train_ID + "|" + Train_Position + "|" + Train_Direction;
        }

        //  sendMessageToRBC(DataConvert.CBIToRBC("10" + "|" + Train_ID +
        // "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));
        // sendMessageToRBC(DataConvert.CBIToTCC("101010101010" + "|" + Train_ID +
        // "|" + Train_ID + "|" + STOPSTATION + "|" + "1" + "|" + "7" + "|" + "1"));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cbi"></param>
        /// <returns></returns>
        //cbi ( 4*16 区间方向控制命令 | 引导进路标志(0/1)+进路性质(0/1)+ 信号机点灯灭灯(2位)+颜色(6位)+20个区段锁闭状态(0/1) 重复40次 )
        public static string CBIToTCC(string cbi)
        {
            // === 声明所有二进制字段变量 ===
            // HEAD部分字段 (消息头，128位)
            string SDCC = "";    // 区间方向控制命令 Section direction control command (256位)
            string RI = "";      // 进路信息 Route information (2560位)  
            string RB;           // 进站信号机红灯断丝信息 Entry signal red light broken wire information (64位)
            string SSS;          // 信号机调车状态 Signal shunting status (128位)
            string RS;           // 对象预留 Reserved数量 (528位)
            string IVVA;         // 接口版本校验信息 Interface version verification information (64位)
            string MESSAGE;      // 最终消息

            // 声明临时变量
            string temp0, temp1;
            string segment;
            string num, type, sign, station_num;
            string JINLU;

            // === 构造消息头 HEAD (128位) ===

            // TYP = Convert.ToString(0X9201, 2).PadLeft(16, '0');
            // UV = Convert.ToString(2, 2).PadLeft(32, '0');
            temp0 = cbi.Split('|')[0];
            for (int i = 0; i < temp0.Length; i++)
            {
                if (temp0[i] == '0')
                {
                    SDCC += "0101";
                }
                else if (temp0[i] == '1')
                {
                    SDCC += "1010";
                }
            }
            temp1 = cbi.Split('|')[1];

            // | 引导进路标志(0/1)+进路性质(0/1)+ 信号机点灯灭灯(2位)+颜色(6位)+20个区段锁闭状态(0/1) 重复40次
            for (int i = 0; i < 40; i++)
            {
                // 每次从temp1中取30个字符进行处理
                // string segment = "";
                // int startIndex = i * 30;
                // if (startIndex < temp1.Length)
                // {
                //     int length = Math.Min(30, temp1.Length - startIndex);
                //     segment = temp1.Substring(startIndex, length);
                // }
                segment = temp1.Substring(i * 30, 30);
                num = Convert.ToString(i + 1, 2).PadLeft(10, '0');  // 添加分号

                if (segment[0] == '0')
                {
                    type = "0";
                }
                else if (segment[0] == '1')
                {
                    type = "1";
                }
                else
                {
                    type = "0";  // 默认值
                }

                if (segment[1] == '0')
                {
                    sign = "00";
                }
                else if (segment[1] == '1')
                {
                    sign = "01";
                }
                else
                {
                    sign = "00";  // 默认值
                }

                station_num = Convert.ToString(0, 2).PadLeft(3, '0');  // 添加分号
                string lock_state = "";  // 初始化字符串

                for (int j = 0; j < 20; j++)  // 修改变量名避免冲突
                {
                    if (segment[10 + j] == '1')
                    {
                        lock_state += "10";  // 修正字符串连接
                    }
                    else
                    {
                        lock_state += "01";  // 修正字符串连接
                    }
                }

                JINLU = station_num + type + sign + num + segment.Substring(2, 8) + lock_state;
                RI += JINLU;  // 将JINLU添加到RI中
            }

            // RB = Convert.ToString(3, 2).PadLeft(8, '0');

            // RB: 64位全1的二进制数
            RB = Convert.ToString(-1, 2).Substring(32).PadLeft(64, '1');
            // SSS: 128位全1的二进制数
            SSS = new string('0', 128);
            RS = new string('0', 528);
            // 修正IVVA字符串连接
            IVVA = Convert.ToString(1, 2).PadLeft(8, '0') + Convert.ToString(0, 2).PadLeft(24, '0') + Convert.ToString(1, 2).PadLeft(8, '0') + Convert.ToString(0, 2).PadLeft(24, '0');
            // IVVA = new string('1', 64);

            MESSAGE = SDCC + RI + RB + SSS + RS + IVVA;
            return MESSAGE;
        }

        //cbi ( 4*16 区间方向控制命令 | 引导进路标志(0/1)+进路性质(0/1)+ 信号机点灯灭灯(2位)+颜色(6位)+20个区段锁闭状态(0/1) 重复40次 )


        public static string TCCToCBI(string tcc)
        {
            // === 从TCC数据包中提取车次号信息 ===
            // 从位置36开始的4个字符转换为车次号
            // tcc.Substring(36, 4): 提取4个十六进制字符
            // "0x" + 字符串: 构造十六进制数字格式
            // Convert.ToInt32("0x...", 16): 十六进制转十进制
            // 示例: "04D2" → 0x04D2 → 1234 → "G1234"
            // 可能取值: G1-G65535 (16位整数范围内的车次号)
            string Train_ID = "G" + Convert.ToString(Convert.ToInt32("0x" + tcc.Substring(36, 4), 16));

            Console.WriteLine("车次：" + Train_ID);


            // === 解析列车运行方向 ===
            // 根据车次号的奇偶性判断运行方向
            // 车次号数值提取: 同上面的转换过程
            string Train_Direction;
            int trainNumber = Convert.ToInt32("0x" + tcc.Substring(36, 4), 16);
            if (trainNumber % 2 == 0)
            {
                // 偶数车次 = 上行方向 (如G1234, G5678)
                Train_Direction = "0";
                Console.WriteLine("方向：上行");

            }
            else
            {
                // 奇数车次 = 下行方向 (如G1235, G5679)
                Train_Direction = "1";
                Console.WriteLine("方向：下行");

            }

            string QF = tcc.Substring(0, 256);
            // === 解析QF区段发车口信息 (32字节，16个发车口) ===
            // QF: 256位二进制数据，每16位表示一个发车口的完整信息
            // 每个发车口包含2个字节：字节0(YFJ+QJZT+JQJ) + 字节1(FJ+辅助办理表示灯)
            string qf_summary = "";  // QF区段发车口信息汇总字符串

            for (int i = 0; i < 16; i++)  // 遍历16个发车口
            {
                // 提取当前发车口的16位数据 (2个字节)
                string departureBits = QF.Substring(i * 16, 16);

                // === 解析字节0 (前8位): YFJ + QJZT + JQJ ===
                string byte0 = departureBits.Substring(0, 8);

                // YFJ信息 (0-3bit)
                string yfj = byte0.Substring(4, 4);  // 后4位
                string yfj_status;
                string yfj_code;
                if (yfj == "1010")
                {
                    yfj_status = "允许发车";
                    yfj_code = "1";
                }
                else
                {
                    yfj_status = "禁止发车";  // 其他情况按禁止发车处理
                    yfj_code = "0";
                }

                // QJZT信息 (4-5bit)
                string qjzt = byte0.Substring(2, 2);  // 第2-3位
                string qjzt_status;
                string qjzt_code;
                if (qjzt == "10")
                {
                    qjzt_status = "空闲";
                    qjzt_code = "1";
                }
                else
                {
                    qjzt_status = "占用";  // 其他情况按占用处理
                    qjzt_code = "0";
                }

                // JQJ信息 (6-7bit)
                string jqj = byte0.Substring(0, 2);  // 前2位
                string jqj_status;
                string jqj_code;
                if (jqj == "10")
                {
                    jqj_status = "空闲且未锁闭";
                    jqj_code = "1";
                }
                else
                {
                    jqj_status = "占用或锁闭";  // 其他情况为占用或锁闭
                    jqj_code = "0";
                }
                //Console.WriteLine(yfj_status+ qjzt_status + jqj_status);


                // === 解析字节1 (后8位): FJ + 辅助办理表示灯 ===
                string byte1 = departureBits.Substring(8, 8);

                // FJ信息 (0-3bit)
                string fj = byte1.Substring(4, 4);  // 后4位
                string fj_status;
                string fj_code;
                if (fj == "0101")
                {
                    fj_status = "发车方向";
                    fj_code = "0";
                }
                else if (fj == "1010")
                {
                    fj_status = "借车方向";
                    fj_code = "1";
                }
                else
                {
                    fj_status = "无方向";  // 其他情况为无方向
                    fj_code = "2";
                }

                // 辅助办理表示灯信息 (4-7bit)
                string light = byte1.Substring(0, 4);  // 前4位
                string light_status;
                string light_code;
                if (light == "0011")
                {
                    light_status = "闪灯";
                    light_code = "1";
                }
                else if (light == "1100")
                {
                    light_status = "稳亮灯";
                    light_code = "2";
                }
                else
                {
                    light_status = "灭灯";  // 其他情况按灭灯处理
                    light_code = "0";
                }

                // 输出当前发车口解析结果 (可选)
                Console.WriteLine($"发车口{i + 1}: YFJ={yfj_status}, QJZT={qjzt_status}, JQJ={jqj_status}, FJ={fj_status}, 表示灯={light_status}");

                // 将当前发车口的状态编码汇总 (格式: YFJ+QJZT+JQJ+FJ+灯状态)
                qf_summary += yfj_code + qjzt_code + jqj_code + fj_code + light_code;
            }


            // QB: 闭塞分区状态信息 (540位)
            string QB = tcc.Substring(256, 540);

            // 解析QB闭塞分区状态
            // 540位 = 60字节，每字节8位，每2位表示一个闭塞分区状态
            // 状态编码: 10=空闲(0), 01=占用(1), 00=故障占用(2), 11=失去分路(3)
            string qb_summary = "";

            // 遍历60个字节
            for (int i = 0; i < 60; i++)
            {
                // 提取当前字节的8位
                string currentByte = QB.Substring(i * 8, 8);

                // 每个字节包含4个闭塞分区状态（每2位一个）
                for (int j = 0; j < 4; j++)
                {
                    // 提取当前闭塞分区的2位状态
                    string blockStatus = currentByte.Substring(j * 2, 2);

                    // 根据状态编码转换为数字代码
                    switch (blockStatus)
                    {
                        case "10":  // 空闲状态
                            qb_summary += "0";
                            break;
                        case "01":  // 占用状态
                            qb_summary += "1";
                            break;
                        case "00":  // 故障占用（预留）
                            qb_summary += "2";
                            break;
                        case "11":  // 失去分路（预留）
                            qb_summary += "3";
                            break;
                        default:    // 未知状态
                            qb_summary += "2";  // 按故障处理
                            break;
                    }
                }
            }

            // SP: 信号限速信息 (80位)
            string SP = tcc.Substring(540, 80);

            // 解析SP信号限速信息
            // 80位 = 10字节，每字节8位，每2位表示一个信号限速状态
            // 状态编码: 01=无限速, 10=有限速, 其他=有限速(按有限速处理)
            string sp_summary = "";

            // 遍历10个字节
            for (int i = 0; i < 10; i++)
            {
                // 提取当前字节的8位
                string currentByte = SP.Substring(i * 8, 8);

                // 每个字节包含4个信号限速状态（每2位一个）
                for (int j = 0; j < 4; j++)
                {
                    // 提取当前信号的2位状态
                    string signalStatus = currentByte.Substring(j * 2, 2);

                    // 根据状态编码转换
                    switch (signalStatus)
                    {
                        case "01":  // 无限速
                            sp_summary += "0";
                            break;
                        case "10":  // 有限速
                            sp_summary += "1";
                            break;
                        default:    // 其他情况按有限速处理
                            sp_summary += "1";
                            break;
                    }
                }
            }

            string XR = tcc.Substring(620, 128);
            // 解析XR信号机灯丝熔断信息
            // 128位 = 16字节，每字节8位表示一个区间的灯丝熔断信息
            // 第1字节: 发车口1区间, 第2字节: 发车口2区间, 第3字节: 发车口3区间, 第4字节: 发车口4区间
            // 每个字节中: 0-1bit=防护信号机1, 2-3bit=防护信号机2, 4-5bit=防护信号机3, 6-7bit=预留
            // 状态编码: 00=断丝, 11=正常, 01=无配置, 其他=断丝
            string xr_summary = "";

            // 遍历16个字节 (16个区间)
            for (int i = 0; i < 16; i++)
            {
                // 提取当前字节的8位
                string currentByte = XR.Substring(i * 8, 8);

                // 解析3个防护信号机的熔断状态 (忽略6-7bit预留位)
                for (int j = 0; j < 3; j++)
                {
                    // 提取当前信号机的2位状态
                    string signalStatus = currentByte.Substring(j * 2, 2);

                    // 根据状态编码转换
                    switch (signalStatus)
                    {
                        case "00":  // 断丝
                            xr_summary += "0";
                            break;
                        case "11":  // 正常
                            xr_summary += "1";
                            break;
                        case "01":  // 无配置
                            xr_summary += "2";
                            break;
                        default:    // 其他情况按断丝处理
                            xr_summary += "0";
                            break;
                    }
                }
            }
            string OI = tcc.Substring(748, 240);
            // 解析OI灾害信息
            // 240位 = 30字节，每字节8位，每2位表示一个灾害状态
            // 状态编码: 01=无灾害, 10=有灾害, 其他=有灾害(按有灾害处理)
            string oi_summary = "";

            // 遍历30个字节
            for (int i = 0; i < 30; i++)
            {
                // 提取当前字节的8位
                string currentByte = OI.Substring(i * 8, 8);

                // 每个字节包含4个灾害状态（每2位一个）
                for (int j = 0; j < 4; j++)
                {
                    // 提取当前灾害的2位状态
                    string disasterStatus = currentByte.Substring(j * 2, 2);

                    // 根据状态编码转换
                    switch (disasterStatus)
                    {
                        case "01":  // 无灾害
                            oi_summary += "0";
                            break;
                        case "10":  // 有灾害
                            oi_summary += "1";
                            break;
                        default:    // 其他情况按有灾害处理
                            oi_summary += "1";
                            break;
                    }
                }
            }

            string QD = tcc.Substring(968, 128);
            // 解析QD发车口信息
            // 128位 = 16字节，每个字节包含一个发车口信息
            // 每个字节8位：bit0-3=YFJ信息, bit4-5=QJZT信息, bit6-7=预留
            string qd_summary = "";

            // 遍历16个字节（16个发车口）
            for (int i = 0; i < 16; i++)
            {
                // 提取当前字节的8位
                string currentByte = QD.Substring(i * 8, 8);

                // 解析YFJ信息 (bit0-3)
                string yfj = currentByte.Substring(4, 4);  // 后4位
                string yfj_code;
                if (yfj == "1010")
                {
                    yfj_code = "1";  // 允许发车
                }
                else
                {
                    yfj_code = "0";  // 禁止发车
                }

                // 解析QJZT信息 (bit4-5)
                string qjzt = currentByte.Substring(2, 2);  // 第2-3位
                string qjzt_code;
                if (qjzt == "10")
                {
                    qjzt_code = "1";  // 空闲
                }
                else
                {
                    qjzt_code = "0";  // 占用
                }

                // 组合该发车口的状态信息
                qd_summary += yfj_code + qjzt_code;
            }
            string GS = tcc.Substring(1096, 256);
            string RB = tcc.Substring(1352, 128);
            string RS = tcc.Substring(1480, 640);

            // IVVA: 接口版本校验信息 (64位)
            string IVVA = tcc.Substring(2120, 64);

            // 解析IVVA版本信息
            // 64位 = 8字节，分为两组版本号
            // 前4字节: 第一个版本号 (v?.?.?.?)
            // 后4字节: 第二个版本号 (v?.?.?.?)
            string version1 = "";
            string version2 = "";

            // 解析前4个字节为第一个版本号
            for (int i = 0; i < 4; i++)
            {
                // 提取当前字节的8位
                string currentByte = IVVA.Substring(i * 8, 8);

                // 将二进制转换为十进制
                int versionPart = Convert.ToInt32(currentByte, 2);

                if (i == 0)
                {
                    version1 = versionPart.ToString();
                }
                else
                {
                    version1 += "." + versionPart.ToString();
                }
            }

            // 解析后4个字节为第二个版本号
            for (int i = 4; i < 8; i++)
            {
                // 提取当前字节的8位
                string currentByte = IVVA.Substring(i * 8, 8);

                // 将二进制转换为十进制
                int versionPart = Convert.ToInt32(currentByte, 2);

                if (i == 4)
                {
                    version2 = versionPart.ToString();
                }
                else
                {
                    version2 += "." + versionPart.ToString();
                }
            }

            string ivva_summary = "v" + version1 + " v" + version2;
            /*
            for (int i = 0; i < 10; i++)
            {
                // 提取当前字节的8位
                string currentByte = SP.Substring(i * 8, 8);
                
                // 每个字节包含4个信号限速状态（每2位一个）
                for (int j = 0; j < 4; j++)
                {
                    // 提取当前信号的2位状态
                    string signalStatus = currentByte.Substring(j * 2, 2);
                    
                    // 根据状态编码转换
                    switch (signalStatus)
                    {
                        case "01":  // 无限速
                            sp_summary += "0";
                            break;
                        case "10":  // 有限速
                            sp_summary += "1";
                            break;
                        default:    // 其他情况按有限速处理
                            sp_summary += "1";
                            break;
                    }
                }
            }
            */

            // === 初始化可能未定义的变量 ===
            // 确保所有变量都有默认值，避免编译错误


            // === 构造最终的CBI消息 ===
            // 将所有解析的信息组合成CBI格式的消息
            string MESSAGE = Train_ID + "|" + Train_Direction + "|" + qf_summary + "|" + qb_summary + "|" + sp_summary + "|" + xr_summary + "|" + oi_summary + "|" + qd_summary;

            return MESSAGE;
        }


        public static string subCTCToCTC(string train_id, string da, string station, DateTime plan, DateTime actual, int stationtrack)
        {
            string TYPE, FUNC_CODE, VERSION, PACKAGE_TOTALNUM, PACKAGE_NUM, WORD_LENGTH, QUERY_COMMAND_INDEX, BUREAU_NUM,
                TIME_YEAR, TIME_MONTH, TIME_DAY, TIME_HOUR, TIME_MINUTE, TIME_SECOND, TRAIN_NUM, SCHEDULE_SEGMENT, TRAIN_ID,
                TRAIN_ID_LENGTH, STATION_NUM, STATION_TRACK_NUM, REPORT_ATTRIBUTE, REPORT_TYPE, DA_MONTH, DA_DAY, DA_HOUR, DA_MINUTE,
                DA_SECOND, DA_YEAR, ENTRANCE, EXIT, BIAS_TIME, MESSAGE;

            TYPE = Convert.ToString(0X91, 2).PadLeft(8, '0');
            FUNC_CODE = Convert.ToString(0X00, 2).PadLeft(8, '0');
            VERSION = Convert.ToString(0X02, 2).PadLeft(8, '0');
            PACKAGE_TOTALNUM = Convert.ToString(1, 2).PadLeft(16, '0');
            PACKAGE_NUM = Convert.ToString(1, 2).PadLeft(16, '0');
            WORD_LENGTH = Convert.ToString(0, 2).PadLeft(16, '0');
            QUERY_COMMAND_INDEX = Convert.ToString(0, 2).PadLeft(32, '0');
            BUREAU_NUM = Convert.ToString(0, 2).PadLeft(8, '0');

            TIME_YEAR = Convert.ToString(DateTime.Now.Year, 2).PadLeft(16, '0');
            TIME_MONTH = Convert.ToString(DateTime.Now.Month, 2).PadLeft(8, '0');
            TIME_DAY = Convert.ToString(DateTime.Now.Day, 2).PadLeft(8, '0');
            TIME_HOUR = Convert.ToString(DateTime.Now.Hour, 2).PadLeft(8, '0');
            TIME_MINUTE = Convert.ToString(DateTime.Now.Minute, 2).PadLeft(8, '0');
            TIME_SECOND = Convert.ToString(DateTime.Now.Second, 2).PadLeft(8, '0');

            TRAIN_NUM = Convert.ToString(1, 2).PadLeft(8, '0');
            SCHEDULE_SEGMENT = Convert.ToString(1, 2).PadLeft(16, '0');
            TRAIN_ID = Convert.ToString(Convert.ToUInt32(train_id.Substring(1, train_id.Length - 1)), 2).PadLeft(32, '0');//车次号数字
            TRAIN_ID_LENGTH = Convert.ToString(0, 2).PadLeft(8, '0');

            switch (station)//BJ:1;YL:3;WQ:4;TJ:6
            {
                case "BJ":
                    STATION_NUM = Convert.ToString(1, 2).PadLeft(16, '0');
                    break;
                case "DZ":
                    STATION_NUM = Convert.ToString(2, 2).PadLeft(16, '0');
                    break;
                case "JN":
                    STATION_NUM = Convert.ToString(3, 2).PadLeft(16, '0');
                    break;
                case "ZZ":
                    STATION_NUM = Convert.ToString(4, 2).PadLeft(16, '0');
                    break;
                case "NJ":
                    STATION_NUM = Convert.ToString(5, 2).PadLeft(16, '0');
                    break;
                case "SH":
                    STATION_NUM = Convert.ToString(6, 2).PadLeft(16, '0');
                    break;
                case "DC":
                    STATION_NUM = Convert.ToString(7, 2).PadLeft(16, '0');
                    break;
                default:
                    STATION_NUM = Convert.ToString(0, 2).PadLeft(16, '0');
                    break;
            }

            STATION_TRACK_NUM = Convert.ToString(stationtrack, 2).PadLeft(8, '0');
            REPORT_ATTRIBUTE = Convert.ToString(1, 2).PadLeft(8, '0');

            if (actual > plan)
            {
                if (da == "0")
                {
                    REPORT_TYPE = Convert.ToString(0X07, 2).PadLeft(8, '0');
                }
                else
                {
                    REPORT_TYPE = Convert.ToString(0X08, 2).PadLeft(8, '0');
                }

                BIAS_TIME = Convert.ToString(actual.Subtract(plan).Hours, 2).PadLeft(8, '0') +
                    Convert.ToString(actual.Subtract(plan).Minutes, 2).PadLeft(8, '0') +
                    Convert.ToString(actual.Subtract(plan).Seconds, 2).PadLeft(8, '0');
            }
            else if (actual == plan)
            {
                if (da == "0")
                {
                    REPORT_TYPE = Convert.ToString(0X01, 2).PadLeft(8, '0');
                }
                else
                {
                    REPORT_TYPE = Convert.ToString(0X02, 2).PadLeft(8, '0');
                }
                BIAS_TIME = Convert.ToString(0, 2).PadLeft(8, '0') +
                    Convert.ToString(0, 2).PadLeft(8, '0') +
                    Convert.ToString(0, 2).PadLeft(8, '0');
            }
            else
            {
                if (da == "0")
                {
                    REPORT_TYPE = Convert.ToString(0X04, 2).PadLeft(8, '0');
                }
                else
                {
                    REPORT_TYPE = Convert.ToString(0X05, 2).PadLeft(8, '0');
                }
                BIAS_TIME = Convert.ToString(plan.Subtract(actual).Hours, 2).PadLeft(8, '0') +
                    Convert.ToString(plan.Subtract(actual).Minutes, 2).PadLeft(8, '0') +
                    Convert.ToString(plan.Subtract(actual).Seconds, 2).PadLeft(8, '0');
            }

            DA_YEAR = Convert.ToString(actual.Year, 2).PadLeft(16, '0');
            DA_MONTH = Convert.ToString(actual.Month, 2).PadLeft(8, '0');
            DA_DAY = Convert.ToString(actual.Day, 2).PadLeft(8, '0');
            DA_HOUR = Convert.ToString(actual.Hour, 2).PadLeft(8, '0');
            DA_MINUTE = Convert.ToString(actual.Minute, 2).PadLeft(8, '0');
            DA_SECOND = Convert.ToString(actual.Second, 2).PadLeft(8, '0');

            ENTRANCE = Convert.ToString(0, 2).PadLeft(16, '0');
            EXIT = Convert.ToString(0, 2).PadLeft(16, '0');

            MESSAGE = TYPE + FUNC_CODE + VERSION + PACKAGE_TOTALNUM + PACKAGE_NUM + WORD_LENGTH + QUERY_COMMAND_INDEX + BUREAU_NUM +
                TIME_YEAR + TIME_MONTH + TIME_DAY + TIME_HOUR + TIME_MINUTE + TIME_SECOND + TRAIN_NUM + SCHEDULE_SEGMENT + TRAIN_ID +
                TRAIN_ID_LENGTH + STATION_NUM + STATION_TRACK_NUM + REPORT_ATTRIBUTE + REPORT_TYPE + DA_YEAR + DA_MONTH + DA_DAY +
                DA_HOUR + DA_MINUTE + DA_SECOND + ENTRANCE + EXIT + BIAS_TIME;

            return MESSAGE;
        }


        public static string CTCTosubCTC(string ctc)
        {
            string MESSAGE = "";
            int stationnum = (ctc.Length - 76) / 54;
            ScheduleInfo[] si = new ScheduleInfo[stationnum];
            string Train_ID = "G" + Convert.ToString(Convert.ToInt32("0x" + ctc.Substring(54, 8), 16));
            string Train_Direction = Convert.ToString(Convert.ToInt32("0x" + ctc.Substring(64, 2), 16));
            for (int i = 1; i <= stationnum; i++)
            {
                si[i - 1].stationname = station_type[Convert.ToUInt32("0x" + ctc.Substring(76 + (i - 1) * 54, 4), 16) - 1];

                if (Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 30, 2), 16) != 255)
                {
                    si[i - 1].arrivetime =
                    Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 30, 2), 16)).PadLeft(2, '0') + ":" +
                    Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 32, 2), 16)).PadLeft(2, '0') + ":" +
                    Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 34, 2), 16)).PadLeft(2, '0');
                }
                else
                {
                    si[i - 1].arrivetime = "";
                }

                if (Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 44, 2), 16) != 255)
                {
                    si[i - 1].departuretime =
                    Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 44, 2), 16)).PadLeft(2, '0') + ":" +
                    Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 46, 2), 16)).PadLeft(2, '0') + ":" +
                    Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 48, 2), 16)).PadLeft(2, '0');
                }
                else
                {
                    si[i - 1].departuretime = "";
                }

                si[i - 1].stationtrack = Convert.ToString(Convert.ToByte("0x" + ctc.Substring(76 + (i - 1) * 54 + 8, 2), 16));

                MESSAGE = MESSAGE + "|" + si[i - 1].departuretime + "|" + si[i - 1].arrivetime + "|" + si[i - 1].stationtrack;
            }


            if (ctc.Substring(2, 2) == "01")
            {
                MESSAGE = "10" + "|" + Train_ID + "|" + Train_ID + "|" + Train_Direction + MESSAGE;
                MESSAGE = MESSAGE + "|" + Convert.ToUInt32("0x" + ctc.Substring(18, 8), 16);
            }
            else
            {
                MESSAGE = "2" + "|" + Train_ID + "|" + Train_ID + "|" + Train_Direction + MESSAGE;
            }

            return MESSAGE;
        }

        struct ScheduleInfo
        {
            public string arrivetime;
            public string departuretime;
            public string stationtrack;
            public string stationname;
        }
    }
}
