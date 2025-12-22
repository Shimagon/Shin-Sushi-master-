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

        // 重複したプレイヤーを削除（DontDestroyOnLoadで残っている可能性）
        RemoveDuplicatePlayers();

        // プレイヤー（CameraRig等）を探して移動
        GameObject player = FindPlayer();

        if (player != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
            Debug.Log($"Player moved to Game Start Spawn Point: {playerSpawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("Player not found for Game Start Spawn!");
        }
    }

    /// <summary>
    /// プレイヤーオブジェクトを探す
    /// </summary>
    GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null) player = GameObject.Find("[CameraRig]");
        if (player == null) player = GameObject.Find("XR Origin");
        return player;
    }

    /// <summary>
    /// 重複したプレイヤーを削除（1つだけ残す）
    /// </summary>
    void RemoveDuplicatePlayers()
    {
        // すべてのプレイヤーを探す
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Tagで見つからない場合は名前で探す
        if (players.Length == 0)
        {
            GameObject player1 = GameObject.Find("Player");
            GameObject player2 = GameObject.Find("[CameraRig]");
            GameObject player3 = GameObject.Find("XR Origin");

            // 見つかったものをリストアップ
            System.Collections.Generic.List<GameObject> foundPlayers = new System.Collections.Generic.List<GameObject>();
            if (player1 != null) foundPlayers.Add(player1);
            if (player2 != null) foundPlayers.Add(player2);
            if (player3 != null) foundPlayers.Add(player3);

            players = foundPlayers.ToArray();
        }

        // 2つ以上ある場合、最初の1つ以外を削除
        if (players.Length > 1)
        {
            Debug.LogWarning($"Found {players.Length} players! Removing duplicates...");

            for (int i = 1; i < players.Length; i++)
            {
                Debug.Log($"Destroying duplicate player: {players[i].name}");
                Destroy(players[i]);
            }
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
