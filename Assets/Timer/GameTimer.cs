using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float timeLimit = 120f;
    public TextMeshProUGUI timerText;
    public string resultSceneName = "PCResult";

    [Header("BGM Settings")]
    public AudioClip mainBgm;
    public AudioClip hurryUpBgm;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f; // 音量調整用
    private AudioSource bgmSource;
    private bool isHurryUpMode = false;


    float currentTime;
    bool isFinished;
    int lastShownSec = -1;

    void Start()
    {
        currentTime = timeLimit;

        // BGM初期化
        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume; // 音量を適用

        if (mainBgm != null)
        {
            bgmSource.clip = mainBgm;
            bgmSource.Play();
        }

        if (timerText != null) timerText.text = ""; 
        UpdateTimerUI(force: true);
    }

    void Update()
    {
        if (isFinished) return;

        currentTime -= Time.deltaTime;

        // 残り30秒でBGM切り替え
        if (!isHurryUpMode && currentTime <= 30f && currentTime > 0f)
        {
            isHurryUpMode = true;
            if (hurryUpBgm != null && bgmSource != null)
            {
                StartCoroutine(SwitchBGM(hurryUpBgm));
            }
        }

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerUI(force: true);

            isFinished = true;
            SceneManager.LoadScene(resultSceneName);
            return;
        }

        // 秒が変わったときだけ更新
        int sec = Mathf.CeilToInt(currentTime);
        if (sec != lastShownSec)
        {
            lastShownSec = sec;
            UpdateTimerUI(force: true);
        }
    }

    void UpdateTimerUI(bool force)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // BGMを滑らかに切り替えるコルーチン
    IEnumerator SwitchBGM(AudioClip newClip)
    {
        float fadeTime = 2.0f; // 2秒かけて切り替え
        float startVolume = bgmSource.volume;

        // フェードアウト
        for (float t = 0; t < fadeTime / 2; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / (fadeTime / 2));
            yield return null;
        }
        bgmSource.volume = 0f;
        bgmSource.Stop();

        // クリップ変更
        bgmSource.clip = newClip;
        bgmSource.Play();

        // フェードイン
        for (float t = 0; t < fadeTime / 2; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / (fadeTime / 2));
            yield return null;
        }
        bgmSource.volume = startVolume;
    }
}
