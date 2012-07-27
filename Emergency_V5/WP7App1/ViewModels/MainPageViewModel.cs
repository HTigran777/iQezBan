using System.Collections.ObjectModel;
using Caliburn.Micro;
using System;
using System.Windows;

namespace WP7App1
{
    public class MainPageViewModel : Screen
    {
        private string _profilefname = string.Empty;
        private string _profilelname = string.Empty;
        private string _profileusername = string.Empty;
        private string _profileemail = string.Empty;
        private string _profiledateofbirth = string.Empty;
        private string _profilepassword = string.Empty;
        
        public MainPageViewModel()
        {
            this.MyContacts = new ObservableCollection<ItemViewModel>();
            this.MyFoundedContacts = new ObservableCollection<ItemViewModel>();
            this.RequestsList = new ObservableCollection<ItemViewModel>();

            MyContacts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(MyContacts_CollectionChanged);
            MyFoundedContacts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(MyFoundedContacts_CollectionChanged);
            RequestsList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(RequestsList_CollectionChanged);
        }

        void RequestsList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) { NotifyOfPropertyChange(() => RequestsList); }
        void MyFoundedContacts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) { NotifyOfPropertyChange(() => MyFoundedContacts); }
        void MyContacts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) { NotifyOfPropertyChange(() => MyContacts); }

        public string ProfileFirstName { get { return _profilefname; } set { _profilefname = value; NotifyOfPropertyChange(() => ProfileFirstName); } }
        public string ProfileLastName { get { return _profilelname; } set { _profilelname = value; NotifyOfPropertyChange(() => ProfileLastName); } }
        public string ProfileUserName { get { return _profileusername; } set { _profileusername = value; NotifyOfPropertyChange(() => ProfileUserName); } }
        public string ProfileEMail { get { return _profileemail; } set { _profileemail = value; NotifyOfPropertyChange(() => ProfileEMail); } }
        public string ProfileDateOfBirth { get { return _profiledateofbirth; } set { _profiledateofbirth = value; NotifyOfPropertyChange(() => ProfileDateOfBirth); } }
        public string ProfilePassword { get { return _profilepassword; } set { _profilepassword = value; NotifyOfPropertyChange(() => ProfilePassword); } }

        public ObservableCollection<ItemViewModel> MyContacts { get; private set; }
        public ObservableCollection<ItemViewModel> MyFoundedContacts { get; private set; }
        public ObservableCollection<ItemViewModel> RequestsList { get; private set; }
    }
}