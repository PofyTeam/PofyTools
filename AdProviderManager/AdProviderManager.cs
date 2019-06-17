#if ADS
using Extensions;
using GoogleMobileAds.Api;
using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace PofyTools
{
    public delegate void AdResultDelegate(ShowResult result);

    public sealed class AdProviderManager : IInitializable
    {
        public const string TAG = "<b>AdProviderManager: </b>";
        public static AdResultDelegate onAdResult;
        public enum AdProvider : int
        {
            Google = 1 << 0,
            Unity = 1 << 1,
        }

        //Singleton
        private static AdProviderManager _instance;

        public AdProviderManager(AdProviderData data)
        {
            this._data = data;

            if (_instance == null)
            {
                _instance = this;
                Initialize();
            }
        }

        public static AdProviderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    //_instance = new AdProviderManager();
                    //_instance.Initialize();
                    Debug.LogError(TAG + "Instance not initialized!");
                }
                return _instance;
            }
        }

        #region IInitializable implementation

        private bool _isInitialized = false;

        private AdProviderData _data;

        public bool Initialize()
        {
            if (!this.IsInitialized)
            {
                //GOOGLE
                if(!string.IsNullOrEmpty(this._data.googleAppId))
                InitializeGoogleAds();

                //UNITY ADS
                if(!string.IsNullOrEmpty(this._data.unityAdsId))
                Advertisement.Initialize(this._data.unityAdsId, this._data.testMode);

                this._isInitialized = true;
                return true;
            }
            return false;
        }

        public bool IsInitialized => this._isInitialized;
        
        #endregion

        #region Common
        private AdProvider _readyRewardProviders;
        private AdProvider _readyInterstitialProviders;

        //private AdProvider _initializedProviders;

        private bool _hasCachedResult = false;
        ShowResult _cachedResult = ShowResult.Failed;

        public void Check()
        {
            //HACK: Check Cache from main thread
            if (this._hasCachedResult)
            {
                onAdResult?.Invoke(this._cachedResult);
                this._hasCachedResult = false;
            }
        }
        public static void AddRewardAdStartListener(VoidDelegate listenerToAdd)
        {
            Instance._rewardAdStarted += listenerToAdd;
        }

        public static void RemoveRewardAdStartListener(VoidDelegate listenerToRemove)
        {
            Instance._rewardAdStarted -= listenerToRemove;

        }

        public static void RemoveAllRewardAdStartListeners()
        {
            Instance._rewardAdStarted = null;
        }

        private VoidDelegate _rewardAdStarted = null;

        public static void ShowRewardAd()
        {
            Instance._rewardAdStarted?.Invoke();

            if (Instance._readyRewardProviders.HasFlag(AdProvider.Google) && Instance._readyRewardProviders.HasFlag(AdProvider.Unity))
            {
                if (Chance.FiftyFifty)
                    RewardBasedVideoAd.Instance.Show();
                else
                    Advertisement.Show("rewardedVideo", Instance._rewardOptions);

            }
            else if (Instance._readyRewardProviders.HasFlag(AdProvider.Google))
                RewardBasedVideoAd.Instance.Show();

            else if (Instance._readyRewardProviders.HasFlag(AdProvider.Unity))
                Advertisement.Show("rewardedVideo", Instance._rewardOptions);
        }

        public static void ShowInterstitial()
        {
            if (Instance._readyInterstitialProviders.HasFlag(AdProvider.Google))
                Instance._interstitialGoogle.Show();

            else if (Instance._readyInterstitialProviders.HasFlag(AdProvider.Unity))
                Advertisement.Show("video", Instance._defaultOptions);
        }

        public static bool HasRewardVideo
        {
            get
            {
                ////Unity
                if (Advertisement.IsReady())
                    Instance._readyRewardProviders = Instance._readyRewardProviders.Add(AdProvider.Unity);
                else
                    Instance._readyRewardProviders = Instance._readyRewardProviders.Remove(AdProvider.Unity);

                //Google
                if (RewardBasedVideoAd.Instance.IsLoaded())
                    Instance._readyRewardProviders = Instance._readyRewardProviders.Add(AdProvider.Google);
                else
                    Instance._readyRewardProviders = Instance._readyRewardProviders.Remove(AdProvider.Google);

                return Instance._readyRewardProviders != 0;
            }
        }

        public static bool HasInterstitial
        {
            get
            {
                if (Instance._interstitialGoogle.IsLoaded())
                    Instance._readyInterstitialProviders = Instance._readyInterstitialProviders.Add(AdProvider.Google);
                else
                    Instance._readyInterstitialProviders = Instance._readyInterstitialProviders.Remove(AdProvider.Google);

                if (Advertisement.IsReady())
                    Instance._readyInterstitialProviders = Instance._readyInterstitialProviders.Add(AdProvider.Unity);
                else
                    Instance._readyInterstitialProviders = Instance._readyInterstitialProviders.Remove(AdProvider.Unity);

                return Instance._readyInterstitialProviders != 0;
            }
        }

        private static void OnAdResult(ShowResult result)
        {
            Instance._cachedResult = result;
            Instance._hasCachedResult = true;
        }

        public void Destroy()
        {
            this._interstitialGoogle.Destroy();
            this._bannerGoogle.Destroy();
        }
        #endregion

        #region Unity


        private ShowOptions _rewardOptions = new ShowOptions
        {
            resultCallback = OnAdResult
        };

        private ShowOptions _defaultOptions = new ShowOptions();

        #endregion

        #region Google AdMob

        public const string GOOGLE_TEST_INTERSTITIAL = "ca-app-pub-3940256099942544/1033173712";
        public const string GOOGLE_TEST_BANNER = "ca-app-pub-3940256099942544/6300978111";
        public const string GOOGLE_TEST_REWARD_AD = "ca-app-pub-3940256099942544/5224354917";

        private void InitializeGoogleAds()
        {
            //GOOGLE AD SDK
#if UNITY_ANDROID
            string appId = this._data.googleAppId;
#else
            string appId = "unexpected_platform";
#endif
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(appId);

            //REWARD AD
            RewardBasedVideoAd.Instance.OnAdLoaded += HandleRewardBasedVideoLoaded;
            RewardBasedVideoAd.Instance.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            RewardBasedVideoAd.Instance.OnAdOpening += HandleRewardBasedVideoOpened;
            RewardBasedVideoAd.Instance.OnAdStarted += HandleRewardBasedVideoStarted;
            RewardBasedVideoAd.Instance.OnAdRewarded += HandleRewardBasedVideoRewarded;
            RewardBasedVideoAd.Instance.OnAdClosed += HandleRewardBasedVideoClosed;
            RewardBasedVideoAd.Instance.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

            RequestGoogleRewardAd();

            //Interstitial
#if UNITY_ANDROID
            string interstitialAdUnitId = (this._data.testMode) ? GOOGLE_TEST_INTERSTITIAL : this._data.googleInterstitialId;
#else
        string interstitialAdUnitId = "unexpected_platform";
#endif
            // Initialize an InterstitialAd.
            this._interstitialGoogle = new InterstitialAd(interstitialAdUnitId);

            this._interstitialGoogle.OnAdLoaded += HandleOnInterstitialAdLoaded;
            this._interstitialGoogle.OnAdFailedToLoad += HandleOnInterstitialAdFailedToLoad;
            this._interstitialGoogle.OnAdOpening += HandleInterstitialOnAdOpened;
            this._interstitialGoogle.OnAdClosed += HandleOnInterstitialAdClosed;
            this._interstitialGoogle.OnAdLeavingApplication += HandleOnInterstitialAdLeavingApplication;

            RequestInterstitial();

#if UNITY_ANDROID
            string bannerAdUnitId = (this._data.testMode) ? GOOGLE_TEST_BANNER : this._data.googleBannerId;
#else
            string bannerAdUnitId = "unexpected_platform";
#endif

            this._bannerGoogle = new BannerView(bannerAdUnitId, AdSize.SmartBanner, AdPosition.Top);

            this._bannerGoogle.OnAdLoaded += HandleOnBannerAdLoaded;
            this._bannerGoogle.OnAdFailedToLoad += HandleOnBannerAdFailedToLoad;
            this._bannerGoogle.OnAdOpening += HandleOnBannerAdOpened;
            this._bannerGoogle.OnAdClosed += HandleOnBannerAdClosed;
            this._bannerGoogle.OnAdLeavingApplication += HandleOnBannerAdLeavingApplication;

            RequestBanner();
        }
        #endregion

        private AdRequest BuildRequest()
        {
            var builder = new AdRequest.Builder();

            foreach(var testDevice in this._data.googleTestDeviceIds)
            {
                builder.AddTestDevice(testDevice);
            }

            return builder.Build();
        }

        #region Google Reward Ad
        private void RequestGoogleRewardAd()
        {
#if UNITY_ANDROID
            string rewardAdUnitId = (this._data.testMode) ? GOOGLE_TEST_REWARD_AD : this._data.googleRewardAdId;
#else
            string rewardAdUnitId = "unexpected_platform";
#endif

            RewardBasedVideoAd.Instance.LoadAd(BuildRequest(), rewardAdUnitId);
        }

        //LISTENERS
        private ShowResult _googleAdResult;
        public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
        {
            Debug.LogError(">>>>>>>>>> GOOGLE LOADED! <<<<<<<<<<<<<<<");
        }
        public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.LogWarning(
                "HandleRewardBasedVideoFailedToLoad event received with message: "
                                 + args.Message);
            this._googleAdResult = ShowResult.Failed;
        }
        public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
        {
            Debug.LogError(">>>>>>>>>> GOOGLE OPENED! <<<<<<<<<<<<<<<");
            this._googleAdResult = ShowResult.Failed;
        }
        public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
        {
            Debug.LogError(">>>>>>>>>> GOOGLE STARTED! <<<<<<<<<<<<<<<");
            this._googleAdResult = ShowResult.Skipped;
        }
        public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
        {
            Debug.LogError(">>>>>>>>>> GOOGLE CLOSED! <<<<<<<<<<<<<<<");
            //Reload Ad
            RequestGoogleRewardAd();
            OnAdResult(this._googleAdResult);
        }
        public void HandleRewardBasedVideoRewarded(object sender, Reward args)
        {
            Debug.LogError(">>>>>>>>>> GOOGLE REWARDED! <<<<<<<<<<<<<<<");
            this._googleAdResult = ShowResult.Finished;
        }
        public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
        {
            Debug.LogError(">>>>>>>>>> GOOGLE LEFT! <<<<<<<<<<<<<<<");
        }
        #endregion

        #region Google Interstitial

        InterstitialAd _interstitialGoogle = null;

        private void RequestInterstitial()
        {
            //AdRequest request = new AdRequest.Builder()
            //    .AddTestDevice(GOOGLE_TEST_DEVICE_ID_0)
            //    .Build();
            this._interstitialGoogle.LoadAd(BuildRequest());
        }

        public void HandleOnInterstitialAdLoaded(object sender, EventArgs args)
        {
        }
        public void HandleOnInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.LogWarning("HandleFailedToReceiveAd event received with message: "
                                + args.Message);
        }
        public void HandleInterstitialOnAdOpened(object sender, EventArgs args)
        {
            //MonoBehaviour.print("HandleAdOpened event received");
        }
        public void HandleOnInterstitialAdClosed(object sender, EventArgs args)
        {
            //MonoBehaviour.print("HandleAdClosed event received");
            RequestInterstitial();
        }
        public void HandleOnInterstitialAdLeavingApplication(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLeavingApplication event received");
        }

        #endregion

        #region Google Banner

        private BannerView _bannerGoogle;

        private void RequestBanner()
        {
            // Create an empty ad request.
            //AdRequest request = new AdRequest.Builder()
            //    .AddTestDevice(GOOGLE_TEST_DEVICE_ID_0)
            //    .Build();

            // Load the banner with the request.
            this._bannerGoogle.LoadAd(BuildRequest());
        }

        public void HandleOnBannerAdLoaded(object sender, EventArgs args)
        {
            //PofyTools.UI.NotificationView.Show("Banner Loaded!", null, -1f);
        }

        public void HandleOnBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.LogWarning("HandleFailedToReceiveAd event received with message: "
                                + args.Message);
        }

        public void HandleOnBannerAdOpened(object sender, EventArgs args)
        {
            //MonoBehaviour.print("HandleAdOpened event received");
        }

        public void HandleOnBannerAdClosed(object sender, EventArgs args)
        {
            //MonoBehaviour.print("HandleAdClosed event received");
            RequestBanner();
        }

        public void HandleOnBannerAdLeavingApplication(object sender, EventArgs args)
        {
            //MonoBehaviour.print("HandleAdLeavingApplication event received");
        }

        public static void ShowBanner()
        {
            if (Instance._bannerGoogle != null)
                Instance._bannerGoogle.Show();
        }

        public static void HideBanner()
        {
            if (Instance._bannerGoogle != null)
                Instance._bannerGoogle.Hide();
        }

        #endregion
    }

    //[System.Serializable]
    //public struct AdProviderManagetData
    //{
    //    public bool useUnityAds;

    //    public string unityAdsId;// = "1083748"; //FATSPACE

    //    public bool useGoogleAds;

    //    public string googleAppId;
    //    public string[] googleTestDeviceIds;
    //    public string googleInterstitialId;
    //    public string googleBannerId;
    //    public string googleRewardAdId;

    //    public bool testMode;

    //    public AdProviderManagetData(
    //                                    string UnityAdsId,
    //                                    string googleAppId = "",
    //                                    string googleBannerId = "",
    //                                    string googleInterstitialId = "",
    //                                    string googleRewardAdId = "",
    //                                    bool testMode = true,
    //                                    params string[] googleTestDeviceIds
    //                                   )
    //    {
    //        this.unityAdsId = UnityAdsId;
    //        this.googleAppId = googleAppId;
    //        this.googleBannerId = googleBannerId;
    //        this.googleInterstitialId = googleInterstitialId;
    //        this.googleRewardAdId = googleRewardAdId;
    //        this.testMode = testMode;
    //        this.googleTestDeviceIds = googleTestDeviceIds;

    //        this.useUnityAds = true;
    //        this.useGoogleAds = true;
    //    }
    //}
}


#endif