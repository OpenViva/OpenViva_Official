//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;

namespace JBooth.FoliageRendering
{
    // because unity are dicks about not serializing these normally
    // and using sealed class so I can't just make a serializable version
    [System.Serializable]
    public class TreePrototypeSerializable
    {
        public TreePrototypeSerializable()
        {

        }

        public TreePrototypeSerializable(TreePrototype p)
        {
            bendFactor = p.bendFactor;
            navMeshLod = p.navMeshLod;
            prefab = p.prefab;
        }
        public float bendFactor;
        public int navMeshLod;
        public GameObject prefab;

        public TreePrototype GetPrototype()
        {
            var tree = new TreePrototype();
            tree.prefab = prefab;
            tree.navMeshLod = navMeshLod;
            tree.bendFactor = bendFactor;
            return tree;
        }

        public static bool operator ==(TreePrototypeSerializable obj1, TreePrototypeSerializable obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(obj1, null))
                return false;
            if (ReferenceEquals(obj2, null))
                return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(TreePrototypeSerializable obj1, TreePrototypeSerializable obj2) => !(obj1 == obj2);

        public override bool Equals(object obj) => Equals(obj as TreePrototypeSerializable);


        public bool IsEqualToTree(TreePrototype tree)
        {
            return tree.prefab == prefab && tree.navMeshLod == navMeshLod && tree.bendFactor == bendFactor;
        }



        public override int GetHashCode()
        {
            return System.HashCode.Combine(prefab, navMeshLod, bendFactor);
        }
        public bool Equals(TreePrototypeSerializable x)
        {
            return x.prefab == prefab && x.navMeshLod == navMeshLod && x.bendFactor == bendFactor;
        }
    }
}
