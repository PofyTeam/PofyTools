using UnityEditor;
using UnityEngine;
using XNode;
using System.IO;
using System;

namespace PofyTools
{
    [System.Serializable]
    [NodeTint(0.6f, 0.5f, 0.3f)]
    [NodeWidth(400)]
    public class CategorySetNode : Node
    {
        public string DEFINITIONS_PATH = "Definitions";
        public string FILE_NAME = "categories";
        public string FILE_EXTENSION = "json";
        public bool PROTECT_DATA = false;

        protected void SingletonCheck()
        {
            Node other = null;

            foreach (var node in this.graph.nodes)
            {
                if (node != this)
                {
                    if (node is CategorySetNode)
                    {
                        other = node;
                        break;
                    }
                }

            }

            if (other != null)
            {
                this.graph.RemoveNode(this);
            }
        }

        [HideInInspector]
        public string _lastPath;

        [ContextMenu("Save")]
        public void Save()
        {
            CategoryDefinitionSet set = new CategoryDefinitionSet(this.DEFINITIONS_PATH, this.FILE_NAME, this.PROTECT_DATA, this.PROTECT_DATA, this.FILE_EXTENSION);
            var content = set.GetContent();

            foreach (var node in this.graph.nodes)
            {
                if (node is CategoryNode)
                {
                    var catNode = node as CategoryNode;
                    catNode.VerifyConnections();

                    CategoryDefinition newDefinition = new CategoryDefinition(catNode.categoryId);
                    newDefinition.baseIds.AddRange(catNode.GetInputValues<string>("baseCategories"));

                    content.Add(newDefinition);
                }
            }

            set.SetContent(content);
            //set.Save();
            //var fullPath = EditorUtility.SaveFilePanel("Save Category Definition Set", Application.dataPath, this.FILE_NAME, this.FILE_EXTENSION);
            DataUtility.PanelSave(_lastPath, set);
            AssetDatabase.SaveAssets();
        }

        [ContextMenu("Save as...")]
        public void SaveAs()
        {
            this._lastPath = EditorUtility.SaveFilePanel("Save Category Definition Set", Application.dataPath, this.FILE_NAME, this.FILE_EXTENSION);
            Save();
        }
    }
}