﻿#define DEBUG_OPERATION
#define DEBUG_SET
//#define DEBUG_GET
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Xml.Schema;
using Abt.Controls.SciChart.Example.Data;
using Abt.Controls.SciChart.Model.DataSeries;
using SuperButton.CommandsDB;
using SuperButton.Data;
using SuperButton.Models.DriverBlock;
using SuperButton.Models.ParserBlock;
using SuperButton.Models.SataticClaass;
using SuperButton.Views;
using SuperButton.Helpers;
using SuperButton.ViewModels;
public struct PacketFields
{
    public object Data2Send;
    public Int16 ID;
    public Int16 SubID;
    public bool IsSet;
    public bool IsFloat;
}

//Inter connection between CRC and Parser classes performed by using simple delegates
public delegate ushort CrcEventhandlerCalcHostFrameCrc(IEnumerable<byte> data, int offset);

namespace SuperButton.Models.ParserBlock
{
    internal delegate void Parser2SendHandler(object sender, Parser2SendEventArgs e);//Event declaration, when parser will finish operation. Rise event
    class ParserRayonM1
    {
        private static readonly object Synlock = new object();             //Singletone variable
        public static readonly object PlotListLock = new object();             //Singletone variable
        private static ParserRayonM1 _parserRayonM1instance;               //Singletone variable
        public event Parser2SendHandler Parser2Plot;
        //private Thread decodeThread;
        private bool stop = false;
        private double _deltaTOneChen = 0;
        private DoubleSeries datasource1 = new DoubleSeries();
        public static ManualResetEvent mre = new ManualResetEvent(false);
        private float iqFactor = (float)Math.Pow(2.0, -15);
        //private int IntegerFactor = 1;

        public event Parser2SendHandler Parser2Send;
        public bool StopParser { get { return stop; } set { stop = value; } }

        //Simple delegate. calls static function from Crc class
        public static CrcEventhandlerCalcHostFrameCrc CrcInputCalc = CrcBase.CalcHostFrameCrc;

        public double TimeIntervalChannel1 = 0;
        public DoubleSeries DsCh1 = new DoubleSeries();
        public List<DoubleSeries> PlotDatalistList = new List<DoubleSeries>();

        //public Queue<double> FifoplotList = new Queue<double>();
        public ConcurrentQueue<float> FifoplotList = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotListCh2 = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotListCh3 = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotListCh4 = new ConcurrentQueue<float>();
        public UInt32 RefreshCounter = 0;
        public UInt32 Ticker = 0;
        public UInt32 TickerC = 1;

        private List<int> exceptionID = new List<int>(); // Contains all the ID that dont need to be descripted by refersh manager class.
        int[] exceptionID_Arr = { 100, 67, 34, 35, 36 }; // 100: Error, 67: Load To/From file params started, 34, 35, 36: Init plots table. , 34, 35, 36
        public ParserRayonM1()
        {
            Rs232Interface.GetInstance.RxtoParser += parseOutdata;
            Rs232Interface.GetInstance.TxtoParser += parseIndata;
            Packetizer packetizer = new Packetizer();

            foreach(var element in exceptionID_Arr)
                exceptionID.Add(element);
        }
        #region Parser_Selection

        //TODO here will switch between parsers depends on sender object

        void parseOutdata(object sender, Rs232InterfaceEventArgs e)
        {

            if(sender is Rs232Interface)//RayonM3 Parser
            {
                ParseOutputData(e.PacketRx.Data2Send, e.PacketRx.ID, e.PacketRx.SubID, e.PacketRx.IsSet,
                    e.PacketRx.IsFloat);
                //Debug.WriteLine("{0} {1}[{2}]={3} {4}.", e.PacketRx.IsSet ? "Set" : "Get", e.PacketRx.ID, e.PacketRx.SubID, e.PacketRx.Data2Send, e.PacketRx.IsFloat ? "F" : "I");

                if(LeftPanelViewModel.GetInstance != null)
                { // perform Get after "set" function
                    if(LeftPanelViewModel.flag == true && DebugViewModel.GetInstance.EnRefresh == false && e.PacketRx.IsSet != false)
                    {
                        Thread.Sleep(1);
                        if(e.PacketRx.ID != 63 && e.PacketRx.ID != 67)
                        {
                            ParseOutputData(/*e.PacketRx.Data2Send*/"", e.PacketRx.ID, e.PacketRx.SubID, false,
                            e.PacketRx.IsFloat);
                        }
                    }
                }
            }//Add Here aditional parsers...
        }

        void parseIndata(object sender, Rs232InterfaceEventArgs e)
        {

            if(sender is Rs232Interface)//RayonM3 Parser
            {
                ParseInputData(e.ParseLength, e.InputChank);
            }//Add Here aditional parsers...
        }

        #endregion
        #region RayonM2_Parser

        //TODO add try/catch

        #region Input_Parse 
        //RayonRs232 old parser

        public void ParseInputData(int length, byte[] dataInput)
        {
            if(Rs232Interface.GetInstance.IsSynced == false)//TODO
            {
                Rs232Interface.GetInstance.IsSynced = true;
                //Rs232Interface.GetInstance.
            }
            else
            {
                //Parser
                int PlotDataSampleLSB;
                int PlotDataSampleMSB;
                int PlotDataSample;
                int i = 0;
                //int limit = (dataInput.Length - (dataInput.Length%12));

                for(; i < dataInput.Length - 24;)
                {

                    if(dataInput[i] == 0xbb && dataInput[i + 1] == 0xcc)
                    {

                        XYPoint xyPoint1 = new XYPoint();
                        XYPoint xyPoint2 = new XYPoint();
                        XYPoint xyPoint3 = new XYPoint();
                        XYPoint xyPoint4 = new XYPoint();
                        XYPoint xyPoint5 = new XYPoint();

                        //    //First Sample

                        //  try
                        //  {


                        PlotDataSampleLSB = (short)dataInput[i + 2];
                        PlotDataSampleMSB = (short)dataInput[i + 3];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint1.Y = (double)PlotDataSample;
                        xyPoint1.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint1);

                        //Second
                        PlotDataSampleLSB = (short)dataInput[i + 4];
                        PlotDataSampleMSB = (short)dataInput[i + 5];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint2.Y = (double)PlotDataSample;
                        xyPoint2.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint2);

                        //Third Sample
                        PlotDataSampleLSB = (short)dataInput[i + 6];
                        PlotDataSampleMSB = (short)dataInput[i + 7];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint3.Y = (double)PlotDataSample;
                        xyPoint3.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint3);

                        //Fourth sample
                        PlotDataSampleLSB = (short)dataInput[i + 8];
                        PlotDataSampleMSB = (short)dataInput[i + 9];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint4.Y = (double)PlotDataSample;
                        xyPoint4.X = _deltaTOneChen;

                        datasource1.Add(xyPoint4);

                        _deltaTOneChen += 0.1;

                        //Fifth sample
                        PlotDataSampleLSB = (short)dataInput[i + 10];
                        PlotDataSampleMSB = (short)dataInput[i + 11];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        if(PlotDataSample != 4.0)
                        {
                            // int a = 5;
                        }

                        xyPoint5.Y = (double)PlotDataSample;
                        xyPoint5.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint5);

                        i = i + 11;
                    }
                    i++;
                }
                Parser2Plot(this, new Parser2SendEventArgs(datasource1));
                datasource1.Clear();
            }
        }

        //not need func:
        private void ParseData(int length, byte[] dataInput)
        {
        }

        #endregion //TODO 

        #region Output_Parse

        //Send data to controller
        public void ParseOutputData(object Data2Send, Int16 Id, Int16 SubId, bool IsSet, bool IsFloat)
        {
#if(DEBUG && DEBUG_OPERATION)
#if DEBUG_SET
            if(IsSet)
                Debug.WriteLine("{0} {1}[{2}]={3} {4}.", IsSet ? "Set" : "Get", Id, SubId, Data2Send, IsFloat ? "F" : "I");
#endif
#if DEBUG_GET
            Debug.WriteLine("{0} {1}[{2}]={3} {4}.", IsSet ? "Set" : "Get", Id, SubId, Data2Send, IsFloat ? "F" : "I");
#endif
#endif

            //TODO add try catch here
            //if(Id == 81 && SubId == 1 && IsSet == true)
            //{
            //   Data2Send = (float)5.22;
            //int i = 0;
            //}
            byte[] temp = new byte[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            char tempChar = (char)0;

            //Task<ushort> crcTask = new Task<ushort>(()=>CrcInputCalc(temp.Take(9), 2));


            temp[0] = 0x49;                  //PreambleLSByte
            temp[1] = 0x5d;                   //PreambleMsbyte
            temp[2] = (byte)(Id);       // ID msb

            tempChar = (char)(tempChar | ((char)(((Id >> 8)) & 0x3F)) | (((char)(SubId & 0x3)) << 6));

            temp[3] = (byte)tempChar;

            tempChar = (char)0;

            tempChar = (char)(tempChar | (char)(SubId >> 2));

            temp[4] = (byte)tempChar;

            if(IsSet == false)                      //Set/Get
            {
                temp[4] |= (1 << 4);
            }

            //if (Data2Send is Double)        //Float/Int
            //{
            //    temp[4] |= (1<<5);  
            //}
            if(IsFloat)        //Float/Int
            {
                temp[4] |= (1 << 5);
            }

            // 1<<6  -- Color 1
            // 1<<7  -- Color 2

            if(IsSet == false)
            {
                //Risng up delegate , to call static function from CRC class
                ushort TempGetCrc = CrcInputCalc(temp.Take(5), 2);
                temp[5] = (byte)(TempGetCrc & 0xFF);
                temp[6] = (byte)((TempGetCrc >> 8) & 0xFF);
                if(Parser2Send != null)
                {
                    Parser2Send(this, new Parser2SendEventArgs(temp));
                }
                return;
            }


            if(Data2Send is double)                                           //Data float
            {
                var datvaluevalue = BitConverter.GetBytes((float)(Data2Send is double ? (double)Data2Send : 0));
                temp[5] = (byte)(datvaluevalue[0]);
                temp[6] = (byte)(datvaluevalue[1]);
                temp[7] = (byte)(datvaluevalue[2]);
                temp[8] = (byte)(datvaluevalue[3]);
            }
            else
            if(Data2Send is int)//Data int
            {

                //Int32 transit =(Int32) Data2Send;
                temp[5] = (byte)(((int)Data2Send & 0xFF));
                temp[6] = (byte)(((int)Data2Send >> 8) & 0xFF);
                temp[7] = (byte)(((int)Data2Send >> 16) & 0xFF);
                temp[8] = (byte)(((int)Data2Send >> 24) & 0xFF);
            }
            else
            //my fix
            if(IsFloat)                                           //Data float
            {

                //float.Parse((string)Data2Send);

                var datvaluevalue = BitConverter.GetBytes((float)(float.Parse((string)Data2Send)));
                float newPropertyValuef = System.BitConverter.ToSingle(datvaluevalue, 0);
                temp[5] = (byte)(datvaluevalue[0]);
                temp[6] = (byte)(datvaluevalue[1]);
                temp[7] = (byte)(datvaluevalue[2]);
                temp[8] = (byte)(datvaluevalue[3]);
            }
            else // String Value
            {
                if(Data2Send.ToString().Length != 0)
                {
                    if(Data2Send.ToString().IndexOf(".") != -1)
                    {
                        Data2Send = Data2Send.ToString().Substring(0, Data2Send.ToString().IndexOf("."));
                    }
                    try
                    {
                        var datvaluevalue = Int32.Parse(Data2Send.ToString());
                        temp[5] = (byte)(((int)datvaluevalue & 0xFF));
                        temp[6] = (byte)(((int)datvaluevalue >> 8) & 0xFF);
                        temp[7] = (byte)(((int)datvaluevalue >> 16) & 0xFF);
                        temp[8] = (byte)(((int)datvaluevalue >> 24) & 0xFF);
                    }
                    catch
                    {
                        var datvaluevalue = UInt32.Parse(Data2Send.ToString());
                        temp[5] = (byte)(((int)datvaluevalue & 0xFF));
                        temp[6] = (byte)(((int)datvaluevalue >> 8) & 0xFF);
                        temp[7] = (byte)(((int)datvaluevalue >> 16) & 0xFF);
                        temp[8] = (byte)(((int)datvaluevalue >> 24) & 0xFF);
                    }
                }
            }


            //Risng up delegate , to call static function from CRC class
            ushort TempCrc = CrcInputCalc(temp.Take(9), 2);   // Delegate won  

            //crcTask.Start();
            // ushort TempCrc = crcTask.Result;

            temp[9] = (byte)(TempCrc & 0xFF);
            temp[10] = (byte)((TempCrc >> 8) & 0xFF);

            //Rise another event that sends out to target



            if(Parser2Send != null)
            {
                Parser2Send(this, new Parser2SendEventArgs(temp));
            }


        }
        #endregion

        #endregion
        #region Start_Parser

        //not use
        private void StartParser()
        {
            GuiUpdateQueue.queueIsFull.WaitOne();
            //TODO Parallel for
            //foreach (var data in inputFifo) 
            // {
            //   byte[] data1 ;
            //   inputFifo.TryDequeue(out data1);
            //   Task.Factory.StartNew(() => ParseInputData(data1));
            //   }
            GuiUpdateQueue.queueIsFull.Reset();
        }

        #endregion

        public void ParseSynchAcktData(List<byte[]> dataList)
        {
            for(int i = 0; i < dataList.Count; i++)
            {
                ParsesynchAckMessege(dataList[i]);
            }

        }
        private void ParsesynchAckMessege(byte[] data)
        {
            var crclsb = data[7];
            var crcmsb = data[8];
            ushort crc = CrcInputCalc(data.Take(7), 0);
            byte[] crcBytes = BitConverter.GetBytes(crc);

            if(crcBytes[0] == crclsb && crcBytes[1] == crcmsb)//CHECK
            {
                Int32 transit = data[6];
                transit <<= 8;
                transit |= data[5];
                transit <<= 8;
                transit |= data[4];
                transit <<= 8;
                transit |= data[3];

                if(transit == 1)
                    Rs232Interface.GetInstance.IsSynced = true;
                else
                    Rs232Interface.GetInstance.IsSynced = false;

                mre.Set();
            }
        }
        public void ParseStandartData(List<byte[]> dataList)
        {
            for(int i = 0; i < dataList.Count; i++)
            {
                ParseInputPacket(dataList[i]);
            }
        }
        public static byte[] DebugData = { };
        public bool ParseInputPacket(byte[] data)
        {
            DebugData = data;
            var crclsb = data[7];
            var crcmsb = data[8];

            ushort crc = CrcInputCalc(data.Take(7), 0);

            byte[] crcBytes = BitConverter.GetBytes(crc);

            if(crcBytes[0] == crclsb && crcBytes[1] == crcmsb)//CHECK
            {
                var cmdlIdLsb = data[0];
                var cmdIdlMsb = data[1] & 0x3F;
                var subIdLsb = (data[1] >> 6) & 0x03;
                var subIdMsb = data[2] & 0x07;
                var getSet = (data[2] >> 4) & 0x01;//ASK
                var intFloat = (data[2] >> 5) & 0x01;
                var farmeColor = (data[3] >> 6) & 0x03;
                bool isInt = (intFloat == 0);//Answer Int=0/////to need check what doing!!!
                                             //Cmd ID
                int commandId = Convert.ToInt16(cmdlIdLsb);
                commandId = commandId + Convert.ToInt16(cmdIdlMsb << 8);
                //Cmd SubID
                int commandSubId = Convert.ToInt16(subIdLsb);
                commandSubId = commandSubId + Convert.ToInt16(subIdMsb << 2);
                //int newPropertyValueInt=0;
                float newPropertyValuef = 0;
                Int32 transit = data[6];
                transit <<= 8;
                transit |= data[5];
                transit <<= 8;
                transit |= data[4];
                transit <<= 8;
                transit |= data[3];
                if(ParametarsWindow.ParametersWindowTabSelected == ParametarsWindowViewModel.DEBUG && !LeftPanelViewModel.GetInstance.StarterOperationFlag || !exceptionID.Contains(commandId))
                {
                    if(isInt)
                    {
                        if(getSet == 1)
                            RefreshManger.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), transit.ToString());
#if(DEBUG && DEBUG_OPERATION)
#if DEBUG_SET
                        if(getSet == 0)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
#endif
#if DEBUG_GET
                        if(getSet == 1)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
#endif
#endif
                    }
                    else
                    {
                        var dataAray = new byte[4];
                        for(int i = 0; i < 4; i++)
                        {
                            dataAray[i] = data[i + 3];
                        }
                        newPropertyValuef = System.BitConverter.ToSingle(dataAray, 0);
                        if(getSet == 1)
                        {
                            RefreshManger.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), newPropertyValuef.ToString());
                        }
#if(DEBUG && DEBUG_OPERATION)
#if DEBUG_SET
                        if(getSet == 0)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, newPropertyValuef, "F", getSet == 0 ? "Set" : "Get");
#endif
#if DEBUG_GET
                        if(getSet == 1)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, newPropertyValuef, "F", getSet == 0 ? "Set" : "Get");
#endif
#endif
                    }
                }
                else if(commandId == 67)
                {
#if(DEBUG && DEBUG_OPERATION)
                    Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
#endif
                    if(commandSubId == 1)
                    {
                        MaintenanceViewModel.PbarParamsCount = Convert.ToUInt32(transit);
                        if(MaintenanceViewModel.GetInstance.SaveToFile == true)
                        {
                            MaintenanceViewModel.ParamsCount = Convert.ToUInt32(transit);
                            Rs232Interface.GetInstance.SendToParser(new PacketFields
                            {
                                Data2Send = 1,
                                ID = 67,
                                SubID = Convert.ToInt16(12),
                                IsSet = true,
                                IsFloat = false
                            }
                            );
                        }
                        else if(MaintenanceViewModel.GetInstance.LoadFromFile == true)
                        {
                            if(MaintenanceViewModel.GetInstance.SelectFile(Convert.ToUInt32(transit)))
                            {
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    Data2Send = 1,
                                    ID = 67,
                                    SubID = Convert.ToInt16(2),
                                    IsSet = true,
                                    IsFloat = false
                                });
                            }
                            else
                            {
                                MaintenanceViewModel.GetInstance.LoadFromFile = false;
                            }
                        }
                    }
                    else if(commandSubId == 12 && getSet == 0)
                    {
                        if(MaintenanceViewModel.ParamsToFile.Count == 0)
                        {
                            Rs232Interface.GetInstance.SendToParser(new PacketFields
                            {
                                Data2Send = 1,
                                ID = 67,
                                SubID = Convert.ToInt16(13),
                                IsSet = false,
                                IsFloat = false
                            }
                            );
                        }
                        else
                        {
                            MaintenanceViewModel.GetInstance.SaveToFile = false;
                        }
                    }
                    else if(commandSubId == 13)
                    {
                        MaintenanceViewModel.GetInstance.DataToList(Convert.ToUInt32((uint)transit));
                    }
                    else if(commandSubId == 2 && getSet == 0)
                    {
                        if(MaintenanceViewModel.GetInstance.LoadFromFile && MaintenanceViewModel.FileToParams.Count > 1)
                        {
                            Rs232Interface.GetInstance.SendToParser(new PacketFields
                            {
                                Data2Send = MaintenanceViewModel.FileToParams.ElementAt(0),
                                ID = 67,
                                SubID = Convert.ToInt16(3),
                                IsSet = true,
                                IsFloat = false
                            });
                            MaintenanceViewModel.FileToParams.RemoveAt(0);
                        }
                        else if(!MaintenanceViewModel.GetInstance.LoadFromFile && MaintenanceViewModel.FileToParams.Count > 1)
                        {
                            MaintenanceViewModel.GetInstance.LoadFromFile = false;
                        }
                    }
                    else if(commandSubId == 3 && getSet == 0)
                    {
                        if(MaintenanceViewModel.FileToParams.Count > 1)
                        {
                            Rs232Interface.GetInstance.SendToParser(new PacketFields
                            {
                                Data2Send = (int)(MaintenanceViewModel.FileToParams.ElementAt(0)),
                                ID = 67,
                                SubID = Convert.ToInt16(3),
                                IsSet = true,
                                IsFloat = false
                            });
                            MaintenanceViewModel.FileToParams.RemoveAt(0);
                            MaintenanceViewModel.GetInstance.PbarValueFromFile = 100 - ((MaintenanceViewModel.FileToParams.Count) * 100 / MaintenanceViewModel.PbarParamsCount);
                        }
                        else if(MaintenanceViewModel.FileToParams.Count == 1)
                        {
                            Rs232Interface.GetInstance.SendToParser(new PacketFields
                            {
                                Data2Send = (int)(MaintenanceViewModel.FileToParams.ElementAt(0)),
                                ID = 67,
                                SubID = Convert.ToInt16(4),
                                IsSet = true,
                                IsFloat = false
                            });
                        }
                    }
                    else if(commandSubId == 4 && getSet == 0)
                    {
                        if(transit == 1)
                        {
                            EventRiser.Instance.RiseEevent(string.Format($"Load Parameters successed"));
                            MaintenanceViewModel.GetInstance.PostRedoState(MaintenanceViewModel._redoState);
                        }
                        else
                        {
                            EventRiser.Instance.RiseEevent(string.Format($"Load Parameters Failed"));
                            MaintenanceViewModel.GetInstance.SaveToFile = false;
                            MaintenanceViewModel.GetInstance.LoadFromFile = false;
                            MaintenanceViewModel.GetInstance.PostRedoState(MaintenanceViewModel._redoState);
                        }
                        MaintenanceViewModel.GetInstance.LoadFromFile = false;

                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = 0,
                            ID = 67,
                            SubID = Convert.ToInt16(2),
                            IsSet = true,
                            IsFloat = false
                        });
                    }
                }
                else if(commandId == 34 && LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Reading plots..."));
                    OscilloscopeParameters.plotCount_temp = transit;
                    OscilloscopeParameters.plotCount = transit;
                    EventRiser.Instance.RiseEevent(string.Format($"Plots count: " + transit.ToString()));
                    if(transit > 0)
                        OscilloscopeParameters.fillPlotList();
                }
                else if(commandId == 35 && LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    OscilloscopeParameters.plotGeneral.Add(transit);
                }
                else if(commandId == 36 && LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    var dataAray = new byte[4];
                    for(int i = 0; i < 4; i++)
                    {
                        dataAray[i] = data[i + 3];
                    }
                    newPropertyValuef = System.BitConverter.ToSingle(dataAray, 0);
                    OscilloscopeParameters.plotFullScale.Add(newPropertyValuef);
                    if(OscilloscopeParameters.plotCount_temp > 0)
                        OscilloscopeParameters.fillPlotList();
                }
                else
                {   // Error ID 100
                    string result;
                    if(Commands.GetInstance.ErrorList.TryGetValue(transit, out result))
                    {
                        EventRiser.Instance.RiseEevent(string.Format($"Com. Error: " + result));
                    }
                    else
                        EventRiser.Instance.RiseEevent(string.Format($"Error: " + transit.ToString()));
                }
                return true;
            }
            return false;
        }
        public static ParserRayonM1 GetInstanceofParser
        {
            get
            {
                if(_parserRayonM1instance != null)
                    return _parserRayonM1instance;
                lock(Synlock)
                {
                    _parserRayonM1instance = new ParserRayonM1();
                    return _parserRayonM1instance;
                }
            }
        }
        //float calcFactor(short dataSample, int ChNo)
        //{
        //    string plotType = "";
        //    if(OscilloscopeParameters.plotType_ls.Count != 0)
        //    {
        //        switch(ChNo)
        //        {
        //            case 1:
        //                plotType = OscilloscopeParameters.plotType_ls.ElementAt(OscilloscopeViewModel.GetInstance.Ch1SelectedIndex);
        //                break;
        //            case 2:
        //                plotType = OscilloscopeParameters.plotType_ls.ElementAt(OscilloscopeViewModel.GetInstance.Ch2SelectedIndex);
        //                break;
        //        }
        //        switch(plotType)
        //        {
        //            case "Integer":
        //                return dataSample * IntegerFactor;
        //            case "Float":
        //            case "Iq24":
        //            case "Iq15":
        //            default:
        //                return dataSample * iqFactor;
        //        }
        //    }
        //    else
        //        return dataSample;
        //}
        public void ParsePlot(List<byte[]> PlotList)
        {
            // In order to achive best performance using good old-fashioned for loop: twice faster! then "foreach (byte[] packet in PlotList)"
            //Debug.WriteLine("ParsePlot 1" + DateTime.Now.ToString("h:mm:ss.fff"));
            //if(!OscilloscopeViewModel.GetInstance.IsFreeze)
            {
                for(var i = 0; i < PlotList.Count; i++)
                {
                    lock(PlotListLock)
                    {
                        //First
                        FifoplotList.Enqueue((short)((PlotList[i][3] << 8) | PlotList[i][2]));

                        //Second
                        if(OscilloscopeParameters.ChanTotalCounter == 1)
                            FifoplotList.Enqueue((short)((PlotList[i][5] << 8) | PlotList[i][4]));
                        else if(OscilloscopeParameters.ChanTotalCounter == 2)
                            FifoplotListCh2.Enqueue((short)((PlotList[i][5] << 8) | PlotList[i][4]));

                        //Third
                        FifoplotList.Enqueue((short)((PlotList[i][7] << 8) | PlotList[i][6]));

                        //Fourth
                        if(OscilloscopeParameters.ChanTotalCounter == 1)
                            FifoplotList.Enqueue((short)((PlotList[i][9] << 8) | PlotList[i][8]));
                        else if(OscilloscopeParameters.ChanTotalCounter == 2)
                            FifoplotListCh2.Enqueue((short)((PlotList[i][9] << 8) | PlotList[i][8]));
                    }

                }
                PlotList.Clear();
                //Debug.WriteLine("ParsePlot 2" + DateTime.Now.ToString("h:mm:ss.fff"));
            }
        }

    }//Class
}//NameSpace


