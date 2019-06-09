namespace PofyTools
{
    using UnityEditor;
    using UnityEngine;

    public static class PofyToolsEditorUtility
    {
        public static T[] LoadAllAssetsAtPath<T>(params string[] paths) where T : Object
        {

            var guids = AssetDatabase.FindAssets("", paths);
            Debug.Log(guids.Length);
            T[] result = new T[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                var h = guids[i];
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(h));
                result[i] = asset;
            }

            return result;
        }
    }


}