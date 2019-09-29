﻿#define RELEASE_MODE
using Abt.Controls.SciChart;
using SuperButton.Models.DriverBlock;
using SuperButton.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using SuperButton.CommandsDB;
using System.Threading;
using System.Collections.Specialized;
using System.ComponentModel;
using SuperButton.Models.ParserBlock;

namespace SuperButton.ViewModels
{
    internal class DebugViewModel : ViewModelBase
    {
        #region FIELDS
        private static readonly object Synlock = new object();
        private static DebugViewModel _instance;
        #endregion FIELDS

        public static DebugViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new DebugViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private DebugViewModel()
        {
        }

        private ObservableCollection<object> _debugList;
        public ObservableCollection<object> DebugList
        {
            get
            {
                return Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"];
            }
            set
            {
                _debugList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _nternalParamList;
        public ObservableCollection<object> InternalParamList
        {
            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["InternalParam List"];
            }
            set
            {
                _nternalParamList = value;
                OnPropertyChanged();
            }
        }

        private string _debugId = "";
        public string DebugID
        {
            get { return _debugId; }
            set { _debugId = value; OnPropertyChanged("DebugID"); }
        }
        private string _debugIndex = "";
        public string DebugIndex
        {
            get { return _debugIndex; }
            set { _debugIndex = value; OnPropertyChanged("DebugIndex"); }
        }

#if !DEBUG || RELEASE_MODE
        private bool _enRefresh = true;
        private bool _debugRefresh = true;
#else
        private bool _enRefresh = false;
        private bool _debugRefresh = false;
#endif
        public bool EnRefresh
        {
            get
            {
                return _enRefresh;
            }
            set
            {
                _enRefresh = value;
                OnPropertyChanged("EnRefresh");
                if(value && LeftPanelViewModel.flag)
                {
                    if(!DebugRefresh)
                    {
                        Thread bkgnd = new Thread(LeftPanelViewModel.GetInstance.BackGroundFunc);
                        bkgnd.Start();
                    }
                    else
                        updateList = true;

                }
                else if(!value)
                {
                    RefreshManger.DataPressed = false;
                    foreach(var list in Commands.GetInstance.DataViewCommandsList)
                    {
                        try
                        {
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].IsSelected = false;
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].BackgroundStd = new SolidColorBrush(Colors.White);
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].BackgroundSmallFont = new SolidColorBrush(Colors.Gray);
                        }
                        catch(Exception)
                        {
                        }
                    }
                    if(DebugRefresh)
                        updateList = true;
                }
            }
        }
        public bool DebugRefresh
        {
            get
            {
                return _debugRefresh;
            }
            set
            {
                _debugRefresh = value;
                OnPropertyChanged("DebugRefresh");
                if(value && LeftPanelViewModel.flag)
                {
                    if(!EnRefresh)
                    {
                        Thread bkgnd = new Thread(LeftPanelViewModel.GetInstance.BackGroundFunc);
                        bkgnd.Start();
                    }
                    else
                        updateList = true;
                }
                else if(!value)
                    if(EnRefresh)
                        updateList = true;
            }
        }
        public static bool updateList = false;
        public ActionCommand addDebugOperation { get { return new ActionCommand(addDebugOperationCmd); } }
        private void addDebugOperationCmd()
        {
            if(DebugID != "" && DebugIndex != "")
            {
                if(!Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(DebugID), Convert.ToInt16(DebugIndex))))
                {
                    var data = new DebugObjModel
                    {
                        ID = DebugID,
                        Index = DebugIndex,
                        IntFloat = DebugIntFloat,
                        GetData = "",
                        SetData = "",
                    };
                    Commands.GetInstance.DebugCommandsList.Add(new Tuple<int, int>(Convert.ToInt16(data.ID), Convert.ToInt16(data.Index)), data);
                    Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].Add(data);
                    RefreshManger.buildGroup();
                }
            }
        }
        public ActionCommand removeDebugOperation { get { return new ActionCommand(removeDebugOperationCmd); } }
        private bool CompareDebugObj(DebugObjModel first, DebugObjModel second)
        {
            if(first.ID == second.ID)
                if(first.Index == second.Index)
                    return true;
            return false;
        }
        private void removeDebugOperationCmd()
        {
            if(DebugID != "" && DebugIndex != "")
            {
                if(Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int>(Convert.ToInt16(DebugID), Convert.ToInt16(DebugIndex))))
                {
                    Commands.GetInstance.DebugCommandsList.Remove(new Tuple<int, int>(Convert.ToInt16(DebugID), Convert.ToInt16(DebugIndex)));
                    var data1 = new DebugObjModel
                    {
                        ID = DebugID,
                        Index = DebugIndex,
                        IntFloat = true,
                        GetData = "",
                        SetData = "",
                    };

                    for(int i = 0; i < Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].Count; i++)
                    {
                        if(CompareDebugObj(data1, Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].ElementAt(i) as DebugObjModel))
                        {
                            Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].RemoveAt(i);
                            RefreshManger.buildGroup();
                        }
                    }
                }
            }
        }
        public ActionCommand GetCS { get { return new ActionCommand(GetCSCmd); } }
        private void GetCSCmd()
        {
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = "",
                ID = Convert.ToInt16(62),
                SubID = Convert.ToInt16(10),
                IsSet = false,
                IsFloat = false
            });

        }
        private bool _debugIntFloat = true;
        public bool DebugIntFloat
        {
            get
            {
                return _debugIntFloat;
            }
            set
            {
                _debugIntFloat = value;
                OnPropertyChanged("DebugIntFloat");
            }
        }

        public ActionCommand ClearDebugOp { get { return new ActionCommand(ClearDebugOpCmd); } }
        private void ClearDebugOpCmd()
        {
            Commands.GetInstance.DebugCommandsList.Clear();
            Commands.GetInstance.DebugCommandsListbySubGroup["Debug List"].Clear();
            RefreshManger.buildGroup();
        }

        #region In_Output_Parse
        public static CrcEventhandlerCalcHostFrameCrc CrcInputCalc = CrcBase.CalcHostFrameCrc;
        //Send data to controller
        public void TxBuildOperation(object Data2Send, Int16 Id, Int16 SubId, bool IsSet, bool IsFloat)
        {
            #region building
            byte[] temp = new byte[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            char tempChar = (char)0;

            temp[0] = 0x49;           //PreambleLSByte
            temp[1] = 0x5d;           //PreambleMsbyte
            temp[2] = (byte)(Id);     // ID msb

            tempChar = (char)(tempChar | ((char)(((Id >> 8)) & 0x3F)) | (((char)(SubId & 0x3)) << 6));
            temp[3] = (byte)tempChar;
            tempChar = (char)0;
            tempChar = (char)(tempChar | (char)(SubId >> 2));
            temp[4] = (byte)tempChar;
            if(IsSet == false)        //Set//Get
                temp[4] |= (1 << 4);

            if(IsFloat)        //Float//Int
                temp[4] |= (1 << 5);

            // 1<<6  -- Color 1
            // 1<<7  -- Color 2

            if(IsSet == false)
            {
                //Risng up delegate , to call static function from CRC class
                ushort TempGetCrc = CrcInputCalc(temp.Take(5), 2);
                temp[5] = (byte)(TempGetCrc & 0xFF);
                temp[6] = (byte)((TempGetCrc >> 8) & 0xFF);
            }
            else
            {
                if(Data2Send is double)                                           //Data float
                {
                    var datvaluevalue = BitConverter.GetBytes((float)(Data2Send is double ? (double)Data2Send : 0));
                    temp[5] = (byte)(datvaluevalue[0]);
                    temp[6] = (byte)(datvaluevalue[1]);
                    temp[7] = (byte)(datvaluevalue[2]);
                    temp[8] = (byte)(datvaluevalue[3]);
                }
                else if(Data2Send is int)//Data int
                {
                    //Int32 transit =(Int32) Data2Send;
                    temp[5] = (byte)(((int)Data2Send & 0xFF));
                    temp[6] = (byte)(((int)Data2Send >> 8) & 0xFF);
                    temp[7] = (byte)(((int)Data2Send >> 16) & 0xFF);
                    temp[8] = (byte)(((int)Data2Send >> 24) & 0xFF);
                }
                else if(IsFloat)                                           //Data float
                {
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
                            Data2Send = Data2Send.ToString().Substring(0, Data2Send.ToString().IndexOf("."));
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
            }
            //Risng up delegate , to call static function from CRC class
            ushort TempCrc = CrcInputCalc(temp.Take(9), 2);   // Delegate won  

            temp[9] = (byte)(TempCrc & 0xFF);
            temp[10] = (byte)((TempCrc >> 8) & 0xFF);
            #endregion building

            StringBuilder hex = new StringBuilder(temp.Length * 2);
            foreach(byte b in temp)
                hex.AppendFormat("{0:X2} ", b);

            string operation = "Tx: 0x";
            operation += hex.ToString();
            DebugTx = operation;
        }

        public void RxBuildOperation(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach(byte b in data)
                hex.AppendFormat("{0:X2} ", b);

            string operation = "Rx: 0x";
            operation += hex.ToString();
            DebugRx = operation;
        }

        #endregion

        private string _debugTx = "";
        private string _debugRx = "";
        public string DebugTx
        {
            get { return _debugTx; }
            set { _debugTx = value; OnPropertyChanged("DebugTx"); }
        }
        public string DebugRx
        {
            get { return _debugRx; }
            set { _debugRx = value; OnPropertyChanged("DebugRx"); }
        }
    }
}





