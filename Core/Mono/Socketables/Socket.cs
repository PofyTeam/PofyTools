namespace PofyTools
{
    using Extensions;
    using System.Collections.Generic;
    using UnityEngine;

    public class Socket : MonoBehaviour, ITransformable, IInitializable
    {
        public enum Action : int
        {
            // default
            None = 0,
            // equip item to owner
            Equip = 1,
            // unequip item from owner
            Unequip = 2,
            // unequip all items
            Empty = 3,
            //TODO: Add socket to owner
            Add = 4,
            //TODO Remove socket from owner
            Remove = 5,
        }
        [Tooltip("Must be unique for every socket on a ISocketable")]
        public string id;

        public int IdHash { get; protected set; }

        public ISocketed owner;
        public int itemLimit = 1;
        //public bool initializeOnStart = false;

        [Header("Offsets")]
        public Vector3 socketPositionOffset = Vector3.zero;
        public Vector3 socketRotationOffset = Vector3.zero;
        public Vector3 socketScaleOffset = Vector3.one;

        protected List<ISocketable> _items = new List<ISocketable>();
        public List<ISocketable> Items { get { return this._items; } }

        public bool IsEmpty { get { return this._items.Count == 0; } }
        public int ItemCount { get { return this._items.Count; } }

        /// <summary>
        /// Removes all items from socket with provided approval
        /// </summary>
        /// <param name="approvedBy"></param>
        /// <returns></returns>
        public bool Empty(SocketActionRequest.SocketingParty approvedBy = SocketActionRequest.SocketingParty.None)
        {
            for (int i = this._items.Count - 1;i >= 0;--i)
            {
                var item = this._items[i];
                SocketActionRequest.TryUnequipItemFromOwner(this.owner, item, this.id, approvedBy);
            }

            return this._items.Count < 0;
        }

        /// <summary>
        /// Add socketable to this socket and call equip callbacks on item and owner.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="inPlace"></param>
        public void AddItem(SocketActionRequest request, bool inPlace = false)
        {
            this._items.Add(request.Item);

            request.Item.Equip(request, inPlace);
            request.Owner.OnItemEquip(request);
        }

        /// <summary>
        /// Removes item provided in request struct.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool RemoveItem(SocketActionRequest request)
        {
            if (this._items.Remove(request.Item))
            {
                request.Item.Unequip(request);
                request.Owner.OnItemUnequip(request);

                return true;
            }

            return false;
        }

        #region IInitializable implementation

        public bool Initialize()
        {
            if (!this._isInitialized)
            {
                if (string.IsNullOrEmpty(this.id))
                {
                    Debug.LogError(ToString() + " ERROR: NO ID!", this);
                    return false;
                }

                this.IdHash = Animator.StringToHash(this.id);

                if (this.owner == null)
                {
                    //this.owner = GetComponentInParent<ISocketed>();
                    Debug.LogError(ToString() + " ERROR: OWNER CAN'T BE NULL!", this);
                    return false;
                }

                if (this.owner.AddSocket(this))
                {
                    ISocketable item = null;
                    SocketActionRequest request = default(SocketActionRequest);
                    foreach (Transform child in this.transform)
                    {
                        item = child.GetComponent<ISocketable>();

                        if (item != null)
                        {
                            item.Initialize();
                            request = new SocketActionRequest(action: Socket.Action.Equip, owner: this.owner, item: item, socket: this);

                            AddItem(request, true);
                        }
                    }

                }
                else
                {
                    Debug.LogError(ToString() + " ERROR: ID MUST BE UNIQUE", this);
                }

                this._isInitialized = true;
                return true;
            }
            return false;
        }

        protected bool _isInitialized = false;

        public bool IsInitialized { get { return this._isInitialized; } }

        #endregion

        //No whitespaces
        private void OnValidate()
        {
            this.id = (string.IsNullOrEmpty(this.id)) ? this.id : this.id.Trim();
        }
    }

    public struct SocketActionRequest
    {
        public const string TAG = "<color=green><b>SocketActionRequest:</b></color> ";

        public enum SocketingParty : int
        {
            None = 0,
            SocketOwner = 1 << 0,
            Item = 1 << 1
        }

        #region Properties
        public SocketActionRequest.SocketingParty ApprovedBy { get; private set; }
        public Socket.Action Action { get; private set; }
        public ISocketable Item { get; private set; }
        public ISocketed Owner { get; private set; }

        public string Id { get; private set; }

        private Socket _socket;

        public Socket Socket
        {
            get
            {
                if (this._socket == null)
                {
                    if ((int)this.Action <= 2)
                        this._socket = this.Owner.GetSocket(this.Id);
                    if (this._socket == null)
                        Debug.LogWarning(TAG + "No socket found for the id: " + this.Id);
                }

                return this._socket;
            }
        }

        #endregion

        #region Instance Methods

        public bool AprovedByAll
        {
            get
            {
                return this.ApprovedBy.HasFlag(SocketingParty.SocketOwner) && this.ApprovedBy.HasFlag(SocketingParty.Item);
            }
        }

        public void ApproveByOwner(ISocketed owner)
        {
            if (owner == this.Owner)
                this.ApprovedBy = this.ApprovedBy.Add(SocketingParty.SocketOwner);
        }

        public void ApproveByItem(ISocketable item)
        {
            if (item == this.Item)
                this.ApprovedBy = this.ApprovedBy.Add(SocketingParty.Item);
        }

        public void RevokeApproval()
        {
            this.ApprovedBy = SocketingParty.None;
        }

        public void ForceApproval()
        {
            this.ApprovedBy = SocketActionRequest.All;
        }

        #endregion

        #region Constructor

        public SocketActionRequest(Socket.Action action = Socket.Action.None, ISocketed owner = null, ISocketable item = null, string id = "", SocketingParty approvedBy = SocketingParty.None, Socket socket = null)
        {
            this.Action = action;
            this.Owner = owner;
            this.Item = item;
            this.Id = id;
            this.ApprovedBy = approvedBy;
            this._socket = socket;
        }

        //        public SocketActionRequest(Socket socket) : this(Socket.Action.None,socket.owner,null,socket.id,ApprovedBy.None)

        #endregion

        #region Object

        public override string ToString()
        {
            return string.Format("[SocketActionRequest: approvedBy={0}, action={1}, item={2}, owner={3}, id={4}, socket={5}, isAprovedByAll={6}]", this.ApprovedBy, this.Action, this.Item, this.Owner, this.Id, this.Socket, this.AprovedByAll);
        }

        #endregion

        #region API Methods

        public static SocketActionRequest TryEquipItemToOwner(ISocketed owner, ISocketable item, string id = "", SocketingParty approvedBy = SocketingParty.None)
        {
            SocketActionRequest request = new SocketActionRequest(action: Socket.Action.Equip, owner: owner, item: item, id: id, approvedBy: approvedBy);

            return ResolveRequest(request);
        }

        public static SocketActionRequest TryUnequipItemFromOwner(ISocketed owner, ISocketable item, string id = "", SocketingParty approvedBy = SocketingParty.None)
        {
            SocketActionRequest request = new SocketActionRequest(action: Socket.Action.Unequip, owner: owner, item: item, id: id, approvedBy: approvedBy);
            return ResolveRequest(request);
        }

        public static SocketActionRequest TryEmptySocket(Socket socket, SocketingParty approvedBy = SocketingParty.None)
        {
            SocketActionRequest request = new SocketActionRequest(action: Socket.Action.Empty, owner: socket.owner, item: null, id: socket.id, approvedBy: approvedBy);
            return ResolveRequest(request);
        }

        /// <summary>
        /// Gets the approval from socket owner first and socketable item second. Returns request with resolved approvedBy field.
        /// </summary>
        /// <returns>Resolved request.</returns>
        /// <param name="request">Request.</param>
        public static SocketActionRequest GetApproval(SocketActionRequest request)
        {
            if (!request.AprovedByAll)
            {
                if (!request.ApprovedBy.HasFlag(SocketingParty.SocketOwner))
                    request = request.Owner.ResolveRequest(request);
                if (!request.ApprovedBy.HasFlag(SocketingParty.Item))
                    request = request.Item.ResolveRequest(request);
            }

            return request;
        }

        /// <summary>
        /// Resolves the request. Resulting in socketing an item to it's owner's socket or ignoring the action.
        /// </summary>
        /// <returns>The request.</returns>
        /// <param name="request">Request.</param>
        public static SocketActionRequest ResolveRequest(SocketActionRequest request)
        {
            Socket socket = null;
            if (request.Action == Socket.Action.None)
            {
                return request;
            }

            if (request.Action == Socket.Action.Empty)
            {
                request.Socket.Empty(request.ApprovedBy);
                return request;
            }


            if (request.Action == Socket.Action.Add || request.Action == Socket.Action.Remove)
            {
                if (!request.ApprovedBy.HasFlag(SocketActionRequest.SocketingParty.SocketOwner))
                    request = request.Owner.ResolveRequest(request);

                if (request.ApprovedBy.HasFlag(SocketActionRequest.SocketingParty.SocketOwner))
                {
                    if (request.Action == Socket.Action.Add)
                        request.Owner.AddSocket(request.Socket);
                    else
                        request.Owner.RemoveSocket(request.Socket);
                }

                return request;
            }

            request = SocketActionRequest.GetApproval(request);

            if (request.AprovedByAll)
            {
                if (request.Action == Socket.Action.Equip)
                {
                    socket = request.Socket;
                    socket.AddItem(request, false);
                    return request;
                }

                if (request.Action == Socket.Action.Unequip)
                {
                    socket = request.Item.Socket;
                    socket.RemoveItem(request);
                    return request;
                }
            }

            Debug.LogWarning(TAG + request.ToString() + "was rejected!");

            return request;
        }

        public static SocketingParty All { get { return SocketingParty.SocketOwner | SocketingParty.Item; } }

        #endregion
    }

    public interface ISocketed : ITransformable, IInitializable, ISocketActionRequestResolver // Character
    {
        bool AddSocket(Socket socket);

        bool RemoveSocket(Socket socket);

        Socket GetSocket(string id);

        Socket GetSocket(int hash);

        bool UnequipAll(SocketActionRequest.SocketingParty approvedBy = SocketActionRequest.SocketingParty.None);

        void OnItemEquip(SocketActionRequest request);

        void OnItemUnequip(SocketActionRequest request);
    }

    public interface ISocketable : ITransformable, IInitializable, ISocketActionRequestResolver// Item
    {
        Socket Socket
        {
            get;
            set;
        }

        bool IsEquipped
        {
            get;
        }

        void Equip(SocketActionRequest request, bool inPlace = false);

        void Unequip(SocketActionRequest request);

    }

    public interface ISocketActionRequestResolver
    {
        SocketActionRequest ResolveRequest(SocketActionRequest request);
    }

    public delegate void SocketActionRequestDelegate(SocketActionRequest request);
    public delegate void SocketableDelegate(ISocketable socketable);
    public delegate void SocketedDelegate(ISocketed socketed);

}