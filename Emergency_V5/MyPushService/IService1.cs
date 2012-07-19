using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace EmergencyService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IMyPushService
    {
        [OperationContract]
        string ClientRegistration(ClientData client, string password);

        [OperationContract]
        ClientData ClientLogin(string username, string password);

        [OperationContract]
        void RegisterPhone(Guid phoneId, string channelUri, string username);

        [OperationContract]
        List<string> PushToSubscribedPhones(NotificationData data);

        [OperationContract]
        List<ClientData> SearchFriends(ClientData client, string username);

        [OperationContract]
        List<string> SendSosNotifications(ClientData client, double latitude, double longitude);
        
        [OperationContract]
        string AddFriend(string clientUsername, string friendUsername);

        [OperationContract]
        List<ClientData> CheckFriendshipsStatus(string clientUsername);

        [OperationContract]
        void CompleteFriendshipRequest(string clientUsername, string friendUsername, bool status);

        [OperationContract]
        string DeleteFriend(string clientUsername, string friendUsername);

        [OperationContract]
        List<ClientData> GetFriendsList(string username);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class ClientData
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int Age { get; set; }
    }

    [DataContract]
    public class NotificationData
    {
        [DataMember]
        public Guid PhoneId { get; set; }
        [DataMember]
        public string PersonName { get; set; }
        [DataMember]
        public string PushType { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public string TileUri { get; set; }
        [DataMember]
        public string BackTitle { get; set; }
        [DataMember]
        public string BackContent { get; set; }
        [DataMember]
        public string BackTileUri { get; set; }
        [DataMember]
        public bool UpdateAppTile { get; set; }

        //Response Properties
        [DataMember]
        public string NotificationStatus { get; set; }
        [DataMember]
        public string NotificationChannelStatus { get; set; }
        [DataMember]
        public string DeviceConnectionStatus { get; set; }

    }
}
