using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// タイトル画面の進行管理
/// チュートリアル画像の切り替えと、ゲームシーンへの遷移を行う
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [Tooltip("表示するチュートリアル画像のリスト（順番に表示）")]
    public List<Sprite> tutorialImages;

    [Tooltip("チュートリアル画像を表示するUI Image")]
    public Image displayImage;

    [Header("References")]
    [Tooltip("操作する寿司（ボタン）への参照")]
    public TutorialSushi tutorialSushi;

    [Header("Scene Settings")]
    [Tooltip("遷移先のメインゲームシーン名")]
    public string mainGameSceneName = "SampleScene";

    [Header("Spawn Settings")]
    [Tooltip("タイトル画面でのプレイヤースポーン位置")]
    public Transform playerSpawnPoint;

    private int currentIndex = 0; // 最初から1枚目を表示

    [Header("Audio")]
    [Tooltip("タイトル画面のBGM（ループ再生）")]
    public AudioClip titleBGM;
    [Tooltip("ゲーム開始時の効果音")]
    public AudioClip startGameSound;

    private AudioSource audioSource;

    private void Awake()
    {
        // AudioSourceコンポーネントを追加または取得
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        // プレイヤーをスポーン位置に移動
        MovePlayerToSpawn();

        // 初期状態のセットアップ
        UpdateTutorialState();

        // BGM再生
        if (titleBGM != null && audioSource != null)
        {
            audioSource.clip = titleBGM;
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.volume = 0.5f; // 適度な音量
            audioSource.Play();
        }
    }

    /// <summary>
    /// プレイヤーをスポーン位置に移動
    /// </summary>
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
            Debug.Log($"Player moved to Title Spawn Point: {playerSpawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("Player not found for Title Spawn!");
        }
    }

    [Tooltip("終了確認用の画像")]
    public Sprite quitConfirmationImage;

    private bool isConfirmingQuit = false;

    /// <summary>
    /// チュートリアルを次の段階に進める
    /// TutorialSushiから呼ばれる
    /// </summary>
    public void AdvanceTutorial()
    {
        // 終了確認中ならキャンセルして通常動作へ
        if (isConfirmingQuit)
        {
            isConfirmingQuit = false;
        }

        currentIndex++;

        if (currentIndex >= tutorialImages.Count)
        {
            // 全ての画像を表示し終わったらゲーム開始
            StartGame();
        }
        else
        {
            // 次の画像を表示
            UpdateTutorialState();
        }
    }

    /// <summary>
    /// チュートリアルを前の段階に戻す
    /// TutorialSushiから呼ばれる
    /// </summary>
    public void PreviousTutorial()
    {
        // 終了確認中ならキャンセルして元の画像に戻る
        if (isConfirmingQuit)
        {
            isConfirmingQuit = false;
            UpdateTutorialState();
            return;
        }

        currentIndex--;

        // 初期状態(0)より小さくならないようにする（画像が消えないようにする）
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        UpdateTutorialState();
    }

    /// <summary>
    /// 現在のインデックスに基づいて表示を更新
    /// </summary>
    private void UpdateTutorialState()
    {
        // 終了確認画面の表示
        if (isConfirmingQuit)
        {
            if (displayImage != null && quitConfirmationImage != null)
            {
                displayImage.sprite = quitConfirmationImage;
                displayImage.gameObject.SetActive(true);
            }
            if (tutorialSushi != null) tutorialSushi.UpdateText("本当に終了？");
            return;
        }

        // 通常画像の更新
        if (displayImage != null && tutorialImages != null && currentIndex >= 0 && currentIndex < tutorialImages.Count)
        {
            displayImage.sprite = tutorialImages[currentIndex];
            displayImage.gameObject.SetActive(true);
        }
        
        // 寿司のテキスト更新
        if (tutorialSushi != null)
        {
            if (currentIndex < tutorialImages.Count - 1)
            {
                tutorialSushi.UpdateText("次へ");
            }
            else
            {
                tutorialSushi.UpdateText("ゲーム開始");
            }
        }
    }

    /// <summary>
    /// ゲームシーンへ遷移
    /// </summary>
    private void StartGame()
    {
        // BGMを止める
        if (audioSource != null) audioSource.Stop();

        // 開始SEを再生
        if (startGameSound != null)
        {
            // AudioSource.PlayClipAtPoint だとシーン遷移で消える可能性があるため
            // 一時的なGameObjectを作成して再生するか、コルーチンで待つ
            StartCoroutine(PlaySoundAndLoadScene());
        }
        else
        {
            LoadGameScene();
        }
    }

    private System.Collections.IEnumerator PlaySoundAndLoadScene()
    {
        // SE再生
        AudioSource.PlayClipAtPoint(startGameSound, Camera.main.transform.position);

        // SEの長さ分だけ待つ（最大2秒程度に制限）
        float waitTime = Mathf.Min(startGameSound.length, 2.0f);
        yield return new WaitForSeconds(waitTime);

        LoadGameScene();
    }

    private void LoadGameScene()
    {
        Debug.Log("Game Start! Loading scene: " + mainGameSceneName);
        SceneManager.LoadScene(mainGameSceneName);
    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    public void ExitGame()
    {
        // まだ確認モードでなければ、確認モードにする
        if (!isConfirmingQuit)
        {
            isConfirmingQuit = true;
            UpdateTutorialState();
        }
        else
        {
            // 2回目なので本当に終了する
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
