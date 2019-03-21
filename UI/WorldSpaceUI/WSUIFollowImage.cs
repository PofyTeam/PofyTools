using UnityEngine;
using UnityEngine.UI;
namespace PofyTools
{

    public class WSUIFollowImage : WSUIFollow
    {
        public Image image;

        public void SetData(Sprite sprite, Transform followTarget, float size = 1f, Vector3 followOffset = default)
        {
            this.followTarget = followTarget;
            this.followOffset = followOffset;

            this.image.sprite = sprite;
            this.image.rectTransform.sizeDelta = Vector2.one * size;
        }
    }

}