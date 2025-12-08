using UnityEngine;
using Valve.VR.InteractionSystem;
using TMPro; // TextMeshProを使う場合

/// <summary>
/// タイトル画面の操作用寿司
/// 触れる/掴むと TitleManager に通知を送る
/// </summary>
[RequireComponent(typeof(Interactable))]
public class TutorialSushi : MonoBehaviour
{
    public enum ActionType
    {
        Next,   // 進む/進化
        Back,   // 戻る/退化
        Quit    // ゲーム終了
    }

    [Header("Settings")]
    [Tooltip("この寿司の機能を選択")]
    public ActionType actionType = ActionType.Next;

    [Header("Audio")]
    [Tooltip("掴んだ/押したときの効果音")]
    public AudioClip grabSound;

    [Header("References")]
    [Tooltip("TitleManagerへの参照")]
    public TitleManager titleManager;

    [Tooltip("表示テキスト（3D Text または TextMeshPro）")]
    public TextMesh textMesh; // 標準の3D Textの場合
    public TextMeshPro textMeshPro; // TextMeshProの場合

    private Interactable interactable;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    void Start()
    {
        // 初期テキスト設定
        UpdateTextBasedOnAction();
    }

    void Update()
    {
        if (interactable != null && interactable.hoveringHand != null)
        {
            Hand hand = interactable.hoveringHand;
            GrabTypes startingGrabType = hand.GetGrabStarting();

            // 「掴んだら（トリガー）」反応するように戻す
            if (startingGrabType != GrabTypes.None)
            {
                OnInteract();
            }
        }
    }

    // OnTriggerEnterは削除（触れるだけの判定はやめる）

    /// <summary>
    /// インタラクション時の処理
    /// </summary>
    private void OnInteract()
    {
        // 効果音再生
        if (grabSound != null)
        {
            AudioSource.PlayClipAtPoint(grabSound, transform.position);
        }

        if (titleManager != null)
        {
            switch (actionType)
            {
                case ActionType.Next:
                    titleManager.AdvanceTutorial();
                    break;
                case ActionType.Back:
                    titleManager.PreviousTutorial();
                    break;
                case ActionType.Quit:
                    titleManager.ExitGame();
                    break;
            }
            
            // フィードバック（音やエフェクト）があればここで再生
        }
    }

    private void UpdateTextBasedOnAction()
    {
        // 静的なテキスト設定（動的にTitleManagerから変えられない場合用）
        switch (actionType)
        {
            case ActionType.Next:
                UpdateText("次へ");
                break;
            case ActionType.Back:
                UpdateText("戻る");
                break;
            case ActionType.Quit:
                UpdateText("終了");
                break;
        }
    }

    /// <summary>
    /// 表示テキストを更新する
    /// </summary>
    public void UpdateText(string newText)
    {
        if (textMesh != null)
        {
            textMesh.text = newText;
        }
        
        if (textMeshPro != null)
        {
            textMeshPro.text = newText;
        }
    }
}
