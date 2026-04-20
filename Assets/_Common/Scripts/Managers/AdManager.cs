using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections;

/* ======================================================================
 * [Class]: AdManager (Singleton)
 * [Role] : 구글 애드몹(Google Mobile Ads) SDK를 초기화하고, 배너 및 전면 광고의 로드/표시를 총괄 관리
 * * [Method Summary]
 * 1. ShowBanner          : 화면 하단에 반응형 배너 광고를 로드하여 표시 (로비 씬 전용)
 * 2. HideBanner          : 현재 표시 중인 배너 광고를 제거하고 메모리에서 해제 (게임 씬 진입 시)
 * 3. ScheduleInterstitial: 게임 오버 시 외부에서 호출. 0.1초 딜레이 후 광고 표시 로직을 실행
 * 4. LoadInterstitialAd  : 다음 표시할 전면 광고를 서버에서 미리 로드하여 대기
 * ====================================================================== */
public class AdManager : PersistentSingleton<AdManager>
{
    // 에디터와 실제 빌드의 광고 ID를 분리
#if UNITY_EDITOR
    // 유니티 에디터 테스트용 (구글 공용 테스트 ID)
    private string bannerID = "ca-app-pub-3940256099942544/6300978111";
    private string interstitialId = "ca-app-pub-3940256099942544/1033173712";
#else
    // 실제 안드로이드 빌드용 (발급받은 진짜 광고 단위 ID)
    private string bannerID = "ca-app-pub-5083599118880469/1835728053";
    private string interstitialId = "ca-app-pub-5083599118880469/3344250845";
#endif

    private BannerView bannerView;
    private InterstitialAd interstitialAd;

    private int gameOverCount = 0;
    private const int AdFrequency = 5; // 5번 게임 오버 시마다 광고 표시

    void Start()
    {
        // SDK 초기화 후 전면 광고 미리 로드
        MobileAds.Initialize(initStatus =>
        {
            LoadInterstitialAd(); 
        });
    }

    // ----------------------------------------------------------------------
    // [Banner(배너 광고)]
    // ----------------------------------------------------------------------
    public void ShowBanner()
    {
        if (bannerView != null)
        {
            return; // 이미 배너가 표시 중이면 무시
        }
        AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        bannerView = new BannerView(bannerID, adaptiveSize, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }


    // ----------------------------------------------------------------------
    // [Interstitial(전면 광고)]
    // ----------------------------------------------------------------------
    public void ScheduleInterstitial()
    {
        StartCoroutine(ShowInterstitialRoutine());
    }


    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        InterstitialAd.Load(interstitialId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null) return;
            interstitialAd = ad;
            interstitialAd.OnAdFullScreenContentClosed += LoadInterstitialAd; // 닫히면 다음 거 미리 로드
        });
    }

    private IEnumerator ShowInterstitialRoutine()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        gameOverCount++;
        Debug.Log($"쥬금 : {gameOverCount} / {AdFrequency}");

        if (gameOverCount >= AdFrequency)
        {
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
                gameOverCount = 0; // 카운트 리셋
            }
            else
            {
                Debug.Log("광고 아직 로드 안 됨");
                LoadInterstitialAd(); // 다음을 위해 다시 로드 시도
            }
        }
    }
    

}
