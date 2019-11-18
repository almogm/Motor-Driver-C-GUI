﻿using System;
using System.Diagnostics;
using Abt.Controls.SciChart.Example.Data;
using SuperButton.Data;
using SuperButton.ViewModels;
using SuperButton.Helpers;

namespace SuperButton.Models.DriverBlock
{
    public class Rs232InterfaceEventArgs : EventArgs
    {
        public readonly PacketFields PacketRx;
        public readonly int ParseLength;
        public readonly byte[] InputChank;
        public readonly string ConnecteButtonLabel;

       // public readonly DoubleSeries Datasource1;
        public byte[] DataChunk { get; private set; }
        public readonly byte SMagicFirst;
        public readonly byte SMagicSecond;
        public readonly byte PMagicFirst;
        public readonly byte PMagicSecond;
        public readonly UInt16 PacketLength;



        public Rs232InterfaceEventArgs(byte[] dataChunk, byte smagicFirst, byte smagicSecond, byte pmagicFirst, byte pmagicSecond, UInt16 packetLength)
        {

            DataChunk = dataChunk;
            SMagicFirst = smagicFirst;
            SMagicSecond = smagicSecond;
            PacketLength = packetLength;
            PMagicFirst = pmagicFirst;
            PMagicSecond = pmagicSecond;
        }

        
        public Rs232InterfaceEventArgs(byte[] dataChunk)
        {
            //LeftPanelViewModel.GetInstance.LedStatusRx = RoundBoolLed.PASSED;
            LeftPanelViewModel.GetInstance.led = LeftPanelViewModel.RX_LED;
            DataChunk = dataChunk;      //Receive packet
            //LeftPanelViewModel.GetInstance.led = -1;
        }

        public Rs232InterfaceEventArgs(string connecteButtonLabel)
        {
            ConnecteButtonLabel = connecteButtonLabel;
        }

   


        public Rs232InterfaceEventArgs(PacketFields packetRx)
        {
            //LeftPanelViewModel.GetInstance.LedStatusTx = RoundBoolLed.PASSED;
            LeftPanelViewModel.GetInstance.led = LeftPanelViewModel.TX_LED;
            PacketRx = packetRx;     // Send Packet
            //LeftPanelViewModel.GetInstance.led = -1;
            //Debug.WriteLine(PacketRx.IsSet);
        }




        //public Rs232InterfaceEventArgs(DoubleSeries xyChannelOnep)
        //{
        //    Datasource1 = xyChannelOnep;
        //}


        public Rs232InterfaceEventArgs(int numberofbytes,byte[] inputBytes)
        {
            ParseLength = numberofbytes;
            InputChank = inputBytes;
        }

    }
}