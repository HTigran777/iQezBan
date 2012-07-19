using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using WP7App1.EmergencyService;
using Notifications;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Device.Location;


namespace WP7App1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private MyPushServiceClient client;
        private LocationManager location;
        private string username, password;
        private StarField _starField;
        private DateTime _lastUpdate = DateTime.Now;
        private CountDown ButtonTimer = new CountDown(60);
        private CountDown reEnable = new CountDown(100);
        private bool isRegistrationOK = false;
        public ObservableCollection<ItemViewModel> myRequests = new ObservableCollection<ItemViewModel>();
        private bool alarmMode = false;

        public bool AlarmMode
        {
            get { return alarmMode; }
            set 
            { 
                alarmMode = value;
                if (alarmMode)
                {
                    myPano.DefaultItem = myPano.Items[5];
                    AlarmBlock.Visibility = System.Windows.Visibility.Visible;
                } 
                else
                {
                    myPano.DefaultItem = myPano.Items[0];
                    AlarmBlock.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        #region MENU
        
        //if user wants to save his Loggin password
        public bool LoggedIn = false;

        /// <summary>
        /// Main Constructor
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            client = new MyPushServiceClient();
            ButtonTimer.Alarm += new EventHandler(ButtonTimer_Alarm);
            reEnable.Alarm += (sender, args) =>
                                  {
                                      reEnable.Stop();
                                      reEnable.ResetTimer();
                                      sosButton.Background = new SolidColorBrush(Colors.Cyan);
                                      sosButton.IsEnabled = true;
                                      ButtonTimer.ResetTimer();
                                  };

            LoggedIn = Bootstrapper.loggedIn;
            username = Bootstrapper.username;
            password = Bootstrapper.password;

            if (LoggedIn)
            {
                LoginBlock.Visibility = System.Windows.Visibility.Collapsed;
                blackBG.Visibility = System.Windows.Visibility.Collapsed;
                if (NotificationManager.InternetIsAvailable())
                {
                    client.ClientLoginAsync(username, password);
                }
                else
                {
                    MessageBox.Show("No internet connection is available.  Try again later.");
                    blackBG.Visibility = System.Windows.Visibility.Visible;
                }
            }

            myRequests.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(myRequests_CollectionChanged);

            client.SendSosNotificationsCompleted += new EventHandler<SendSosNotificationsCompletedEventArgs>(client_SendSosNotificationsCompleted);
            client.SearchFriendsCompleted += new EventHandler<SearchFriendsCompletedEventArgs>(client_SearchFriendsCompleted);
            client.ClientLoginCompleted += new EventHandler<ClientLoginCompletedEventArgs>(client_ClientLoginCompleted);
            client.AddFriendCompleted += new EventHandler<AddFriendCompletedEventArgs>(client_AddFriendCompleted);
            client.CheckFriendshipsStatusCompleted += new EventHandler<CheckFriendshipsStatusCompletedEventArgs>(client_CheckFriendshipsStatusCompleted);
            client.DeleteFriendCompleted += new EventHandler<DeleteFriendCompletedEventArgs>(client_DeleteFriendCompleted);
            client.CompleteFriendshipRequestCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CompleteFriendshipRequestCompleted);
            client.GetFriendsListCompleted += new EventHandler<GetFriendsListCompletedEventArgs>(client_GetFriendsListCompleted);
            client.ClientRegistrationCompleted += new EventHandler<ClientRegistrationCompletedEventArgs>(client_ClientRegistrationCompleted);

            LoadSettings();
        }

        /// <summary>
        /// Requests Vis/InVis
        /// </summary>
        void myRequests_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Requests.ItemsSource = myRequests;
            requestBlock.Visibility = myRequests.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Loading User Settings
        /// </summary>
        void LoadSettings()
        {
            Requests.ItemsSource = myRequests;
            requestBlock.Visibility = myRequests.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            googlemap.Visibility = Visibility.Visible;
            radioHybrid.IsChecked = true;

            if (IsolatedStorageSettings.ApplicationSettings.Contains("btnReleaseTimer"))
            {
                ButtonTimer.CountDownTimer = Convert.ToDouble(IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"]);
                sosTime.Text = IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"].ToString();
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"] = "60";
                sosTime.Text = IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"].ToString();
            }

            ButtonTimer.CountDownTimer = Convert.ToDouble(IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"]);

            AlarmMode = true;
            googlemap.Center = new GeoCoordinate(40.123661, 44.477149);
            googlemap.ZoomLevel = 16;
        }

        /// <summary>
        /// Send SOS alarm when SOS button timer ended: RELEASE SOS BUTTON
        /// </summary>
        void ButtonTimer_Alarm(object sender, EventArgs e)
        {
            if (NotificationManager.InternetIsAvailable())
            {
                sosButton.Background = new SolidColorBrush(Colors.Red);
                sosButton.IsEnabled = false;
                location = new LocationManager();
                location.StartLocationCapturing();
                location.username = username;
                //client.SendSosNotificationsAsync(new ClientData { Username = username });
                reEnable.Start();
            }
            else
            {
                ButtonTimer.ResetTimer();
                MessageBox.Show("No internet connection is available.  Try again later.");
            }
        }

        /// <summary>
        /// Overloading "Back" button to exit the application
        /// </summary>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit SOS Caller ?", "Exit", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Game m = new Game();
                m.Exit();
            }
            else
            { e.Cancel = true; }
        }

        /// <summary>
        /// Functions needed for StarField Animation 
        /// NOTE: Do not change this code !!!
        /// </summary>
        #region StarField
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            panelStarfield.UpdateLayout();
            if (NavigationContext.QueryString.ContainsKey("receivedLatitude"))
            {
                string receivedLatitude = NavigationContext.QueryString["receivedLatitude"];
                string receivedLongitude = NavigationContext.QueryString["receivedLongitude"];
                MessageBox.Show("Latitude:" + receivedLatitude + "\nLongitude:" + receivedLongitude);
            }
        }

        private static bool _initializedAfterScreenSizeChanged = false;

        private void InitIfNeededAfterScreenSizeIsKnown()
        {
            if (_initializedAfterScreenSizeChanged) return;
            _initializedAfterScreenSizeChanged = true;

            _starField = new StarField(panelStarfield);
            _lastUpdate = DateTime.Now;
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double msec = (now - _lastUpdate).TotalMilliseconds;
            if (msec == 0)
            {
                return;
            }

            _starField.UpdateStars(msec);

            _lastUpdate = DateTime.Now;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            panelStarfield.Width = 480;
            panelStarfield.Height = 800;

            InitIfNeededAfterScreenSizeIsKnown();
        }
        #endregion

        /// <summary>
        /// Remove friend "MenuItem" release function
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this user from your friends list ?", "Remove user ?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Here goes code that removes selected user from my friends list
                //The user ID stored in MenuItem.Tag as shown below:
                client.DeleteFriendAsync(username, (sender as MenuItem).Tag.ToString());
                MessageBox.Show("UserID:\n" + (sender as MenuItem).Tag);
            }
        }

        /// <summary>
        /// Lists background
        /// </summary>
        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as StackPanel).Background = null;
        }

        /// <summary>
        /// Lists background
        /// </summary>
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as StackPanel).Background = Resources["AeroWhite5"] as Brush;
        }

        /// <summary>
        /// Search Button release function
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Here goes Search Button code
            //the search text box name is: searchBox
            if (searchBox.Text.Length > 0)
            {
                if (NotificationManager.InternetIsAvailable())
                    client.SearchFriendsAsync(new ClientData { FirstName = searchBox.Text });
                else
                    MessageBox.Show("No internet connection is available.  Try again later.");
            }
            else
            {
                MessageBox.Show("Pleas enter a friend name to search.");
            }
        }

        /// <summary>
        /// Add to friends list "MenuItem" function
        /// </summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Here goes code that adds selected user to your friends list
            //The user ID stored in MenuItem.Tag as shown below:
            client.AddFriendAsync(username, (sender as MenuItem).Tag.ToString());
            MessageBox.Show("UserID:\n" + (sender as MenuItem).Tag);
        }

        /// <summary>
        /// "LogOut" button function
        /// </summary>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            myPano.DefaultItem = myPano.Items[0];
            ShowHideLoginBlock(true);
            blackBG.Visibility = System.Windows.Visibility.Visible;
            logBox.Text = string.Empty;
            passBox.Password = string.Empty;
            //Here goes code thats logs out current profile

            if (IsolatedStorageSettings.ApplicationSettings.Contains("Username") && IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove("Username");
                IsolatedStorageSettings.ApplicationSettings.Remove("Password");
            }
        }

        /// <summary>
        /// Try to Login: "Login Button" function
        /// </summary>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (NotificationManager.InternetIsAvailable())
            {
                client.ClientLoginAsync(logBox.Text, passBox.Password);
                DispandAnimation_2.Begin();
            }
            else
            {
                MessageBox.Show("No internet connection is available. Try again later.");
                blackBG.Visibility = System.Windows.Visibility.Visible;
                ShowHideLoginBlock(true);
            }

            sosButton.IsEnabled = true;
            sosButton.Background = new SolidColorBrush(Colors.Cyan);
            ButtonTimer.ResetTimer();
        }

        /// <summary>
        /// Fide LoginBlock GRID when "Login block" 's dispand animation finiches 
        /// </summary>
        private void DispandAnimation_Completed(object sender, EventArgs e)
        {
            //LoginBlock.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Starting SOS button release timer
        /// </summary>
        private void sosButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ButtonTimer.Start();
        }

        /// <summary>
        /// Stops SOS button release timer and resets
        /// </summary>
        private void sosButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonTimer.Stop();
        }

        /// <summary>
        /// Change photo Button function
        /// </summary>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Here goes code that changes user photo
            MessageBox.Show("Here must be \"choose photo\" dialog.");
        }

        /// <summary>
        /// TextBox in Settings menu; TextChanged event function
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sosTime.Text != IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"].ToString())
                btnTimer.Visibility = System.Windows.Visibility.Visible;
            else
            { btnTimer.Visibility = System.Windows.Visibility.Collapsed; }
        }

        /// <summary>
        /// SOS button settings save button function
        /// </summary>
        private void btnTimer_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"] = sosTime.Text;
            ButtonTimer.CountDownTimer = Convert.ToDouble(sosTime.Text);
            btnTimer.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Hides Login block
        /// </summary>
        void ShowHideLoginBlock(bool state)
        {
            if (state)
            {
                LoginBlock.Visibility = System.Windows.Visibility.Visible;
                ExpandAnimation.Begin();
            }
            else
            {
                DispandAnimation.Begin();
            }
        }

        /// <summary>
        /// Hides Registration block
        /// </summary>
        void ShowHideRegisterBlock(bool state)
        {
            if (state)
            {
                regBlock.Visibility = System.Windows.Visibility.Visible;
                regExpandAnimation.Begin();
            }
            else
            {
                regDispandAnimation.Begin();
            }
        }
        
        /// <summary>
        /// Login Block "Exit Animation" completed event: Hides Login block
        /// </summary>
        private void DispandAnimation_Completed_1(object sender, EventArgs e)
        {
            LoginBlock.Visibility = System.Windows.Visibility.Collapsed;
        }
        
        /// <summary>
        /// Cancel Registration Button
        /// </summary>
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ShowHideRegisterBlock(false);
            regBlock.Visibility = System.Windows.Visibility.Collapsed;
            ShowHideLoginBlock(true);
        }

        /// <summary>
        /// Register Button
        /// </summary>
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (regUName.Text == string.Empty ||
                regFName.Text == string.Empty ||
                regLName.Text == string.Empty ||
                regEmail.Text == string.Empty ||
                regDOB.Text == string.Empty ||
                regPass.Password == string.Empty ||
                regPassVerify.Password == string.Empty)
            {
                isRegistrationOK = false;
                MessageBox.Show("Please complete all fields.");
            }
            if (isRegistrationOK)
            {
                client.ClientRegistrationAsync(new ClientData { Username = regUName.Text, FirstName = regFName.Text, LastName = regLName.Text, Email = regEmail.Text, Age = int.Parse(regDOB.Text) }, regPass.Password);
                ShowHideRegisterBlock(false);
                regBlock.Visibility = System.Windows.Visibility.Collapsed;
                ShowHideLoginBlock(true);
            }
        }

        /// <summary>
        /// Open registration page Button
        /// </summary>
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            ShowHideLoginBlock(false);
            ShowHideRegisterBlock(true);
        }

        /// <summary>
        /// Releases search button when Enter key pressed
        /// </summary>
        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Button_Click(this, null);
        }

        /// <summary>
        /// Changes focus from Login Textbox to Password box when "Enter" key presed
        /// </summary>
        private void logBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) passBox.Focus();
        }

        /// <summary>
        /// Releases Login function when "Enter" key pressed
        /// </summary>
        private void passBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Button_Click_2(this, null);
        }

        /// <summary>
        /// Profile page functions
        /// </summary>
        #region Profile Data

        #region Profile First Name
        string _proffname = string.Empty;

        /// <summary>
        /// Make profile first name editable
        /// </summary>
        private void editProfFName_Click(object sender, RoutedEventArgs e)
        {
            profFName.Width = 260;
            editProfFName.Visibility = System.Windows.Visibility.Collapsed;
            acceptFName.Visibility = System.Windows.Visibility.Visible;
            cancelFName.Visibility = System.Windows.Visibility.Visible;
            _proffname = profFName.Text;
            profFName.IsReadOnly = false;
            profFName.Focus();
        }

        /// <summary>
        /// Accept changes to profile first name
        /// </summary>
        private void acceptFName_Click(object sender, RoutedEventArgs e)
        {
            profFName.Width = 330;
            editProfFName.Visibility = System.Windows.Visibility.Visible;
            acceptFName.Visibility = System.Windows.Visibility.Collapsed;
            cancelFName.Visibility = System.Windows.Visibility.Collapsed;
            profFName.IsReadOnly = true;
        }

        /// <summary>
        /// Cancel changes to profile first name
        /// </summary>
        private void cancelFName_Click(object sender, RoutedEventArgs e)
        {
            profFName.Width = 330;
            editProfFName.Visibility = System.Windows.Visibility.Visible;
            acceptFName.Visibility = System.Windows.Visibility.Collapsed;
            cancelFName.Visibility = System.Windows.Visibility.Collapsed;
            profFName.IsReadOnly = true;
            profFName.Text = _proffname;
        }
        #endregion

        #region Profile Last Name
        string _proflname = string.Empty;

        /// <summary>
        /// Make profile last name editable
        /// </summary>
        private void editProfLName_Click(object sender, RoutedEventArgs e)
        {
            profLName.Width = 260;
            editProfLName.Visibility = System.Windows.Visibility.Collapsed;
            acceptLName.Visibility = System.Windows.Visibility.Visible;
            cancelLName.Visibility = System.Windows.Visibility.Visible;
            _proflname = profLName.Text;
            profLName.IsReadOnly = false;
            profLName.Focus();
        }

        /// <summary>
        /// Accept changes to profile last name
        /// </summary>
        private void acceptLName_Click(object sender, RoutedEventArgs e)
        {
            profLName.Width = 330;
            editProfLName.Visibility = System.Windows.Visibility.Visible;
            acceptLName.Visibility = System.Windows.Visibility.Collapsed;
            cancelLName.Visibility = System.Windows.Visibility.Collapsed;
            profLName.IsReadOnly = true;
        }

        /// <summary>
        /// Cancel changes to profile last name
        /// </summary>
        private void cancelLName_Click(object sender, RoutedEventArgs e)
        {
            profLName.Width = 330;
            editProfLName.Visibility = System.Windows.Visibility.Visible;
            acceptLName.Visibility = System.Windows.Visibility.Collapsed;
            cancelLName.Visibility = System.Windows.Visibility.Collapsed;
            profLName.IsReadOnly = true;
            profLName.Text = _proflname;
        }
        #endregion

        #region Profile Username
        string _profuname = string.Empty;

        /// <summary>
        /// Make profile Username editable
        /// </summary>
        private void editProfUName_Click(object sender, RoutedEventArgs e)
        {
            profUName.Width = 260;
            editProfUName.Visibility = System.Windows.Visibility.Collapsed;
            acceptUName.Visibility = System.Windows.Visibility.Visible;
            cancelUName.Visibility = System.Windows.Visibility.Visible;
            _profuname = profUName.Text;
            profUName.IsReadOnly = false;
            profUName.Focus();
        }

        /// <summary>
        /// Accept changes to profile Username
        /// </summary>
        private void acceptUName_Click(object sender, RoutedEventArgs e)
        {
            profUName.Width = 330;
            editProfUName.Visibility = System.Windows.Visibility.Visible;
            acceptUName.Visibility = System.Windows.Visibility.Collapsed;
            cancelUName.Visibility = System.Windows.Visibility.Collapsed;
            profUName.IsReadOnly = true;
        }

        /// <summary>
        /// Cancel changes to profile Username
        /// </summary>
        private void cancelUName_Click(object sender, RoutedEventArgs e)
        {
            profUName.Width = 330;
            editProfUName.Visibility = System.Windows.Visibility.Visible;
            acceptUName.Visibility = System.Windows.Visibility.Collapsed;
            cancelUName.Visibility = System.Windows.Visibility.Collapsed;
            profUName.IsReadOnly = true;
            profUName.Text = _profuname;
        }
        #endregion

        #region Profile EMail
        string _profemail = string.Empty;

        /// <summary>
        /// Make profile EMail editable
        /// </summary>
        private void editProfEMail_Click(object sender, RoutedEventArgs e)
        {
            profEMail.Width = 260;
            editProfEMail.Visibility = System.Windows.Visibility.Collapsed;
            acceptEMail.Visibility = System.Windows.Visibility.Visible;
            cancelEMail.Visibility = System.Windows.Visibility.Visible;
            _profemail = profEMail.Text;
            profEMail.IsReadOnly = false;
            profEMail.Focus();
        }

        /// <summary>
        /// Accept changes to profile EMail
        /// </summary>
        private void acceptEMail_Click(object sender, RoutedEventArgs e)
        {
            profEMail.Width = 330;
            editProfEMail.Visibility = System.Windows.Visibility.Visible;
            acceptEMail.Visibility = System.Windows.Visibility.Collapsed;
            cancelEMail.Visibility = System.Windows.Visibility.Collapsed;
            profEMail.IsReadOnly = true;
        }

        /// <summary>
        /// Cancel changes to profile EMail
        /// </summary>
        private void cancelEMail_Click(object sender, RoutedEventArgs e)
        {
            profEMail.Width = 330;
            editProfEMail.Visibility = System.Windows.Visibility.Visible;
            acceptEMail.Visibility = System.Windows.Visibility.Collapsed;
            cancelEMail.Visibility = System.Windows.Visibility.Collapsed;
            profEMail.IsReadOnly = true;
            profEMail.Text = _profemail;
        }
        #endregion

        #region Profile Date Of Birth (DOB)
        string _profdob = string.Empty;

        /// <summary>
        /// Make profile Date of birth editable
        /// </summary>
        private void editProfDOB_Click(object sender, RoutedEventArgs e)
        {
            profDOB.Width = 260;
            editProfDOB.Visibility = System.Windows.Visibility.Collapsed;
            acceptDOB.Visibility = System.Windows.Visibility.Visible;
            cancelDOB.Visibility = System.Windows.Visibility.Visible;
            _profdob = profDOB.Text;
            profDOB.IsReadOnly = false;
            profDOB.Focus();
        }

        /// <summary>
        /// Accept changes to Date of birth
        /// </summary>
        private void acceptDOB_Click(object sender, RoutedEventArgs e)
        {
            profDOB.Width = 330;
            editProfDOB.Visibility = System.Windows.Visibility.Visible;
            acceptDOB.Visibility = System.Windows.Visibility.Collapsed;
            cancelDOB.Visibility = System.Windows.Visibility.Collapsed;
            profDOB.IsReadOnly = true;
        }

        /// <summary>
        /// Cancel changes to Date of birth
        /// </summary>
        private void cancelDOB_Click(object sender, RoutedEventArgs e)
        {
            profDOB.Width = 330;
            editProfDOB.Visibility = System.Windows.Visibility.Visible;
            acceptDOB.Visibility = System.Windows.Visibility.Collapsed;
            cancelDOB.Visibility = System.Windows.Visibility.Collapsed;
            profDOB.IsReadOnly = true;
            profDOB.Text = _profdob;
        }
        #endregion

        //#region Profile Password

        //string _profpass = string.Empty;

        ///// <summary>
        ///// Make profile Password editable
        ///// </summary>
        //private void editProfPass_Click(object sender, RoutedEventArgs e)
        //{
        //    profPass.Width = 260;
        //    editProfPass.Visibility = System.Windows.Visibility.Collapsed;
        //    acceptPass.Visibility = System.Windows.Visibility.Visible;
        //    cancelPass.Visibility = System.Windows.Visibility.Visible;
        //    _profpass = profPass.Text;
        //    profPass.IsReadOnly = false;
        //    profPass.Focus();
        //}

        ///// <summary>
        ///// Accept changes to profile Password
        ///// </summary>
        //private void acceptPass_Click(object sender, RoutedEventArgs e)
        //{
        //    profPass.Width = 330;
        //    editProfPass.Visibility = System.Windows.Visibility.Visible;
        //    acceptPass.Visibility = System.Windows.Visibility.Collapsed;
        //    cancelPass.Visibility = System.Windows.Visibility.Collapsed;
        //    profPass.IsReadOnly = true;
        //}

        ///// <summary>
        ///// Cancel changes to profile Password
        ///// </summary>
        //private void cancelPass_Click(object sender, RoutedEventArgs e)
        //{
        //    profPass.Width = 330;
        //    editProfPass.Visibility = System.Windows.Visibility.Visible;
        //    acceptPass.Visibility = System.Windows.Visibility.Collapsed;
        //    cancelPass.Visibility = System.Windows.Visibility.Collapsed;
        //    profPass.IsReadOnly = true;
        //    profPass.Text = _profpass;
        //}

        //#endregion

        #endregion

        /// <summary>
        /// Showes selected users profile information from Request list [!!!]
        /// </summary>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ShowFriendsInformation((sender as MenuItem).Tag.ToString(), 0);
        }

        /// <summary>
        /// Shows friend information
        /// </summary>
        private void ShowFriendsInformation(string uname, int listnum)
        {
            ObservableCollection<ItemViewModel> col = new ObservableCollection<ItemViewModel>();
            switch (listnum)
            {
                case 0: col = Requests.ItemsSource as ObservableCollection<ItemViewModel>; break;
                case 1: col = Contacts.ItemsSource as ObservableCollection<ItemViewModel>; break;
                case 2: col = FoundedContacts.ItemsSource as ObservableCollection<ItemViewModel>; break;
            }
            var sel = (from p in col
                       where p.UserName == uname
                       select p).FirstOrDefault();
            if (sel != null)
            {
                fr_Username.Text = sel.UserName;
                fr_FirstName.Text = sel.FirstName;
                fr_LastName.Text = sel.LastName;
                fr_EMail.Text = sel.EMail;
                fr_DOB.Text = sel.DateOfBirth;

                friendsProfile.Visibility = System.Windows.Visibility.Visible;
                profAnimation_expand.Begin();
            }
        }

        /// <summary>
        /// Hides friends profile information GRID after animation is ended
        /// </summary>
        private void profAnimation_dispand_Completed(object sender, EventArgs e)
        {
            friendsProfile.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Close friends profile information GRID
        /// </summary>
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            profAnimation_dispand.Begin();
        }

        /// <summary>
        /// Reject friend request
        /// </summary>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            client.CompleteFriendshipRequestAsync(username, (sender as MenuItem).Tag.ToString(), false);
        }

        /// <summary>
        /// Accept friend request
        /// </summary>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            client.CompleteFriendshipRequestAsync(username, (sender as MenuItem).Tag.ToString(), true);
        }

        /// <summary>
        /// Showes selected users profile information from FoundedContacts list [!!!]
        /// </summary>
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            ShowFriendsInformation((sender as MenuItem).Tag.ToString(), 2);
        }

        /// <summary>
        /// Showes selected users profile information from Contacts list [!!!]
        /// </summary>
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            ShowFriendsInformation((sender as MenuItem).Tag.ToString(), 1);
        }

        /// <summary>
        /// Changing Map view style by RadioButtons
        /// </summary>
        private void radio_Checked(object sender, RoutedEventArgs e)
        {
            hybrid.Visibility = Visibility.Collapsed;
            street.Visibility = Visibility.Collapsed;
            satellite.Visibility = Visibility.Collapsed;
            switch ((sender as RadioButton).Name)
            {
                case "radioHybrid": hybrid.Visibility = Visibility.Visible; break;
                case "radioStreet": street.Visibility = Visibility.Visible; break;
                case "radioSatelite": satellite.Visibility = Visibility.Visible; break;
            }
        }

        #endregion

        #region Registration Events

        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regUName.Text.Length > 0)
            {
                if (regUName.Text.Length > 4 && regUName.Text.Length <= 15)
                {
                    isRegistrationOK = true;
                    errTextBlock.Text = "";
                }
                else
                {
                    isRegistrationOK = false;
                    errTextBlock.Text = "Username lenght must be more than 4 and less than 15";
                }
            }
            else
                errTextBlock.Text = "";
        }

        public bool IsValid(string emailaddress)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(emailaddress);
            if (match.Success)
                return true;
            else
                return false;
        }

        public bool IsPasswordStrong(string password)
        {
            return Regex.IsMatch(password, @"^(?=^.{6,15}$)(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?!.*\s).*$");
        }

        private void regFirstname_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regFName.Text.Length > 0)
            {
                if (Regex.IsMatch(regFName.Text, @"^[a-zA-Z]+(([\'\- ][a-zA-Z ])?[a-zA-Z]*)*$"))
                {
                    isRegistrationOK = true;
                    errTextBlock.Text = "";
                }
                else
                {
                    errTextBlock.Text = "Firstname can't contain digits or some special simbols.";
                    isRegistrationOK = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regLastname_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regLName.Text.Length > 0)
            {
                if (Regex.IsMatch(regLName.Text, @"^[a-zA-Z]+(([\'\- ][a-zA-Z ])?[a-zA-Z]*)*$"))
                {
                    errTextBlock.Text = "";
                    isRegistrationOK = true;
                }
                else
                {
                    errTextBlock.Text = "Lastname can't contain digits or some special simbols.";
                    isRegistrationOK = false;
                }
            }
            else
                errTextBlock.Text = "";            
        }

        private void regVerifyPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regPassVerify.Password.Length > 0)
            {
                if (regPass.Password == regPassVerify.Password)
                {
                    errTextBlock.Text = "";
                    isRegistrationOK = true;
                }
                else
                {
                    errTextBlock.Text = "Verify password doesn't match password.";
                    isRegistrationOK = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regPass.Password.Length > 0)
            {
                if (IsPasswordStrong(regPass.Password))
                {
                    errTextBlock.Text = "";
                    isRegistrationOK = true;
                }
                else
                {
                    errTextBlock.Text = "password must contain at least one lowercase, one uppercase letter and one digit";
                    isRegistrationOK = false;
                }
            }
        }

        private void regMail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regEmail.Text.Length > 0)
            {
                if (IsValid(regEmail.Text))
                {
                    errTextBlock.Text = "";
                    isRegistrationOK = true;
                }
                else
                {
                    errTextBlock.Text = "Email format is wrong.";
                    isRegistrationOK = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regDateofBirth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regDOB.Text.Length > 0)
            {
                try
                {
                    int age = int.Parse(regDOB.Text);
                    if (age > 100)
                    {
                        errTextBlock.Text = "Age must be more than 100.";
                        isRegistrationOK = false;
                    }
                    else
                    {
                        errTextBlock.Text = "";
                        isRegistrationOK = true;
                    }
                }
                catch
                {
                    errTextBlock.Text = "Date of birth format is wrong.";
                    isRegistrationOK = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        #endregion

        #region SERVICES

        /// <summary>
        /// Show SOS Notification end results
        /// </summary>
        void client_SendSosNotificationsCompleted(object sender, SendSosNotificationsCompletedEventArgs e)
        {
            string resultString = "";
            foreach (var result in e.Result)
            {
                resultString += result + " | ";
            }
            MessageBox.Show(resultString);
        }

        /// <summary>
        /// Searching friends and generating MyFoundedContacts List data
        /// </summary>
        void client_SearchFriendsCompleted(object sender, SearchFriendsCompletedEventArgs e)
        {
            MainPageViewModel contacts = new MainPageViewModel();
            foreach (var result in e.Result)
            {
                contacts.MyFoundedContacts.Add(new ItemViewModel() { FirstName = result.FirstName, LastName = result.LastName, EMail = result.Email, UserName = result.Username, DateOfBirth = result.Age.ToString() });
            }
            FoundedContacts.ItemsSource = contacts.MyFoundedContacts;
        }

        /// <summary>
        /// Login function
        /// </summary>
        void client_ClientLoginCompleted(object sender, ClientLoginCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                //MessageBox.Show("Successfully Loged in");
                LoginBlock.Visibility = System.Windows.Visibility.Collapsed;
                blackBG.Visibility = System.Windows.Visibility.Collapsed;
                profFName.Text = e.Result.FirstName;
                profLName.Text = e.Result.LastName;
                profEMail.Text = e.Result.Email;
                profUName.Text = e.Result.Username;
                profDOB.Text = e.Result.Age.ToString();


                //Saving user password if checkbox checked

                username = e.Result.Username;
                password = passBox.Password;

                if (savePass.IsChecked == true)
                {
                    IsolatedStorageSettings.ApplicationSettings["Username"] = username;
                    IsolatedStorageSettings.ApplicationSettings["Password"] = password;
                }

                client.CheckFriendshipsStatusAsync(username);
                client.GetFriendsListAsync(username);

                NotificationManager manager = new NotificationManager();
                manager.Username = e.Result.Username;
                manager.SetupNotificationChannel();
            }
            else
            {
                MessageBox.Show("Incorrect Login or Password");
                blackBG.Visibility = System.Windows.Visibility.Visible;
                ShowHideLoginBlock(true);
            }
        }

        /// <summary>
        /// Delete friend
        /// </summary>
        void client_DeleteFriendCompleted(object sender, DeleteFriendCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
        }

        /// <summary>
        /// Check status for new friend requests
        /// </summary>
        void client_CheckFriendshipsStatusCompleted(object sender, CheckFriendshipsStatusCompletedEventArgs e)
        {
            if (e.Result.Count != 0)
            {
                string s = "You have friendship request from:\n";
                foreach (var fr in e.Result)
                    s += fr.FirstName + " " + fr.LastName + '\n';
                MessageBox.Show(s);

                foreach (var result in e.Result)
                {
                    myRequests.Add(new ItemViewModel() { FirstName = result.FirstName, LastName = result.LastName, EMail = result.Email, UserName = result.Username, DateOfBirth = result.Age.ToString() });
                }
            }
        }

        /// <summary>
        /// Add new friend
        /// </summary>
        void client_AddFriendCompleted(object sender, AddFriendCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
        }

        /// <summary>
        /// Complete friend request, Accept or Reject it
        /// </summary>
        void client_CompleteFriendshipRequestCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

        }

        /// <summary>
        /// Geting friends list and generating MyContacts List data
        /// </summary>
        void client_GetFriendsListCompleted(object sender, GetFriendsListCompletedEventArgs e)
        {
            MainPageViewModel contacts = new MainPageViewModel();
            if (e.Result != null)
            {
                foreach (var fr in e.Result)
                {
                    contacts.MyContacts.Add(new ItemViewModel { UserName = fr.Username, FirstName = fr.FirstName, LastName = fr.LastName, EMail = fr.Email, DateOfBirth = fr.Age.ToString() });
                }
                Contacts.ItemsSource = contacts.MyContacts;
            }
        }

        /// <summary>
        /// Register new client account
        /// </summary>
        void client_ClientRegistrationCompleted(object sender, ClientRegistrationCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
            if (e.Result != "Your account is successfully registered.")
            {
                ShowHideLoginBlock(false);
                ShowHideRegisterBlock(true);
            }
        }

        #endregion
    }
}