using System;
using UnityEngine;
using XNode;

namespace Guvernal
{
    [Serializable, NodeTint(0.6f, 0.8f, 0.3f)]
    public class WorldNode : Node
    {
        public float size = 1;
        public ulong endYear;
        public OriginEvent originEvent;
    }

    [Flags]
    public enum OriginEvent
    {
        Civilization =  1 << 0,
        Magic =         1 << 1,
        Technology =    1 << 2,
    }

    public class Faction
    {
        public Vector2 ethicals;
        public ulong yearOfOrigin;
    }
}