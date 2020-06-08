﻿#define TEST_LOADER_MODE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Example.Common;
using Abt.Controls.SciChart.Example.Data;
using SuperButton.CommandsDB;
using SuperButton.Data;
using SuperButton.Models.DriverBlock;
using SuperButton.Models.ParserBlock;
using SuperButton.Models.SataticClaass;
using SuperButton.ViewModels;
using SuperButton.Views;
using System.Globalization;
using SuperButton.Common;
using System.Windows;
using SuperButton.Helpers;
using System.ComponentModel;
using SuperButton.Annotations;
using MotorController.ViewModels;

namespace SuperButton.Models.DriverBlock
{

    internal delegate void Rs232RxHandler(object sender, Rs232InterfaceEventArgs e);


    internal class Rs232Interface : ConnectionBase
    {
        //TOdo move global members to base clase, and create there ctor (lesson interfase)
        //MEMBERS
        public event Rs232RxHandler RxtoParser;
        public event Rs232RxHandler TxtoParser;
        public event Rs232RxHandler Driver2Mainmodel;

        public event Rs232RxHandler Rx2Packetizer;
        public event Parser2SendHandler AutoBaudEcho;
        public PacketFields RxPacket;
        public static SerialPort _comPort;         //Serial Port

        //Once created , could not be changed (READ ONLY)
        private static readonly object Synlock = new object();          //Single tone variable
        private static readonly object ConnectLock = new object();      //Single tone variable
        private static readonly object DisonnectLock = new object();    //Single tone variable
        private static Rs232Interface _instance;                        //Single tone variable
        private static bool _isSynced = false;                          //Sincronization flag
        private static readonly object Sendlock = new object();         //Semaphore
        private static List<ConnectionBase.ComDevice> _comDevicesList = new List<ComDevice>();

        #region Properties

        public bool IsSynced
        {
            get
            {
                return _isSynced;
            }
            set
            {
                _isSynced = value;
            }
        }
        #endregion
        public delegate void DataRecived(byte[] dataBytes);
        //Defining event based on the above delegate
        //public event DataRecived DataRecivedEvent;

        //Contsractor

        public Rs232Interface()
        {
            //Create queue object and set queue size
            GuiUpdateQueue stundartUpdateQueue = new GuiUpdateQueue();

            //for (int i = 0; i < 200; i++)
            //{
            //    xyPointBuff[i]=new XYPoint();
            //}

            //Counter = 0;


            //ParserRayonM1.GetInstanceofParser.Parser2Send += SendDataHendler;
        }
        public override void Disconnect(int mode = 0)
        {
            RefreshManger.GetInstance.DisconnectedFlag = true;
            switch(mode)
            {
                case 0:
                    EventRiser.Instance.RiseEevent(string.Format($"Disconnecting..."));
                    LeftPanelViewModel.GetInstance.BlinkLedsTicks(LeftPanelViewModel.STOP);
                    LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.STOP);
                    LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.STOP);
                    LeftPanelViewModel.GetInstance.led = -1;
                    if(Rs232Interface._comPort != null)
                    {
                        if(_comPort.IsOpen)
                        {
                            if(RxtoParser != null)
                            {
                                _isSynced = false;
                                Thread.Sleep(100);
                                DataViewModel temp = (DataViewModel)Commands.GetInstance.DataCommandsListbySubGroup["DeviceSynchCommand"][0];
                                Commands.AssemblePacket(out RxPacket, Int16.Parse(temp.CommandId), Int16.Parse(temp.CommandSubId), true, false, 0);
                                RxtoParser(this, new Rs232InterfaceEventArgs(RxPacket));

                                ParserRayonM1.mre.WaitOne(1000);

                                if(Rs232Interface.GetInstance.IsSynced == false)
                                {

                                    _comPort.DataReceived -= DataReceived;
                                    _comPort.Close();
                                    _comPort.Dispose();

                                    if(Driver2Mainmodel != null)
                                    {
                                        Driver2Mainmodel(this, new Rs232InterfaceEventArgs("Connect"));
                                    }
                                    else
                                    {
                                        throw new NullReferenceException("No Listeners to this event");
                                    }
                                    LeftPanelViewModel.busy = false;
                                }
                                LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Not Connected";

                                LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.STOP);
                                EventRiser.Instance.RiseEevent(string.Format($"Disconnected"));
                            }
                            LeftPanelViewModel.busy = false;
                        }
                        else
                        {
                            LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Not Connected";
                            EventRiser.Instance.RiseEevent(string.Format($"Disconnected"));

                            _isSynced = false;
                            _comPort.DataReceived -= DataReceived;
                            _comPort.Close();
                            _comPort.Dispose();
                            Driver2Mainmodel(this, new Rs232InterfaceEventArgs("Connect"));
                            LeftPanelViewModel.busy = false;
                        }
                        _comPort = null;
                        OscilloscopeViewModel.GetInstance.ChComboEn = false;
                    }
                    else
                    {
                        LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Not Connected";
                        EventRiser.Instance.RiseEevent(string.Format($"Disconnected"));

                        _isSynced = false;
                        _comPort.DataReceived -= DataReceived;
                        _comPort.Close();
                        _comPort.Dispose();
                        Driver2Mainmodel(this, new Rs232InterfaceEventArgs("Connect"));
                        LeftPanelViewModel.busy = false;

                        _comPort = null;
                        OscilloscopeViewModel.GetInstance.ChComboEn = false;
                    }
                    break;
                case 1:
                    LeftPanelViewModel.GetInstance.RefreshParamsTick(LeftPanelViewModel.STOP);
                    LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Not Connected";
                    break;
            }
            LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
        }

        #region Auto_Connect

        //This method auto detects baud rate, and open connection
        //*******************************************************
        private string _baudRate = "";
        private string _comPortStr = "";

        public string BaudRate
        {
            get { return _baudRate; }
            set { if(_baudRate == value) return; _baudRate = value; }
        }
        public string ComPortStr
        {
            get { return _comPortStr; }
            set { if(_comPortStr == value) return; _comPortStr = value; }
        }
        public override void AutoConnect()
        {
            if(_isSynced == false && LeftPanelViewModel.GetInstance.ConnectButtonContent == "Connect") //Driver is not synchronized
            {
                //Gets aviable ports list and initates them
                _comDevicesList =
                    (SerialPort.GetPortNames()).Select(o => new ComDevice { Portname = o, Baudrate = 921600 }).ToList();

                //Iterates though  the ports,Looks for apropriate Com Port ( where the driver connected)
                if(Configuration.SelectedCom != null && Configuration.SelectedCom != "")//foreach (var comDevice in _comDevicesList)
                {
                    // Add text to logger panel
                    EventRiser.Instance.RiseEevent(string.Format($"Connecting at {Configuration.SelectedCom}"));
                    var ComPort = new SerialPort
                    {
                        PortName = Configuration.SelectedCom,
                        Parity = Parity.None,
                        DataBits = 0x00000008,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,

                        ReadTimeout = 500,
                        WriteTimeout = 500,

                        ReadBufferSize = 8192
                    };
                    try
                    {
                        var Cleaner = "";
                        ComPort.Open(); //Try to open

                        if(ComPort.IsOpen)
                        {
                            EventRiser.Instance.RiseEevent(string.Format($"Success"));
                            EventRiser.Instance.RiseEevent(string.Format($"Autobaud process..."));

                            foreach(var baudRate in BaudRates) //Iterate though baud rates
                            {
                                if(_isSynced) // Baudrate found
                                {
                                    if(Driver2Mainmodel != null)
                                    {
                                        Driver2Mainmodel(this, new Rs232InterfaceEventArgs("Disconnect"));
                                    }
                                    else
                                    {
                                        throw new NullReferenceException("No Listeners on this event");
                                    }
                                    _comPort.DiscardInBuffer();        //Reset internal rx buffer
                                    EventRiser.Instance.RiseEevent(string.Format($"Success"));
                                    EventRiser.Instance.RiseEevent(string.Format($"Baudrate: {_comPort.BaudRate}"));
                                    _baudRate = _comPort.BaudRate.ToString();
                                    _comPortStr = Configuration.SelectedCom;
                                    LeftPanelViewModel.busy = false;
                                    LeftPanelViewModel.GetInstance.StarterOperation(LeftPanelViewModel.STOP);
                                    LeftPanelViewModel.GetInstance.StarterOperation(LeftPanelViewModel.START);
                                    LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
                                    return;
                                }
                                else if(_isSynced == false)  // Looking for Baudrate
                                {
                                    ComPort.BaudRate = baudRate;

                                    ComPort.DataReceived -= DataReceived;
                                    ComPort.DataReceived += DataReceived;

                                    ParserRayonM1.GetInstanceofParser.Parser2Send -= SendDataHendler;
                                    ParserRayonM1.GetInstanceofParser.Parser2Send += SendDataHendler;

                                    _comPort = ComPort;

                                    //Init synchronization packet, and rises event for parser
                                    if(RxtoParser != null)
                                    {
                                        DataViewModel temp = (DataViewModel)Commands.GetInstance.DataCommandsListbySubGroup["DeviceSynchCommand"][1];
                                        Commands.AssemblePacket(out RxPacket, Int16.Parse(temp.CommandId), Int16.Parse(temp.CommandSubId), false, false, 0);
                                        RxtoParser(this, new Rs232InterfaceEventArgs(RxPacket));
                                    }
                                    Thread.Sleep(100);// while with timeout of 1 second
                                    Cleaner = ComPort.ReadExisting();
                                }
                            }
                            EventRiser.Instance.RiseEevent(string.Format($"Failed"));
#if TEST_LOADER_MODE
                            EventRiser.Instance.RiseEevent(string.Format($"Testing loader mode"));
                            // Autobaud A A A Echo operation detect
                            ComPort.BaudRate = BaudRates[5];

                            ComPort.DataReceived -= DataReceived;
                            ComPort.DataReceived += DataReceived;

                            AutoBaudEcho -= SendDataHendler;
                            AutoBaudEcho += SendDataHendler;
                            _comPort = ComPort;

                            //Init synchronization packet, and rises event for parser

                            byte[] A = new byte[1] { 65 };
                            for(int i = 0; i < 5; i++)
                            {
                                if(AutoBaudEcho != null)
                                {
                                    AutoBaudEcho(this, new Parser2SendEventArgs(A));
                                    Thread.Sleep(500);
                                }
                                else
                                    break;
                            }
                            if(AutoBaudEcho != null)
                            {
                                EventRiser.Instance.RiseEevent(string.Format($"Failed to communicate with unit"));
                            }
                            Thread.Sleep(100);// while with timeout of 1 second
                            Cleaner = ComPort.ReadExisting();
#endif
                            _baudRate = _comPort.BaudRate.ToString();
                            _comPortStr = Configuration.SelectedCom;
                            ComPort.Close();
                            LeftPanelViewModel.busy = false;
                            LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
                            return;
                        }
                        EventRiser.Instance.RiseEevent(string.Format($"Failed"));
                        ComPort.Close();
                        LeftPanelViewModel.busy = false;
                        LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
                        return;
                    }
                    catch(Exception)
                    {
                        EventRiser.Instance.RiseEevent(string.Format($"Failed"));
                        ComPort.Close();
                        ComPort.Dispose();
                        LeftPanelViewModel.busy = false;
                        LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
                        return;
                    }
                }
                else
                    EventRiser.Instance.RiseEevent(string.Format($"No COM Port Selected!"));
            }
            else if(_isSynced == true)
            {
                LeftPanelViewModel.busy = false;
                LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
                return;
            }

            LeftPanelViewModel.busy = false;
            LeftPanelViewModel.GetInstance.ConnectButtonEnable = true;
            return;
        }

        #endregion

        #region Manual_Connect

        //This method manually and open connection
        //
        //
        //
        //
        //
        //********************************************************

        public virtual bool ManualConnect()
        {

            if(_isSynced == false) //Driver is not synchronized
            {
                //Gets aviable ports list and initates them
                _comDevicesList =
                    (SerialPort.GetPortNames()).Select(o => new ComDevice { Portname = o, Baudrate = 921600 }).ToList();

                //Iterates though  the ports,Looks for apropriate Com Port ( where the driver connected)
                foreach(var comDevice in _comDevicesList)
                {
                    var tmpcom = new SerialPort
                    {
                        PortName = comDevice.Portname,
                        DataBits = comDevice.DataBits,
                        StopBits = comDevice.StopBits
                    };
                    try
                    {
                        tmpcom.Open(); //Try to open
                        if(tmpcom.IsOpen)
                        {
                            foreach(var baudRate in BaudRates) //Iterate though baud rates
                            {
                                tmpcom.BaudRate = baudRate;
                                //tmpcom.DataReceived += SyncDataReceived;

                                //Moves to parser block
                                //ProtocolParser.GetInstance.BuildPacketToSend("0", "400" /*CommandId*/, "0" /* subid*/,
                                //    true /*IsSet*/);

                                Thread.Sleep(50);
                                // tmpcom.DataReceived -= SyncDataReceived;

                                if(_isSynced)
                                {
                                    return true;
                                }
                            }
                        }
                        tmpcom.Close();
                    }
                    catch(Exception)
                    {
                        tmpcom.Close();
                        tmpcom.Dispose();
                        return false;
                    }
                }
            }
            return _isSynced;
        }
        #endregion

        #region Send_Mechanism

        //     public void SendData(byte[] packetToSend)
        //     SendDataHendler(object sender, Parser2SendEventArgs parser2SendEventArgs)
        //
        //
        //
        //
        //********************************************************
        public void SendDataHendler(object sender, Parser2SendEventArgs parser2SendEventArgs)
        {
            SendData(parser2SendEventArgs.BytesTosend, _comPort);
        }


        private void SendData(byte[] packetToSend, object comPort)
        {
            lock(Sendlock)
            {
                var serialPort = comPort as SerialPort;
                if(serialPort != null && serialPort.IsOpen)
                {
                    try
                    {
                        //Debug.WriteLine(DateTime.Now + "." + DateTime.Now.Millisecond);
                        Thread.Sleep(5);
                        serialPort.Write(packetToSend, 0, packetToSend.Length); // Send through RS232 cable
#if DEBUG
                        if(packetToSend.Length == 11)
                        {
                            var cmdlIdLsb = packetToSend[2];
                            var cmdIdlMsb = packetToSend[3] & 0x3F;
                            var subIdLsb = (packetToSend[3] >> 6) & 0x03;
                            var subIdMsb = packetToSend[4] & 0x07;
                            var getSet = (packetToSend[4] >> 4) & 0x01;
                            int commandId = Convert.ToInt16(cmdlIdLsb);
                            commandId = commandId + Convert.ToInt16(cmdIdlMsb << 8);
                            int commandSubId = Convert.ToInt16(subIdLsb);
                            commandSubId = commandSubId + Convert.ToInt16(subIdMsb << 2);
                            Int32 transit = packetToSend[8];
                            transit <<= 8;
                            transit |= packetToSend[7];
                            transit <<= 8;
                            transit |= packetToSend[6];
                            transit <<= 8;
                            transit |= packetToSend[5];

                            if(commandId == DebugOutput.GetInstance.ID && commandSubId == DebugOutput.GetInstance.subID && getSet == 0)
                            {
                                Debug.WriteLine("SendData data: " + transit);
                            }

                        }
                        else
                        {

                        }
#endif
                        serialPort.DiscardOutBuffer();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void SendToParser(PacketFields messege)
        {
            RxtoParser?.Invoke(this, new Rs232InterfaceEventArgs(messege)); // then go to "void parseOutdata(object sender, Rs232InterfaceEventArgs e)"
            //Debug.WriteLine("{0} {1}[{2}]={3} {4}.", messege.IsSet ? "Set" : "Get", messege.ID, messege.SubID, messege.Data2Send, messege.IsFloat ? "F" : "I");
        }

        #endregion

        #region Read_Mechanism


        // public void ReadDataEventHandler(byte[] packetToSend)
        //     
        //
        // read Data will read always two byte and send to parser 
        // parcer will check preemble and know how mane data fields it should get
        //
        //
        //
        //********************************************************

        public void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Create new parent Task
            SerialPort port = (SerialPort)sender;
            if(port != null)
            {
                byte[] buffer = new byte[port.BytesToRead];
                byte[] bufferTemp = new byte[0];
                try
                {
                    port.Read(buffer, 0, buffer.Length);
                }
                catch(Exception er)
                {
                    Debug.WriteLine(er.Message);

                }
                if(Rx2Packetizer != null && buffer.Length > 0)
                {
                    //for(int i = 0; i < buffer.Length;i++)
                    //    Debug.Write(buffer[i].ToString());
                    //Debug.WriteLine("");
                    Rx2Packetizer(this, new Rs232InterfaceEventArgs(buffer)); // Go to Packetizer -> MakePacketsBuff function
                }
            }
        }
        //public void Connect()
        //{
        //    _comPort.DataReceived += DataReceived;
        //}

        #endregion

        //STATIC METHODS  STATIC METHODS   STATIC METHODS   STATIC METHODS   STATIC METHODS   STATIC METHODS   STATIC METHODS   STATIC METHODS   STATIC METHODS   STATIC METHODS    

        public static Rs232Interface GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new Rs232Interface();
                    return _instance;
                }
            }
        }
    }
}

