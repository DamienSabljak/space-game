using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GoogleMobileAds.Api;


public class GoogleAds : MonoBehaviour
{
    private BannerView bannerView;
    public bool enableAds = false;
    private string testAd_banner = "ca-app-pub-3940256099942544/6300978111";//test ad
    private string banner_mainMenu = "ca-app-pub-3518865317160753~4767955145";//real ad -ONLY USE ON LAUNCH
    //or this one?
    //ca-app-pub-3518865317160753/7010975107


    // Start is called before the first frame update
    void Start()
    {
        if(enableAds)
        {
            // Initialize the Google Mobile Ads SDK.
            Debug.Log("~~~~~~~ ADS ENABLED, INITIALIZING ~~~~~~~");
            MobileAds.Initialize(initStatus => { });
            this.RequestBanner();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RequestBanner()
    {
        #if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/6300978111";//this is the testAdBanner
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
        #else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the bottom of the screen.
        Debug.Log("creating banner ad");
        this.bannerView = new BannerView(testAd_banner, AdSize.Banner, AdPosition.Bottom);
    }

}
