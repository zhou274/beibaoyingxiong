using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;
using TMPro;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    public GameObject 
        pauseMenu,
        gameOverMenu;

    public RectTransform playerHPBar;

    public Text
        coinText,
        distanceTraveledText;

    public TextMeshProUGUI scoreText, bestScoreText;

    public Image soundBtn;

    public Sprite
        soundOn,
        soundOff;

    public string clickid;
    private StarkAdManager starkAdManager;

    // Behaviour messages
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(this.gameObject);
        }
    }

    // Behaviour messages
    void Start()
    {
        if (PlayerPrefs.GetInt(Constants.SOUND_STATE, 1) == 1)
        {
            soundBtn.sprite = soundOn;
        }
        else
        {
            soundBtn.sprite = soundOff;
        }

    }

    public void UpdateCoin(float coinAmout)
    {
        coinText.text = coinAmout + "";
    }

    public void UpdateDistance(float distance)
    {
        distanceTraveledText.text = distance + "m";
    }

    public void UpdateScore(float distance)
    {
        PlayerPrefs.SetFloat("Score", distance);
        scoreText.text = "得分: " + distance;
        float best = PlayerPrefs.GetFloat(Constants.BEST_SCORE, 0);

        if (distance > best)
        {
            PlayerPrefs.SetFloat(Constants.BEST_SCORE, distance);
            bestScoreText.text = "最高: " + distance;
        }
        else
        {
            bestScoreText.text = "最高: " + best;
        }
    }

    public void UpdatePlayerHP(int amount)
    {
        playerHPBar.localScale = new Vector3(Mathf.Clamp01(playerHPBar.localScale.x + (amount / 100.0f)), 1.0f, 1.0f);
    }

    // Pause button is clicked
    public void PauseBtn_Onclick()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0.0f;

        SoundManager.Instance.PlaySound(Constants.CLICK_SOUND);
    }

    // Play button is clicked
    public void PlayBtn_Onlick()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;

        SoundManager.Instance.PlaySound(Constants.CLICK_SOUND);
    }

    // Restart button is clicked
    public void RestartBtn_Onclick()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Play");

        SoundManager.Instance.PlaySound(Constants.CLICK_SOUND);
    }
    public void ContinueBtn_Onclick()
    {
        ShowVideoAd("18i600gg8b9ag1ek2e",
            (bol) => {
                if (bol)
                {
                    Time.timeScale = 1.0f;
                    PlayerController.isContinue = true;
                    SceneManager.LoadScene("Play");
                    SoundManager.Instance.PlaySound(Constants.CLICK_SOUND);
                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
    }

    // Menu button is clicked
    public void MenuBtn_Onlick()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Menu");

        SoundManager.Instance.PlaySound(Constants.CLICK_SOUND);
    }
    
    public void GameOverShow()
    {

        ShowInterstitialAd("2d2ajifhd53c4flfj2",
           () => {
               Debug.Log("--插屏广告完成--");

           },
           (it, str) => {
               Debug.LogError("Error->" + str);
           });
        gameOverMenu.SetActive(true);
        Time.timeScale = 0.0f;
    }


    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }

    

    // Sound button is clicked
    public void SoundBtn_Onclick()
    {
        if (PlayerPrefs.GetInt(Constants.SOUND_STATE, 1) == 1)
        {
            PlayerPrefs.SetInt(Constants.SOUND_STATE, 0);
            soundBtn.sprite = soundOff;

            SoundManager.Instance.TurnOffSound();
        }
        else
        {
            PlayerPrefs.SetInt(Constants.SOUND_STATE, 1);
            soundBtn.sprite = soundOn;

            SoundManager.Instance.PlaySound(Constants.CLICK_SOUND);
            SoundManager.Instance.TurnOnSound();
        }
    }



    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }
}
