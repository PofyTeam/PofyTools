using Extensions;
using PofyTools;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityDefinition : Definition
{
    #region Contstructors
    public AbilityDefinition()
    {
        this.baseAbilities = new List<string>();
        this.baseAbilities.Add("new_base");
        this.cost = new List<AbilityCost>();
    }
    public AbilityDefinition(string key)
    {
        this.id = key;
        this.baseAbilities = new List<string>();
        this.baseAbilities.Add("new_base");
        this.cost = new List<AbilityCost>();
    }
    #endregion

    //[Header("Display Name ID")]
    //public string displayNameId;

    //[TextArea]
    [Header("Description")]
    public string descriptionId;

    [Header("Base Abilities")]
    public List<string> baseAbilities = new List<string>();

    //id is same as of the category
    public int IdHash { get; private set; }

    [Header("Group ID")]
    [Tooltip("Group ID is used for visual grouping under one button.")]
    public string groupId;
    public int GroupIdHash { get; private set; }
    public int groupOrder;

    public List<AbilityCost> cost = new List<AbilityCost>();
}

[System.Serializable]
public struct AbilityCost
{
    public CurrencyType type;
    public int amount;

    public AbilityCost(CurrencyType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
/// <summary>
/// ...Goran...
/// </summary>
public enum CurrencyType : int
{
    ExperiencePoints = 0,
    Legs = 1,
    Arms = 2,
    Heads = 3,
}

[System.Serializable]
public class AbilityData : DefinableData<AbilityDefinition>
{
    #region CategoryData

    public AbilityData(AbilityDefinition definition) : base(definition) { }

    public AbilityData()
    {
        this._subabilities = new List<AbilityData>();
        this._superabilities = new List<AbilityData>();
    }

    #region Runtime Data
    [System.NonSerialized]
    public List<AbilityData> _subabilities = new List<AbilityData>();
    [System.NonSerialized]
    public List<AbilityData> _superabilities = new List<AbilityData>();
    [System.NonSerialized]
    public int _level = -1;

    [System.NonSerialized]
    public List<AbilityData> _parents = new List<AbilityData>();

    [System.NonSerialized]
    public List<AbilityData> _childern = new List<AbilityData>();

    public string Id { get { return this.id; } }
    #endregion

    #region Initialization API
    public static void MakeDirectConnection(AbilityData parent, AbilityData child)
    {
        //Debug.LogFormat("Parent: <size=16>{0}</size> Child: <size=16>{1}</size>...", parent.id, child.id);

        parent._childern.AddOnce(child);
        //Debug.LogWarningFormat("Parent <size=16>{0}</size> already has a reference to a <size=16>{1}</size> child!", parent.id, child.id);
        child._parents.AddOnce(parent);
        //Debug.LogWarningFormat("Child <size=16>{1}</size> already has a reference to a <size=16>{0}</size> parent!", parent.id, child.id);

        child.AddSuperability(parent);
        parent.AddSubability(child);

    }


    /// <summary>
    /// Add Sub-Ability and propagate to all Super-Abilities
    /// </summary>
    /// <param name="data"></param>
    public void AddSubability(AbilityData data)
    {
        if (this._subabilities.AddOnce(data))
            Debug.LogFormat("<color=green>Subability Added: {0} to {1}!</color>", data.id, this.id);
        foreach (var ability in this._superabilities)
        {
            ability.AddSubability(data);
        }
    }

    /// <summary>
    /// Add Super-Ability and propagate to all Sub-Abilities
    /// </summary>
    /// <param name="data"></param>
    public void AddSuperability(AbilityData data)
    {

        //Add Once
        if (this._superabilities.AddOnce(data))
            Debug.LogFormat("<color=green>Superability Added: {0} to {1}!</color>", data.id, this.id);

        var parents = data._superabilities;
        foreach (var parent in parents)
        {
            if (this._superabilities.AddOnce(parent))
                Debug.LogFormat("<color=green>Superability Added: {0} to {1}!</color>", parent.id, this.id);
        }


        foreach (var child in this._childern)
        {
            child.AddSuperability(data);
        }



    }

    /// <summary>
    /// Is Ability with abilityId Sub-Ability of this Ability (xD)
    /// </summary>
    /// <param name="abilityId"></param>
    /// <returns></returns>
    public bool IsSubabilityOf(string abilityId)
    {
        if (this.id == abilityId)
            return true;

        foreach (var superabilityId in this.descriptor.superabilityIds)
        {
            if (abilityId == superabilityId)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    [System.NonSerialized]
    public Descriptor descriptor = new Descriptor();

    [System.Serializable]
    public class Descriptor
    {
        public string id;
        public List<string> subabilityIds = new List<string>();
        public List<string> superabilityIds = new List<string>();

        public override string ToString()
        {
            var result = this.id;
            result += "\n\tSUBS:";
            foreach (var sub in this.subabilityIds)
            {
                result += "\n\t\t" + sub;
            }
            result += "\n\tSUPERS";
            foreach (var super in this.superabilityIds)
            {
                result += "\n\t\t" + super;
            }

            return result;
        }
    }

    #endregion

    public enum State : int
    {
        Unavailable = 0,
        Available = 1,
        Active = 2,
    }

    public State state;

}

[System.Serializable]
public class AbilityDefinitionSet : DefinitionSet<AbilityDefinition>
{
    public AbilityDefinitionSet(string fullPath, string filename, bool scramble = false, bool encode = false, string extension = "") : base(fullPath, filename, scramble, encode, extension)
    {
    }

    #region Save
    public void Optimize()
    {
        for (int i = this._content.Count - 1;i >= 0;i--)
        {
            var element = this._content[i];
            if (string.IsNullOrEmpty(element.id))
            {
                this._content.RemoveAt(i);
                continue;
            }

            //element.displayNameId = (string.IsNullOrEmpty(element.displayNameId)) ? element.id.ToTitle() : element.displayNameId;

            element.baseAbilities.RemoveAll(x => string.IsNullOrEmpty(x) || x == element.id);
        }
    }

    public override void Save()
    {
        Optimize();
        base.Save();
    }
    #endregion
}

[System.Serializable]
public class AbilityDataSet : DataSet<string, AbilityData>
{
    public AbilityDataSet(DefinitionSet<AbilityDefinition> abilityDefinitionSet)
    {
        Initialize(abilityDefinitionSet.GetContent());
    }

    public AbilityDataSet(List<AbilityDefinition> _content)
    {
        Initialize(_content);
    }

    /// <summary>
    /// Topmost Abilities.
    /// </summary>
    [System.NonSerialized]
    public List<AbilityData> rootAbilities;

    /// <summary>
    /// Bottommost Abilities.
    /// </summary>
    [System.NonSerialized]
    public List<AbilityData> leafAbilities;

    private List<AbilityData> _currentLevel;// = new List<AbilityData>();

    public bool Initialize(List<AbilityDefinition> abilityDefinitions)
    {
        if (!this.IsInitialized)
        {
            this.rootAbilities = new List<AbilityData>();
            this.leafAbilities = new List<AbilityData>();
            this._currentLevel = new List<AbilityData>();

            this.content = new Dictionary<string, AbilityData>(abilityDefinitions.Count);

            bool newData = this._content.Count == 0 && abilityDefinitions.Count != 0;

            if (newData)
            {
                foreach (var definition in abilityDefinitions)
                {
                    AbilityData data = new AbilityData(definition);

                    AddContent(data.id, data);

                    //Find Root
                    if (definition.baseAbilities.Count == 0 || string.IsNullOrEmpty(definition.baseAbilities[0]))
                    {
                        data._level = 0;
                        this.rootAbilities.Add(data);
                    }
                }

                if (this.rootAbilities.Count > 0)
                {
                    Debug.LogFormat("<size=20>ROOT FOUND:{0}</size>", this.rootAbilities.Count);
                    this._currentLevel.Clear();
                    this._currentLevel.AddRange(this.rootAbilities);

                    for (int i = 0;i < this._currentLevel.Count;++i)
                    {
                        var superData = this._currentLevel[i];

                        foreach (var data in this._content)
                        {
                            foreach (var superId in data.Definition.baseAbilities)
                            {
                                if (superId == superData.id)
                                {
                                    AbilityData.MakeDirectConnection(superData, data);
                                    //debug
                                    data._level = superData._level + 1;

                                    this._currentLevel.Add(data);
                                }
                            }
                        }
                    }
                }

            }

            Initialize();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Data-Crunching Initialization
    /// </summary>
    /// <returns></returns>
    public override bool Initialize()
    {
        if (!this.IsInitialized)
        {

            //find leafs
            foreach (var data in this._content)
            {
                if (data._subabilities.Count == 0)
                    this.leafAbilities.Add(data);
            }

            ////Sort By level (Debug)
            //this._content.Sort((x, y) => x._level.CompareTo(y._level));

            foreach (var data in this._content)
            {
                data.descriptor.id = data.id;
                foreach (var sub in data._subabilities)
                {
                    data.descriptor.subabilityIds.Add(sub.id);
                }
                foreach (var sup in data._superabilities)
                {
                    data.descriptor.superabilityIds.Add(sup.id);
                }

                Debug.Log(data.descriptor.ToString());
            }

            this.IsInitialized = true;
            return true;
        }
        return false;
    }

    public List<AbilityData.Descriptor> GetDescriptors()
    {
        List<AbilityData.Descriptor> result = new List<AbilityData.Descriptor>(this._content.Count);

        foreach (var data in this._content)
        {
            result.Add(data.descriptor);
        }

        return result;
    }
}

