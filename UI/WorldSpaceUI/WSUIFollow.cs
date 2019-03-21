using UnityEngine;
namespace PofyTools
{
    public class WSUIFollow : WSUIBase
    {
        public Transform followTarget;
        public Vector3 followOffset;

        public override bool UpdateElement(WSUIManager.UpdateData data)
        {
            //follow
            this._rectTransform.position = this.followTarget.position + this.followOffset;

            return base.UpdateElement(data);
        }

    }
}