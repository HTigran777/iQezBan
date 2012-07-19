using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;
using WP7App1.EmergencyService;

namespace Notifications
{

    public class LocationManager
    {
        private MyPushServiceClient client;
        private GeoCoordinateWatcher watcher;
        private GeoCoordinate cordinates;
        private string statusString = string.Empty;
        public string username;

        public GeoCoordinate Cordinates
        {
            get { return cordinates; }
        }

        public string StatusString
        {
            get { return statusString; }
            set { statusString = value; }
        }

        public LocationManager()
        {
            StartLocationCapturing();
        }

        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // The Location Service is disabled or unsupported.
                    // Check to see if the user has disabled the Location Service.
                    if (watcher.Permission == GeoPositionPermission.Denied)
                    {
                        // The user has disabled the Location Service on their device.
                        statusString = "You have disabled Location Service.";
                    }
                    else
                    {
                        statusString = "Location Service is not functioning on this device.";
                    }
                    break;
                case GeoPositionStatus.Initializing:
                    statusString = "Location Service is retrieving data...";
                    // The Location Service is initializing.
                    break;
                case GeoPositionStatus.NoData:
                    // The Location Service is working, but it cannot get location data.
                    statusString = "Location data is not available.";
                    break;
                case GeoPositionStatus.Ready:
                    // The Location Service is working and is receiving location data.
                    statusString = "Location data is available.";
                    break;
            }
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            cordinates = e.Position.Location;
            client.SendSosNotificationsAsync(new ClientData { Username = username }, cordinates.Latitude, cordinates.Longitude);
            watcher.Stop();

            // Access the position information thusly:
            //epl.Latitude.ToString("0.000");
            //epl.Longitude.ToString("0.000");
            //epl.Altitude.ToString();
            //epl.HorizontalAccuracy.ToString();
            //epl.VerticalAccuracy.ToString();
            //epl.Course.ToString();
            //epl.Speed.ToString();
            e.Position.Timestamp.LocalDateTime.ToString();
        }

        public GeoCoordinate GetCurrentPosition()
        {
            return cordinates;
        }

        public void StartLocationCapturing()
        {
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default)
                {
                    MovementThreshold = 20
                };

                watcher.PositionChanged += this.watcher_PositionChanged;
                watcher.StatusChanged += this.watcher_StatusChanged;
                watcher.Start();
            }
        }

        public void StopLocationCapturing()
        {
            if (watcher != null)
                watcher.Stop();

        }
    }
}
