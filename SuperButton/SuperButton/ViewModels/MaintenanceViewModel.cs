﻿using System;
using System.Collections.ObjectModel;
using SuperButton.CommandsDB;
using SuperButton.Models.DriverBlock;

namespace SuperButton.ViewModels
{
    class MaintenanceViewModel : ViewModelBase
    {
        private static readonly object Synlock = new object();
        private static MaintenanceViewModel _instance;
        public static MaintenanceViewModel GetInstance
        {
            get
            {
                lock (Synlock)
                {
                    if (_instance != null) return _instance;
                    _instance = new MaintenanceViewModel();
                    return _instance;
                }
            }
        }
        private MaintenanceViewModel()
        {

        }

        private ObservableCollection<object> _maintenanceList;
        public ObservableCollection<object> MaintenanceList
        {
            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Maintenance List"];
            }
            set
            {
                _maintenanceList = value;
                OnPropertyChanged();
            }
        }

        private bool _save;
        public bool Save
        {
            get { return _save; }
            set
            {
                _save = value;
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = true?1:0,
                    ID = 63,
                    SubID = Convert.ToInt16(0),
                    IsSet = true,
                    IsFloat = true
                }
                );
                OnPropertyChanged();
            }
        }
        private bool _manufacture;
        public bool Manufacture
        {
            get { return _manufacture; }
            set
            {
                _manufacture = value;
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = true ? 1 : 0,
                    ID = 63,
                    SubID = Convert.ToInt16(1),
                    IsSet = true,
                    IsFloat = true
                }
                );
                OnPropertyChanged();
            }
        }
        private bool _reboot;
        public bool Reboot
        {
            get { return _reboot; }
            set
            {
                _reboot = value;
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = true ? 1 : 0,
                    ID = 63,
                    SubID = Convert.ToInt16(2),
                    IsSet = true,
                    IsFloat = true
                }
                );
                OnPropertyChanged();
            }
        }
        private bool _enableWrite;
        public bool EnableWrite
        {
            get { return _enableWrite; }
            set
            {
                _enableWrite = value;
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = true ? 1 : 0,
                    ID = 63,
                    SubID = Convert.ToInt16(10),
                    IsSet = true,
                    IsFloat = true
                }
                );
                OnPropertyChanged();
            }
        }
        private bool _enableLoder;
        public bool EnableLoder
        {
            get { return _enableLoder; }
            set
            {
                _enableLoder = value;
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = true ? 1 : 0,
                    ID = 65,
                    SubID = Convert.ToInt16(0),
                    IsSet = true,
                    IsFloat = true
                }
                );
                OnPropertyChanged();
            }
        }
    }
}
