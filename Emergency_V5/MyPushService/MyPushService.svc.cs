using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Data;

namespace EmergencyService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class MyPushService : IMyPushService
    {
        public void RegisterPhone(Guid phoneId, string channelUri, string username)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                int clientId;
                var deviceId = phoneId.ToString();
                var deviceI = (from c in context.Devices
                               where c.DeviceID == deviceId
                               select c).FirstOrDefault<Device>();
                using (EmergencyDBEntities context2 = new EmergencyDBEntities())
                {
                    clientId = (from c in context.Clients
                                where c.Username == username
                                select c.ClientID).FirstOrDefault();
                }
                if (context.Devices.Count() == 0 || deviceI == null)
                {
                    context.Devices.AddObject(new Device { DeviceID = phoneId.ToString(), DeviceUrl = channelUri, ClientID = clientId });
                    context.SaveChanges();
                }
                foreach (var device in context.Devices)
                {
                    Debug.WriteLine(device.DeviceID.ToString());
                    if (device.DeviceID == phoneId.ToString())
                        if (device.DeviceUrl != channelUri)
                            device.DeviceUrl = channelUri;
                        else if (device.ClientID != clientId)
                            device.ClientID = clientId;
                }
                context.SaveChanges();
            }
        }

        public List<string> PushToSubscribedPhones(NotificationData data)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                foreach (var entry in context.Devices)
                {
                    if (data.PhoneId.ToString() == entry.DeviceID)
                    {
                        Uri uri = new Uri(entry.DeviceUrl);
                        byte[] payload = new byte[0];

                        var request = (HttpWebRequest)WebRequest.Create(uri);
                        request.Method = "POST";
                        request.ContentType = "text/xml";

                        if (data.PushType == "toast")
                        {
                            payload = GetToastPayload(data.Title, data.PersonName, "");
                            request.Headers.Add("X-WindowsPhone-Target", "toast");
                            request.Headers.Add("X-NotificationClass", "2");
                        }
                        else if (data.PushType == "tile")
                        {
                            payload = GetTilePayload(data);
                            request.Headers.Add("X-WindowsPhone-Target", "token");
                            request.Headers.Add("X-NotificationClass", "1");
                        }
                        else if (data.PushType == "raw")
                        {
                            payload = Encoding.UTF8.GetBytes(data.Title);
                            request.Headers.Add("X-NotificationClass", "3");
                        }

                        request.ContentLength = payload.Length;

                        using (Stream requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(payload, 0, payload.Length);
                        }

                        try
                        {
                            var response = (HttpWebResponse)request.GetResponse();
                            data.NotificationStatus = response.Headers["X-NotificationStatus"];
                            data.NotificationChannelStatus = response.Headers["X-SubscriptionStatus"];
                            data.DeviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];
                        }
                        catch (Exception ex) { return new List<string> { "Device is not connected!", ex.Message }; }
                    }
                }
            }
            return new List<string> { data.NotificationStatus, data.NotificationChannelStatus, data.DeviceConnectionStatus };
        }

        private byte[] GetToastPayload(string title, string personName, string param)
        {
            string payload = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                "<wp:Toast>" +
                "<wp:Text1>{0}</wp:Text1>" +
                "<wp:Text2>{1}</wp:Text2>" +
                "<wp:Param>{2}</wp:Param>" +
                "</wp:Toast>" +
                "</wp:Notification>",
                title, personName, param);//, "/MainPage.xaml");//?id=" + personId.ToString());

            return Encoding.UTF8.GetBytes(payload);
        }

        private byte[] GetTilePayload(NotificationData data)
        {
            string payload = string.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                "<wp:Tile" +
                (!data.UpdateAppTile ? ">" : "Id=\"/MainPage.xaml?id=" + data.PersonName + "\">") +
                "<wp:BackgroundImage>{2}</wp:BackgroundImage>" +
                "<wp:Count>{1}</wp:Count>" +
                "<wp:Title>{0}</wp:Title>" +
                "<wp:BackBackgroundImage>{3}</wp:BackBackgroundImage>" +
                "<wp:BackTitle>{4}</wp:BackTitle>" +
                "<wp:BackContent>{5}</wp:BackContent>" +
                "</wp:Tile> " +
                "</wp:Notification>",
                data.Title, data.Count, data.TileUri, data.BackTileUri, data.BackTitle, data.BackContent);

            return Encoding.UTF8.GetBytes(payload);
        }

        public List<string> SendSosNotifications(ClientData client, double latitude, double longitude)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                string notificationStatus = "";
                string notificationChannelStatus = "";
                string deviceConnectionStatus = "";

                var sender = (from c in context.Clients
                              where c.Username == client.Username
                              select c).FirstOrDefault();
                var clients = (from f in context.Friendships
                               join c in context.Clients
                               on f.FriendID equals c.ClientID
                               where f.ClientID == sender.ClientID && f.Status == true
                               select c);
                foreach (var cl in clients)
                {
                    List<string> uriStrings;
                    using (EmergencyDBEntities context2 = new EmergencyDBEntities())
                    {
                        uriStrings = (from d in context2.Devices
                                      where d.ClientID == cl.ClientID
                                      select d.DeviceUrl).ToList<string>();
                    }
                    if (uriStrings.Count != 0)
                    {
                        foreach (var uriString in uriStrings)
                        {
                            Uri uri = new Uri(uriString);
                            byte[] payload = new byte[0];

                            var request = (HttpWebRequest)WebRequest.Create(uri);
                            request.Method = "POST";
                            request.ContentType = "text/xml";

                            payload = GetToastPayload("Emergency", sender.FirstName + " " + sender.LastName, "?receivedLatitude=" + latitude + "&amp;receivedLongitude=" + longitude);
                            request.Headers.Add("X-WindowsPhone-Target", "toast");
                            request.Headers.Add("X-NotificationClass", "2");

                            request.ContentLength = payload.Length;

                            using (Stream requestStream = request.GetRequestStream())
                            {
                                requestStream.Write(payload, 0, payload.Length);
                            }

                            try
                            {
                                var response = (HttpWebResponse)request.GetResponse();
                                notificationStatus = response.Headers["X-NotificationStatus"];
                                notificationChannelStatus = response.Headers["X-SubscriptionStatus"];
                                deviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];
                                SaveNotificationHistory(new HistoryData {ClientID = sender.ClientID,
                                                                            FriendID = cl.ClientID,
                                                                            Longitude = longitude.ToString(),
                                                                            Latitude = latitude.ToString(),
                                                                            NotificationChannelStatus = notificationChannelStatus,
                                                                            NotificationStatus = notificationStatus,
                                                                            DeviceConnectionStatus = deviceConnectionStatus,
                                                                            DateTime = DateTime.Now });
                            }
                            catch (Exception ex)
                            {
                                SaveNotificationHistory(new HistoryData {ClientID = sender.ClientID,
                                                                            FriendID = cl.ClientID,
                                                                            Longitude = longitude.ToString(),
                                                                            Latitude = latitude.ToString(),
                                                                            DateTime = DateTime.Now,
                                                                            ErrorMessage = ex.Message });
                            }//return new List<string> { "Device is not connected!", ex.Message }; }
                        }
                    }
                }
                return new List<string> { notificationStatus, notificationChannelStatus, deviceConnectionStatus };
                //return new List<string> { "You don't have any friend." };
            }

        }

        public List<ClientData> SearchFriends(ClientData client, string username)
        {
            List<ClientData> friend;
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                friend = (from c in context.Clients
                          where (c.FirstName.StartsWith(client.FirstName) || c.LastName.StartsWith(client.FirstName)) && c.Username != username
                          select new ClientData { Username = c.Username, FirstName = c.FirstName, LastName = c.LastName, Age = c.Age.Value, Email = c.Email }).ToList<ClientData>();
            }
            return friend;
        }

        public ClientData ClientLogin(string username, string password)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                var client = (from c in context.Clients
                              where c.Username == username && c.Password == password
                              select new ClientData { Username = c.Username, FirstName = c.FirstName, LastName = c.LastName, Age = c.Age.Value, Email = c.Email }).FirstOrDefault();
                if (client != null)
                    return client;
            }
            return null;
        }

        public string AddFriend(string clientUsername, string friendUsername)
        {
            string notificationStatusToast = "";
            string notificationChannelStatusToast = "";
            string deviceConnectionStatusToast = "";
            string notificationStatusTile = "";
            string notificationChannelStatusTile = "";
            string deviceConnectionStatusTile = "";

            Client friendDB, clientDB;
            Friendship isAlreadyFriends;
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                friendDB = (from c in context.Clients
                            where friendUsername == c.Username
                            select c).FirstOrDefault();
            }
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                clientDB = (from c in context.Clients
                            where clientUsername == c.Username
                            select c).FirstOrDefault();
            }
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                isAlreadyFriends = (from f in context.Friendships
                                    where f.ClientID == clientDB.ClientID && f.FriendID == friendDB.ClientID
                                    select f).FirstOrDefault();
            }
            if (isAlreadyFriends != null)
                return friendDB.FirstName + " " + friendDB.LastName + " is already your friend.";
            else
            {
                using (EmergencyDBEntities context = new EmergencyDBEntities())
                {
                    context.Friendships.AddObject(new Friendship { ClientID = clientDB.ClientID, FriendID = friendDB.ClientID, Status = false });
                    context.SaveChanges();
                }
                List<string> uriStrings;
                using (EmergencyDBEntities context = new EmergencyDBEntities())
                {
                    uriStrings = (from d in context.Devices
                                  where d.ClientID == friendDB.ClientID
                                  select d.DeviceUrl).ToList<string>();
                }
                if (uriStrings.Count != 0)
                {
                    foreach (var uriString in uriStrings)
                    {
                        Uri uri = new Uri(uriString);
                        byte[] payload = new byte[0];

                        var requestToast = (HttpWebRequest)WebRequest.Create(uri);
                        requestToast.Method = "POST";
                        requestToast.ContentType = "text/xml";

                        payload = GetToastPayload("Friend request:", clientDB.FirstName + " " + clientDB.LastName, "");
                        requestToast.Headers.Add("X-WindowsPhone-Target", "toast");
                        requestToast.Headers.Add("X-NotificationClass", "2");

                        requestToast.ContentLength = payload.Length;

                        using (Stream requestStream = requestToast.GetRequestStream())
                        {
                            requestStream.Write(payload, 0, payload.Length);
                        }

                        var requestTile = (HttpWebRequest)WebRequest.Create(uri);
                        requestTile.Method = "POST";
                        requestTile.ContentType = "text/xml";

                        payload = GetTilePayload(new NotificationData { Count = 1 });
                        requestTile.Headers.Add("X-WindowsPhone-Target", "token");
                        requestTile.Headers.Add("X-NotificationClass", "1");

                        requestTile.ContentLength = payload.Length;

                        using (Stream requestStream = requestTile.GetRequestStream())
                        {
                            requestStream.Write(payload, 0, payload.Length);
                        }

                        try
                        {
                            var responseToast = (HttpWebResponse)requestToast.GetResponse();

                            notificationStatusToast = responseToast.Headers["X-NotificationStatus"];
                            notificationChannelStatusToast = responseToast.Headers["X-SubscriptionStatus"];
                            deviceConnectionStatusToast = responseToast.Headers["X-DeviceConnectionStatus"];

                            var responseTile = (HttpWebResponse)requestToast.GetResponse();

                            notificationStatusTile = responseTile.Headers["X-NotificationStatus"];
                            notificationChannelStatusTile = responseTile.Headers["X-SubscriptionStatus"];
                            deviceConnectionStatusTile = responseTile.Headers["X-DeviceConnectionStatus"];
                            return "Add request have been sent to " + friendDB.FirstName + " " + friendDB.LastName + ".";
                        }
                        catch (Exception ex) { return "Failed to send add request. Exception: " + ex.Message; }
                    }
                    return "";
                }
                else
                    return "Friend doesn't have any device.";
            }
        }

        public List<ClientData> CheckFriendshipsStatus(string clientUsername)
        {
            Client clientDB, friendDB;
            List<Friendship> isAlreadyFriends;
            List<ClientData> friendsDB = new List<ClientData>();
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                clientDB = (from c in context.Clients
                            where clientUsername == c.Username
                            select c).FirstOrDefault();
            }
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                isAlreadyFriends = (from f in context.Friendships
                                    where f.FriendID == clientDB.ClientID && f.Status == false
                                    select f).ToList<Friendship>();
            }

            foreach (var isAlreadyFriend in isAlreadyFriends)
            {
                using (EmergencyDBEntities context = new EmergencyDBEntities())
                {
                    friendDB = (from c in context.Clients
                                where c.ClientID == isAlreadyFriend.ClientID
                                select c).FirstOrDefault();
                }
                friendsDB.Add(new ClientData { Username = friendDB.Username, FirstName = friendDB.FirstName, LastName = friendDB.LastName, Email = friendDB.Email, Age = friendDB.Age.Value });
            }
            return friendsDB;
        }

        public void CompleteFriendshipRequest(string clientUsername, string friendUsername, bool status)
        {
            string notificationStatus = "";
            string notificationChannelStatus = "";
            string deviceConnectionStatus = "";

            Client clientDB, friendDB;
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                clientDB = (from c in context.Clients
                            where clientUsername == c.Username
                            select c).FirstOrDefault();
            }
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                friendDB = (from c in context.Clients
                            where friendUsername == c.Username
                            select c).FirstOrDefault();
            }
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                byte[] payload = new byte[0];

                if (status)
                {
                    foreach (var friendToUpdate in context.Friendships)
                    {
                        if (friendToUpdate.ClientID == friendDB.ClientID && friendToUpdate.FriendID == clientDB.ClientID && friendToUpdate.Status == false)
                            friendToUpdate.Status = status;
                    }
                    context.Friendships.AddObject(new Friendship() { ClientID = clientDB.ClientID, FriendID = friendDB.ClientID, Status = true });
                    context.SaveChanges();

                    payload = GetToastPayload("Friend request confirmed by:", clientDB.FirstName + " " + clientDB.LastName, "");

                }
                else
                {
                    context.Friendships.DeleteObject((from f in context.Friendships
                                                      where f.ClientID == friendDB.ClientID && f.FriendID == clientDB.ClientID && f.Status == status
                                                      select f).FirstOrDefault());
                    context.SaveChanges();

                    payload = GetToastPayload("Friend request disconfirmed by:", clientDB.FirstName + " " + clientDB.LastName, "");

                }

                List<string> uriStrings;

                using (EmergencyDBEntities context2 = new EmergencyDBEntities())
                {
                    uriStrings = (from d in context2.Devices
                                  where d.ClientID == friendDB.ClientID
                                  select d.DeviceUrl).ToList<string>();
                }

                foreach (var uriString in uriStrings)
                {
                    Uri uri = new Uri(uriString);


                    var requestToast = (HttpWebRequest)WebRequest.Create(uri);
                    requestToast.Method = "POST";
                    requestToast.ContentType = "text/xml";

                    requestToast.Headers.Add("X-WindowsPhone-Target", "toast");
                    requestToast.Headers.Add("X-NotificationClass", "2");

                    requestToast.ContentLength = payload.Length;

                    using (Stream requestStream = requestToast.GetRequestStream())
                    {
                        requestStream.Write(payload, 0, payload.Length);
                    }

                    try
                    {
                        var response = (HttpWebResponse)requestToast.GetResponse();

                        notificationStatus = response.Headers["X-NotificationStatus"];
                        notificationChannelStatus = response.Headers["X-SubscriptionStatus"];
                        deviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];
                    }
                    catch (Exception) { }
                }
            }
        }
        
        public string DeleteFriend(string clientUsername, string friendUsername)
        {
            string notificationStatusToast = "";
            string notificationChannelStatusToast = "";
            string deviceConnectionStatusToast = "";

            Client friendDB, clientDB;
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                friendDB = (from c in context.Clients
                            where friendUsername == c.Username
                            select c).FirstOrDefault();
            }
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                clientDB = (from c in context.Clients
                            where clientUsername == c.Username
                            select c).FirstOrDefault();
            }

            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                context.Friendships.DeleteObject((from f in context.Friendships
                                                  where f.ClientID == friendDB.ClientID && f.FriendID == clientDB.ClientID
                                                  select f).FirstOrDefault());
                context.Friendships.DeleteObject((from f in context.Friendships
                                                  where f.ClientID == clientDB.ClientID && f.FriendID == friendDB.ClientID
                                                  select f).FirstOrDefault());
                context.SaveChanges();
            }
            List<string> uriStrings;
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                uriStrings = (from d in context.Devices
                              where d.ClientID == friendDB.ClientID
                              select d.DeviceUrl).ToList<string>();
            }
            if (uriStrings.Count != 0)
            {
                foreach (var uriString in uriStrings)
                {
                    Uri uri = new Uri(uriString);
                    byte[] payload = new byte[0];

                    var requestToast = (HttpWebRequest)WebRequest.Create(uri);
                    requestToast.Method = "POST";
                    requestToast.ContentType = "text/xml";

                    payload = GetToastPayload("Friend request:", clientDB.FirstName + " " + clientDB.LastName, "");
                    requestToast.Headers.Add("X-WindowsPhone-Target", "toast");
                    requestToast.Headers.Add("X-NotificationClass", "2");

                    requestToast.ContentLength = payload.Length;

                    using (Stream requestStream = requestToast.GetRequestStream())
                    {
                        requestStream.Write(payload, 0, payload.Length);
                    }

                    try
                    {
                        var responseToast = (HttpWebResponse)requestToast.GetResponse();

                        notificationStatusToast = responseToast.Headers["X-NotificationStatus"];
                        notificationChannelStatusToast = responseToast.Headers["X-SubscriptionStatus"];
                        deviceConnectionStatusToast = responseToast.Headers["X-DeviceConnectionStatus"];
                        //return "Friend have been deleted " + friendDB.FirstName + " " + friendDB.LastName + ".";
                    }
                    catch (Exception ex) { }//return "Failed to send delete notification. Exception: " + ex.Message; }
                }
                return friendDB.FirstName + " " + friendDB.LastName + " have been deleted.";
            }
            else
                return "Friend doesn't have any device.";
        }

        public List<ClientData> GetFriendsList(string username)
        {
            List<ClientData> friends = new List<ClientData>();
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                var sender = (from c in context.Clients
                              where c.Username == username
                              select c).FirstOrDefault();
                var clients = (from f in context.Friendships
                               join c in context.Clients
                               on f.FriendID equals c.ClientID
                               where f.ClientID == sender.ClientID && f.Status == true
                               select c);
                try
                {
                    foreach (var cl in clients)
                    {
                        friends.Add(new ClientData() { Username = cl.Username, FirstName = cl.FirstName, LastName = cl.LastName, Age = cl.Age.Value, Email = cl.Email });
                    }
                }
                catch { return new List<ClientData>(); }
            }
            return friends;
        }

        public string ClientRegistration(ClientData client, string password)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                if ((from c in context.Clients
                     where c.Username == client.Username
                     select c).FirstOrDefault() != null)
                { return "User with current username is already exist. Please choose another username."; }
                else if ((from c in context.Clients
                          where c.Email == client.Email
                          select c).FirstOrDefault() != null)
                { return "User with current email is already exist. Please enter another email address."; }
                else
                {
                    context.Clients.AddObject(new Client { Username = client.Username, Password = password, Email = client.Email, FirstName = client.FirstName, LastName = client.LastName, Age = client.Age });
                    context.SaveChanges();
                    return "Your account is successfully registered.";
                }
            }
        }

        public string ChangeProfileField(ClientData client)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                if ((from c in context.Clients
                     where c.Email == client.Email
                     select c).FirstOrDefault() != null)
                { return "User with current email is already exist. Please enter another email address."; }
                //context.Dispose();
            }

            using (EmergencyDBEntities context2 = new EmergencyDBEntities())
            {
                foreach (var clientToUpdate in context2.Clients)
                {
                    if (clientToUpdate.Username == client.Username)
                    {
                        if (client.FirstName != null)
                            clientToUpdate.FirstName = client.FirstName;
                        if (client.LastName != null)
                            clientToUpdate.LastName = client.LastName;
                        if (client.Email != null)
                            clientToUpdate.Email = client.Email;
                        if (client.Age != 0)
                            clientToUpdate.Age = client.Age;
                        break;
                    }
                }
                context2.SaveChanges();
            }
            return "Your changes have been saved.";
        }


        public string ChangePassword(string username, string password)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                var client = (from c in context.Clients
                              where c.Username == username && c.Password == password
                              select c).FirstOrDefault();

                if (client == null)
                { return "Old password is wrong."; }

                else
                {
                    client.Password = password;
                    context.SaveChanges();
                }

                return "Your changes have been saved.";
            }
        }

        public void SaveNotificationHistory(HistoryData historyData)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                context.Notifications.AddObject(new Notification { ClientID = historyData.ClientID, FriendID = historyData.FriendID, Latitude = historyData.Latitude, Longitude = historyData.Longitude, Datetime = historyData.DateTime, DeviceConnectionStatus = historyData.DeviceConnectionStatus, ErrorMessage = historyData.ErrorMessage, NotificationChannelStatus = historyData.NotificationChannelStatus, NotificationStatus = historyData.NotificationStatus });
                context.SaveChanges();
            }
        }

        public Dictionary<ClientData, HistoryData> GetNotificationHistory(string username)
        {
            using (EmergencyDBEntities context = new EmergencyDBEntities())
            {
                Dictionary<ClientData, HistoryData> dictionaryToReturn = new Dictionary<ClientData, HistoryData>();
                var id = (from c in context.Clients
                          where c.Username == username
                          select c.ClientID).FirstOrDefault();
                var notifications = (from c in context.Notifications
                                     where c.FriendID == id && c.NotificationStatus =="Received" && c.NotificationChannelStatus =="Active" && c.DeviceConnectionStatus == "Connected"
                                     select c);
                
                foreach (var notification in notifications)
                {
                    Client client;
                    using (EmergencyDBEntities context2 = new EmergencyDBEntities())
                    {
                        client = (from c in context2.Clients
                                      where c.ClientID == notification.ClientID
                                      select c).FirstOrDefault();
                    }
                    dictionaryToReturn.Add(new ClientData{ FirstName = client.FirstName, LastName = client.LastName }, new HistoryData { ClientID = notification.ClientID, FriendID = notification.FriendID, Latitude = notification.Latitude, Longitude = notification.Longitude, DateTime = notification.Datetime, DeviceConnectionStatus = notification.DeviceConnectionStatus, ErrorMessage = notification.ErrorMessage, NotificationChannelStatus = notification.NotificationChannelStatus, NotificationStatus = notification.NotificationStatus });
                }
                return dictionaryToReturn;
            }
        }
    }
}
