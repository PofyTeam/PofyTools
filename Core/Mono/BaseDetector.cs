namespace PofyTools
{
	using UnityEngine;
	using System.Collections;
    using System.Collections.Generic;

    public abstract class BaseDetector : MonoBehaviour, ICollidable, ITransformable
	{


		#region ICollidable implementation

		protected Collider _selfCollider;

		public Collider SelfCollider {
			get {
				return this._selfCollider;
			}
		}

		protected Rigidbody _selfRigidbody;

		public Rigidbody SelfRigidbody {
			get {
				return this._selfRigidbody;
			}
		}

        public List<Collider> AllColliders { get { return null; } }
        
        #endregion

        #region Mono

        protected virtual void Awake ()
		{
			this._selfCollider = GetComponent<Collider> ();
			this._selfRigidbody = GetComponent<Rigidbody> ();
		}

		#endregion

		public MonoBehaviour target;
		public bool detectStay = false;
		public bool detectExit = false;
	}
}
