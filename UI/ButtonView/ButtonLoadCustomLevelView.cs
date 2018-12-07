using UnityEngine;

namespace PofyTools
{
    public class ButtonLoadCustomLevelView : ButtonView
    {
        [SerializeField]
        protected string _sceneToLoad;

        #region implemented abstract members of ButtonView

        protected override void OnClick()
        {
            LevelLoader.LoadCustomScene(this._sceneToLoad);
        }

        #endregion

    }

}
