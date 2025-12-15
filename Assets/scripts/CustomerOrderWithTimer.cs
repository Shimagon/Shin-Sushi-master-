using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerOrderWithTimer : MonoBehaviour
{
    [Header("注文設定")]
    [Tooltip("ランダムに選択する寿司名リスト（SushiType.sushiTypeName と一致させる必要があります）")]
    public string[] possibleSushiTypes = { "Maguro", "Tamago", "Salmon" };

    [Tooltip("1回の注文ごとの制限時間（秒）")]
    public float timeLimit = 45f;

    [Tooltip("正解または時間切れ後、次の注文を出すまでの遅延時間（秒）")]
    public float nextOrderDelay = 1.0f;

    // 現在の注文状態
    private string currentRequestedSushi;
    private float remainingTime;
    private bool isOrderActive = false;

    [Header("UI（頭上のキャンバス）")]
    [Tooltip("注文UIをまとめたワールドスペースキャンバス")]
    public GameObject orderCanvas;

    [Tooltip("注文内容（寿司名）を表示するテキスト")]
    public TMP_Text orderText;      // テキスト表示を併用したいので追加

    [Tooltip("寿司アイコンを表示するImage UI")]
    public Image orderImage;

    [Tooltip("残り時間を表示するテキスト（任意）")]
    public TMP_Text timerText;

    [Header("寿司アイコン画像")]
    public Sprite maguroSprite;
    public Sprite tamagoSprite;
    public Sprite salmonSprite;

    [Header("注文ボイス")]
    public AudioClip maguroSound;
    public AudioClip tamagoSound;
    public AudioClip salmonSound;

    [Header("リアクション（任意）")]
    public GameObject correctEffect;
    public GameObject wrongEffect;
    public AudioClip[] correctSounds;
    public AudioClip[] wrongSounds;
    public AudioClip[] timeoutSounds; // 時間切れ用の音リスト
    public Animator animator;
    public string correctTrigger = "Happy";
    public string wrongTrigger = "Sad";

    void Start()
    {
        // 最初は注文UIを非表示
        if (orderCanvas != null)
            orderCanvas.SetActive(false);

        // ※変更点: ゲーム開始時には注文せず、席についてから ActivateOrder() で開始する
    }

    /// <summary>
    /// 注文を開始する（CustomerSittingから呼ばれる）
    /// </summary>
    public void ActivateOrder()
    {
        if (!isOrderActive) // すでに始まっていなければ開始
        {
            StartNewOrder();
        }
    }

    void Update()
    {
        if (!isOrderActive) return;

        // 残り時間を減少させる
        remainingTime -= Time.deltaTime;

        // タイムアウト処理
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            OnTimeout();
        }

        // 残り時間UIの更新
        UpdateTimerUI();
        // 残り時間UIの更新
        UpdateTimerUI();
    }

    // =======================
    // 🔗 外部アクセス用 (SushiThrowableから呼ぶ)
    // =======================
    
    /// <summary>
    /// その寿司を欲しがっているか判定
    /// </summary>
    public bool WantsSushi(string sushiType)
    {
        if (!isOrderActive) return false;
        return currentRequestedSushi == sushiType;
    }

    /// <summary>
    /// 寿司を受け取る（SushiThrowableから呼ばれる）
    /// </summary>
    public void ReceiveSushi(string sushiType, bool isCorrect)
    {
        if (!isOrderActive) return;

        if (isCorrect)
        {
            OnReceiveCorrectSushi(null); // オブジェクト参照は必須ではないのでnull
        }
        else
        {
            OnReceiveWrongSushi(null);
        }
    }

    // =======================
    // 🔁 新しい注文を生成
    // =======================
    void StartNewOrder()
    {
        if (possibleSushiTypes == null || possibleSushiTypes.Length == 0)
        {
            Debug.LogWarning("[CustomerOrderWithTimer] possibleSushiTypes が空です。");
            return;
        }

        // 寿司をランダムに選ぶ
        int rand = Random.Range(0, possibleSushiTypes.Length);
        currentRequestedSushi = possibleSushiTypes[rand];

        // 制限時間リセット
        remainingTime = timeLimit;
        isOrderActive = true;

        // UI を表示
        if (orderCanvas != null)
            orderCanvas.SetActive(true);

        // 🔹テキストに寿司名を表示（英語のまま）
        if (orderText != null)
            orderText.text = currentRequestedSushi;

        // 🔹画像切り替え
        UpdateOrderImage();

        UpdateTimerUI();

        UpdateTimerUI();

        // 🔹注文ボイス再生
        PlayOrderSound(currentRequestedSushi);

        Debug.Log($"[CustomerOrderWithTimer] 新しい注文: {currentRequestedSushi}（制限時間: {timeLimit} 秒）");
    }

    // =======================
    // 🔊 注文ボイス再生
    // =======================
    void PlayOrderSound(string sushiType)
    {
        AudioClip clip = null;
        switch (sushiType)
        {
            case "Maguro":
                clip = maguroSound;
                break;
            case "Tamago":
                clip = tamagoSound;
                break;
            case "Salmon":
                clip = salmonSound;
                break;
        }

        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }

    // =======================
    // 🖼 寿司アイコンの変更
    // =======================
    void UpdateOrderImage()
    {
        if (orderImage == null) return;

        Sprite sprite = null;

        switch (currentRequestedSushi)
        {
            case "Maguro":
                sprite = maguroSprite;
                break;
            case "Tamago":
                sprite = tamagoSprite;
                break;
            case "Salmon":
                sprite = salmonSprite;
                break;
            default:
                sprite = null;
                break;
        }

        orderImage.sprite = sprite;
        orderImage.enabled = (sprite != null);

        // 画像があるならテキストは非表示にして邪魔にならないようにする
        if (orderText != null)
        {
            orderText.enabled = (sprite == null);
        }
    }

    // =======================
    // ⏱ 残り時間UIの更新
    // =======================
    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int sec = Mathf.Max(0, Mathf.CeilToInt(remainingTime));
        timerText.text = $"{sec}s";
    }

    // =======================
    // 🎯 寿司との衝突判定
    // =======================
    private void OnTriggerEnter(Collider other)
    {
        if (!isOrderActive) return;

        // 衝突したオブジェクトが寿司か確認
        SushiType sushi = other.GetComponent<SushiType>();
        if (sushi == null) return;

        string sushiTypeName = sushi.GetSushiType();   // チームメンバーのスクリプトを使用

        // 正解かどうか判定
        if (sushiTypeName == currentRequestedSushi)
        {
            OnReceiveCorrectSushi(other.gameObject);
        }
        else
        {
            OnReceiveWrongSushi(other.gameObject);
        }
    }

    // =======================
    // ✅ 正しい寿司を受け取ったとき
    // =======================
    void OnReceiveCorrectSushi(GameObject sushiObj)
    {
        Debug.Log($"[CustomerOrderWithTimer] 正しい寿司が届きました: {currentRequestedSushi}");

        isOrderActive = false;

        // 注文UIを非表示
        if (orderCanvas != null)
            orderCanvas.SetActive(false);

        // エフェクト
        if (correctEffect != null)
        {
            var fx = Instantiate(correctEffect, transform.position + Vector3.up * 2f, Quaternion.identity);
            Destroy(fx, 2f);
        }

        // 効果音（ランダム再生）
        if (correctSounds != null && correctSounds.Length > 0)
        {
            var clip = correctSounds[Random.Range(0, correctSounds.Length)];
            if(clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        // アニメーション
        if (animator != null && !string.IsNullOrEmpty(correctTrigger))
            animator.SetTrigger(correctTrigger);

        // 必要なら寿司オブジェクトを削除
        // Destroy(sushiObj);

        StartCoroutine(StartNextOrderAfterDelay());
    }

    // =======================
    // ❌ 間違った寿司を受け取ったとき
    // =======================
    void OnReceiveWrongSushi(GameObject sushiObj)
    {
        string sushiName = sushiObj != null ? sushiObj.name : "Unknown";
        Debug.Log($"[CustomerOrderWithTimer] 間違った寿司です（要求: {currentRequestedSushi} / 受取: {sushiName}）");

        // 間違いの場合は注文を維持するか、すぐ次の注文に切り替えるか好みで調整可能
        // 今は「間違っても注文を続ける」仕様

        // エフェクト
        if (wrongEffect != null)
        {
            var fx = Instantiate(wrongEffect, transform.position + Vector3.up * 2f, Quaternion.identity);
            Destroy(fx, 2f);
        }

        // 効果音
        if (wrongSounds != null && wrongSounds.Length > 0)
        {
            var clip = wrongSounds[Random.Range(0, wrongSounds.Length)];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        // アニメーション
        if (animator != null && !string.IsNullOrEmpty(wrongTrigger))
            animator.SetTrigger(wrongTrigger);

        // 必要なら間違った寿司を削除
        // Destroy(sushiObj);
    }

    // =======================
    // ⏰ 時間切れ処理
    // =======================
    void OnTimeout()
    {
        if (!isOrderActive) return;

        Debug.Log($"[CustomerOrderWithTimer] 注文時間切れ（要求: {currentRequestedSushi}）");

        isOrderActive = false;

        if (orderCanvas != null)
            orderCanvas.SetActive(false);

        // 時間切れでも「間違い」と同じリアクション
        if (wrongEffect != null)
        {
            var fx = Instantiate(wrongEffect, transform.position + Vector3.up * 2f, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (timeoutSounds != null && timeoutSounds.Length > 0)
        {
            var clip = timeoutSounds[Random.Range(0, timeoutSounds.Length)];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        // 時間切れは「がっかり」アニメーション（wrongTrigger）を流用、もし分けたければ変数追加可能
        if (animator != null && !string.IsNullOrEmpty(wrongTrigger))
            animator.SetTrigger(wrongTrigger);

        StartCoroutine(StartNextOrderAfterDelay());
    }

    // =======================
    // 🔄 次の注文へ進む
    // =======================
    IEnumerator StartNextOrderAfterDelay()
    {
        yield return new WaitForSeconds(nextOrderDelay);
        StartNewOrder();
    }
}
