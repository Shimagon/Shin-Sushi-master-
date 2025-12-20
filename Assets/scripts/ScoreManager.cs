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
    public float penaltyPerCustomerPerSecond = 1.0f; // 変更可能にする

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
    // D is below C
    // E is below 0 or specialized logic

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

    void Start()
    {
        if (scoreText != null)
        {
            scoreText.color = defaultColor; // 初期色を設定
        }
        UpdateScoreUI();
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

            // 1ポイント以上溜まったら減点
            if (scoreAccumulator >= 1.0f)
            {
                int reduceAmount = Mathf.FloorToInt(scoreAccumulator);
                AddScore(-reduceAmount); // 減点実行
                scoreAccumulator -= reduceAmount; // 減らした分を引く
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
