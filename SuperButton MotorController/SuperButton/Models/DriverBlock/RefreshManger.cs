﻿//#define REFRESH_MANAGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperButton.CommandsDB;
using SuperButton.ViewModels;
using System.Collections.ObjectModel;
using SuperButton.Views;
using System.Collections;
using SuperButton.Helpers;
using System.Diagnostics;
using System.Windows.Threading; // For Dispatcher.
using System.Windows;
using System.Windows.Media;
using SuperButton.Models.ParserBlock;

namespace SuperButton.Models.DriverBlock
{

    public class RefreshManger
    {
        public static int tab = Views.ParametarsWindow.ParametersWindowTabSelected;
        private static readonly object Synlock = new object();
        private static RefreshManger _instance;

        public static Dictionary<string, ObservableCollection<object>> BuildGroup = new Dictionary<string, ObservableCollection<object>>();
        public static Dictionary<Tuple<int, int>, DataViewModel> BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
        public static int TempTab = 0;
        private static bool _dataPressed = false;
        public static bool DataPressed { get { return _dataPressed; } set { _dataPressed = value; } }
        public static RefreshManger GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new RefreshManger();
                    buildGroup();
                    return _instance;
                }
            }
        }

        public static void buildGroup()
        {
            BuildGroup = new Dictionary<string, ObservableCollection<object>>();
            BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
            //var DataViewlist = CommandsDB.Commands.GetInstance.DataViewCommandsList;
            //var EnumViewlist = CommandsDB.Commands.GetInstance.EnumViewCommandsList;
            var AllDataList = CommandsDB.Commands.GetInstance.DataCommandsListbySubGroup;
            var AllEnumList = CommandsDB.Commands.GetInstance.EnumCommandsListbySubGroup;
            var AllCalList = CommandsDB.Commands.GetInstance.CalibartionCommandsListbySubGroup;
            var AllBoolList = Commands.GetInstance.DigitalInputListbySubGroup;
            var AllDebugList = Commands.GetInstance.DebugCommandsListbySubGroup;

            foreach(var list in AllEnumList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    var data = new DataViewModel
                    {
                        CommandName = ((DataViewModel)sub_list).CommandName,
                        CommandId = ((DataViewModel)sub_list).CommandId,
                        CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                        CommandValue = ((DataViewModel)sub_list).CommandValue,
                        IsFloat = ((DataViewModel)sub_list).IsFloat,
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
            foreach(var list in AllDataList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    var data = new DataViewModel
                    {
                        CommandName = ((DataViewModel)sub_list).CommandName,
                        CommandId = ((DataViewModel)sub_list).CommandId,
                        CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                        CommandValue = ((DataViewModel)sub_list).CommandValue,
                        IsFloat = ((DataViewModel)sub_list).IsFloat,
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
            foreach(var list in AllCalList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    if(list.Key == "Calibration List")
                    {
                        CalibrationButtonModel temp = sub_list as CalibrationButtonModel;
                        string CommandName = temp.CommandName;
                        string CommandId = temp.CommandId;
                        string CommandSubId = temp.CommandSubId;
                        bool IsFloat = temp.IsFloat;
                        var data = new DataViewModel
                        {
                            CommandName = CommandName,
                            CommandId = CommandId,
                            CommandSubId = CommandSubId,
                            IsFloat = IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                    else
                    {
                        var data = new DataViewModel
                        {
                            CommandName = ((DataViewModel)sub_list).CommandName,
                            CommandId = ((DataViewModel)sub_list).CommandId,
                            CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                            CommandValue = ((DataViewModel)sub_list).CommandValue,
                            IsFloat = ((DataViewModel)sub_list).IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                }
            }
            foreach(var list in AllBoolList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    if(list.Key == "Digital Input List")
                    {
                        BoolViewIndModel temp = sub_list as BoolViewIndModel;
                        string CommandName = temp.CommandName;
                        string CommandId = temp.CommandId;
                        string CommandSubId = temp.CommandSubId;
                        bool IsFloat = temp.IsFloat;
                        var data = new DataViewModel
                        {
                            CommandName = CommandName,
                            CommandId = CommandId,
                            CommandSubId = CommandSubId,
                            IsFloat = IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                    else
                    {
                        var data = new DataViewModel
                        {
                            CommandName = ((DataViewModel)sub_list).CommandName,
                            CommandId = ((DataViewModel)sub_list).CommandId,
                            CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                            CommandValue = ((DataViewModel)sub_list).CommandValue,
                            IsFloat = ((DataViewModel)sub_list).IsFloat,
                        };
                        BuildGroup[list.Key].Add(data);
                    }
                }
            }
            foreach(var list in AllDebugList)
            {
                BuildGroup.Add(list.Key, new ObservableCollection<object>());
                foreach(var sub_list in list.Value)
                {
                    DebugObjModel temp = sub_list as DebugObjModel;
                    string CommandName = "";
                    string CommandId = temp.ID;
                    string CommandSubId = temp.Index;
                    bool IsFloat = temp.IntFloat;
                    var data = new DataViewModel
                    {
                        CommandName = CommandName,
                        CommandId = CommandId,
                        CommandSubId = CommandSubId,
                        IsFloat = !IsFloat,
                    };
                    BuildGroup[list.Key].Add(data);
                }
            }
            //foreach (var list in AllEnumList)
            //{
            //    foreach (var sub_list in list.Value)
            //    {
            //        var data = new DataViewModel
            //        {
            //            CommandName = ((DataViewModel)sub_list).CommandName,
            //            CommandId = ((DataViewModel)sub_list).CommandId,
            //            CommandSubId = ((DataViewModel)sub_list).CommandSubId,
            //            CommandValue = ((DataViewModel)sub_list).CommandValue,
            //            IsFloat = ((DataViewModel)sub_list).IsFloat,
            //        };
            //        BuildList.Add(new Tuple<int, int>(Int32.Parse(((DataViewModel)sub_list).CommandId), Int32.Parse(((DataViewModel)sub_list).CommandSubId)), data);
            //    }
            //}
            //foreach (var list in AllDataList)
            //{
            //    foreach (var sub_list in list.Value)
            //    {
            //        var data = new DataViewModel
            //        {
            //            CommandName = ((DataViewModel)sub_list).CommandName,
            //            CommandId = ((DataViewModel)sub_list).CommandId,
            //            CommandSubId = ((DataViewModel)sub_list).CommandSubId,
            //            CommandValue = ((DataViewModel)sub_list).CommandValue,
            //            IsFloat = ((DataViewModel)sub_list).IsFloat,
            //        };
            //        BuildList.Add(new Tuple<int, int>(Int32.Parse(((DataViewModel)sub_list).CommandId), Int32.Parse(((DataViewModel)sub_list).CommandSubId)), data);
            //    }
            //}
        }
        public string[] GroupToExecute(int tabIndex)//
        {
            string[] PanelElements = new string[] { "DriverStatus List", "Channel List", "MotionCommand List2", "MotionCommand List",
                                                    "Profiler Mode", "S.G.List", "S.G.Type", "PowerOut List", "Control", "Motor",
                                                    "MotionStatus List", "Digital Input List", "Position counters List",
                                                    "UpperMainPan List" };// , "Driver Type" ,
            string[] arr = new string[] { };
            if(DebugViewModel.GetInstance.EnRefresh)
            {
                switch(tabIndex)
                {
                    case 0:
                        arr = new string[] { "Motion Limit" }; // "Control", "Motor", already in PanelElements array
                        break;
                    case 1:
                        arr = new string[] { "Hall", "Qep1", "Qep2", "SSI_Feedback", "Qep1Bis", "Qep2Bis" };
                        break;
                    case 2:
                        arr = new string[] { "PIDCurrent", "PIDSpeed", "PIDPosition" };
                        break;
                    case 3:
                        arr = new string[] { "DeviceSerial", "BaudrateList" };
                        break;
                    case 4:
                        arr = new string[] { "Calibration Result List", "Calibration List" };
                        break;
                    case 5:
                        arr = new string[] { "CurrentLimit List" };
                        break;
                    case 7:
                        arr = new string[] { "AnalogCommand List" };
                        break;
                    default:
                        break;
                }
                arr = arr.Concat(PanelElements).ToArray();
            }
            if(DebugViewModel.GetInstance.DebugRefresh)
            {
                switch(tabIndex)
                {
                    case 8:
                        arr = arr.Concat(new string[] { "Debug List" }).ToArray();
                        break;
                    default:
                        break;
                }
            }

            return arr;
        }
        //public List<byte> arr = new List<byte>();
        //public List<byte> arr2 = new List<byte>();
        //public List<byte> arr3 = new List<byte>();
        //public List<byte> arr4 = new List<byte>();
        public void StartRefresh()
        {
            if(true)//!RefreshManger.DataPressed LeftPanelViewModel.GetInstance.EnRefresh && 
            {
                tab = Views.ParametarsWindow.ParametersWindowTabSelected;
                if(ParametarsWindow.WindowsOpen == false)
                    tab = -1;

                Debug.WriteLine("StartRefresh: " + DateTime.Now.ToString("h:mm:ss.fff"));
                if(tab != TempTab || DebugViewModel.updateList)
                {
                    BuildList = new Dictionary<Tuple<int, int>, DataViewModel>();
                    foreach(var list in BuildGroup)
                    {
                        if(GroupToExecute(tab).Contains(list.Key))
                        {
                            foreach(var sub_list in list.Value)
                            {
                                var data = new DataViewModel
                                {
                                    CommandName = ((DataViewModel)sub_list).CommandName,
                                    CommandId = ((DataViewModel)sub_list).CommandId,
                                    CommandSubId = ((DataViewModel)sub_list).CommandSubId,
                                    CommandValue = ((DataViewModel)sub_list).CommandValue,
                                    IsFloat = ((DataViewModel)sub_list).IsFloat,
                                    IsSelected = ((DataViewModel)sub_list).IsSelected,
                                };
                                if(!BuildList.ContainsKey(new Tuple<int, int>(Int32.Parse(data.CommandId), Int32.Parse(data.CommandSubId))))
                                {
                                    BuildList.Add(new Tuple<int, int>(Int32.Parse(((DataViewModel)sub_list).CommandId), Int32.Parse(((DataViewModel)sub_list).CommandSubId)), data);
                                }
                            }
                        }
                    }
                    TempTab = tab;
                    if(DebugViewModel.updateList)
                        DebugViewModel.updateList = false;
#if REFRESH_MANAGER
                    Debug.WriteLine(" --- Tab --- ");
#endif
                }
                if(BuildList.Count == 0)
                {
                    if(DebugViewModel.GetInstance.DebugRefresh)
                        DebugViewModel.GetInstance.DebugRefresh = false;
                    TempTab = -1;
                    return;
                }
                //Debug.WriteLine("2: " + DateTime.Now.ToString("h:mm:ss.fff"));
                foreach(var command in BuildList)
                {
                    if(!command.Value.IsSelected)
                    {
                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = command.Value.CommandValue,
                            ID = Convert.ToInt16(command.Value.CommandId),
                            SubID = Convert.ToInt16(command.Value.CommandSubId),
                            IsSet = false,
                            IsFloat = command.Value.IsFloat
                        });
                    }
                    else
                    {
                    }
                    Thread.Sleep(1);
                    //Thread.SpinWait(1000);
                }

#if REFRESH_MANAGER
                Debug.WriteLine("EndRefresh: " + DateTime.Now.ToString("h:mm:ss.fff"));
#endif
            }
        }
        static int ConnectionCount = 0;
        //public bool _oneSelected = false;
        public void VerifyConnection()
        {
            while(true && Rs232Interface._comPort.IsOpen)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = "",
                    ID = Convert.ToInt16(1),
                    SubID = Convert.ToInt16(0),
                    IsSet = false,
                    IsFloat = false
                });
                Thread.Sleep(500);
                ConnectionCount++;
                if(ConnectionCount == 5)
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Connection Lost"));
                    LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Not Connected";
                    DisconnectedFlag = true;
                }
            }
            EventRiser.Instance.RiseEevent(string.Format($"Connection Lost"));
            LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Not Connected";
            RefreshManger.GetInstance.DisconnectedFlag = true;
            Task.Run((Action)Rs232Interface.GetInstance.Disconnect);
        }
        //private void MouseLeaveCommandFunc()
        //{
        //    RefreshManger.DataPressed = false;
        //    foreach(var list in Commands.GetInstance.DataViewCommandsList)
        //    {
        //        try
        //        {
        //            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].IsSelected = false;
        //            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].Background2 = new SolidColorBrush(Colors.White);
        //            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].Background = new SolidColorBrush(Colors.Gray);
        //        }
        //        catch(Exception e)
        //        {

        //        }
        //    }

        //    SuperButton.Models.DriverBlock.RefreshManger.GetInstance._oneSelected = false;
        //}
        string CalibrationGetStatus(string returnedValue)
        {
            switch(Convert.ToInt16(returnedValue))
            {
                case 0:
                    return "Idle";
                case 1:
                    return "in process";
                case 2:
                    return "failure";
                case 3:
                    return "success";
                default:
                    return "no info(" + returnedValue + ")";
            }
        }
        string CalibrationGetError(string returnedValue)
        {
            switch(Convert.ToInt32(returnedValue))
            {
                //uint16_t hallFeedlErr:1;         // 0x01 - 1
                //uint16_t encPhaseErr:1;          // 0x02 - 2
                //uint16_t encoderHallMismach:1;  // 0x04  - 4
                //uint16_t overTemperature:1;     // 0x08  - 8
                //uint16_t overVoltage:1;         // 0x010 - 16
                //uint16_t underVoltage:1;        //0x20   - 32
                //uint16_t speedRangeErr:1;       //0x40   - 64
                //uint16_t positionErr:1;         //0x80   - 128
                //uint16_t gateDriverFault:1;     //0x0100 - 256
                //uint16_t nOCTW:1;               //0x0200 - 512
                //uint16_t gateDriverInit:1;      //0x0400 - 1024
                //uint16_t motorStall:1;           //0x0800 - 2048
                //uint16_t Reserved3:1;           //0x1000 - 4096
                //uint16_t Reserved4:1;           //0x2000 - 8192
                //uint16_t ADCoffset:1;           //0x4000 - 16384
                //uint16_t FetShort:1;            //0x8000 - 32768
                case 0:
                    return "All OK !";
                case 1:
                    return "hall Feedl Err";
                case 2:
                    return "enc Phase Err";
                case 4:
                    return "encoder Hall Mismach";
                case 8:
                    return "overTemperature";
                case 16:
                    return "over Voltage";
                case 32:
                    return "under Voltage";
                case 64:
                    return "speed Range Err";
                case 128:
                    return "position Err";
                case 256:
                    return "gate Driver Fault";
                case 512:
                    return "nOCTW";
                case 1024:
                    return "gate Driver Init";
                case 2048:
                    return "motorStall";
                case 4096:
                    return "Reserved3";
                case 8192:
                    return "Reserved4";
                case 16384:
                    return "ADC offset";
                case 32768:
                    return "Fet Short";
                default:
                    return "no info(" + returnedValue + ")";
            }
        }

        string YAxisUnits(string channel, string returnedValue)
        {
            string value = "";
            switch(Convert.ToInt32(returnedValue))
            {
                case 0:
                    value = "[A]";
                    break;
                case 1:
                    value = "[V]";
                    break;
                case 5:
                    value = "[Elec_Angle]";
                    break;
                case 6:
                    value = "[Mech_Angle]";
                    break;
                case 10:
                    value = "[RPM/V]";
                    break;
                case 11:
                    value = "[Count/Sec]";
                    break;
                case 12:
                    value = "[Round/Min]";
                    break;
                case 13:
                    value = "[Counts]";
                    break;
                default:
                    value = "N/A";
                    break;
            }
            return channel + ": " + value;
        }
        //public static int CalibrationTimeOut = 10;
        //private static int PrecedentIdx = 0;
        public bool DisconnectedFlag = false;
        public void YAxisLegend(int Sel, int identifier)
        {
            if(LeftPanelViewModel.GetInstance.StarterOperationFlag || DisconnectedFlag)
            {
                if(Sel <= OscilloscopeViewModel.GetInstance.Channel1SourceItems.Count && Sel <= OscilloscopeViewModel.GetInstance.Channel2SourceItems.Count)
                {
                    try
                    {
                        OscilloscopeViewModel.GetInstance.YAxisUnits = "CH" + identifier.ToString() + ": " + OscilloscopeViewModel.GetInstance.ChannelYtitles.First(x => x.Key == OscilloscopeViewModel.GetInstance.SelectedCh1DataSource).Value;
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
            else if(Sel <= OscilloscopeViewModel.GetInstance.ChannelYtitles.Count)
                OscilloscopeViewModel.GetInstance.YAxisUnits = "CH" + identifier.ToString() + ": " + OscilloscopeViewModel.GetInstance.ChannelYtitles.ElementAt(Sel).Value;
        }
        internal void UpdateModel(Tuple<int, int> commandidentifier, string newPropertyValue)
        {
            try
            {
                if(LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    Debug.WriteLine("Starter: " + commandidentifier.Item1.ToString() + ' ' + commandidentifier.Item2.ToString());

                    switch(commandidentifier.Item1)
                    {
                        case 1:
                            LeftPanelViewModel.GetInstance.StarterCount += 1;
                            break;
                        case 66:
                            LeftPanelViewModel.GetInstance.StarterCount += 1;
                            break;
                        case 60:
                            LeftPanelViewModel.GetInstance.StarterCount += 1;
                            break;
                        case 62:
                            LeftPanelViewModel.GetInstance.StarterCount += 1;
                            break;
                        default:
                            break;
                    }
                }
                LeftPanelViewModel.GetInstance.ValueChange = false;
                if(DebugViewModel.GetInstance.EnRefresh || LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    #region Calibration
                    if(commandidentifier.Item1 == 6)
                    {
                        if(Commands.GetInstance.CalibartionCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                        {
                            if(Convert.ToInt16(newPropertyValue) == 0)
                                Commands.GetInstance.CalibartionCommandsList[new Tuple<int, int>(6, commandidentifier.Item2)].ButtonContent = "Run";
                            else if(Convert.ToInt16(newPropertyValue) == 1)
                                Commands.GetInstance.CalibartionCommandsList[new Tuple<int, int>(6, commandidentifier.Item2)].ButtonContent = "Running";
                        }
                        else if(Commands.GetInstance.DataViewCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = CalibrationGetStatus(newPropertyValue);
                    }
                    #endregion Calibration
                    #region Plot_Channels
                    else if(commandidentifier.Item1 == 60 && commandidentifier.Item2 <= 2)
                    {
                        int Sel = 0;
                        if(Int32.Parse(newPropertyValue) >= 0)
                        {
                            Sel = Int32.Parse(newPropertyValue);

                            if((LeftPanelViewModel.GetInstance.StarterOperationFlag || DisconnectedFlag))
                            {
                                if(Sel <= OscilloscopeViewModel.GetInstance.Channel1SourceItems.Count && Sel <= OscilloscopeViewModel.GetInstance.Channel2SourceItems.Count)
                                {
                                    if(commandidentifier.Item2 == 1)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch1SelectedIndex = Sel;
                                        OscilloscopeViewModel.GetInstance.SelectedCh1DataSource = OscilloscopeViewModel.GetInstance.Channel1SourceItems.ElementAt(Sel);
                                        YAxisLegend(Sel, commandidentifier.Item2);
                                    }
                                    else if(commandidentifier.Item2 == 2)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch2SelectedIndex = Sel;
                                        OscilloscopeViewModel.GetInstance.SelectedCh2DataSource = OscilloscopeViewModel.GetInstance.Channel2SourceItems.ElementAt(Sel);
                                        YAxisLegend(Sel, commandidentifier.Item2);
                                        DisconnectedFlag = false;
                                    }
                                }
                            }
                            else
                            {
                                if(Sel <= OscilloscopeViewModel.GetInstance.ChannelYtitles.Count)
                                {
                                    if(commandidentifier.Item2 == 1)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch1SelectedIndex = Sel;
                                    }
                                    else if(commandidentifier.Item2 == 2)
                                    {
                                        OscilloscopeViewModel.GetInstance.Ch2SelectedIndex = Sel;
                                    }
                                    YAxisLegend(Sel, commandidentifier.Item2);
                                }
                            }
                        }
                    }
                    /*else if(commandidentifier.Item1 == 66)
                    {
                        if(commandidentifier.Item2 == 0)
                            OscilloscopeParameters.IfullScale = float.Parse(newPropertyValue);
                        else if(commandidentifier.Item2 == 1)
                            OscilloscopeParameters.VfullScale = float.Parse(newPropertyValue);
                        OscilloscopeParameters.InitList();
                    }*/
                    #endregion Plot_Channels
                    #region DataView_EnumView
                    else if(commandidentifier.Item1 == 33)
                    {
                        LeftPanelViewModel.GetInstance.DriverStat = CalibrationGetError(newPropertyValue);
                    }
                    else if(commandidentifier.Item1 == 12 && commandidentifier.Item2 == 1) // Power Output Command
                    {
                        if(newPropertyValue == "1")
                            BottomPanelViewModel.GetInstance.PowerOutputChecked = true;
                        else
                            BottomPanelViewModel.GetInstance.PowerOutputChecked = false;

                    }
                    else if(commandidentifier.Item1 == 1 && commandidentifier.Item2 == 0) // MotorStatus
                    {
                        if(LeftPanelViewModel.GetInstance.ConnectTextBoxContent == "Connected")
                        {
                            if(newPropertyValue == "1" || newPropertyValue == "0")
                            {
                                ConnectionCount = 0;
                                LeftPanelViewModel.GetInstance.LedMotorStatus = Convert.ToInt16(newPropertyValue);
                                Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            }
                        }
                        else if(LeftPanelViewModel.GetInstance.ConnectTextBoxContent == "Not Connected")
                        {
                            LeftPanelViewModel.GetInstance.ConnectTextBoxContent = "Connected";
                            if(DisconnectedFlag)
                            {
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    Data2Send = "",
                                    ID = Convert.ToInt16(60),
                                    SubID = Convert.ToInt16(1), // SelectedCh1DataSource
                                    IsSet = false,
                                    IsFloat = false
                                });
                                Thread.Sleep(2);
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    Data2Send = "",
                                    ID = Convert.ToInt16(60),
                                    SubID = Convert.ToInt16(2), // SelectedCh2DataSource
                                    IsSet = false,
                                    IsFloat = false
                                });
                                Rs232Interface.GetInstance.SendToParser(new PacketFields
                                {
                                    Data2Send = "1",
                                    ID = Convert.ToInt16(64),
                                    SubID = Convert.ToInt16(0), // AutoBaud (Synch)
                                    IsSet = true,
                                    IsFloat = false
                                });
                            }
                        }
                    }
                    //else if(commandidentifier.Item1 == 65 && commandidentifier.Item2 == 0) // EnableLoader
                    //{
                    //    MaintenanceViewModel.GetInstance.EnableLoder = (newPropertyValue == 0.ToString()) ? false : true;
                    //}
                    else if(Commands.GetInstance.DataViewCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2))
                        && Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].IsSelected == false)
                    {
                        Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                        if(commandidentifier.Item1 == 62 && commandidentifier.Item2 < 3)
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                        else if(commandidentifier.Item1 == 62 && commandidentifier.Item2 == 10)
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = "0x" + int.Parse(newPropertyValue).ToString("X");
                    }
                    else if(Commands.GetInstance.EnumViewCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                    {
                        try
                        {
                            int index = Convert.ToInt32(newPropertyValue) - Convert.ToInt32(Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue);
                            if(index < Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandList.Count && index >= 0)
                            {
                                Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].SelectedItem =
                                Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandList[index];
                            }
                        }
                        catch
                        {
                        }
                    }
                    #endregion DataView_EnumView
                    #region DigitalInput
                    else if(Commands.GetInstance.DigitalInputList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)))
                    {
                        Commands.GetInstance.DigitalInputList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = Convert.ToInt16(newPropertyValue) == 1 ? 1 : 0;
                    }
                    #endregion DigitalInput
                }
                #region StartUpAPP
                else if(commandidentifier.Item1 == 62)
                {
                    switch(commandidentifier.Item2)
                    {
                        case 0:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 1:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 2:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 3:
                            Commands.GetInstance.DataViewCommandsListLP[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = newPropertyValue;
                            break;
                        case 10:
                            Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].CommandValue = "0x" + int.Parse(newPropertyValue).ToString("X");
                            break;
                    }
                }
                #endregion StartUpAPP
                //}
                #region DebugTab
                //if(DebugViewModel.GetInstance.DebugRefresh)
                if(Commands.GetInstance.DebugCommandsList.ContainsKey(new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2))) // Debug Panel
                {
                    try
                    {
                        Commands.GetInstance.DebugCommandsList[new Tuple<int, int>(commandidentifier.Item1, commandidentifier.Item2)].GetData = newPropertyValue;
                        DebugViewModel.GetInstance.RxBuildOperation(ParserRayonM1.DebugData);
                    }
                    catch { }
                }
                #endregion DebugTab
            }
            catch(Exception error)
            {
                Debug.WriteLine(error.Message);
            }
        }
    }
}
