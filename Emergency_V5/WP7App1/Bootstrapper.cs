using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Notifications;
using System.IO.IsolatedStorage;

namespace WP7App1
{
    public class Bootstrapper : PhoneBootstrapper
    {
        PhoneContainer container;
        #region GUID fields
        //static NotificationManager manager;
        public static Guid phoneId;
#endregion

        public static string username, password;
        public static bool loggedIn = false;
        
        protected override void Configure()
        {
            container = new PhoneContainer(RootFrame);
            container.RegisterPhoneServices();
            container.PerRequest<MainPageViewModel>();
            container.PerRequest<ItemViewModel>();

            #region GUID Generation and PushNotification Channel Establishing
            if (IsolatedStorageSettings.ApplicationSettings.Contains("DeviceId"))
            {
                phoneId = (Guid)IsolatedStorageSettings.ApplicationSettings["DeviceId"];
            }
            else
            {
                phoneId = Guid.NewGuid();
                IsolatedStorageSettings.ApplicationSettings["DeviceId"] = phoneId;
            }
            //manager = new NotificationManager();
            //manager.SetupNotificationChannel();
#endregion

            if (IsolatedStorageSettings.ApplicationSettings.Contains("Username") && IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
            {
                username = (string)IsolatedStorageSettings.ApplicationSettings["Username"];
                password = (string)IsolatedStorageSettings.ApplicationSettings["Password"];
                loggedIn = true;
            }
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}