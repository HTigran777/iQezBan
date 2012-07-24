using System;
using Microsoft.Phone.Notification;
using WP7App1.EmergencyService;
using System.Windows;
using WP7App1;
using System.Net.NetworkInformation;
using System.Text;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

namespace Notifications
{
    public class NotificationManager
    {
        public delegate void ToastEventsHandler(List<string> message);
        public static event ToastEventsHandler ToastNotificationReceived;
        const string channelName = "EmergencyChannel";
        static public StringBuilder message;
        HttpNotificationChannel channel;
        MyPushServiceClient pushClient = new MyPushServiceClient();
        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public static bool InternetIsAvailable()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            return true;
        }

        public void SetupNotificationChannel()
        {
            if (!InternetIsAvailable()) return;
            channel = HttpNotificationChannel.Find(channelName);

            if (channel == null)
            {
                channel = new HttpNotificationChannel(channelName);
                HookupHandlers();
                channel.Open();
            }
            else
            {
                HookupHandlers();
                try
                {
                    pushClient.RegisterPhoneAsync(WP7App1.Bootstrapper.phoneId, channel.ChannelUri.ToString(), username);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        private void HookupHandlers()
        {
            channel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(channel_ChannelUriUpdated);

            channel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(channel_ErrorOccurred);
            pushClient.RegisterPhoneCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(pushCLient_RegisterPhoneCompleted);

            channel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(channel_ShellToastNotificationReceived);
            channel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(channel_HttpNotificationReceived);

        }

        void pushCLient_RegisterPhoneCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw new Exception("Error registering the phone with the web service", e.Error);
            }
        }

        void channel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            throw new Exception(e.Message);
        }

        void channel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            if (!channel.IsShellTileBound)
                channel.BindToShellTile();

            if (!channel.IsShellToastBound)
                channel.BindToShellToast();

            try
            {
                pushClient.RegisterPhoneAsync(WP7App1.Bootstrapper.phoneId, channel.ChannelUri.ToString(), username);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        void channel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        void channel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            List<string> message = new List<string>();
            string relativeUri = string.Empty;

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                if (string.Compare(
                    key,
                    "wp:Text1",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    message.Add(e.Collection[key]);
                }
                else if (string.Compare(
                key,
                "wp:Text2",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    message.Add(e.Collection[key]);
                }
                else if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    message.Add(e.Collection[key]);
                }
            }
            ToastNotificationReceived(message);
        }
    }
}
