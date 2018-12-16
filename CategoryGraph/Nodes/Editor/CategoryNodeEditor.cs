using UnityEditor;
using XNodeEditor;

namespace PofyTools
{
    [CustomNodeEditor(typeof(CategoryNode))]
    public class CategoryNodeEditor : NodeEditor
    {
        //CategoryNode _categoryNode;

        public override void OnHeaderGUI()
        {
            //this.target.name = ObjectNames.NicifyVariableName(this.target.GetValue(this.target.GetOutputPort("id")).ToString());
            //InitiateRename();
            base.OnHeaderGUI();
        }

        /// <summary> Called whenever the xNode editor window is updated </summary>
        public override void OnBodyGUI()
        {
            var _categoryNode = this.target as CategoryNode;

            base.OnBodyGUI();

            var baseCats = _categoryNode.GetInputValues<string>("baseCategories");

            foreach (string baseCat in baseCats)
            {
                EditorGUILayout.LabelField(baseCat);
            }

            //if (baseCats.Length == 0)
            //    EditorGUILayout.LabelField("[ROOT]");

            //if (!this._categoryNode.GetOutputPort("id").IsConnected)
            //    EditorGUILayout.LabelField("[LEAF]");
        }

        //TODO 20181216: find a place to subscribe to node update delegate
        void OnNodeUpdate()
        {
            //this.target.name = ObjectNames.NicifyVariableName(this.target.GetValue(this.target.GetOutputPort("id")).ToString());

        }
    }
}
