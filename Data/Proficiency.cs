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

        public List<ProficiencyLevel> levels = new List<ProficiencyLevel>();
    }

    [System.Serializable]
    public class ProficiencyLevel
    {
        public int requiredPoints;
        public float value = 0f;
    }

    [System.Serializable]
    public class ProficiencyData : DefinableData<ProficiencyDefinition>
    {
        public ProficiencyData() { }

        public ProficiencyData(ProficiencyDefinition definition) : base(definition)
        {
        }

        [SerializeField]
        private int _currentLevelIndex;

        public int CurrentLevelIndex { get { return this._currentLevelIndex; } }

        public int currentPointCount;

        [System.NonSerialized]
        public float buff = 0f;
        [System.NonSerialized]
        public float _inheritedBuff = 0f;

        public float TotalBuff { get { return buff + _inheritedBuff; } }

        #region API

        public string DisplayLevel
        {
            get { return (HasNextLevel) ? (_currentLevelIndex + 1).ToString() : (_currentLevelIndex + 1).ToString() + "(MAX)"; }
        }

        /// <summary>
        /// Current Proficiency Level data
        /// </summary>
        public ProficiencyLevel CurrentLevel
        {
            get { return this.Definition.levels[_currentLevelIndex]; }
        }

        /// <summary>
        /// Is next level of proficiency available
        /// </summary>
        public bool HasNextLevel
        {
            get { return this.Definition.levels.Count - 1 > this._currentLevelIndex; }
        }

        /// <summary>
        /// Required points for next level of proficiency.
        /// </summary>
        public int NextLevelRequirements
        {
            get { return this.Definition.levels[this._currentLevelIndex + 1].requiredPoints; }
        }

        public bool AddPoints(int amount = 1)
        {
            this.currentPointCount += amount;
            Debug.Log(this.id + " current point count: " + this.currentPointCount + " current level: " + this.DisplayLevel);
            return false;
        }
        #endregion

        public void AddLevel()
        {
            this._currentLevelIndex++;

            this.buff += this.CurrentLevel.value;
            Debug.Log("Level Up! " + this.id);
        }

        public void ApplyPoints()
        {
            while (this.HasNextLevel && this.currentPointCount >= this.NextLevelRequirements)
            {
                this.currentPointCount -= this.NextLevelRequirements;
                AddLevel();
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

}