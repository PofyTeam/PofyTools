using System;
using UnityEditor;
using UnityEngine;
using XNode;

namespace Guvernal
{

    /// <summary> Defines an example nodegraph that can be created as an asset in the Project window. </summary>
    [Serializable, CreateAssetMenu(fileName = "New Universe Graph", menuName = "PofyTools/Universe Graph")]
    public class UniverseGraph : NodeGraph
    {
        public void ResetGraph()
        {
            this.nodes.Clear();
        }
    }
}