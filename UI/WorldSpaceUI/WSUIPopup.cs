﻿using UnityEngine;
using UnityEngine.UI;

namespace PofyTools
{
    public class WSUIPopup : WSUIBase//StateableActor, IPoolable<ScreenInfo>
    {
        public Text message;
        public AnimationCurve alphaCurve;

        public float speed = 1;
        public float duration = 1;
        private float _timer = 0;

        #region Managable Element

        public override bool UpdateElement(WSUIManager.UpdateData data)
        {
            this._timer -= data.deltaTime;
            if (this._timer < 0)
                this._timer = 0;

            //float normalizedTime = this.alphaCurve.Evaluate(1 - this._timer / this.duration);
            //this._canvasGroup.alpha = normalizedTime;
            //this._rectTransform.localScale = normalizedTime * this._startScale;

            //move up
            this._rectTransform.Translate(this.speed * data.deltaTime * this._rectTransform.up, Space.Self);

            base.UpdateElement(data);

            if (this._timer <= 0)
                return true;
            return false;
        }

        #endregion

        #region IPoolable

        /// <summary>
        /// Active instances are visible in the scene and updated by the manager
        /// </summary>
        public override void Activate()
        {
            if (!this.IsActive)
            {
                //custom
                this._timer = this.duration;
                base.Activate();
            }
        }

        public void SetMessageData(ResourceManager.WorldSpaceInfoData data)
        {
            this.message.text = data.message;
            this.message.color = data.color;
            this.duration = data.duration;
            this._rectTransform.position = data.position;
        }

        #endregion

    }
}