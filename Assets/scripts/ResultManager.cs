using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("UI References")]
    public Image rankImage;
    public RawImage replayImage; // ダイジェスト映写用
    public TMPro.TMP_Text scoreText; // 表示用テキスト

    [Header("Rank Sprites")]
    public Sprite rankS;
    public Sprite rankA;
    public Sprite rankB;
    public Sprite rankC;
    public Sprite rankD;
    public Sprite rankE;

    [Header("Replay Settings")]
    [Header("Replay Settings")]
    public float frameRate = 0.1f; // 10FPSで再生

    [Header("Scene Settings")]
    public string titleSceneName = "TitleScene";

    [Header("Spawn Settings")]
    public Transform spawnPoint; // プレイヤーの強制移動先

    [Header("Stats UI")]
    public TMPro.TMP_Text statsText; // 詳細統計表示用

    void Start()
    {
        // カーソルを表示
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MovePlayerAndReset();
        ShowResult();
        StartCoroutine(PlayDigest());
    }

    void MovePlayerAndReset()
    {
        // プレイヤー（CameraRig等）を探して移動
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // もしTagで見つからなければ "CameraRig" や "XR Origin" という名前で探す予備手段
        if (player == null) player = GameObject.Find("Player");
        if (player == null) player = GameObject.Find("[CameraRig]");

        if (player != null && spawnPoint != null)
        {
            // 位置を強制移動
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
            Debug.Log($"Player moved to Result Spawn Point: {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("Player or SpawnPoint not found!");
        }
    }

    void ShowResult()
    {
        if (ScoreManager.Instance == null) return;

        int score = ScoreManager.Instance.GetCurrentScore();
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        // 統計情報の表示
        if (statsText != null)
        {
            float avgTime = ScoreManager.Instance.servedCount > 0 
                ? ScoreManager.Instance.totalServiceTime / ScoreManager.Instance.servedCount 
                : 0f;

            statsText.text = $"提供数: {ScoreManager.Instance.servedCount}回\n" +
                             $"ミス数: {ScoreManager.Instance.wrongCount}回\n" +
                             $"提供失敗: {ScoreManager.Instance.missedCount}回\n" +
                             $"平均提供時間: {avgTime:F1}秒\n" +
                             $"クレーマー滞在: {ScoreManager.Instance.totalAngryTime:F1}秒";
        }

        // ランク判定
        Sprite resultRank = rankE;
        if (score >= ScoreManager.Instance.rankS) resultRank = rankS;
        else if (score >= ScoreManager.Instance.rankA) resultRank = rankA;
        else if (score >= ScoreManager.Instance.rankB) resultRank = rankB;
        else if (score >= ScoreManager.Instance.rankC) resultRank = rankC;
        else if (score >= 0) resultRank = rankD;

        if (rankImage != null)
        {
            rankImage.sprite = resultRank;
        }
    }

    IEnumerator PlayDigest()
    {
        if (ScoreManager.Instance == null || replayImage == null) yield break;

        List<Texture2D> frames = ScoreManager.Instance.replayFrames;
        Debug.Log($"Replay Frames Count: {frames.Count}"); // デバッグログ

        if (frames.Count == 0)
        {
            Debug.LogWarning("再生するフレームが0枚です！録画がされていないか、データが渡っていません。");
            yield break;
        }

        int index = 0;
        while (true)
        {
            if (frames[index] != null)
            {
                replayImage.texture = frames[index];
            }
            
            yield return new WaitForSeconds(frameRate);
            
            index++;
            if (index >= frames.Count) index = 0;
        }
    }

    public void OnTitleButtonClicked()
    {
        // スコアリセット
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
            ScoreManager.Instance.replayFrames.Clear(); // メモリ開放
        }
        SceneManager.LoadScene(titleSceneName);
    }
}
