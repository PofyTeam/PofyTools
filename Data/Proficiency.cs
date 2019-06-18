using System.Collections.Generic;
using UnityEngine;

namespace PofyTools
{
    [System.Serializable]
    public class ProficiencyDefinition : Definition
    {
        public ProficiencyDefinition(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Level represents key-value pair collection where key is current level (index + 1) and the value is required points for next level
        /// </summary>
        public int[] levels = new int[0];
    }

    //[System.Serializable]
    //public class ProficiencyLevel
    //{
    //    public int requiredPoints;
    //    public float value = 0f;
    //}

    [System.Serializable]
    public class ProficiencyData : DefinableData<ProficiencyDefinition>
    {
        #region Constructors
        public ProficiencyData() { }

        public ProficiencyData(ProficiencyDefinition definition) : base(definition)
        {
        }
        #endregion

        #region Serializable Data
        [SerializeField] private int _currentLevelIndex;
        public int CurrentLevelIndex { get { return this._currentLevelIndex; } }

        [SerializeField] public int _currentPointCount;
        public int CurrentPointCount => this._currentPointCount;
        #endregion

        #region API
        /// <summary>
        /// Formatted string for displaying current level (index + 1).
        /// </summary>
        public string DisplayLevel => (this.HasNextLevel) ? (this._currentLevelIndex + 1).ToString() : (this._currentLevelIndex + 1).ToString() + "(MAX)";

        /// <summary>
        /// Is next level of proficiency available
        /// </summary>
        public bool HasNextLevel => this.Definition.levels.Length - 1 > this._currentLevelIndex;

        /// <summary>
        /// Required points for next level of proficiency.
        /// </summary>
        public int NextLevelRequirements => this.HasNextLevel ? this.Definition.levels[this._currentLevelIndex + 1] : int.MaxValue;

        public bool AddPoints(int amount = 1)
        {
            this._currentPointCount += amount;
            Debug.Log(this.id + " current point count: " + this._currentPointCount + " current level: " + this.DisplayLevel);
            return false;
        }
        #endregion

        public void LevelUp()
        {
            this._currentLevelIndex++;

            //this.buff += this.CurrentLevel.value;
            Debug.Log("Level Up! " + this.id);
        }

        public void ApplyPoints()
        {
            while (this.HasNextLevel && this._currentPointCount >= this.NextLevelRequirements)
            {
                this._currentPointCount -= this.NextLevelRequirements;
                LevelUp();
            }
        }

    }

    [System.Serializable]
    public class ProficiencyDataSet : DefinableDataSet<ProficiencyData, ProficiencyDefinition>
    {
        #region Constructors
        public ProficiencyDataSet() { }
        public ProficiencyDataSet(DefinitionSet<ProficiencyDefinition> definitionSet) : base(definitionSet) { }
        public ProficiencyDataSet(List<ProficiencyDefinition> _content) : base(_content) { }
        #endregion

        #region API
        public void AddPoints(CategoryData.Descriptor descriptor, int amount = 1)
        {
            foreach (var superactegory in descriptor.supercategoryIds)
            {
                GetValue(superactegory).AddPoints(amount);
            }

            GetValue(descriptor.id).AddPoints(amount);
        }

        public void ApplyPoints()
        {
            foreach (var data in this._content)
            {
                data.ApplyPoints();
            }
        }

        public void CalculateBuffs(CategoryDataSet categoryDataSet)
        {
            //Calculate self buff for each data
            foreach (var data in this._content)
            {
                data.buff = 0f;
                data._inheritedBuff = 0f;

                for (int i = data.CurrentLevelIndex; i >= 0; --i)
                {
                    data.buff += data.Definition.levels[i].value;
                }
            }

            //add buffs to subcategories
            foreach (var data in this._content)
            {
                foreach (var subId in categoryDataSet.GetValue(data.id).descriptor.subcategoryIds)
                {
                    GetValue(subId)._inheritedBuff += data.buff;
                }
            }

        }

        #endregion
    }

    /// <summary>
    /// Generic proficiency reward solver.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ProficiencyRewardSolver<TKey, TValue>
    {
        //category id
        public string id;
        //reward key
        public TKey key;
        //reward for each proficiency level
        public TValue[] values;
    }
}