using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Caliburn.Micro;
using System.Collections;

namespace WP7App1
{
    public class AlarmItem : PropertyChangedBase
    {
        private string _firstname;
        public string FirstName
        {
            get
            {
                return _firstname;
            }
            set
            {
                if (value != _firstname)
                {
                    _firstname = value;
                    NotifyOfPropertyChange(() => FirstName);
                }
            }
        }

        private string _lastname;
        public string LastName
        {
            get
            {
                return _lastname;
            }
            set
            {
                if (value != _lastname)
                {
                    _lastname = value;
                    NotifyOfPropertyChange(() => LastName);
                }
            }
        }

        private Point _alarmlocation;
        public Point AlarmLocation
        {
            get
            {
                return _alarmlocation;
            }
            set
            {
                if (value != _alarmlocation)
                {
                    _alarmlocation = value;
                    NotifyOfPropertyChange(() => AlarmLocation);
                }
            }
        }

        private DateTime _dateofalarm;
        public DateTime DateOfAlarm
        {
            get
            {
                return _dateofalarm;
            }
            set
            {
                if (value != _dateofalarm)
                {
                    _dateofalarm = value;
                    NotifyOfPropertyChange(() => DateOfAlarm);
                }
            }
        }

        public int CompareTo(AlarmItem item)
        {
            if (DateOfAlarm > item.DateOfAlarm) return 1;
            if (DateOfAlarm < item.DateOfAlarm) return -1;
            return 0;
        }
    }
}