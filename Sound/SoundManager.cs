
namespace PofyTools
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    //    [RequireComponent(typeof(AudioListener))]
    public class SoundManager : MonoBehaviour, IDictionary<string, AudioClip>
    {
        public const string TAG = "<color=red><b><i>SoundManager: </i></b></color>";
        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundManager>();
                    if (_instance == null)
                        Debug.LogError(TAG + "No instance in the scene!");
                    else
                        _instance.Initialize();
                }

                return _instance;
            }
            private set { _instance = value; }
        }

        [Header("Sounds")]
        public AudioClip[] clips;

        public int voices = 1;
        private int _head = 0;
        public Range volumeVariationRange = new Range(0.9f, 1), pitchVariationRange = new Range(0.95f, 1.05f);

        public AudioListener audioListener { get; private set; }

        private List<AudioSource> _sources;

        [Header("Music")]
        public AudioClip music;

        public bool crossMixMusic;
        public float crossMixDuration = 0.2f;

        public bool duckMusicOnSound;
        public float duckOnSoundTransitionDuration = 0.1f, duckOnSoundVolume = 0.2f;

        private AudioSource _musicSource
        {
            get
            {
                return this._musicSources[this._musicHead];
            }
        }

        private AudioSource[] _musicSources;
        private int _musicHead = -1;

        [Range(0, 1)] public float musicVolume = 1;

        [Header("Master")]
        [Range(0, 1)]
        public float masterVolume = 1;

        [Header("Resources")]
        public string resourcePath = "Sound";
        public bool loadFromResources = true;
        private Dictionary<string, AudioClip> _dictionary;

        [Header("AudioListener")]
        public bool attachAudioListener;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                Initialize();
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        void Initialize()
        {
            if (this.attachAudioListener)
                this.audioListener = this.gameObject.AddComponent<AudioListener>();

            this._musicSources = new AudioSource[2];
            this._musicSources[0] = this.gameObject.AddComponent<AudioSource>();
            this._musicSources[1] = this.gameObject.AddComponent<AudioSource>();
            this._musicHead = 0;

            if (this.loadFromResources)
                LoadResourceSounds();
            LoadPrefabSounds();

            DontDestroyOnLoad(this.gameObject);
        }

        void LoadResourceSounds()
        {
            AudioClip[] resourceClips = Resources.LoadAll<AudioClip>(this.resourcePath);

            this._dictionary = new Dictionary<string, AudioClip>(resourceClips.Length + this.clips.Length);

            foreach (var clip in resourceClips)
            {
                this[clip.name] = clip;
            }
        }

        void LoadPrefabSounds()
        {

            if (this.music != null)
            {
                this._musicSource.clip = this.music;

                this._musicSource.loop = true;
                this._musicSource.volume = this.musicVolume * this.masterVolume;
            }

            if (this._dictionary == null)
                this._dictionary = new Dictionary<string, AudioClip>(this.clips.Length);

            this._sources = new List<AudioSource>(voices);
            for (int i = 0; i < this.voices; ++i)
            {
                this._sources.Add(this.gameObject.AddComponent<AudioSource>());
            }

            for (int i = this.clips.Length - 1; i >= 0; --i)
            {
                this._dictionary[this.clips[i].name] = this.clips[i];
            }
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                ResumeAll();
            }
            else
            {
                PauseAll();
            }
        }

        #region Play

        public static AudioSource Play(string clip, float volume = 1f, float pitch = 1f, bool loop = false, bool lowPriority = false)
        {
            AudioClip audioClip = null;
            if (Instance.TryGetValue(clip, out audioClip))
            {
                return PlayOnAvailableSource(audioClip, volume, pitch, loop, lowPriority);
            }
            Debug.LogWarning(TAG + "Sound not found - " + clip);
            return null;
        }

        public static AudioSource Play(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false, bool lowPriority = false)
        {
            return PlayOnAvailableSource(clip, volume, pitch, loop, lowPriority);
        }

        //plays a clip with pitch/volume variation
        public static AudioSource PlayVariation(string clip, bool loop = false, bool lowPriority = true)
        {
            return Play(clip, Instance.volumeVariationRange.Random, Instance.pitchVariationRange.Random, loop, lowPriority);
        }

        //plays a clip with pitch/volume variation
        public static AudioSource PlayVariation(AudioClip clip, bool loop = false, bool lowPriority = false)
        {
            return Play(clip, Instance.volumeVariationRange.Random, Instance.pitchVariationRange.Random, loop, lowPriority);
        }

        public static AudioSource PlayRandomFrom(params string[] clips)
        {
            return PlayVariation(clips[Random.Range(0, clips.Length)]);
        }

        public static AudioSource PlayRandomFrom(List<string> list)
        {
            return PlayVariation(list[Random.Range(0, list.Count)]);
        }

        public static AudioSource PlayRandomCustom(params AudioClip[] clips)
        {
            return PlayVariation(clips[Random.Range(0, clips.Length)]);
        }

        public static void PlayMusic()
        {
            Instance._musicSource.Play();
        }

        public static bool IsMusicPlaying()
        {
            return Instance._musicSource.isPlaying;
        }

        public static void PlayCustomMusic(AudioClip newMusic)
        {
            //set up the other music source
            var source = Instance._musicSources[1 - Instance._musicHead];

            source.clip = newMusic;
            source.loop = true;


            if (Instance.crossMixMusic)
            {
                source.volume = 0;
                source.Play();
                Instance.CrossMix(Instance.crossMixDuration);
            }
            else
            {
                if (Instance._musicSource.isPlaying)
                    Instance._musicSource.Stop();

                source.volume = Instance.masterVolume * Instance.musicVolume;
                Instance._musicHead = 1 - Instance._musicHead;
                Instance._musicSource.Play();
            }


        }

        //Plays clip that is not in manager's dictionary
        private static AudioSource PlayOnAvailableSource(AudioClip clip, float volume = 1, float pitch = 1, bool loop = false, bool lowPriority = false)
        {
            AudioSource source = Instance._sources[Instance._head];
            int startHeadPosition = Instance._head;

            while (source.isPlaying)
            {
                Instance._head++;
                if (Instance._head == Instance._sources.Count)
                {
                    Instance._head = 0;
                }
                source = Instance._sources[Instance._head];

                if (Instance._head == startHeadPosition)
                {
                    if (lowPriority)
                    {
                        return null;
                    }

                    while (source.loop)
                    {
                        Instance._head++;
                        if (Instance._head == Instance._sources.Count)
                        {
                            Instance._head = 0;
                        }
                        source = Instance._sources[Instance._head];
                        Debug.Log(Instance._head);
                        if (Instance._head == startHeadPosition)
                        {
                            break;
                        }
                    }
                    break;
                }
            }

            source.clip = clip;
            source.volume = volume * Instance.masterVolume;
            source.pitch = pitch;
            source.loop = loop;

            source.Play();

            if (Instance.duckMusicOnSound)
                DuckMusicOnSound(clip);
            return source;
        }

        #endregion

        #region Mute

        public static void MuteAll()
        {
            MuteSound(true);
            MuteMusic(true);
        }

        public static void UnMuteAll()
        {
            MuteSound(false);
            MuteMusic(false);
        }

        public static void MuteSound(bool mute = true)
        {
            for (int i = 0, Controller_sourcesCount = Instance._sources.Count; i < Controller_sourcesCount; i++)
            {
                var source = Instance._sources[i];
                source.mute = mute;
            }
        }

        public static void MuteMusic(bool mute = true)
        {
            Instance._musicSource.mute = mute;
        }

        public static void PauseAll()
        {

            PauseMusic();
            PauseSound();
        }

        public static void PauseMusic()
        {
            Instance._musicSource.Pause();
        }

        public static void PauseSound()
        {
            for (int i = 0, Controller_sourcesCount = Instance._sources.Count; i < Controller_sourcesCount; i++)
            {
                var source = Instance._sources[i];
                source.Pause();
            }
        }

        public static void ResumeAll()
        {
            ResumeMusic();
            ResumeSound();
        }

        public static void ResumeMusic()
        {
            Instance._musicSource.UnPause();
        }

        public static void ResumeSound()
        {
            for (int i = 0, Controller_sourcesCount = Instance._sources.Count; i < Controller_sourcesCount; i++)
            {
                var source = Instance._sources[i];
                source.UnPause();
            }
        }

        public static void StopAll()
        {
            Instance._musicSource.Stop();

            for (int i = 0, Controller_sourcesCount = Instance._sources.Count; i < Controller_sourcesCount; i++)
            {
                var source = Instance._sources[i];
                source.Stop();
                source.loop = false;
            }
        }

        #endregion

        #region Ducking

        //Music Ducking
        private float _musicDuckingVolume;
        private float _musicDuckingTimer;
        private float _musicDuckingDuration;

        //Sound Ducking
        private float _soundDuckingVolume;
        private float _soundDuckingTimer;
        private float _soundDuckingDuration;


        public static bool IsMusicDucked
        {
            get { return !(Instance._musicSource.volume > Instance._musicDuckingVolume); }
        }

        public static void FadeIn(float duration)
        {
            DuckAll(1, duration);
        }

        public static void FadeOut(float duration)
        {
            DuckAll(0, duration);
        }

        public static void DuckAll(float duckToVolume = 1f, float duckingDuration = 0.5f)
        {
            DuckMusic(duckToVolume, duckingDuration);
            DuckSound(duckToVolume, duckingDuration);
        }

        public static void DuckMusic(float duckToVolume = 0f, float duckingDuration = 0.5f, bool onSound = false)
        {
            Instance.StopCoroutine(Instance.DuckMusicState());

            Instance._musicDuckingVolume = duckToVolume * Instance.musicVolume * Instance.masterVolume;
            Instance._musicDuckingDuration = duckingDuration;
            Instance._musicDuckingTimer = duckingDuration;

            if (!onSound)
                Instance.StartCoroutine(Instance.DuckMusicState());
            else
                Instance.StartCoroutine(Instance.DuckMusicOnSound());
        }

        public static void DuckSound(float duckToVolume = 0f, float duckingDuration = 0.5f)
        {
            Instance.StopCoroutine(Instance.DuckSoundState());

            Instance._soundDuckingVolume = duckToVolume * Instance.masterVolume;
            Instance._soundDuckingDuration = duckingDuration;
            Instance._soundDuckingTimer = duckingDuration;

            Instance.StartCoroutine(Instance.DuckSoundState());
        }

        IEnumerator DuckMusicOnSound()
        {
            yield return DuckMusicState();
            yield return new WaitForSeconds(Mathf.Max(this._duckOnSoundDuration - this.duckOnSoundTransitionDuration, 0));
            DuckMusic(1);
        }

        IEnumerator DuckMusicState()
        {
            while (this._musicDuckingTimer > 0)
            {
                this._musicDuckingTimer -= Time.unscaledDeltaTime;
                if (this._musicDuckingTimer < 0)
                    this._musicDuckingTimer = 0;

                float normalizedTime = 1 - this._musicDuckingTimer / this._musicDuckingDuration;
                this._musicSource.volume = Mathf.Lerp(this._musicSource.volume, this._musicDuckingVolume, normalizedTime);
                yield return null;
            }
            //            SoundManager.IsMusicDucked = this._musicSource.volume
            //Restore on sound end
        }

        private float _duckOnSoundDuration = 0;

        private static void DuckMusicOnSound(AudioClip sound)
        {
            Instance.StopCoroutine(Instance.DuckMusicState());
            //Debug.Log(sound.length);

            Instance._duckOnSoundDuration = sound.length;

            DuckMusic(Instance.duckOnSoundVolume, Instance.duckOnSoundTransitionDuration, true);
        }

        IEnumerator DuckSoundState()
        {
            while (this._soundDuckingTimer > 0)
            {
                this._soundDuckingTimer -= Time.unscaledDeltaTime;
                if (this._soundDuckingTimer < 0)
                    this._soundDuckingTimer = 0;

                float normalizedTime = 1 - this._soundDuckingTimer / this._soundDuckingDuration;
                foreach (var source in this._sources)
                {
                    source.volume = Mathf.Lerp(source.volume, this._soundDuckingVolume, normalizedTime);
                }
                yield return null;
            }
        }

        #endregion

        #region Cross-Mixing

        private float _crossMixDuration, _crossMixTimer;
        private AudioSource _currentMusicSource, _targetMusicSource;

        private void CrossMix(float duration)
        {
            StopCoroutine(this.CrossMix());

            this._crossMixDuration = duration;
            this._crossMixTimer = duration;

            this._currentMusicSource = this._musicSources[this._musicHead];
            this._targetMusicSource = this._musicSources[1 - this._musicHead];
            this._musicHead = 1 - this._musicHead;

            StartCoroutine(this.CrossMix());
        }

        private IEnumerator CrossMix()
        {
            while (this._crossMixTimer > 0)
            {
                this._crossMixTimer -= Time.unscaledDeltaTime;

                if (this._crossMixTimer < 0)
                    this._crossMixTimer = 0;

                float normalizedTime = 1 - this._crossMixTimer / this._crossMixDuration;

                this._currentMusicSource.volume = (1 - normalizedTime) * this.masterVolume * this.musicVolume;
                this._targetMusicSource.volume = normalizedTime * this.masterVolume * this.musicVolume;

                yield return null;
            }
        }

        #endregion

        #region IDictionary implementation

        public bool ContainsKey(string key)
        {
            return this._dictionary.ContainsKey(key);
        }

        public void Add(string key, AudioClip value)
        {
            this._dictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return this._dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out AudioClip value)
        {
            return this._dictionary.TryGetValue(key, out value);
        }

        public AudioClip this[string index]
        {
            get
            {
                return this._dictionary[index];
            }
            set
            {
                this._dictionary[index] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return this._dictionary.Keys;
            }
        }

        public ICollection<AudioClip> Values
        {
            get
            {
                return this._dictionary.Values;
            }
        }

        #endregion

        #region ICollection implementation

        public void Add(KeyValuePair<string, AudioClip> item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            this._dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, AudioClip> item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, AudioClip>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, AudioClip> item)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get
            {
                return this._dictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<string, AudioClip>> GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        #endregion
    }
}