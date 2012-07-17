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

namespace WP7App1
{
    public class ItemViewModel : PropertyChangedBase
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

        private string _username;
        public string UserName
        {
            get
            {
                return _username;
            }
            set
            {
                if (value != _username)
                {
                    _username = value;
                    NotifyOfPropertyChange(() => UserName);
                }
            }
        }

        private string _email;
        public string EMail
        {
            get
            {
                return _email;
            }
            set
            {
                if (value != _email)
                {
                    _email = value;
                    NotifyOfPropertyChange(() => EMail);
                }
            }
        }

        private string _dob;
        public string DateOfBirth
        {
            get
            {
                return _dob;
            }
            set
            {
                if (value != _dob)
                {
                    _dob = value;
                    NotifyOfPropertyChange(() => DateOfBirth);
                }
            }
        }
    }
}