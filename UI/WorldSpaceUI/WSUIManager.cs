using System.Collections.Generic;
using UnityEngine;

namespace PofyTools
{
    public sealed class WSUIManager : IInitializable
    {
        public const string TAG = "<b>WorldSpaceUIManager :</b>";
        public static WSUIManager Instance;

        #region IInitializable implementation

        private bool _isInitialized = false;
        public bool IsInitialized => this._isInitialized;
        private Transform _character, _camera;

        public bool Initialize(Transform character, Transform camera)
        {
            this._character = character;
            this._camera = camera;

            return Initialize();
        }

        public bool Initialize()
        {
            if (!this.IsInitialized)
            {
                Instance = this;

                //Initialize Pool
                this._pools = new Dictionary<System.Type, Stack<WSUIBase>>();
                this._activeElements = new List<WSUIBase>();

                this._isInitialized = true;
                return true;
            }
            return false;
        }

        #endregion

        #region Pool
        private Dictionary<System.Type, Stack<WSUIBase>> _pools = null;

        public Stack<WSUIBase> GetStack(System.Type type)
        {
            Stack<WSUIBase> stack = null;
            if (!this._pools.TryGetValue(type, out stack))
            {
                stack = new Stack<WSUIBase>();
                this._pools[type] = stack;
            }

            return stack;
        }

        #endregion

        [SerializeField] public List<WSUIBase> _activeElements;

        public void AddElement(WSUIBase element)
        {
            this._activeElements.Add(element);
        }

        public struct UpdateData
        {
            public float deltaTime;
            public Vector3 playerPosition;
            public Vector3 cameraPosition;
            public Vector3 cameraUp;
        }

        public void Update(float delatTime)
        {
            var data = new UpdateData()
            {
                deltaTime = delatTime,
                playerPosition = _character.position,
                cameraPosition = _camera.position,
                cameraUp = _camera.up,
            };

#if UNITY_EDITOR

            if (Input.GetKeyDown(KeyCode.P))
            {
                var newElement = ObtainPositionImage() as WSUIPositionImage;
                newElement.transform.position = data.playerPosition;
                newElement.image.rectTransform.sizeDelta = Vector2.one * Random.Range(1f, 5f);
                newElement.Activate();
                //Debug.LogError("[P]LACED!");
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                var newElement = ObtainFollowImage() as WSUIFollow;
                newElement.followTarget = _character;
                newElement.followOffset = new Vector3(0f, 2.5f, 0f);

                newElement.Activate();
                //Debug.LogError("[P]LACED!");
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                foreach (var e in this._activeElements)
                {
                    e.Deactivate();
                }

                //Debug.LogError("[O]LL IS DEACTIVATED!");
            }
#endif


            for (int i = this._activeElements.Count - 1; i >= 0; i--)
            {
                var element = this._activeElements[i];

                //update only active elements
                if (element.IsActive && element.UpdateElement(data))
                {
                    element.Deactivate();
                }
            }
        }

        //Calls on LateUpdate
        public void FreeInactiveElements()
        {
            for (int i = this._activeElements.Count - 1; i >= 0; i--)
            {
                var element = this._activeElements[i];

                if (!element.IsActive)
                {
                    this._activeElements.RemoveAt(i);
                    element.Free();
                }
            }
        }

        public void Purge()
        {
            this._pools = null;
        }

        #region Static API

        public struct PoolData
        {
            public WSUIManager manager;
            public Stack<WSUIBase> stack;
            public bool poolable;
        }

        public static WSUIBase PopOrRegister(System.Type type, WSUIBase prefab)
        {
            var stack = Instance.GetStack(type);
            WSUIBase instance = null;

            if (stack.Count > 0)
                instance = stack.Pop();
            else
                instance = GameObject.Instantiate<WSUIBase>(prefab);

            var data = new PoolData()
            {
                manager = Instance,
                stack = stack,
                poolable = true
            };

            instance.SetPoolData(data);

            return instance;
        }

        /// <summary>
        /// Register outside instance with manager.
        /// </summary>
        /// <param name="instance">Instance to register with manager.</param>
        /// <param name="poolTrack">Should instance be returned to manager's pool.</param>
        public static void RegisterWithManager(WSUIBase instance, bool poolTrack = false)
        {
            Stack<WSUIBase> stack = (poolTrack) ? Instance.GetStack(instance.GetType()) : null;

            var poolData = new PoolData()
            {
                manager = Instance,
                stack = stack,
                poolable = poolTrack
            };

            instance.SetPoolData(poolData);
        }

        public enum PushResult
        {
            Destroyed = -1,
            None = 0,
            Success = 1,
        }

        public static PushResult PushOrDestroy(WSUIBase instance, Stack<WSUIBase> stack, PushResult forcedResult = PushResult.None)
        {
            if (stack == null) forcedResult = PushResult.Destroyed;

            if (forcedResult == PushResult.Destroyed || (forcedResult != PushResult.Success && stack.Count >= GameManager.Data.wsuiData.stackLimit))
            {
                GameObject.Destroy(instance.gameObject);
                return PushResult.Destroyed;
            }

            stack.Push(instance);
            return PushResult.Success;
        }

        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainPositionText() => PopOrRegister(typeof(WSUIPositionText), GameManager.Data.wsuiData.positionTextPrefab);
        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainFollowText() => PopOrRegister(typeof(WSUIFollowText), GameManager.Data.wsuiData.followTextPrefab);
        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainPositionImage() => PopOrRegister(typeof(WSUIPositionImage), GameManager.Data.wsuiData.positionImagePrefab);
        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainFollowImage() => PopOrRegister(typeof(WSUIFollowImage), GameManager.Data.wsuiData.followImagePrefab);
        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainPopup() => PopOrRegister(typeof(WSUIPopup), GameManager.Data.wsuiData.popupPrefab);
        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainActionMessage() => PopOrRegister(typeof(UIActionMessage), GameManager.Data.wsuiData.uiActionMessagePrefab);
        /// <summary>
        /// Obtained Instance is not automatically added to active elements.
        /// Use instance.Activate to activate it.
        /// </summary>
        /// <returns></returns>
        public static WSUIBase ObtainBar() => PopOrRegister(typeof(WSUIBar), GameManager.Data.wsuiData.barPrefab);

        #endregion
    }
}