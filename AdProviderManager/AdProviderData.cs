namespace PofyTools
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Ad Provider Data", menuName = "PofyTools/Ad Provider Data")]
    public class AdProviderData : ScriptableObject
    {
        public bool useUnityAds;
        public string unityAdsId;

        public bool useGoogleAds;

        public string googleAppId;
        public string[] googleTestDeviceIds;
        public string googleInterstitialId;
        public string googleBannerId;
        public string googleRewardAdId;

        public bool testMode;

    }
}