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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using WP7App1.EmergencyService;
using Notifications;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;

namespace WP7App1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private MyPushServiceClient client;
        private LocationManager location;
        private string username, password;
        double receivedLatitude, receivedLongitude;
        private StarField _starField;
        private DateTime _lastUpdate = DateTime.Now;
        private CountDown ButtonTimer = new CountDown(60);
        private CountDown reEnable = new CountDown(100);
        public ObservableCollection<ItemViewModel> myRequests = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<AlarmItem> alarmsList = new ObservableCollection<AlarmItem>();
        private bool alarmMode = false;
        private bool? CanUseLocation = null;
        private bool? CanRecieveNotifications = null;
        bool rightUsername = false;
        bool rightFirstName = false;
        bool rightLastName = false;
        bool rightPassword = false;
        bool rightverifyPassword = false;
        bool rightEmail = false;
        bool rightDateOfBirth = false;

        public bool AlarmMode
        {
            get { return alarmMode; }
            set
            {
                alarmMode = value;
                AlarmBlock.Visibility = alarmMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
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
                                      sosButton.Source = new BitmapImage(new Uri("SOS_Button/alarmNormal.png", UriKind.Relative));
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

            myRequests.CollectionChanged += (sender, args) => { Requests.ItemsSource = myRequests; requestBlock.Visibility = myRequests.Count > 0 ? Visibility.Visible : Visibility.Collapsed; };
            //alarmsList.CollectionChanged += (sender, args) => { alarms.ItemsSource = alarmsList; alarmsHistory.Visibility = alarmsList.Count > 0 ? Visibility.Visible : Visibility.Collapsed; };

            client.SendSosNotificationsCompleted += new EventHandler<SendSosNotificationsCompletedEventArgs>(client_SendSosNotificationsCompleted);
            client.SearchFriendsCompleted += new EventHandler<SearchFriendsCompletedEventArgs>(client_SearchFriendsCompleted);
            client.ClientLoginCompleted += new EventHandler<ClientLoginCompletedEventArgs>(client_ClientLoginCompleted);
            client.AddFriendCompleted += new EventHandler<AddFriendCompletedEventArgs>(client_AddFriendCompleted);
            client.CheckFriendshipsStatusCompleted += new EventHandler<CheckFriendshipsStatusCompletedEventArgs>(client_CheckFriendshipsStatusCompleted);
            client.DeleteFriendCompleted += new EventHandler<DeleteFriendCompletedEventArgs>(client_DeleteFriendCompleted);
            client.CompleteFriendshipRequestCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CompleteFriendshipRequestCompleted);
            client.GetFriendsListCompleted += new EventHandler<GetFriendsListCompletedEventArgs>(client_GetFriendsListCompleted);
            client.ClientRegistrationCompleted += new EventHandler<ClientRegistrationCompletedEventArgs>(client_ClientRegistrationCompleted);
            client.ChangeProfileFieldCompleted += new EventHandler<ChangeProfileFieldCompletedEventArgs>(client_ChangeProfileFieldCompleted);
            client.ChangePasswordCompleted += new EventHandler<ChangePasswordCompletedEventArgs>(client_ChangePasswordCompleted);
            client.GetNotificationHistoryCompleted += new EventHandler<GetNotificationHistoryCompletedEventArgs>(client_GetNotificationHistoryCompleted);
            NotificationManager.ToastNotificationReceived += new NotificationManager.ToastEventsHandler(NotificationManager_ToastNotificationReceived);

            LoadSettings();
        }

        /// <summary>
        /// Alarms History item mouse left button up
        /// </summary>
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TurnONAlarm((Point)(sender as StackPanel).Tag);
        }

        /// <summary>
        /// Alarms History item mouse left button down
        /// </summary>
        private void StackPanel_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            (sender as StackPanel).Background = Resources["AeroWhite5"] as Brush;
        }

        /// <summary>
        /// Save Notification button state
        /// </summary>
        private void tgNotific_Checked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["toggleNotific"] = tgNotific.IsChecked == true ? "ON" : "OFF";
            CanRecieveNotifications = tgNotific.IsChecked;
        }

        /// <summary>
        /// Save Location button state
        /// </summary>
        private void tgLocation_Checked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["toggleLocation"] = tgLocation.IsChecked == true ? "ON" : "OFF";
            CanUseLocation = tgLocation.IsChecked;
        }

        /// <summary>
        /// Turns off alarm mode: hides map
        /// </summary>
        void TurnOffAlarm()
        {
            AlarmMode = false;
        }

        /// <summary>
        /// Turns on alarm mode: showes map; pin to location
        /// </summary>
        void TurnONAlarm(Point alarmPos)
        {
            //40.123661, 44.477149 my home
            AlarmMode = true;
            googlemap.Center = new GeoCoordinate(alarmPos.X, alarmPos.Y);
            googlemap.ZoomLevel = 16;
            alarmPin.Location = new GeoCoordinate(alarmPos.X, alarmPos.Y);
            alarmPin.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// PinPush OnTap function
        /// </summary>
        private void alarmPin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Some Information");
        }
        
        /// <summary>
        /// Loading User Settings
        /// </summary>
        void LoadSettings()
        {
            Requests.ItemsSource = myRequests;
            alarms.ItemsSource = alarmsList;
            requestBlock.Visibility = myRequests.Count > 0 ?Visibility.Visible : Visibility.Collapsed;
            //alarmsHistory.Visibility = alarmsList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            googlemap.Visibility = Visibility.Visible;
            radioStreet.IsChecked = true;

            #region Load SOS Button release timer settings

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("btnReleaseTimer")) IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"] = "1";
            timePick.SelectedIndex = Convert.ToInt32(IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"]);
            initButtonTimer(timePick.SelectedIndex);

            #endregion

            #region Load Location button state
            if (IsolatedStorageSettings.ApplicationSettings.Contains("toggleLocation"))
            {
                tgLocation.IsChecked = IsolatedStorageSettings.ApplicationSettings["toggleLocation"].ToString() == "ON"
                                           ? true
                                           : false;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings["toggleLocation"] = "OFF";
                tgLocation.IsChecked = false;
            }
            CanUseLocation = tgLocation.IsChecked;
            #endregion

            #region Load Notification button state
            if (IsolatedStorageSettings.ApplicationSettings.Contains("toggleNotific"))
            {
                tgNotific.IsChecked = IsolatedStorageSettings.ApplicationSettings["toggleNotific"].ToString() == "ON"
                                           ? true
                                           : false;
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings["toggleNotific"] = "OFF";
                tgNotific.IsChecked = false;
            }
            CanRecieveNotifications = tgNotific.IsChecked;
            #endregion

            //TurnONAlarm(new Point(40.123661, 44.477149));
        }

        /// <summary>
        /// Save SOS Button timer
        /// </summary>
        private void ListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timePick != null)
            {
                IsolatedStorageSettings.ApplicationSettings["btnReleaseTimer"] = timePick.SelectedIndex;
                initButtonTimer(timePick.SelectedIndex);
            }
        }

        /// <summary>
        /// Showes change password dialog
        /// </summary>
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            oldPassText.Password = string.Empty;
            newPassText.Password = string.Empty;
            retyprPassText.Password = string.Empty;
            changePass.Visibility = Visibility.Visible;
            ChangePassExpandAnimation.Begin();
        }

        /// <summary>
        /// Cancelchanging password button function
        /// </summary>
        private void CancelPass_Btn_Click(object sender, RoutedEventArgs e)
        {
            ChangePassDispandAnimation_Hide.Begin();
        }

        /// <summary>
        /// Hides change password dialog
        /// </summary>
        private void ChangePassword_Btn_Click(object sender, RoutedEventArgs e)
        {
            ChangePassDispandAnimation_Black.Begin();
        }

        /// <summary>
        /// Hides change password dialog black background on AnimationCompleted
        /// </summary>
        private void ChangePassDispandAnimation_Hide_Completed(object sender, EventArgs e)
        {
            changePass.Visibility = Visibility.Collapsed;             
        }

        /// <summary>
        /// Hides change password dialog but black background
        /// </summary>
        private void ChangePassDispandAnimation_Black_Completed(object sender, EventArgs e)
        {
            //ChangePassExpandAnimation.Begin();
            //stex
            if (oldPassText.Password == string.Empty || newPassText.Password == string.Empty || retyprPassText.Password == string.Empty)
            {
                MessageBox.Show("Please complete all fields.");
                ChangePassExpandAnimation.Begin();
            }
            else
            {
                if (rightPassword && rightverifyPassword)
                {
                    client.ChangePasswordAsync(username, newPassText.Password);
                    changePass.Visibility = Visibility.Collapsed;
                }
            }  
        }

        /// <summary>
        /// Initializes Button Timer
        /// </summary>
        void initButtonTimer(int choice)
        {
            switch (choice)
            {
                case 0:
                    ButtonTimer.CountDownTimer = 10; break;
                case 1:
                    ButtonTimer.CountDownTimer = 60; break;
                case 2:
                    ButtonTimer.CountDownTimer = 100; break;
                default:
                    ButtonTimer.CountDownTimer = 60; break;
            }
        }

        /// <summary>
        /// Send SOS alarm when SOS button timer ended: RELEASE SOS BUTTON
        /// </summary>
        void ButtonTimer_Alarm(object sender, EventArgs e)
        {
            var answer = false;
            if (NotificationManager.InternetIsAvailable())
            {
                sosButton.Source = new BitmapImage(new Uri("SOS_Button/alarmDisabled.png", UriKind.Relative));
                if (CanUseLocation == true)
                {
                    location = new LocationManager();
                    location.StartLocationCapturing();
                    location.username = username;
                }
                else
                {
                    if (MessageBox.Show("Using of your location is disabled, to help your friends easily find you we recomend to send your location, do you wish to send your location?", "Location Disabled", MessageBoxButton.OKCancel)
                        == MessageBoxResult.OK)
                    {
                        location = new LocationManager();
                        location.StartLocationCapturing();
                        location.username = username;
                        answer = true;
                    }
                }
                //client.SendSosNotificationsAsync(new ClientData { Username = username });
                reEnable.Start();

                if (CanUseLocation != true && answer)
                {
                    if (MessageBox.Show("Using of your location is disabled, do you wish to enable using of your location?", "Location Disabled", MessageBoxButton.OKCancel)
                        == MessageBoxResult.OK)
                    {
                        IsolatedStorageSettings.ApplicationSettings["toggleLocation"] = "ON";
                        tgLocation.IsChecked = true;
                    }
                }
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
            e.Cancel = true;
            if (AlarmMode)
            {
                if (MessageBox.Show("Are you sure you want to Turn off alarm ?", "Exit", MessageBoxButton.OKCancel) ==
                    MessageBoxResult.OK)
                {
                    TurnOffAlarm();
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to exit SOS Caller ?", "Exit", MessageBoxButton.OKCancel) ==
                    MessageBoxResult.OK)
                {
                    var m = new Game();
                    m.Exit();
                }
            }
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
                string receivedLatitudeSting = NavigationContext.QueryString["receivedLatitude"];
                string receivedLongitudeSting = NavigationContext.QueryString["receivedLongitude"];
                //MessageBox.Show("Latitude:" + receivedLatitudeSting + "\nLongitude:" + receivedLongitudeSting);
                receivedLatitude = Convert.ToDouble(receivedLatitudeSting);
                receivedLongitude = Convert.ToDouble(receivedLongitudeSting);
                client.GetNotificationHistoryAsync(username);
                TurnONAlarm(new Point(receivedLatitude, receivedLongitude));
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
                ObservableCollection<ItemViewModel> items = (Contacts.ItemsSource as ObservableCollection<ItemViewModel>);
                var itemToDelete = (from c in items
                                    where c.UserName == (sender as MenuItem).Tag.ToString()
                                    select c).FirstOrDefault();
                items.Remove(itemToDelete);
                Contacts.ItemsSource = items;
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
                    client.SearchFriendsAsync(new ClientData { FirstName = searchBox.Text }, username);
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

            sosButton.Source = new BitmapImage(new Uri("SOS_Button/alarmNormal.png", UriKind.Relative));
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
            sosButton.Source = new BitmapImage(new Uri("SOS_Button/alarmPressed.png", UriKind.Relative));
        }

        /// <summary>
        /// Stops SOS button release timer and resets
        /// </summary>
        private void sosButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonTimer.Stop();
            sosButton.Source = new BitmapImage(new Uri("SOS_Button/alarmNormal.png", UriKind.Relative));
        }

        /// <summary>
        /// Change SOS button image to Pressed
        /// </summary>
        private void sosButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            sosButton.Source = new BitmapImage(new Uri("SOS_Button/alarmPressed.png", UriKind.Relative));
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
                MessageBox.Show("Please complete all fields.");
            }
            else
            {    
                if (rightDateOfBirth && rightUsername && rightPassword && rightverifyPassword && rightFirstName && rightLastName && rightEmail)
                {
                    client.ClientRegistrationAsync(new ClientData { Username = regUName.Text, FirstName = regFName.Text, LastName = regLName.Text, Email = regEmail.Text, Age = int.Parse(regDOB.Text) }, regPass.Password);
                    ShowHideRegisterBlock(false);
                    regBlock.Visibility = System.Windows.Visibility.Collapsed;
                    ShowHideLoginBlock(true);
                    regUName.Text = "";
                    regFName.Text = "";
                    regLName.Text = "";
                    regPass.Password = "";
                    regPassVerify.Password = "";
                    regEmail.Text = "";
                    regDOB.Text = "";
                }
                else
                {
                    int count = 0;
                    string s = "Please complete ";
                    if (!rightUsername)
                    {
                        s += "username, ";
                        count++;
                    }
                    if (!rightPassword)
                    {
                        s += "password, ";
                        count++;
                    }
                    if (!rightverifyPassword)
                    {
                        s += "verifyPassword, ";
                        count++;
                    }
                    if (!rightFirstName)
                    {
                        s += "firstname, ";
                        count++;
                    }
                    if (!rightLastName)
                    {
                        s += "lastname, ";
                        count++;
                    }
                    if (!rightEmail)
                    {
                        s += "Email, ";
                        count++;
                    }
                    if (!rightDateOfBirth)
                    {
                        s += "dateOfBirth, ";
                        count++;
                    }
                    s = s.Remove(s.Length - 2);

                    if (count == 1)
                        s += " field correctly.";
                    else
                        s += " fields correctly.";
                    MessageBox.Show(s);
                }
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
            FirstNameValidation(profFName);
            if (rightFirstName)
                client.ChangeProfileFieldAsync(new ClientData { Username = username, FirstName = profFName.Text });
            else
                profFName.Text = _proffname;
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
            LastNameValidation(profLName);
            if (rightLastName)
                client.ChangeProfileFieldAsync(new ClientData { Username = username, LastName = profLName.Text });
            else
                profLName.Text = _proflname;
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
        //string _profuname = string.Empty;

        ///// <summary>
        ///// Make profile Username editable
        ///// </summary>
        //private void editProfUName_Click(object sender, RoutedEventArgs e)
        //{
        //    profUName.Width = 260;
        //    editProfUName.Visibility = System.Windows.Visibility.Collapsed;
        //    acceptUName.Visibility = System.Windows.Visibility.Visible;
        //    cancelUName.Visibility = System.Windows.Visibility.Visible;
        //    _profuname = profUName.Text;
        //    profUName.IsReadOnly = false;
        //    profUName.Focus();
        //}

        ///// <summary>
        ///// Accept changes to profile Username
        ///// </summary>
        //private void acceptUName_Click(object sender, RoutedEventArgs e)
        //{
        //    profUName.Width = 330;
        //    editProfUName.Visibility = System.Windows.Visibility.Visible;
        //    acceptUName.Visibility = System.Windows.Visibility.Collapsed;
        //    cancelUName.Visibility = System.Windows.Visibility.Collapsed;
        //    profUName.IsReadOnly = true;
        //}

        ///// <summary>
        ///// Cancel changes to profile Username
        ///// </summary>
        //private void cancelUName_Click(object sender, RoutedEventArgs e)
        //{
        //    profUName.Width = 330;
        //    editProfUName.Visibility = System.Windows.Visibility.Visible;
        //    acceptUName.Visibility = System.Windows.Visibility.Collapsed;
        //    cancelUName.Visibility = System.Windows.Visibility.Collapsed;
        //    profUName.IsReadOnly = true;
        //    profUName.Text = _profuname;
        //}
        #endregion

        #region Profile EMail
        string _profemail = string.Empty;
        string validatingEmailFieldtText = string.Empty;

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
            validatingEmailFieldtText = _profemail;
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
            EmailValidation(profEMail);
            if (rightEmail)
                client.ChangeProfileFieldAsync(new ClientData { Username = username, Email = profEMail.Text });
            else
                profEMail.Text = _profemail;
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
            DOBValidation(profDOB);
            if (rightDateOfBirth)
                client.ChangeProfileFieldAsync(new ClientData { Username = username, Age = int.Parse(profDOB.Text) });
            else
                profDOB.Text = _profdob;
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
            var itemToDelete = (from c in myRequests
                                where c.UserName == (sender as MenuItem).Tag.ToString()
                                select c).FirstOrDefault();
            myRequests.Remove(itemToDelete);
        }

        /// <summary>
        /// Accept friend request
        /// </summary>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            client.CompleteFriendshipRequestAsync(username, (sender as MenuItem).Tag.ToString(), true);
            var itemToDelete = (from c in myRequests
                                where c.UserName == (sender as MenuItem).Tag.ToString()
                                select c).FirstOrDefault();
            myRequests.Remove(itemToDelete);
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
                    rightUsername = true;
                    errTextBlock.Text = "";
                }
                else
                {
                    rightUsername = false;
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
            return Regex.IsMatch(password, @"^(?=^.{6,15}$)(?=.*\d)(?=.*[a-z])(?!.*\s).*$");
        }

        private void regFirstname_LostFocus(object sender, RoutedEventArgs e)
        {
            FirstNameValidation(sender as TextBox);
        }

        private void FirstNameValidation(TextBox regFName)
        {
            TextBlock errTextBlock;
            if(regFName.Name.Contains("reg"))
                errTextBlock = this.errTextBlock;
            else
                errTextBlock = this.profileValidationTextBlock;

            if (regFName.Text.Length > 0)
            {
                if (Regex.IsMatch(regFName.Text, @"^[a-zA-Z]+(([\'\- ][a-zA-Z ])?[a-zA-Z]*)*$"))
                {
                    rightFirstName = true;
                    errTextBlock.Text = "";
                }
                else
                {
                    errTextBlock.Text = "Firstname can't contain digits or some special simbols.";
                    rightFirstName = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regLastname_LostFocus(object sender, RoutedEventArgs e)
        {
            LastNameValidation(sender as TextBox);            
        }

        private void LastNameValidation(TextBox regLName)
        {
            TextBlock errTextBlock;
            if (regLName.Name.Contains("reg"))
                errTextBlock = this.errTextBlock;
            else
                errTextBlock = this.profileValidationTextBlock;

            if (regLName.Text.Length > 0)
            {
                if (Regex.IsMatch(regLName.Text, @"^[a-zA-Z]+(([\'\- ][a-zA-Z ])?[a-zA-Z]*)*$"))
                {
                    errTextBlock.Text = "";
                    rightLastName = true;
                }
                else
                {
                    errTextBlock.Text = "Lastname can't contain digits or some special simbols.";
                    rightLastName = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regVerifyPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            VerifyPasswordValidation(sender as PasswordBox);
        }

        private void VerifyPasswordValidation(PasswordBox regPassVerify)
        {
            TextBlock errTextBlock;
            if (regPassVerify.Name.Contains("reg"))
            {
                errTextBlock = this.errTextBlock;
            }
            else
            {
                errTextBlock = this.passchangeError;
                regPass = this.newPassText;
            }

            if (regPassVerify.Password.Length > 0)
            {
                if (regPass.Password == regPassVerify.Password)
                {
                    errTextBlock.Text = "";
                    rightverifyPassword = true;
                }
                else
                {
                    errTextBlock.Text = "Verify password doesn't match password.";
                    rightverifyPassword = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordValidation(sender as PasswordBox);
        }

        private void PasswordValidation(PasswordBox regPass)
        {
            TextBlock errTextBlock;
            if (regPass.Name.Contains("reg"))
                errTextBlock = this.errTextBlock;
            else
                errTextBlock = this.passchangeError;

            if (regPass.Password.Length > 0)
            {
                if (IsPasswordStrong(regPass.Password))
                {
                    errTextBlock.Text = "";
                    rightPassword = true;
                }
                else
                {
                    errTextBlock.Text = "Password must contain at least one letter and one digit";
                    rightPassword = false;
                }
            }
        }

        private void regMail_LostFocus(object sender, RoutedEventArgs e)
        {
            EmailValidation(sender as TextBox);
        }

        private void EmailValidation(TextBox regEmail)
        {
            TextBlock errTextBlock;
            if (regEmail.Name.Contains("reg"))
                errTextBlock = this.errTextBlock;
            else
                errTextBlock = this.profileValidationTextBlock;

            if (regEmail.Text.Length > 0)
            {
                if (IsValid(regEmail.Text))
                {
                    errTextBlock.Text = "";
                    rightEmail = true;
                }
                else
                {
                    errTextBlock.Text = "Email format is wrong.";
                    rightEmail = false;
                }
            }
            else
                errTextBlock.Text = "";
        }

        private void regDateofBirth_LostFocus(object sender, RoutedEventArgs e)
        {
            DOBValidation(sender as TextBox);
        }

        private void DOBValidation(TextBox regDOB)
        {
            TextBlock errTextBlock;
            if (regDOB.Name.Contains("reg"))
                errTextBlock = this.errTextBlock;
            else
                errTextBlock = this.profileValidationTextBlock;

            if (regDOB.Text.Length > 0)
            {
                try
                {
                    int age = int.Parse(regDOB.Text);
                    if (age > 100)
                    {
                        errTextBlock.Text = "Age must be more than 100.";
                        rightDateOfBirth = false;
                    }
                    else
                    {
                        errTextBlock.Text = "";
                        rightDateOfBirth = true;
                    }
                }
                catch
                {
                    errTextBlock.Text = "Date of birth format is wrong.";
                    rightDateOfBirth = false;
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
                //profUName.Text = e.Result.Username;
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
                client.GetNotificationHistoryAsync(username);

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
            client.GetFriendsListAsync(username);
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

        /// <summary>
        /// Changing profile fields
        /// </summary>
        void client_ChangeProfileFieldCompleted(object sender, ChangeProfileFieldCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
            if (e.Result == "User with current email is already exist. Please enter another email address.")
            {
                profEMail.Text = validatingEmailFieldtText;
                editProfEMail_Click(this, null);
            }
        }

        /// <summary>
        /// Changeing password
        /// </summary>
        void client_ChangePasswordCompleted(object sender, ChangePasswordCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
            if (e.Result == "Old password is wrong.")
            {
                changePass.Visibility = System.Windows.Visibility.Visible;
                ChangePassExpandAnimation.Begin();
            }
        }

        /// <summary>
        /// Received toast notification
        /// </summary>

        void NotificationManager_ToastNotificationReceived(List<string> message)
        {
            //string a = Regex.Split(message[2], @"receivedLatitude=.*.&").FirstOrDefault();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                int start = message[2].IndexOf(@"?receivedLatitude=") + 18;
                int end = message[2].LastIndexOf(@"&receivedLongitude=");
                string value = message[2].Substring(start, end - start);
                double receivedLatitude = Convert.ToDouble(value);
                start = end + 19;
                value = message[2].Substring(start, (message[2].Length - start));
                double receivedLongitude = Convert.ToDouble(value);
                TurnONAlarm(new Point(receivedLatitude, receivedLongitude));
                MessageBox.Show("Received " + message[0] + " from " + message[1]);
                client.GetNotificationHistoryAsync(username);
            });
        }

        /// <summary>
        /// Getting notifications history data
        /// </summary>
        void client_GetNotificationHistoryCompleted(object sender, GetNotificationHistoryCompletedEventArgs e)
        {
            var resultList = e.Result.ToList();
            foreach (var result in resultList)
            {
                double receivedLatitude = Convert.ToDouble(result.Value.Latitude);
                double receivedLongitude = Convert.ToDouble(result.Value.Longitude);
                alarmsList.Add(new AlarmItem() { FirstName = result.Key.FirstName, LastName = result.Key.LastName, AlarmLocation = new Point(receivedLatitude, receivedLongitude), DateOfAlarm = result.Value.DateTime.Value });
            }
            //alarms.ItemsSource = alarmsList;
            //alarms.Visibility = System.Windows.Visibility.Visible;
            //alarmsHistory.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion
    }
}