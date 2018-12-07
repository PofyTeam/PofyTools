namespace PofyTools.Data
{
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    [ExecuteInEditMode]
    public abstract class DataEditor : MonoBehaviour
    {
        public abstract void SaveData();
        public abstract void LoadData();
        public abstract void Refresh();

#if UNITY_EDITOR
        [ContextMenu("Load")]
        void CommandLoad()
        {
            LoadData();
        }

        [ContextMenu("Save")]
        void CommandSave()
        {
            SaveData();
        }

        [ContextMenu("Refresh")]
        void CommandRefresh()
        {
            Refresh();
        }

        void OnPrefabUpdated(GameObject prefab)
        {
            if (this != null)
            {
                if (prefab == this.gameObject)
                {
                    SaveData();
                    //AssetDatabase.Refresh(ImportAssetOptions.Default);
                    Refresh();
                }
            }
            else
            {
                //Debug.LogError("THIS is NULL! Data NOT Saved!");
            }
        }

        protected virtual void OnValidate()
        {
            PrefabUtility.prefabInstanceUpdated -= OnPrefabUpdated;
            PrefabUtility.prefabInstanceUpdated += OnPrefabUpdated;
        }
#endif
    }

}