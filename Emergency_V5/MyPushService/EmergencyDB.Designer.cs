﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]
#region EDM Relationship Metadata

[assembly: EdmRelationshipAttribute("EmergencyDBModel", "FK_Devices_Clients", "Clients", System.Data.Metadata.Edm.RelationshipMultiplicity.One, typeof(EmergencyService.Client), "Devices", System.Data.Metadata.Edm.RelationshipMultiplicity.Many, typeof(EmergencyService.Device), true)]

#endregion

namespace EmergencyService
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class EmergencyDBEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new EmergencyDBEntities object using the connection string found in the 'EmergencyDBEntities' section of the application configuration file.
        /// </summary>
        public EmergencyDBEntities() : base("name=EmergencyDBEntities", "EmergencyDBEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new EmergencyDBEntities object.
        /// </summary>
        public EmergencyDBEntities(string connectionString) : base(connectionString, "EmergencyDBEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new EmergencyDBEntities object.
        /// </summary>
        public EmergencyDBEntities(EntityConnection connection) : base(connection, "EmergencyDBEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Client> Clients
        {
            get
            {
                if ((_Clients == null))
                {
                    _Clients = base.CreateObjectSet<Client>("Clients");
                }
                return _Clients;
            }
        }
        private ObjectSet<Client> _Clients;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Device> Devices
        {
            get
            {
                if ((_Devices == null))
                {
                    _Devices = base.CreateObjectSet<Device>("Devices");
                }
                return _Devices;
            }
        }
        private ObjectSet<Device> _Devices;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Friendship> Friendships
        {
            get
            {
                if ((_Friendships == null))
                {
                    _Friendships = base.CreateObjectSet<Friendship>("Friendships");
                }
                return _Friendships;
            }
        }
        private ObjectSet<Friendship> _Friendships;

        #endregion
        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Clients EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToClients(Client client)
        {
            base.AddObject("Clients", client);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Devices EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToDevices(Device device)
        {
            base.AddObject("Devices", device);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Friendships EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToFriendships(Friendship friendship)
        {
            base.AddObject("Friendships", friendship);
        }

        #endregion
    }
    

    #endregion
    
    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="EmergencyDBModel", Name="Client")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Client : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Client object.
        /// </summary>
        /// <param name="clientID">Initial value of the ClientID property.</param>
        /// <param name="username">Initial value of the Username property.</param>
        /// <param name="password">Initial value of the Password property.</param>
        /// <param name="email">Initial value of the Email property.</param>
        /// <param name="firstName">Initial value of the FirstName property.</param>
        /// <param name="lastName">Initial value of the LastName property.</param>
        public static Client CreateClient(global::System.Int32 clientID, global::System.String username, global::System.String password, global::System.String email, global::System.String firstName, global::System.String lastName)
        {
            Client client = new Client();
            client.ClientID = clientID;
            client.Username = username;
            client.Password = password;
            client.Email = email;
            client.FirstName = firstName;
            client.LastName = lastName;
            return client;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ClientID
        {
            get
            {
                return _ClientID;
            }
            set
            {
                if (_ClientID != value)
                {
                    OnClientIDChanging(value);
                    ReportPropertyChanging("ClientID");
                    _ClientID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ClientID");
                    OnClientIDChanged();
                }
            }
        }
        private global::System.Int32 _ClientID;
        partial void OnClientIDChanging(global::System.Int32 value);
        partial void OnClientIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Username
        {
            get
            {
                return _Username;
            }
            set
            {
                OnUsernameChanging(value);
                ReportPropertyChanging("Username");
                _Username = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Username");
                OnUsernameChanged();
            }
        }
        private global::System.String _Username;
        partial void OnUsernameChanging(global::System.String value);
        partial void OnUsernameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Password
        {
            get
            {
                return _Password;
            }
            set
            {
                OnPasswordChanging(value);
                ReportPropertyChanging("Password");
                _Password = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Password");
                OnPasswordChanged();
            }
        }
        private global::System.String _Password;
        partial void OnPasswordChanging(global::System.String value);
        partial void OnPasswordChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Email
        {
            get
            {
                return _Email;
            }
            set
            {
                OnEmailChanging(value);
                ReportPropertyChanging("Email");
                _Email = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Email");
                OnEmailChanged();
            }
        }
        private global::System.String _Email;
        partial void OnEmailChanging(global::System.String value);
        partial void OnEmailChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String FirstName
        {
            get
            {
                return _FirstName;
            }
            set
            {
                OnFirstNameChanging(value);
                ReportPropertyChanging("FirstName");
                _FirstName = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("FirstName");
                OnFirstNameChanged();
            }
        }
        private global::System.String _FirstName;
        partial void OnFirstNameChanging(global::System.String value);
        partial void OnFirstNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String LastName
        {
            get
            {
                return _LastName;
            }
            set
            {
                OnLastNameChanging(value);
                ReportPropertyChanging("LastName");
                _LastName = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("LastName");
                OnLastNameChanged();
            }
        }
        private global::System.String _LastName;
        partial void OnLastNameChanging(global::System.String value);
        partial void OnLastNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> Age
        {
            get
            {
                return _Age;
            }
            set
            {
                OnAgeChanging(value);
                ReportPropertyChanging("Age");
                _Age = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Age");
                OnAgeChanged();
            }
        }
        private Nullable<global::System.Int32> _Age;
        partial void OnAgeChanging(Nullable<global::System.Int32> value);
        partial void OnAgeChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("EmergencyDBModel", "FK_Devices_Clients", "Devices")]
        public EntityCollection<Device> Devices
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedCollection<Device>("EmergencyDBModel.FK_Devices_Clients", "Devices");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedCollection<Device>("EmergencyDBModel.FK_Devices_Clients", "Devices", value);
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="EmergencyDBModel", Name="Device")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Device : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Device object.
        /// </summary>
        /// <param name="deviceID">Initial value of the DeviceID property.</param>
        /// <param name="deviceUrl">Initial value of the DeviceUrl property.</param>
        /// <param name="clientID">Initial value of the ClientID property.</param>
        public static Device CreateDevice(global::System.String deviceID, global::System.String deviceUrl, global::System.Int32 clientID)
        {
            Device device = new Device();
            device.DeviceID = deviceID;
            device.DeviceUrl = deviceUrl;
            device.ClientID = clientID;
            return device;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String DeviceID
        {
            get
            {
                return _DeviceID;
            }
            set
            {
                if (_DeviceID != value)
                {
                    OnDeviceIDChanging(value);
                    ReportPropertyChanging("DeviceID");
                    _DeviceID = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("DeviceID");
                    OnDeviceIDChanged();
                }
            }
        }
        private global::System.String _DeviceID;
        partial void OnDeviceIDChanging(global::System.String value);
        partial void OnDeviceIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String DeviceName
        {
            get
            {
                return _DeviceName;
            }
            set
            {
                OnDeviceNameChanging(value);
                ReportPropertyChanging("DeviceName");
                _DeviceName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("DeviceName");
                OnDeviceNameChanged();
            }
        }
        private global::System.String _DeviceName;
        partial void OnDeviceNameChanging(global::System.String value);
        partial void OnDeviceNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String DeviceUrl
        {
            get
            {
                return _DeviceUrl;
            }
            set
            {
                OnDeviceUrlChanging(value);
                ReportPropertyChanging("DeviceUrl");
                _DeviceUrl = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("DeviceUrl");
                OnDeviceUrlChanged();
            }
        }
        private global::System.String _DeviceUrl;
        partial void OnDeviceUrlChanging(global::System.String value);
        partial void OnDeviceUrlChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ClientID
        {
            get
            {
                return _ClientID;
            }
            set
            {
                OnClientIDChanging(value);
                ReportPropertyChanging("ClientID");
                _ClientID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ClientID");
                OnClientIDChanged();
            }
        }
        private global::System.Int32 _ClientID;
        partial void OnClientIDChanging(global::System.Int32 value);
        partial void OnClientIDChanged();

        #endregion
    
        #region Navigation Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [XmlIgnoreAttribute()]
        [SoapIgnoreAttribute()]
        [DataMemberAttribute()]
        [EdmRelationshipNavigationPropertyAttribute("EmergencyDBModel", "FK_Devices_Clients", "Clients")]
        public Client Client
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Client>("EmergencyDBModel.FK_Devices_Clients", "Clients").Value;
            }
            set
            {
                ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Client>("EmergencyDBModel.FK_Devices_Clients", "Clients").Value = value;
            }
        }
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [BrowsableAttribute(false)]
        [DataMemberAttribute()]
        public EntityReference<Client> ClientReference
        {
            get
            {
                return ((IEntityWithRelationships)this).RelationshipManager.GetRelatedReference<Client>("EmergencyDBModel.FK_Devices_Clients", "Clients");
            }
            set
            {
                if ((value != null))
                {
                    ((IEntityWithRelationships)this).RelationshipManager.InitializeRelatedReference<Client>("EmergencyDBModel.FK_Devices_Clients", "Clients", value);
                }
            }
        }

        #endregion
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="EmergencyDBModel", Name="Friendship")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Friendship : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Friendship object.
        /// </summary>
        /// <param name="friendshipID">Initial value of the FriendshipID property.</param>
        /// <param name="clientID">Initial value of the ClientID property.</param>
        /// <param name="friendID">Initial value of the FriendID property.</param>
        /// <param name="status">Initial value of the Status property.</param>
        public static Friendship CreateFriendship(global::System.Int32 friendshipID, global::System.Int32 clientID, global::System.Int32 friendID, global::System.Boolean status)
        {
            Friendship friendship = new Friendship();
            friendship.FriendshipID = friendshipID;
            friendship.ClientID = clientID;
            friendship.FriendID = friendID;
            friendship.Status = status;
            return friendship;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 FriendshipID
        {
            get
            {
                return _FriendshipID;
            }
            set
            {
                if (_FriendshipID != value)
                {
                    OnFriendshipIDChanging(value);
                    ReportPropertyChanging("FriendshipID");
                    _FriendshipID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("FriendshipID");
                    OnFriendshipIDChanged();
                }
            }
        }
        private global::System.Int32 _FriendshipID;
        partial void OnFriendshipIDChanging(global::System.Int32 value);
        partial void OnFriendshipIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ClientID
        {
            get
            {
                return _ClientID;
            }
            set
            {
                OnClientIDChanging(value);
                ReportPropertyChanging("ClientID");
                _ClientID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ClientID");
                OnClientIDChanged();
            }
        }
        private global::System.Int32 _ClientID;
        partial void OnClientIDChanging(global::System.Int32 value);
        partial void OnClientIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 FriendID
        {
            get
            {
                return _FriendID;
            }
            set
            {
                OnFriendIDChanging(value);
                ReportPropertyChanging("FriendID");
                _FriendID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("FriendID");
                OnFriendIDChanged();
            }
        }
        private global::System.Int32 _FriendID;
        partial void OnFriendIDChanging(global::System.Int32 value);
        partial void OnFriendIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean Status
        {
            get
            {
                return _Status;
            }
            set
            {
                OnStatusChanging(value);
                ReportPropertyChanging("Status");
                _Status = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Status");
                OnStatusChanged();
            }
        }
        private global::System.Boolean _Status;
        partial void OnStatusChanging(global::System.Boolean value);
        partial void OnStatusChanged();

        #endregion
    
    }

    #endregion
    
}
