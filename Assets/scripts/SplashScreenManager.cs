using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// スプラッシュスクリーン（ロゴ表示）管理
/// 企業ロゴとゲームタイトルを順番に表示してからタイトル画面へ
/// </summary>
public class SplashScreenManager : MonoBehaviour
{
    [System.Serializable]
    public class LogoData
    {
        [Tooltip("ロゴ画像")]
        public Sprite logoSprite;

        [Tooltip("表示時間（秒）")]
        public float displayDuration = 3f;

        [Tooltip("フェードイン時間（秒）")]
        public float fadeInDuration = 0.5f;

        [Tooltip("フェードアウト時間（秒）")]
        public float fadeOutDuration = 0.5f;

        [Tooltip("ロゴ表示時のサウンド")]
        public AudioClip logoSound;
    }

    [Header("Logo Settings")]
    [Tooltip("表示するロゴのリスト（順番に表示）")]
    public LogoData[] logos;

    [Header("UI References")]
    [Tooltip("ロゴを表示するUI Image")]
    public Image logoImage;

    [Tooltip("背景色（通常は黒）")]
    public Color backgroundColor = Color.black;

    [Header("Scene Settings")]
    [Tooltip("最後に遷移するシーン名（通常はタイトルシーン）")]
    public string nextSceneName = "TitleScene";

    [Header("Skip Settings")]
    [Tooltip("スキップ可能にする")]
    public bool canSkip = true;

    [Tooltip("スキップ用のキー")]
    public KeyCode skipKey = KeyCode.Space;

    [Header("VR Settings")]
    [Tooltip("カメラからの距離（VR用）")]
    public float distanceFromCamera = 3f;

    [Tooltip("Canvasの参照（自動でカメラ前に配置）")]
    public Canvas canvas;

    private AudioSource audioSource;
    private bool isPlaying = true;
    private Camera mainCamera;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // メインカメラを取得
        mainCamera = Camera.main;

        // カメラの背景を真っ白にする
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.white; // 真っ白
        }

        // CanvasをVR対応に設定
        SetupCanvasForVR();

        // 初期状態は透明
        if (logoImage != null)
        {
            Color c = logoImage.color;
            c.a = 0f;
            logoImage.color = c;
            logoImage.gameObject.SetActive(false);
        }

        // ロゴ表示シーケンス開始
        StartCoroutine(PlayLogoSequence());
    }

    /// <summary>
    /// CanvasをVR用に設定（カメラの前に配置）
    /// </summary>
    void SetupCanvasForVR()
    {
        if (canvas == null || mainCamera == null) return;

        // Canvas を World Space に設定
        canvas.renderMode = RenderMode.WorldSpace;

        // カメラの前に配置
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 canvasPosition = mainCamera.transform.position + cameraForward * distanceFromCamera;

        canvas.transform.position = canvasPosition;
        canvas.transform.rotation = Quaternion.LookRotation(cameraForward);

        // Canvas のスケールを調整（VRで見やすいサイズ）
        canvas.transform.localScale = Vector3.one * 0.002f; // 適切なサイズに調整

        Debug.Log($"Canvas positioned at {canvasPosition} for VR");
    }

    void Update()
    {
        // スキップ処理
        if (canSkip && isPlaying && Input.GetKeyDown(skipKey))
        {
            SkipToNextScene();
        }
    }


    /// <summary>
    /// ロゴ表示シーケンス
    /// </summary>
    IEnumerator PlayLogoSequence()
    {
        if (logos == null || logos.Length == 0)
        {
            Debug.LogWarning("ロゴが設定されていません。次のシーンへ移動します。");
            LoadNextScene();
            yield break;
        }

        foreach (LogoData logo in logos)
        {
            if (logo.logoSprite != null)
            {
                yield return StartCoroutine(ShowLogo(logo));
            }
        }

        // 全てのロゴ表示が終わったら次のシーンへ
        LoadNextScene();
    }

    /// <summary>
    /// 1つのロゴを表示
    /// </summary>
    IEnumerator ShowLogo(LogoData logo)
    {
        if (logoImage == null) yield break;

        // ロゴ画像を設定
        logoImage.sprite = logo.logoSprite;
        logoImage.gameObject.SetActive(true);

        // サウンド再生
        if (logo.logoSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(logo.logoSound);
        }

        // フェードイン
        yield return StartCoroutine(FadeImage(logoImage, 0f, 1f, logo.fadeInDuration));

        // 表示時間待機
        yield return new WaitForSeconds(logo.displayDuration);

        // フェードアウト
        yield return StartCoroutine(FadeImage(logoImage, 1f, 0f, logo.fadeOutDuration));

        // 非表示
        logoImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 画像をフェード
    /// </summary>
    IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
    }

    /// <summary>
    /// スキップして次のシーンへ
    /// </summary>
    void SkipToNextScene()
    {
        StopAllCoroutines();
        LoadNextScene();
    }

    /// <summary>
    /// 次のシーンをロード
    /// </summary>
    void LoadNextScene()
    {
        isPlaying = false;
        Debug.Log($"Loading next scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}
