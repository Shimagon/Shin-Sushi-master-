using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("制限時間（秒）")]
    public float gameDuration = 120f; // 2分

    [Tooltip("リザルトシーンの名前")]
    public string resultSceneName = "ResultScene";

    [Header("UI References")]
    [Tooltip("残り時間を表示するテキスト (TimerText)")]
    public TMP_Text mainTimerText;

    [Header("Audio")]
    public AudioClip timeUpSound;

    private float currentTimer;
    private bool isGameActive = true;

    [Header("Spawn Settings")]
    public Transform playerSpawnPoint; // ゲーム開始時のプレイヤースポーン位置

    void Start()
    {
        currentTimer = gameDuration;

        // プレイヤー移動処理
        MovePlayerToSpawn();
    }

    void MovePlayerToSpawn()
    {
        if (playerSpawnPoint == null) return;

        // プレイヤー（CameraRig等）を探して移動
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null) player = GameObject.Find("[CameraRig]");
        if (player == null) player = GameObject.Find("XR Origin");

        if (player != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
            Debug.Log($"Player moved to Game Start Spawn Point: {playerSpawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("Player found for Game Start Spawn!");
        }
    }

    void Update()
    {
        if (!isGameActive) return;

        currentTimer -= Time.deltaTime;

        // UI更新
        if (mainTimerText != null)
        {
            int min = Mathf.FloorToInt(currentTimer / 60);
            int sec = Mathf.FloorToInt(currentTimer % 60);
            mainTimerText.text = $"{min:00}:{sec:00}";
        }

        // タイムアップ判定
        if (currentTimer <= 0)
        {
            currentTimer = 0;
            FinishGame();
        }
    }

    void FinishGame()
    {
        isGameActive = false;
        Debug.Log("タイムアップ！終了！");

        // 最終スコア保存などはScoreManagerが随時やってるのでOK
        
        // 録画停止（もしあれば）
        HighlightRecorder recorder = FindObjectOfType<HighlightRecorder>();
        if (recorder != null)
        {
            recorder.StopRecording();
        }

        // タイムアップ音
        if (timeUpSound != null)
        {
            AudioSource.PlayClipAtPoint(timeUpSound, transform.position);
        }

        // 少し待ってからリザルトへ（余韻）
        StartCoroutine(GoToResultDelay());
    }

    System.Collections.IEnumerator GoToResultDelay()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(resultSceneName);
    }
}
