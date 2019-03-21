using UnityEngine;

namespace PofyTools
{
    /// <summary>
    /// Base World Space UI Behaviour:
    /// Distance Fade From PlayerCharacter
    /// Look at Character Camera
    /// </summary>
    [RequireComponent(typeof(Canvas)), RequireComponent(typeof(CanvasGroup))]
    public class WSUIBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _rectTransform;

        #region Managable Element

        public virtual bool UpdateElement(WSUIManager.UpdateData data)
        {
            //billboard
            this._rectTransform.rotation = Quaternion.LookRotation(this._rectTransform.position - data.cameraPosition, data.cameraUp);

            //fade
            float playerDistanceSqr = (this._rectTransform.position - data.playerPosition).sqrMagnitude;
            float cameraDistanceSqr = (this._rectTransform.position - data.cameraPosition).sqrMagnitude;

            this._canvasGroup.alpha = (1 - GameManager.Data.wsuiData.fadeDistanceSqrRange.Percentage(playerDistanceSqr)) * GameManager.Data.wsuiData.fadeNearClipRange.Percentage(cameraDistanceSqr);

            return false;
        }

        #endregion

        #region IPoolable
        protected WSUIManager.PoolData _data;
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Sets element's pool data. Elements without stack reference will be destoryed when Free is called.
        /// </summary>
        /// <param name="data"></param>
        public void SetPoolData(WSUIManager.PoolData data)
        {
            this._data = data;
        }

        /// <summary>
        /// Active instances are visible in the scene and updated by the manager
        /// </summary>
        public virtual void Activate()
        {
            if (!this.IsActive)
            {
                this.gameObject.SetActive(true);
                this.IsActive = true;

                //Add to manager's update stack
                if (this._data.manager != null)
                    this._data.manager.AddElement(this);
                else
                    Debug.LogError("Element has no manager!");
            }
        }

        /// <summary>
        /// Deactivated instances are removed from the manager
        /// </summary>
        public virtual void Deactivate()
        {
            if (this.IsActive)
            {
                this.gameObject.SetActive(false);
                this.IsActive = false;
            }
        }

        /// <summary>
        /// Frees or destorys element. Needs to be inactive to be freed.
        /// </summary>
        public virtual void Free()
        {
            if (this.IsActive)
            {
                Debug.LogError("Can not free active UI element!");
                return;
            }

            this.gameObject.SetActive(false);

            WSUIManager.PushOrDestroy(this, this._data.stack, !this._data.poolable ? WSUIManager.PushResult.Destroyed : WSUIManager.PushResult.None); //this._data.stack.Push(this);
        }

        #endregion
    }
}