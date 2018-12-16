using UnityEngine;
using XNodeEditor;

namespace PofyTools
{
    [CustomNodeEditor(typeof(CategorySetNode))]
    public class CategorySetNodeEditor : NodeEditor
    {


        public override void OnHeaderGUI()
        {
            //this.target.name = ObjectNames.NicifyVariableName(this.target.GetValue(this.target.GetOutputPort("id")).ToString());
            //InitiateRename();
            base.OnHeaderGUI();
        }

        /// <summary> Called whenever the xNode editor window is updated </summary>
        public override void OnBodyGUI()
        {
            var _categorySetNode = this.target as CategorySetNode;

            base.OnBodyGUI();
            if (string.IsNullOrEmpty(_categorySetNode._lastPath))
            {
                if (GUILayout.Button("Save"))
                {
                    _categorySetNode.SaveAs();
                }
            }
            else
            {
                if (GUILayout.Button("Save"))
                {
                    _categorySetNode.Save();
                }
                if (GUILayout.Button("Save as.."))
                {
                    _categorySetNode.SaveAs();
                }
            }



            //if(GUILayout.Button("Clear All"))
            //{
            //    (_categorySetNode.graph as CategoryGraph).ResetGraph();
            //}
        }

        void OnNodeUpdate()
        {
            //this.target.name = ObjectNames.NicifyVariableName(this.target.GetValue(this.target.GetOutputPort("id")).ToString());

        }
    }
}
