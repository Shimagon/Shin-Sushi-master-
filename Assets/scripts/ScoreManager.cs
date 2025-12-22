using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使用

/// <summary>
/// スコア管理システム（シングルトン）
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Statistics")]
    public int servedCount = 0;
    public int wrongCount = 0;
    public int missedCount = 0;
    public float totalServiceTime = 0f;
    public float totalAngryTime = 0f;

    [Header("Score Settings")]
    [Tooltip("現在のスコア")]
    public int currentScore = 0;

    [Tooltip("ハイスコア")]
    public int highScore = 0;

    [Header("Score Logic")]
    [Tooltip("クレーマー1人あたりの毎秒減点数")]
    public float penaltyPerCustomerPerSecond = 10.0f; // 変更可能にする

    [Tooltip("一度に減点する単位（この値が溜まるまで減点処理・音再生を行わない）")]
    public float penaltyStep = 10.0f; // 10ポイント単位で減らす

    [Header("UI References")]
    [Tooltip("スコア表示用のテキスト")]
    public TMP_Text scoreText;

    [Tooltip("ハイスコア表示用のテキスト")]
    public TMP_Text highScoreText;

    [Header("Replay Data")]
    // リザルトシーンへ受け渡す画像のリスト
    public System.Collections.Generic.List<Texture2D> replayFrames = new System.Collections.Generic.List<Texture2D>();

    [Header("Rank Thresholds")]
    public int rankS = 5000;
    public int rankA = 3000;
    public int rankB = 1000;
    public int rankC = 500;
    public int rankD = 100; // デフォルト値
    public int rankE = 0;   // デフォルト値

    [Header("UI Feedback")]
    [Tooltip("スコア増加時の色")]
    public Color positiveColor = Color.green;
    [Tooltip("スコア減少時の色")]
    public Color negativeColor = Color.red;
    [Tooltip("通常時の色")]
    public Color defaultColor = Color.white;
    [Tooltip("色が戻るまでの時間")]
    public float colorResetTime = 0.5f;

    [Header("Effects")]
    [Tooltip("スコア増加時のエフェクト")]
    public GameObject positiveScoreEffect;

    [Tooltip("スコア減少時のエフェクト")]
    public GameObject negativeScoreEffect;

    [Tooltip("スコア獲得時の効果音")]
    public AudioClip scoreSound;

    [Tooltip("スコア減少時の効果音")]
    public AudioClip negativeScoreSound; // 新規追加

    [Tooltip("ハイスコア更新時の効果音")]
    public AudioClip highScoreSound;

    private AudioSource audioSource;
    private int activeAngryCustomerCount = 0;
    private float scoreAccumulator = 0f;
    private Coroutine colorCoroutine;

    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSourceを追加
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ハイスコアを読み込み
        LoadHighScore();
    }

    [Header("Recording Settings")]
    public float recordInterval = 0.5f; // 0.5秒ごとに撮影
    public int maxFrames = 50; // 最大枚数（約25秒分）
    private bool isRecording = false;

    void Start()
    {
        if (scoreText != null)
        {
            scoreText.color = defaultColor; // 初期色を設定
        }
        UpdateScoreUI();
        
        // 録画開始（シーン名チェックなどを入れても良いが、とりあえず開始）
        StartRecording();
    }

    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            replayFrames.Clear();
            StartCoroutine(RecordGameLoop());
        }
    }

    public void StopRecording()
    {
        isRecording = false;
        StopAllCoroutines(); // 録画などのコルーチンを止める
    }

    System.Collections.IEnumerator RecordGameLoop()
    {
        while (isRecording)
        {
            yield return new WaitForSeconds(recordInterval);
            yield return new WaitForEndOfFrame();

            // 画面キャプチャ（Texture2D作成）
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            
            // リストに追加
            replayFrames.Add(texture);

            // 枚数制限を超えたら古いものを削除
            if (replayFrames.Count > maxFrames)
            {
                Texture2D old = replayFrames[0];
                replayFrames.RemoveAt(0);
                Destroy(old); // メモリリーク防止
            }
        }
    }

    void Update()
    {
        // クレーマー滞在中の減点処理
        if (activeAngryCustomerCount > 0)
        {
            // 時間計測
            totalAngryTime += Time.deltaTime;

            // 1人あたり毎秒設定ポイント分減らす
            scoreAccumulator += Time.deltaTime * activeAngryCustomerCount * penaltyPerCustomerPerSecond;

            // ステップ値以上溜まったら減点（例：10ポイント溜まったら-10する）
            if (scoreAccumulator >= penaltyStep)
            {
                int step = Mathf.FloorToInt(penaltyStep);
                // もしpenaltyStepが1未満の設定なら1にするなどのガードがあってもいいが、
                // 今回は10なのでそのまま
                if (step < 1) step = 1; 

                AddScore(-step); // 減点実行
                scoreAccumulator -= step; // 減らした分を引く
            }
        }
    }

    // --- クレーマー管理 ---
    public void RegisterAngryCustomer()
    {
        activeAngryCustomerCount++;
        Debug.Log($"クレーマー来店。現在のクレーマー数: {activeAngryCustomerCount}");
    }

    public void UnregisterAngryCustomer()
    {
        if (activeAngryCustomerCount > 0)
        {
            activeAngryCustomerCount--;
            Debug.Log($"クレーマー撃退。現在のクレーマー数: {activeAngryCustomerCount}");
        }
    }

    // イベント定義
    public System.Action<int> OnScoreChange;

    /// <summary>
    /// スコアを追加（マイナスなら減点）
    /// </summary>
    public void AddScore(int points)
    {
        if (points == 0) return;

        currentScore += points;

        Debug.Log($"スコア変動: {points}点 (合計: {currentScore}点)");

        // ハイスコア更新チェック
        bool isNewHighScore = false;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            isNewHighScore = true;
            Debug.Log($"新しいハイスコア: {highScore}点！");
        }

        // イベント通知（録画トリガーなど）
        OnScoreChange?.Invoke(points);

        // UIを更新
        UpdateScoreUI();

        // 効果音と演出
        if (points > 0)
        {
            PlayScoreSound(isNewHighScore);
            PlayEffect(positiveScoreEffect);
            FlashColor(positiveColor); // 緑に光らせる
        }
        else
        {
            PlayNegativeSound(); // 減点音を再生
            PlayEffect(negativeScoreEffect);
            FlashColor(negativeColor); // 赤に光らせる
        }
    }

    private void PlayNegativeSound()
    {
        if (audioSource != null && negativeScoreSound != null)
        {
            audioSource.PlayOneShot(negativeScoreSound);
        }
    }

    private void FlashColor(Color targetColor)
    {
        if (scoreText == null) return;

        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(ResetColorCoroutine(targetColor));
    }

    private System.Collections.IEnumerator ResetColorCoroutine(Color targetColor)
    {
        scoreText.color = targetColor;
        yield return new WaitForSeconds(colorResetTime);
        scoreText.color = defaultColor;
    }

    /// <summary>
    /// エフェクト再生
    /// </summary>
    private void PlayEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null && scoreText != null)
        {
            // スコアテキストの近くなどにエフェクトを出す
            Instantiate(effectPrefab, scoreText.transform.position, Quaternion.identity, scoreText.transform.parent);
        }
    }

    /// <summary>
    /// スコアをリセット
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        scoreAccumulator = 0f;
        UpdateScoreUI();
        Debug.Log("スコアをリセットしました");
    }

    /// <summary>
    /// UIを更新
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"スコア: {currentScore}"; // 日本語に変更
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"ハイスコア: {highScore}"; // 日本語に変更
        }
    }

    /// <summary>
    /// スコア獲得音を再生
    /// </summary>
    private void PlayScoreSound(bool isHighScore)
    {
        if (audioSource == null) return;

        if (isHighScore && highScoreSound != null)
        {
            audioSource.PlayOneShot(highScoreSound);
        }
        else if (scoreSound != null)
        {
            audioSource.PlayOneShot(scoreSound);
        }
    }

    /// <summary>
    /// ハイスコアを保存
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ハイスコアを読み込み
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// 現在のスコアを取得
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// ハイスコアを取得
    /// </summary>
    public int GetHighScore()
    {
        return highScore;
    }
}
