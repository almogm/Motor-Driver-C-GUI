﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperButton.Models.DriverBlock;
using SuperButton.ViewModels;
using SuperButton.Helpers;
using SuperButton.Views;
using System.Diagnostics;

namespace SuperButton.Models.ParserBlock
{

    internal delegate void PacketizerEventHandler(object sender, PacketizerEventArgs e);//Event declaration, when parser will finish operation. Rise event

    class Packetizer
    {
        public List<byte[]> StandartPacketsList = new List<byte[]>();
        public List<byte[]> PlotPacketsList = new List<byte[]>();
        public List<byte[]> PlotBodePacketsList = new List<byte[]>();
        public List<byte[]> StandartPacketsListNew = new List<byte[]>();
        // private  int DEBCount;
        private byte[] pack = new byte[11];
        private byte[] bode_pack = new byte[15];
        private int plotpacketState = 0;
        private int plotbodepacketState = 0;
        private int standpacketState = 0;
        private int standpacketStateNew = 0;
        private int standpacketIndexCounter = 0;
        // private int plotpacketSize = 0;

        private int _synchAproveState;
        private int _synchAproveIndexCounter;
        // private int _synchAproveState = 0;
        byte[] readypacket = new byte[9];
        //Once created , could not be changed (READ ONLY)
        private static readonly object Synlock = new object(); //Single tone variable
        private static Packetizer _instance;               //Single tone variable

        public int length;
        public byte[] data;
        Int32 TempA = 0;

        public static readonly object Packetizerlock = new object(); //Single tone variable
        //public event PacketizerEventHandler ToPacketizer;

        public static Packetizer GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new Packetizer();
                    return _instance;
                }
            }
        }

        public Packetizer()
        {
            Task.Factory.StartNew(action: () =>
            {
                Thread.Sleep(10);
                Rs232Interface.GetInstance.Rx2Packetizer += MakePacketsBuff;
            });
        }

        public void MakePacketsBuff(object sender, Rs232InterfaceEventArgs e)
        {
            if(e.DataChunk[0] == 65 && e.DataChunk.Length < 2 && LeftPanelViewModel.GetInstance.ConnectTextBoxContent == "Not Connected")
            {
                EventRiser.Instance.RiseEevent(string.Format($"Unit is at loader mode"));
                EventRiser.Instance.RiseEevent(string.Format($"Ready for FW update"));
                Rs232Interface.GetInstance.AutoBaudEcho -= Rs232Interface.GetInstance.SendDataHendler;
                return;
            }
            else if(sender is Rs232Interface) //RayonM3 Parser
            {
                if(Rs232Interface.GetInstance.IsSynced || DebugViewModel.GetInstance._forceConnectMode)//Already Synchronized
                {
                    if(e.DataChunk.Length == 0)
                        return;

                    length = e.DataChunk.Length;
                    data = e.DataChunk;

                    for(int i = 0; i < length; i++)
                    {
                        FiilsPlotPackets(data[i]); //Plot packets
                        FiilsPlotBodePackets(data[i]); //Plot packets
                        //FiilsStandartPackets(data[i]);//Standart Packets                    
                        FiilsStandartPacketsNew(data[i]);//Standart Packets New Updeted
                    }
                    if(PlotPacketsList.Count > 0)
                    {
                        ParserRayonM1.GetInstanceofParser.ParsePlot(PlotPacketsList);
                        //PlotPacketsList.Clear();
                    }               //send to plot parser  
                    if(PlotBodePacketsList.Count > 0)
                    {
                        Debug.WriteLine("Bode Count: " + PlotBodePacketsList.Count);
                        BodeViewModel.GetInstance.ParseBodePlot(PlotBodePacketsList);
                        PlotBodePacketsList.Clear();
                        //PlotPacketsList.Clear();
                    }
                    if(StandartPacketsListNew.Count > 0)
                    {
                        //ParserRayonM1.GetInstanceofParser.ParseStandartData(StandartPacketsListNew);
                        //foreach(var packet in StandartPacketsListNew)
                        //ParserRayonM1.GetInstanceofParser.ParseInputPacket(packet);
                        StandartPacketsListNew.Clear(); // Joseph add

                    } //send to Standart parser                             
                }
                else
                {
                    if(e.DataChunk.Length == 0)
                        return;

                    PlotPacketsList.Clear();
                    StandartPacketsListNew.Clear();


                    length = e.DataChunk.Length;
                    data = e.DataChunk;
                    StringBuilder hex = new StringBuilder(length * 2);
                    foreach(byte b in data)
                        hex.AppendFormat("{0:X2} ", b);
                    Debug.WriteLine(hex.ToString());


                    for(int i = 0; i < length; i++)
                    {
                        AproveSynchronization(data[i]); //Plot packets
                    }
                    if(StandartPacketsListNew.Count > 0)
                    {
                        ParserRayonM1.GetInstanceofParser.ParseSynchAcktData(StandartPacketsListNew);
                        StandartPacketsListNew.Clear();
                    }
                }
            }
        }
        private void FiilsStandartPacketsNew(byte ch)
        {
            switch(standpacketStateNew)
            {
                case (0):	//First magic
                    if(ch == 0x8b)
                    { standpacketStateNew++; }
                    break;
                case (1)://Second magic
                    if(ch == 0x3c)
                    { standpacketStateNew++; }
                    else
                        standpacketStateNew = 0;
                    break;
                case (2):
                case (3):
                case (4):
                case (5):
                case (6):
                case (7):
                case (8):
                case (9):
                    readypacket[(standpacketIndexCounter++)] = ch;
                    standpacketStateNew++;
                    break;
                case (10):
                    readypacket[standpacketIndexCounter] = ch;
                    StandartPacketsListNew.Add(readypacket);
                    ParserRayonM1.GetInstanceofParser.ParseInputPacket(readypacket);
                    standpacketStateNew = standpacketIndexCounter = 0;
                    break;
                default:
                    standpacketStateNew = standpacketIndexCounter = 0;
                    break;
            }
        }
        private void AproveSynchronization(byte ch)
        {
            switch(_synchAproveState)
            {
                case (0):	//First magic
                    if(ch == 0x8b)
                    { _synchAproveState++; }
                    break;
                case (1)://Second magic
                    if(ch == 0x3c)
                    { _synchAproveState++; }
                    else
                        _synchAproveState = 0;
                    break;
                case (2):
                case (3):
                case (4):
                case (5):
                case (6):
                case (7):
                case (8):
                case (9):
                    readypacket[(_synchAproveIndexCounter++)] = ch;
                    _synchAproveState++;
                    break;
                case (10):
                    readypacket[_synchAproveIndexCounter] = ch;
                    StandartPacketsListNew.Add(readypacket);
                    _synchAproveState = _synchAproveIndexCounter = 0;
                    break;
                default:
                    _synchAproveState = _synchAproveIndexCounter = 0;
                    break;
            }
        }
        private void FiilsStandartPackets(byte ch)
        {
            try
            {
                switch(standpacketState)
                {
                    case (0):   //First magic
                        if(ch == 0x8b)
                        {

                            // pack[plotpacketState] = ch;
                            standpacketState++;
                            TempA = 0;
                        }
                        break;
                    case (1)://Second magic

                        if(ch == 0x3c)
                        {
                            pack[standpacketState] = ch;
                            standpacketState++;
                        }
                        else
                            standpacketState = 0;
                        break;
                    case (2):
                        if(ch == 213)
                        {
                            standpacketState++;
                        }
                        else
                            standpacketState = 0;
                        break;
                    case (3):
                        standpacketState++;
                        break;
                    case (4):

                        standpacketState++;

                        break;
                    case (5):
                        TempA = ch;

                        standpacketState++;

                        break;
                    case (6):
                        TempA = TempA + Convert.ToInt16((ch << 8));

                        standpacketState++;
                        break;
                    case (7):
                        TempA = TempA + Convert.ToInt16((ch << 16));
                        standpacketState++;
                        break;
                    case (8):
                        TempA = TempA + Convert.ToInt16((ch << 24));
                        standpacketState = 0;
                        LeftPanelViewModel.ChankLen = TempA;
                        LeftPanelViewModel.mre.Set();
                        break;


                    default:
                        standpacketState = 0;
                        break;
                }
            }
            catch(Exception e) { Debug.WriteLine(e.Message); }
        }
        private void FiilsPlotPackets(byte ch)
        {

            byte[] readypacket;


            switch(plotpacketState)
            {
                case (0):   //First magic
                    if(ch == 0xbb)
                    {
                        pack[plotpacketState] = ch;
                        plotpacketState++;
                    }
                    break;
                case (1):   //Second magic
                    if(ch == 0xcc)
                    {
                        pack[plotpacketState] = ch;
                        plotpacketState++;
                    }
                    else
                        plotpacketState = 0;
                    break;
                case (2):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (3):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (4):
                    pack[plotpacketState] = ch;
                    plotpacketState++;

                    break;
                case (5):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (6):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (7):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (8):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (9):
                    pack[plotpacketState] = ch;
                    plotpacketState++;
                    break;
                case (10)://CheckSum

                    byte chechSum = 0;

                    chechSum += pack[2];
                    chechSum += pack[3];
                    chechSum += pack[4];
                    chechSum += pack[5];
                    chechSum += pack[6];
                    chechSum += pack[7];
                    chechSum += pack[8];
                    chechSum += pack[9];

                    if(chechSum == ch)
                    {
                        pack[plotpacketState] = ch;
                        readypacket = new byte[11];
                        Array.Copy(pack, 0, readypacket, 0, 11);

                        PlotPacketsList.Add(readypacket);
                        //ParserRayonM1.GetInstanceofParser.ParsePlot(PlotPacketsList);

                    }
                    else
                    {
                        //readypacket = new byte[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        //PlotPacketsList.Add(readypacket);
                        //ParserRayonM1.GetInstanceofParser.ParsePlot(PlotPacketsList);
                    }

                    plotpacketState = 0;
                    break;

                default:
                    plotpacketState = 0;
                    break;
            }
        }

        private void FiilsPlotBodePackets(byte ch)
        {

            byte[] readypacket;


            switch(plotbodepacketState)
            {
                case (0):   //First magic
                    if(ch == 0xaf)
                    {
                        bode_pack[plotbodepacketState] = ch;
                        plotbodepacketState++;
                    }
                    break;
                case (1):   //Second magic
                    if(ch == 0xfb)
                    {
                        bode_pack[plotbodepacketState] = ch;
                        plotbodepacketState++;
                    }
                    else
                        plotbodepacketState = 0;
                    break;
                case (2):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (3):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (4):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;

                    break;
                case (5):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (6):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (7):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (8):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (9):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (10):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (11):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (12):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (13):
                    bode_pack[plotbodepacketState] = ch;
                    plotbodepacketState++;
                    break;
                case (14)://CheckSum

                    byte chechSum = 0;

                    for(int i = 2; i < 14; i++)
                        chechSum += bode_pack[i];

                    if(chechSum == ch)
                    {
                        bode_pack[plotbodepacketState] = ch;
                        readypacket = new byte[15];
                        Array.Copy(bode_pack, 0, readypacket, 0, 15);

                        PlotBodePacketsList.Add(readypacket);
                        //ParserRayonM1.GetInstanceofParser.ParsePlot(PlotPacketsList);
                    }

                    plotbodepacketState = 0;
                    break;

                default:
                    plotbodepacketState = 0;
                    break;
            }
        }
    }
}



