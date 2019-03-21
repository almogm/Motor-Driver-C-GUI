﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SuperButton.Models;
using SuperButton.Models.DriverBlock;
using System.Diagnostics;
using System.Windows.Controls;

namespace SuperButton.ViewModels
{
    class EnumViewModel : DataViewModel
    {


        private readonly List<string> _commandList = new List<string>();
        public bool IsUpdate = false;
        private int Count = 0;
        private string _selectedValue = "0";
        //private string _selectedindex="0";

        public List<string> CommandList
        {

            get { return _commandList; }
            set
            {
                foreach(var str in value)
                {
                    _commandList.Add(str);
                }

            }

        }
        public EnumViewModel(IEnumerable<string> enumlist)

        {
            _commandList.AddRange(enumlist);

        }

        new public string CommandValue
        {
            get { return base.CommandValue; }
            set
            {

                base.CommandValue = value;
                if(Count > 0)
                {
                    SelectedValue = CommandList[Convert.ToInt16(value) - 1];
                    Count++;
                }
                if(Count == 5)
                    Count = -1;
            }
        }

        public ICommand SelectedItemChanged1
        {
            get
            {
                return new RelayCommand(SendData, IsEnabled);
            }
        }

        private new void SendData()
        {
            if(!LeftPanelViewModel.GetInstance.ValueChange)
            {
                try
                {
                    if(Convert.ToInt16(_selectedValue) < 0) // SelectedValue
                    {
                        SelectedValue = "0";
                    }
                }
                catch
                {
                    _selectedValue = CommandList.FindIndex(x => x.StartsWith(_selectedValue)).ToString();
                }
                if(Count == 0 && _selectedValue != null && Convert.ToInt16(_selectedValue) >= 0) // SelectedValue SelectedValue
                {
                    int StartIndex = 0;
                    int ListIndex = Convert.ToInt16(_selectedValue); //SelectedValue
                    foreach(var List in SuperButton.CommandsDB.Commands.GetInstance.EnumViewCommandsList)
                    {
                        if((ListIndex < List.Value.CommandList.Count() && List.Value.CommandList[ListIndex] == CommandList[Convert.ToInt16(_selectedValue)]) || (ListIndex == 0 && List.Value.CommandList[ListIndex] == _selectedValue)) // SelectedValue SelectedValue
                        {
                            if(List.Value.CommandValue != "")
                            {
                                StartIndex = Convert.ToInt16(List.Value.CommandValue);
                                break;
                            }
                        }
                    }
                    //if(!LeftPanelViewModel.GetInstance.ValueChange)
                    BuildPacketTosend((ListIndex + StartIndex).ToString());
                }
            }
           else
                LeftPanelViewModel.GetInstance.ValueChange = false;
            
            if(Count == -1)
                Count = 0;
        }

        private bool IsEnabled()
        {
            return true;
        }
        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");

            }
        }
        public string SelectedValue
        {
            get { return _selectedValue != null ? (CommandList.FindIndex(x => x.StartsWith(_selectedValue))).ToString() : _selectedValue; }
            set
            {
                if(_selectedValue == value)
                {
                    _selectedValue = value;
                    OnPropertyChanged("SelectedValue");
                    LeftPanelViewModel.GetInstance.ValueChange = false;
                    return;
                }
                if(_selectedValue != null)
                {
                    try
                    {
                        if(Convert.ToInt16(value) >= 0 && Convert.ToInt16(value) < CommandList.Count)
                        {
                            _selectedValue = CommandList[Convert.ToInt16(value)];
                        }
                        LeftPanelViewModel.GetInstance.ValueChange = false;
                    }
                    catch(Exception e)
                    {
                        _selectedValue = value;
                        OnPropertyChanged("SelectedValue");
                    }
                }
                else
                {
                    _selectedValue = value;
                    OnPropertyChanged("SelectedValue");
                }
            }
        }
        //public string SelectedValue
        //{
        //    get { return _selectedValue; }
        //    set
        //    {

        //        if (_selectedValue != null)
        //        {
        //            _selectedValue = value;

        //            var index = (CommandList.FindIndex(x => x.StartsWith(value)) + 1).ToString();
        //            OnPropertyChanged();
        //            if (!IsUpdate)
        //                BuildPacketTosend(index);
        //            else
        //            {
        //                IsUpdate = false;

        //            }

        //        }
        //        else
        //        {
        //            _selectedValue = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}
        //public string Selectedindex
        //{
        //    get { return _selectedindex; }
        //    set
        //    {

        //        if (_selectedindex != null)
        //        {
        //            _selectedindex = value;

        //            OnPropertyChanged();


        //        }
        //    }
        //}

        public EnumViewModel()
        {
            // TODO: Complete member initialization
        }
        private void BuildPacketTosend(string val)
        {
            if(LeftPanelViewModel.GetInstance.ConnectButtonContent == "Disconnect")
            {
                Task.Factory.StartNew(action: () =>
                {
                    var tmp = new PacketFields
                    {
                        Data2Send = val,
                        ID = Convert.ToInt16(CommandId),
                        SubID = Convert.ToInt16(CommandSubId),
                        IsSet = true,
                        IsFloat = IsFloat
                    };
                    Rs232Interface.GetInstance.SendToParser(tmp);
                });
            }
        }
    }
}

