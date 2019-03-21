namespace PofyTools
{
    public class DelayManager
    {
        public const int ELEMENT_LIMIT = 64;
        private static DelayManager _instance = null;
        public static DelayManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DelayManager();

                return _instance;
            }

        }

        #region Static API
        public static void Purge()
        {
            _instance = null;
        }
        public static int DelayAction(VoidDelegate method, float delay, object instance)
        {
            return Instance.AddElement(method, delay, instance);
        }

        public static void Tick(float deltaTime)
        {
            Instance.Update(deltaTime);
        }
        #endregion

        public struct Description
        {
            public object instance;
            public float duration;
            public VoidDelegate callback;

            public bool Update(float deltaTime)
            {
                this.duration -= deltaTime;
                return this.duration <= 0f;
            }
        }

        private Description[] _elements = new Description[ELEMENT_LIMIT];
        private int AddElement(VoidDelegate method, float delay, object instance)
        {
            for (int i = 0; i < this._elements.Length; ++i)
            {
                if (this._elements[i].callback == null)
                {
                    var desc = new Description()
                    {
                        instance = instance,
                        duration = delay,
                        callback = method
                    };

                    this._elements[i] = desc;
                    return i;
                }
            }

            return -1;
        }

        private void Update(float deltaTime)
        {
            for (int i = 0; i < this._elements.Length; ++i)
            {
                if (this._elements[i].callback != null)
                {
                    if (this._elements[i].Update(deltaTime))
                    {
                        if (this._elements[i].instance != null)
                            this._elements[i].callback.Invoke();

                        this._elements[i] = default;
                    }
                }
            }
        }

    }
}