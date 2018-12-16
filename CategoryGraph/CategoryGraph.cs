using System;
using UnityEditor;
using UnityEngine;
using XNode;

namespace PofyTools
{

    /// <summary> Defines an example nodegraph that can be created as an asset in the Project window. </summary>
    [Serializable, CreateAssetMenu(fileName = "New Category Graph", menuName = "PofyTools/Category Graph")]
    public class CategoryGraph : NodeGraph
    {
        [SerializeField]
        private Node _setNode = null;
        public Node SetNode { get { return this._setNode; } }
        public bool HasSetNode { get { return this._setNode != null; } }

        [ContextMenu("Save Category Definition Set")]
        public void SaveDefinitions()
        {
            if(this._setNode!=null)
            (this._setNode as CategorySetNode).Save();
            AssetDatabase.SaveAssets();
        }

        /// <summary> Add a node to the graph by type </summary>
        public override Node AddNode(Type type)
        {
            if (type == typeof(CategorySetNode))
            {
                if (this._setNode != null)
                {
                    //destroy?
                    return null;
                }

                this._setNode = base.AddNode(type) as CategorySetNode;
                return this._setNode;
            }
            else
                return base.AddNode(type);
        }

        public override void RemoveNode(Node node)
        {
            if (node != this._setNode)
                base.RemoveNode(node);
        }

        public void ResetGraph()
        {
            if (Application.isPlaying)
            {
                for (int i = this.nodes.Count - 1; i >= 0; i--)
                {
                    if (this.nodes[i] != this._setNode)
                        Destroy(this.nodes[i]);
                }
            }

            this.nodes.Clear();
            this.nodes.Add(this._setNode);
        }

        //private void OnEnable()
        //{
        //    if (this._setNode == null)
        //    {
        //        this._setNode = AddNode<CategorySetNode>();
        //        this._setNode.name = ObjectNames.NicifyVariableName(typeof(CategorySetNode).Name);
        //    }
        //}
    }
}