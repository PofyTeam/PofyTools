//using PofyTools;
//using UnityEditor;
//using UnityEngine;


//public class CategoryEditorWindow : EditorWindow
//{
//    private string[] _ids = null;

//    [MenuItem("PofyTools/Ability Definition Editor")]
//    static void Init()
//    {
//        // Get existing open window or if none, make a new one
//        AbilityEditorWindow window = (AbilityEditorWindow)EditorWindow.GetWindow(typeof(CategoryEditorWindow));
//        window.Show();
//    }
//    private CategoryDefinitionSet _definitions;
//    public void ReadData()
//    {
//        this._definitions = new CategoryDefinitionSet(this._path, this._filename, this._protected, this._protected, this._extension);
//        this._definitions.Initialize();
//    }
//    private string _path = "definitions", _filename = "categories", _extension = "json";
//    private bool _protected = false;

//    void OnGUI()
//    {
//        EditorGUILayout.BeginVertical();
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("PATH:");
//        this._path = EditorGUILayout.TextField(this._path);
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("File Name:");
//        this._filename = EditorGUILayout.TextField(this._filename);
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("File Extension:");
//        this._extension = EditorGUILayout.TextField(this._extension);
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("DATA PROTECTION:");
//        this._protected = EditorGUILayout.Toggle(this._protected);
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();
//        if (GUILayout.Button("Load Data"))
//        {
//            if (this._dirty)
//            {
//                if (EditorUtility.DisplayDialog("Changes not saved", "Discard changes?", "Discard", "Cancel"))
//                {
//                    ReadData();
//                    this._dirty = false;
//                }
//            }
//            else
//            {
//                ReadData();
//                this._dirty = false;
//            }

//        }

//        if (GUILayout.Button("Save Data"))
//        {
//            this._definitions.Save();
//            AssetDatabase.Refresh();
//            this._dirty = false;
//        }

//        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.EndVertical();

//        if (this._definitions == null || this._definitions.GetContent().Count == 0)
//        {
//            return;
//        }

//        DrawContent();

//        if (GUILayout.Button("Add New Category Definition"))
//        {
//            AddDefinition();
//        }

//    }

//    private Vector2 _scrollPosition;

//    void DrawContent()
//    {
//        int indexToRemove = -1;
//        this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Sort By:", GUILayout.Width(60));
//        if (GUILayout.Button("Id", GUILayout.Width(270)))
//        {
//            //sort by id
//            this._definitions.GetContent().Sort((x, y) => x.id.CompareTo(y.id));
//        }

//        EditorGUILayout.LabelField("PofyTools", GUILayout.Width(360));

//        if (GUILayout.Button("Base Category", GUILayout.Width(310)))
//        {
//            //this._definitions.GetContent().Sort((x, y) => x.baseCategories[0].CompareTo(y.baseCategories[0]));
//        }
//        EditorGUILayout.EndHorizontal();

//        for (int i = 0;i < this._definitions.Count;i++)
//        {
//            var definition = this._definitions.GetContent()[i];

//            var id = definition.id;
//            var descriptionId = definition.categoryDescription;

//            //var groupId = definition.groupId;
//            //var groupOrder = definition.groupOrder;

//            if (definition.baseCategories.Count == 0)
//                definition.baseCategories.Add("");
//            var baseCategory = definition.baseCategories[0];
//            EditorGUILayout.BeginHorizontal();

//            if (GUILayout.Button("Remove", GUILayout.Width(60)))
//            {
//                indexToRemove = i;
//            }

//            EditorGUILayout.LabelField("Id:", GUILayout.Width(20));
//            definition.id = EditorGUILayout.TextField(definition.id, GUILayout.Width(250));
//            EditorGUILayout.LabelField("Description Id:", GUILayout.Width(100));
//            definition.categoryDescription = definition.id + "_description";
//            EditorGUILayout.LabelField(definition.categoryDescription, GUILayout.Width(250));

//            EditorGUILayout.LabelField("Base Ability:", GUILayout.Width(60));
//            definition.baseCategories[0] = EditorGUILayout.TextField(definition.baseCategories[0], GUILayout.Width(250));

//            //EditorGUILayout.LabelField("Group Id:", GUILayout.Width(40));
//            //definition.groupId = EditorGUILayout.TextField(definition.groupId, GUILayout.Width(250));
//            //EditorGUILayout.LabelField("Group Order:", GUILayout.Width(40));
//            //definition.groupOrder = EditorGUILayout.IntField(
//            //    string.IsNullOrEmpty(definition.groupId) ? -1 : definition.groupOrder, GUILayout.Width(30));
            
//            EditorGUILayout.LabelField("Cost:", GUILayout.Width(40));
//            if (definition.cost.Count == 0)
//            {
//                AddCost(definition);
//            }

//            for (int c = 0;c < definition.cost.Count;c++)
//            {
//                var cost = definition.cost[c];
//                cost.type = (CurrencyType)EditorGUILayout.EnumPopup(definition.cost[c].type, GUILayout.Width(80));
//                cost.amount = EditorGUILayout.IntField(definition.cost[c].amount, GUILayout.Width(80));
//                definition.cost[c] = cost;

//            }

//            if (GUILayout.Button("Add Cost", GUILayout.Width(80)))
//            {
//                AddCost(definition);
//            }
//            EditorGUILayout.EndHorizontal();

//            if (
//                id != definition.id ||
//                descriptionId != definition.descriptionId ||
//                groupId != definition.groupId ||
//                groupOrder != definition.groupOrder ||
//                baseCategory != definition.baseAbilities[0]
//                )
//            {
//                EditorUtility.SetDirty(this);
//                this._dirty = true;
//            }
//        }

//        if (indexToRemove != -1)
//        {
//            RemoveDefinition(indexToRemove);
//        }

//        EditorGUILayout.EndScrollView();
//    }

//    void AddDefinition()
//    {
//        var newDef = new AbilityDefinition("new_ability");
//        this._definitions.AddContent(newDef);
//        //newDef.baseAbilities.Add("new_base_ability");
//        _dirty = true;
//    }

//    void RemoveDefinition(int index)
//    {
//        if (EditorUtility.DisplayDialog("Remove Entry", "Are you sure?", "Remove", "Cancel"))
//        {
//            this._definitions.GetContent().RemoveAt(index);
//            this._dirty = true;
//        }
//    }

//    void AddCost(AbilityDefinition definition)
//    {

//        var cost = new AbilityCost(CurrencyType.ExperiencePoints,200);
//        definition.cost.Add(cost);
//    }

//    private bool _dirty = false;

//    void OnDestroy()
//    {
//        if (this._dirty)
//        {
//            if (EditorUtility.DisplayDialog("Changes not saved", "Save changes?", "Save", "Discard"))
//            {
//                this._definitions.Save();
//                AssetDatabase.Refresh();
//                this._dirty = false;
//            }
//        }

//        this._definitions = null;
//    }
//}