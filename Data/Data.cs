namespace PofyTools
{
    using Extensions;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    [System.Serializable]
    public abstract class CollectionPair<TKey, TValue> : IContentProvider<List<TValue>>, IInitializable
    {
        [SerializeField]
        protected List<TValue> _content = new List<TValue>();
        public Dictionary<TKey, TValue> content = new Dictionary<TKey, TValue>();

        #region Constructors

        public CollectionPair()
        {
            this._content = new List<TValue>();
            this.content = new Dictionary<TKey, TValue>();
        }

        public CollectionPair(int capacity)
        {
            this._content = new List<TValue>(capacity: capacity);
            this.content = new Dictionary<TKey, TValue>(capacity: capacity);
        }

        public CollectionPair(IList<TValue> values)
        {
            this._content = new List<TValue>(collection: values);
            this.content = new Dictionary<TKey, TValue>(capacity: values.Count);
        }

        public CollectionPair(params TValue[] values)
        {
            this._content = new List<TValue>(collection: values);
            this.content = new Dictionary<TKey, TValue>(capacity: values.Length);
        }

        public CollectionPair(IDictionary<TKey, TValue> values)
        {
            this._content = new List<TValue>(collection: values.Values);
            this.content = new Dictionary<TKey, TValue>(values);
        }

        public CollectionPair(params KeyValuePair<TKey, TValue>[] values) : this(values.Length)
        {
            foreach (var pair in values)
            {
                this._content.Add(pair.Value);
                this.content.Add(pair.Key, pair.Value);
            }
        }

        #endregion

        #region IInitializable

        public virtual bool IsInitialized { get; protected set; }

        public virtual bool Initialize()
        {
            if (!this.IsInitialized)
            {
                if (this._content.Count != 0)
                {
                    if (BuildDictionary())
                    {
                        this.IsInitialized = true;
                        return true;
                    }
                    Debug.LogWarning("Failed to buid a dictionary. Aborting Collection Pair Initialization... " + typeof(TValue).ToString());
                    return false;
                }

                Debug.LogWarning("Content not available. Aborting Collection Pair Initialization... " + typeof(TValue).ToString());
                return false;
            }
            return false;
        }
        #endregion

        protected abstract bool BuildDictionary();

        #region IContentProvider

        public virtual void SetContent(List<TValue> content)
        {
            this._content = content;
        }

        public virtual List<TValue> GetContent()
        {
            return this._content;
        }

        public virtual void AddContent(TKey key, TValue value)
        {
            if (!this._content.Contains(value))
            {
                this._content.Add(value);

                if (!this.content.ContainsKey(key))
                    this.content[key] = value;
                else
                {
                    Debug.LogWarning(ToString() + ": Duplicate key! Aborting... " + key.ToString());
                }
            }
            else
            {
                Debug.LogWarning(ToString() + ": Duplicate value! Aborting... " + value.ToString());
            }
        }

        public virtual bool RemoveContent(TKey key)
        {
            TValue outValue = default(TValue);

            if (this.content.TryGetValue(key, out outValue))
            {
                this.content.Remove(key);
                this._content.Remove(outValue);

                return true;
            }
            return false;
        }

        public virtual bool RemoveContent(TValue value)
        {
            if (this._content.Remove(value))
            {
                TKey key = default(TKey);
                bool keyFound = false;

                foreach (var pair in this.content)
                {
                    if (pair.Value.Equals(value))
                    {
                        key = pair.Key;
                        keyFound = true;
                        break;
                    }
                }

                if (keyFound)
                {
                    this.content.Remove(key);
                }
                return true;
            }
            return false;
        }

        public void ClearContent()
        {
            this._content.Clear();
            this.content.Clear();

            this.IsInitialized = false;
        }
        #endregion

        #region API
        /// <summary>
        /// Gets content's element via key.
        /// </summary>
        /// <param name="key">Element's key.</param>
        /// <param name="runtime">Requires initialized set.</param>
        /// <returns>Content's element.</returns>
        public TValue GetValue(TKey key, bool runtime = true)
        {
            TValue result = default(TValue);

            if (!this.IsInitialized && runtime)
            {
                Debug.LogWarning("Data Set Not Initialized! " + typeof(TValue).ToString());
                return result;
            }

            if (!this.content.TryGetValue(key, out result))
                Debug.LogWarning("Value Not Found For Key: " + key);

            return result;
        }

        /// <summary>
        /// Gets random element from content.
        /// </summary>
        /// <returns>Random element</returns>
        public TValue GetRandom()
        {
            return this._content.GetRandom();
        }

        /// <summary>
        /// Gets random element from content or type's default value.
        /// </summary>
        /// <returns>Random element</returns>
        public TValue TryGetRandom()
        {
            return this._content.TryGetRandom();
        }

        /// <summary>
        /// Gets random element different from the last random pick.
        /// </summary>
        /// <param name="lastRandomIndex">Index of previously randomly obtained element.</param>
        /// <returns>Random element different from last random.</returns>
        public TValue GetNextRandom(ref int lastIndex)
        {
            return this._content.GetNextRandom(ref lastIndex);
        }

        #endregion

        #region IList
        /// <summary>
        /// Content's element count.
        /// </summary>
        public int Count
        {
            get { return this._content.Count; }
        }

        /// <summary>
        /// Get content from List via index.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public TValue this[int arg]
        {
            get
            {
                return this._content[arg];
            }
        }

        public void RemoveAt(int index)
        {
            var element = this._content[index];
            this._content.RemoveAt(index);

            foreach (var pair in this.content)
            {
                if (pair.Value.Equals(element))
                {
                    //TODO: Check if breaks iterator 
                    this.content.Remove(pair.Key);
                    break;
                }
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return this._content.GetEnumerator();
        }

        #endregion

        #region IDicitionary

        public List<TKey> GetKeys()
        {
            return new List<TKey>(this.content.Keys);
        }

        public void GetKeys(List<TKey> list)
        {
            list.Clear();
            list.AddRange(this.content.Keys);
        }

        public virtual bool ContainsKey(TKey key)
        {
            if (this.IsInitialized) return this.content.ContainsKey(key);
            return false;
        }

        public bool ContainsValue(TValue value)
        {
            return this.content.ContainsValue(value);
        }

        public bool TryGetValue(TKey key, out TValue outValue)
        {
            if (this.IsInitialized)return this.content.TryGetValue(key, out outValue);
            outValue = default;
            return default;
        }

        //public TValue this[TKey arg]
        //{
        //    get
        //    {
        //        return this.content[arg];
        //    }
        //}

        #endregion

        public override string ToString()
        {
            string result = "";
            foreach (var id in this._content)
            {
                result += id.ToString() + "\n";
            }
            return result;
        }
    }
    /// <summary>
    /// Collection of keyable values obtainable via key or index.
    /// </summary>
    /// <typeparam name="TKey"> Key Type.</typeparam>
    /// <typeparam name="TValue">Value Type.</typeparam>
    [System.Serializable]
    public abstract class DataSet<TKey, TValue> : CollectionPair<TKey, TValue> where TValue : IData<TKey>
    {
        #region CollectionPair
        protected override bool BuildDictionary()
        {
            if (this.content == null)
                this.content = new Dictionary<TKey, TValue>(this._content.Count);
            else
                this.content.Clear();

            //Add content from list to dictionary
            foreach (var element in this._content)
            {
                if (this.content.ContainsKey(element.Id))
                    Debug.LogWarning("Id \"" + element.Id + "\" present in the set. Overwriting...");
                this.content[element.Id] = element;
            }

            return true;
        }

        public virtual bool AddContent(TValue data)
        {
            if (this._content.AddOnce(data))
            {
                if (this.IsInitialized)
                    this.content[data.Id] = data;
                return true;
            }

            return false;
        }

        public override bool RemoveContent(TValue data)
        {
            if (this._content.Remove(data))
            {
                if (this.IsInitialized)
                    this.content[data.Id] = data;
                return true;
            }
            return false;
        }

        public override bool ContainsKey(TKey key)
        {
            return (this.IsInitialized) ? this.content.ContainsKey(key) : this._content.Exists(x => x.Id.Equals(key));
        }

        #endregion
    }

    public interface IData<T>
    {

        T Id { get; }
    }
    public abstract class Data<T> : IData<T>
    {
        public T id = default(T);
        public T Id => this.id;

        public static implicit operator T(Data<T> data)
        {
            return data.id;
        }
    }

    #region JSON Approach
    public abstract class Definition : Data<string>
    {
        //TODO: Implement fast hash search
        [System.NonSerialized]
        public int hash;

        public override string ToString()
        {
            return this.id;
        }
    }
    public class DefinableData<T> : Data<string>, IDefinable<T> where T : Definition
    {
        public DefinableData() { }
        public DefinableData(T definition)
        {
            Define(definition);
        }

        #region IDefinable

        public T Definition
        {
            get;
            protected set;
        }

        public bool IsDefined { get { return this.Definition != null; } }

        public void Define(T definition)
        {
            this.Definition = definition;
            this.id = this.Definition.id;
        }

        public void Undefine()
        {
            this.Definition = null;
            this.id = string.Empty;
        }

        #endregion
    }
    /// <summary>
    /// Collection of definitions obtainable via key or index
    /// </summary>
    /// <typeparam name="T">Definition Type</typeparam>
    [System.Serializable]
    public class DefinitionSet<T> : DataSet<string, T> where T : Definition
    {
        /// <summary>
        /// Definition set file path.
        /// </summary>
        protected string _path;
        protected string _filename;
        protected string _extension;

        protected bool _scrable;
        protected bool _encode;

        public string FullPath
        {
            get
            {
                return this._path + "/" + this._filename + "." + this._extension;
            }
        }

        public string ResourcePath
        {
            get
            {
                return this._path + "/" + this._filename;
            }
        }

        /// <summary>
        /// Definition Set via file path
        /// </summary>
        /// <param name="path">Definition set file path.</param>
        public DefinitionSet(string fullPath, string filename, bool scramble = false, bool encode = false, string extension = "")
        {
            this._path = fullPath;
            this._filename = filename;
            this._extension = extension;

            this._encode = encode;
            this._scrable = scramble;
        }

        #region IInitializable implementation

        public override bool Initialize()
        {
            Load();
            return base.Initialize();
        }

        #endregion

        #region Instance Methods
        public virtual void Save()
        {
            SaveDefinitionSet(this);
        }

        public virtual void Load()
        {
            LoadDefinitionSet(this);
            this.IsInitialized = false;
        }

        #endregion

        #region IO

        public static void LoadDefinitionSet(DefinitionSet<T> definitionSet)
        {
            //DataUtility.LoadOverwrite(definitionSet.FullPath, definitionSet, definitionSet._scrable, definitionSet._encode);
            DataUtility.ResourceLoad(definitionSet.ResourcePath, definitionSet, definitionSet._scrable, definitionSet._encode);

        }

        public static void SaveDefinitionSet(DefinitionSet<T> definitionSet)
        {
            //DataUtility.Save(definitionSet._path, definitionSet._filename, definitionSet, definitionSet._scrable, definitionSet._encode, definitionSet._extension);
            DataUtility.ResourceSave(definitionSet._path, definitionSet._filename, definitionSet, definitionSet._scrable, definitionSet._encode, definitionSet._extension);
        }

        #endregion
    }

    /// <summary>
    /// Collection of loaded data.
    /// </summary>
    /// <typeparam name="TData">Data Type</typeparam>
    /// <typeparam name="TDefinition">Definition Type</typeparam>
    [System.Serializable]
    public class DefinableDataSet<TData, TDefinition> : DataSet<string, TData> where TDefinition : Definition where TData : DefinableData<TDefinition>, new()
    {
        public DefinableDataSet() { }

        public DefinableDataSet(DefinitionSet<TDefinition> definitionSet)
        {
            Initialize(definitionSet.GetContent());
        }

        public DefinableDataSet(List<TDefinition> _content)
        {
            Initialize(_content);
        }

        public bool Initialize(List<TDefinition> _content)
        {
            foreach (var element in _content)
            {
                TData data = new TData();
                data.Define(element);

                this._content.Add(data);
            }

            return Initialize();
        }

        public void DefineSet(DefinitionSet<TDefinition> definitionSet)
        {
            foreach (var data in this._content)
            {
                data.Define(definitionSet.GetValue(data.id));
            }
        }

    }
    public interface IDefinable<T> where T : Definition
    {
        T Definition
        {
            get;
        }

        bool IsDefined { get; }

        void Define(T definition);

        void Undefine();
    }
    #endregion JSON Approach

    #region ScriptableObject Approach
    public abstract class Config : ScriptableObject, IData<string>
    {
        [System.NonSerialized]
        public int hash;

        //public string id;

        public string Id => this.name;

        public override string ToString()
        {
            return this.name;
        }
    }

    public class ConfigSet<T> : DataSet<string, T> where T : Config
    {
        public ConfigSet(T[] content)
        {
            this._content = new List<T>(content);
        }
    }

    public class ConfigurableData<T> : Data<string>, IConfigurable<T> where T : Config
    {
        public ConfigurableData() { }
        public ConfigurableData(T config)
        {
            Configure(config);
        }

        #region IConfigurable

        public T Config
        {
            get;
            protected set;
        }

        public bool IsConfigured { get { return this.Config != null; } }

        public void Configure(T definition)
        {
            this.Config = definition;
            this.id = this.Config.Id;
        }

        public void Unconfigure()
        {
            this.Config = null;
            this.id = string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// Collection of loaded data.
    /// </summary>
    /// <typeparam name="TData">Data Type</typeparam>
    /// <typeparam name="TConfig">Definition Type</typeparam>
    [System.Serializable]
    public class ConfigurableDataSet<TData, TConfig> : DataSet<string, TData> where TConfig : Config where TData : ConfigurableData<TConfig>, new()
    {
        public ConfigurableDataSet() { }

        public ConfigurableDataSet(ConfigSet<TConfig> configSet)
        {
            Initialize(configSet.GetContent());
        }

        public ConfigurableDataSet(List<TConfig> _content)
        {
            Initialize(_content);
        }

        public bool Initialize(List<TConfig> _content)
        {
            foreach (var element in _content)
            {
                AddData(element);
            }

            return Initialize();
        }

        public void ConfigureSet(ConfigSet<TConfig> configSet, bool extend = false)
        {
            //foreach (var data in this._content)
            //{
            //    data.Configure(configSet.GetValue(data.id));
            //}
            TData data = null;
            foreach (var config in configSet)
            {
                if (this.TryGetValue(config.Id, out data))
                {
                    data.Configure(config);
                }
                else
                {
                    AddData(config);
                }
            }
        }

        public bool AddData(TConfig config)
        {
            TData data = new TData();
            data.Configure(config);
            AddContent(data);
            return true;
        }

    }

    public interface IConfigurable<T> where T : Config
    {
        T Config { get; }

        bool IsConfigured { get; }

        void Configure(T config);

        void Unconfigure();
    }

    #endregion ScriptableObject Approach



    public interface IDatable<TKey, TValue> where TValue : Data<TKey>
    {
        TValue Data { get; }

        void AppendData(TValue data);

        void ReleaseData();
    }

    public interface IContentProvider<T>
    {
        void SetContent(T content);

        T GetContent();

    }

    public static class DataUtility
    {
        public const string TAG = "<color=yellow><b>DataUtility: </b></color>";

        #region LOAD
        public enum LoadResult : int
        {
            NullObject = -3,
            NullPath = -2,
            FileNotFound = -1,
            Done = 0
        }

        public static LoadResult LoadOverwrite(string fullPath, object objectToOverwrite, bool unscramble = false, bool decode = false)
        {
            if (objectToOverwrite == null)
            {
                Debug.LogWarningFormat("{0}Object to overwrite is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                return LoadResult.NullObject;
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return LoadResult.NullPath;
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogWarningFormat("{0}File \"{1}\" not found! Aborting...", TAG, fullPath);
                return LoadResult.FileNotFound;
            }

            var json = File.ReadAllText(fullPath);

            json = (unscramble) ? DataUtility.UnScramble(json) : json;
            json = (decode) ? DataUtility.DecodeFrom64(json) : json;

            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            Debug.LogFormat("{0}File \"{1}\" loaded successfully!", TAG, fullPath);
            return LoadResult.Done;
        }

        public static LoadResult ResourceLoad(string relativePath, object objectToOverwrite, bool unscramble = false, bool decode = false)
        {
            if (objectToOverwrite == null)
            {
                Debug.LogWarningFormat("{0}Object to overwrite is NULL! Aborting... (\"{1}\")", TAG, relativePath);
                return LoadResult.NullObject;
            }

            var textAsset = Resources.Load<TextAsset>(relativePath);

            if (textAsset == null)
            {
                Debug.LogWarningFormat("{0}File \"{1}\" not found! Aborting...", TAG, relativePath);
                return LoadResult.FileNotFound;
            }

            string json = textAsset.text;

            json = (unscramble) ? DataUtility.UnScramble(json) : json;
            json = (decode) ? DataUtility.DecodeFrom64(json) : json;

            JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            Debug.LogFormat("{0}File \"{1}\" loaded successfully!", TAG, relativePath);
            return LoadResult.Done;
        }

        //TODO: T Load

        #endregion

        #region SAVE
        [System.Flags]
        public enum SaveResult : int
        {
            Done = 1 << 0,
            NullObject = 1 << 1,
            NullPath = 1 << 2,
            DirectoryCreated = 1 << 3,
        }

        public static SaveResult Save(string fullPath, string filename, object objectToSave, bool scramble = false, bool encode = false, string extension = "")
        {
            SaveResult result = 0;

            //Check input
            if (objectToSave == null)
            {
                Debug.LogWarningFormat("{0}Object you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                result = result.Add(SaveResult.NullObject);
                return result;
            }

            //Check Path
            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                result = result.Add(SaveResult.NullPath);
                return result;
            }

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                result = result.Add(SaveResult.DirectoryCreated);
            }

            var json = JsonUtility.ToJson(objectToSave);

            json = (encode) ? DataUtility.EncodeTo64(json) : json;
            json = (scramble) ? DataUtility.Scramble(json) : json;

            File.WriteAllText(fullPath + "/" + filename + ((string.IsNullOrEmpty(extension)) ? "" : ("." + extension)), json);

            result = result.Add(SaveResult.Done);
            Debug.LogFormat("{0}File \"{1}\" saved successfully!", TAG, fullPath + "/" + filename + "." + extension);
            return result;
        }

        public static SaveResult ResourceSave(string relativePath, string filename, object objectToSave, bool scramble = false, bool encode = false, string extension = "")
        {
            string fullPath = Application.dataPath + "/Resources/" + relativePath;
            return Save(fullPath, filename, objectToSave, scramble, encode, extension);

        }

        public static SaveResult PanelSave(string fullPathNameExtension, object objectToSave)
        {
            var json = JsonUtility.ToJson(objectToSave);

            //json = (encode) ? DataUtility.EncodeTo64(json) : json;
            //json = (scramble) ? DataUtility.Scramble(json) : json;

            File.WriteAllText(fullPathNameExtension, json);

            var result = SaveResult.Done;
            Debug.LogFormat("{0}File \"{1}\" saved successfully!", TAG, fullPathNameExtension);
            return result;
        }

        #endregion

        #region SCRAMBLE

        static string Scramble(string toScramble)
        {
            StringBuilder toScrambleSB = new StringBuilder(toScramble);
            StringBuilder scrambleAddition = new StringBuilder(toScramble.Substring(0, toScramble.Length / 2 + 1));
            for (int i = 0, j = 0; i < toScrambleSB.Length; i = i + 2, ++j)
            {
                scrambleAddition[j] = toScrambleSB[i];
                toScrambleSB[i] = 'c';
            }

            StringBuilder finalString = new StringBuilder();
            int totalLength = toScrambleSB.Length;
            string length = totalLength.ToString();
            finalString.Append(length);
            finalString.Append("!");
            finalString.Append(toScrambleSB.ToString());
            finalString.Append(scrambleAddition.ToString());

            return finalString.ToString();
        }

        static string UnScramble(string scrambled)
        {
            int indexOfLenghtMarker = scrambled.IndexOf("!");
            string strLength = scrambled.Substring(0, indexOfLenghtMarker);
            int lengthOfRealData = int.Parse(strLength);
            StringBuilder toUnscramble = new StringBuilder(scrambled.Substring(indexOfLenghtMarker + 1, lengthOfRealData));
            string substitution = scrambled.Substring(indexOfLenghtMarker + 1 + lengthOfRealData);
            for (int i = 0, j = 0; i < toUnscramble.Length; i = i + 2, ++j)
                toUnscramble[i] = substitution[j];

            return toUnscramble.ToString();
        }

        #endregion

        #region ENCODE

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
            return returnValue;
        }

        #endregion

        #region Textures

        public static void IncrementSaveToPNG(string filePath, string fileName, Texture2D texture)
        {
            int count = 0;

            if (texture == null)
            {
                Debug.LogWarningFormat("{0}Texture you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fileName);
                return;
            }

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return;
            }

            if (filePath[filePath.Length - 1] != '/' && fileName[0] != '/')
            {
                filePath += "/";
            }

            while (File.Exists(filePath + fileName + count + ".png"))
            {
                count++;
            }

            SaveToPNG(filePath + fileName + count + ".png", texture);
        }

        public static void SaveToPNG(string fullPath, Texture2D texture)
        {
            if (texture == null)
            {
                Debug.LogWarningFormat("{0}Texture you are trying to save is NULL! Aborting... (\"{1}\")", TAG, fullPath);
                return;
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogWarningFormat("{0}Invalid path! Aborting...", TAG);
                return;
            }

            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        }

        #endregion

        #region Utilities

        public static List<string> OptimizeStringList(List<string> toOptimize)
        {
            toOptimize.Sort();
            for (int i = toOptimize.Count - 1; i >= 0; --i)
            {
                toOptimize[i] = toOptimize[i].Trim().ToLower();
                if (i < toOptimize.Count - 1)
                {
                    var left = toOptimize[i];
                    var right = toOptimize[i + 1];
                    if (left == right)
                    {
                        toOptimize.RemoveAt(i);
                    }
                }
            }
            return toOptimize;
        }

        public static string OptimizeString(string toOptimize)
        {
            return toOptimize.Trim().ToLower();
        }

        public static void OptimizeDefinitions<T>(List<T> definitions) where T : Definition
        {
            foreach (var definition in definitions)
            {
                definition.id = OptimizeString(definition.id);
            }

            definitions.Sort((x, y) => x.id.CompareTo(y.id));
        }

        #endregion
    }

    /// <summary>
    /// String Float Pair.
    /// </summary>
    [System.Serializable]
    public struct StringValue
    {
        [SerializeField]
        private string _key;
        public string Key
        {
            get { return this._key; }
        }
        public float value;

        public StringValue(string key, float value)
        {
            this._key = key;
            this.value = value;
        }

        public StringValue(string key)
        {
            this._key = key;
            this.value = 0f;
        }

        #region Implicit Casts

        public static implicit operator float(StringValue stringValue)
        {
            return stringValue.value;
        }

        public static implicit operator string(StringValue stringValue)
        {
            return stringValue._key;
        }

        #endregion
    }

    /// <summary>
    /// String Int Pair.
    /// </summary>
    [System.Serializable]
    public struct StringAmount
    {
        [SerializeField]
        private string _key;
        public string Key
        {
            get { return this._key; }
        }
        public int amount;

        public StringAmount(string key, int amount)
        {
            this._key = key;
            this.amount = amount;
        }

        public StringAmount(string key)
        {
            this._key = key;
            this.amount = 0;
        }

        #region Implicit Casts

        public static implicit operator int(StringAmount stringAmount)
        {
            return stringAmount.amount;
        }

        public static implicit operator string(StringAmount stringAmount)
        {
            return stringAmount._key;
        }

        #endregion
    }


}