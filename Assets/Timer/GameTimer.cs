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



    float currentTime;
    bool isFinished;
    int lastShownSec = -1;

    void Start()
    {
        currentTime = timeLimit;

        if (timerText != null) timerText.text = ""; 
        UpdateTimerUI(force: true);
    }

    void Update()
    {
        if (isFinished) return;

        currentTime -= Time.deltaTime;
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
}
