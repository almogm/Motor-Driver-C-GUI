﻿using System.Collections.ObjectModel;
using SuperButton.CommandsDB;
using System.Windows.Input;
using SuperButton.Models;
using System.Windows.Controls;
using System;
using System.Threading;

namespace SuperButton.ViewModels
{
    enum eTab
    {
        CONTROL = 0,
        FEED_BACKS = 1,
        PID = 2,
        DEVICE = 3,
        I_O = 4,
        CALIBRATION = 5,
        MAINTENANCE = 6,
        DEBUG = 7
    };
    internal class ParametarsWindowViewModel : ViewModelBase
    {
        private CalibrationViewModel _calibrationViewModel;
        private MotionViewModel _motionViewModel;
        private MaintenanceViewModel _maintenanceViewModel;
        private FeedBackViewModel _feedBackViewModel;
        private DebugViewModel _debugViewModel;
        private IOViewModel _ioViewModel;
        //private LoadParamsViewModel _loadParamsViewModel;
        public ParametarsWindowViewModel()
        {
            _calibrationViewModel = CalibrationViewModel.GetInstance;
            _motionViewModel = MotionViewModel.GetInstance;
            _maintenanceViewModel = MaintenanceViewModel.GetInstance;
            _feedBackViewModel = FeedBackViewModel.GetInstance;
            _debugViewModel = DebugViewModel.GetInstance;
            _ioViewModel = IOViewModel.GetInstance;
            //_loadParamsViewModel = LoadParamsViewModel.GetInstance;
        }
        ~ParametarsWindowViewModel() { }
        public ObservableCollection<object> ControlList
        {
            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["Control"];
            }
        }

        public ObservableCollection<object> MotorlList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Motor"];
            }

        }
        private ObservableCollection<object> _motorLimitlList;
        public ObservableCollection<object> MotorLimitlList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Motion Limit"];
            }
            set
            {
                _motorLimitlList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _currentLimitList;
        public ObservableCollection<object> CurrentLimitList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["CurrentLimit List"];
            }
            set
            {
                _currentLimitList = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<object> DigitalFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Digital"];
            }

        }
        public ObservableCollection<object> AnalogFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Analog"];
            }

        }
        private ObservableCollection<object> _pidCurrentList;
        public ObservableCollection<object> PidCurrentList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["PIDCurrent"];
            }
            set
            {
                _pidCurrentList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _pidSpeedList;
        public ObservableCollection<object> PidSpeedList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["PIDSpeed"];
            }
            set
            {
                _pidSpeedList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _pidPositionList;
        public ObservableCollection<object> PidPositionList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["PIDPosition"];
            }
            set
            {
                _pidPositionList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _baudrateList;
        public ObservableCollection<object> DeviceSerialList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["DeviceSerial"];
            }

        }
        public ObservableCollection<object> BaudrateList
        {

            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["BaudrateList"];
            }
            set
            {
                _baudrateList = value;
                OnPropertyChanged();
            }

        }

        public virtual ICommand TestEnumChange
        {
            get
            {
                return new RelayCommand(EnumChange, CheckValue);
            }
        }
        public CalibrationViewModel CalibrationViewModel
        {
            get { return _calibrationViewModel; }
        }

        public MotionViewModel MotionViewModel
        {
            get { return _motionViewModel; }
        }
        public IOViewModel IOViewModel
        {
            get { return _ioViewModel; }
        }

        public MaintenanceViewModel MaintenanceViewModel
        {
            get { return _maintenanceViewModel; }
        }

        public FeedBackViewModel FeedBackViewModel
        {
            get { return _feedBackViewModel; }
        }
        //public LoadParamsViewModel LoadParamsViewModel
        //{
        //    get { return _loadParamsViewModel; }
        //}
        public DebugViewModel DebugViewModel
        {
            get { return _debugViewModel; }
        }

        private bool CheckValue()
        {
            return true;
        }

        private void EnumChange()
        {
        }
    }
}
