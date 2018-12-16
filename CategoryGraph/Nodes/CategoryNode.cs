using UnityEngine;
using XNode;

namespace PofyTools
{
    [System.Serializable]
    [NodeTint(0.6f, 0.8f, 0.3f)]
    public class CategoryNode : Node
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple)] public string baseCategories;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple)] public string categoryId = "new_definition";

        // GetValue should be overridden to return a value for any specified output port
        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "categoryId")
                return this.name;
            else
                return null;
        }

        protected override void Init()
        {
            this.categoryId = this.name;
        }
    }
}