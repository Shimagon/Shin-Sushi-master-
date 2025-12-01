using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// 米釜（お釜）- 掴んだら米を左手に生成する
/// </summary>
[RequireComponent(typeof(Interactable))]
public class RiceContainer : MonoBehaviour
{
    [Header("Rice Settings")]
    [Tooltip("生成する米のPrefab")]
    public GameObject ricePrefab;

    [Tooltip("米の生成位置オフセット（手からの相対位置）")]
    public Vector3 riceSpawnOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Audio")]
    [Tooltip("米を掴んだときの効果音")]
    public AudioClip riceGrabSound;

    [Header("Settings")]
    [Tooltip("連続生成のクールダウン時間（秒）")]
    public float cooldownTime = 0.5f;

    private Interactable interactable;
    private float lastSpawnTime = 0f;

    void Awake()
    {
        interactable = GetComponent<Interactable>();

        Debug.Log("[RiceContainer] Awake: Initialized and subscribing to events.");

        // 掴んだときのイベントに登録
        interactable.onAttachedToHand += OnAttachedToHand;
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.onAttachedToHand -= OnAttachedToHand;
        }
    }

    // ホバー開始時に呼ばれる（SteamVRのInteractableがSendMessageで呼ぶ）
    private void OnHandHoverBegin(Hand hand)
    {
        Debug.Log($"[RiceContainer] Hand Hover Begin: {hand.name}");
    }

    // ホバー終了時に呼ばれる
    private void OnHandHoverEnd(Hand hand)
    {
        Debug.Log($"[RiceContainer] Hand Hover End: {hand.name}");
    }

    // ---------------------------------------------------------
    // 変更: Updateで毎フレーム監視する（より確実な方法）
    // ---------------------------------------------------------
    void Update()
    {
        // ホバー中の手を取得
        if (interactable != null && interactable.hoveringHand != null)
        {
            Hand hand = interactable.hoveringHand;

            // 掴む操作（Grip or Trigger）が開始されたかチェック
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None)
            {
                Debug.Log($"[RiceContainer] Grab detected in Update! Type: {startingGrabType}. Spawning Rice...");
                
                // お釜自体をアタッチするのではなく、直接米を生成して持たせる
                SpawnRice(hand, startingGrabType);
            }
        }
    }

    // 米生成ロジックを分離
    private void SpawnRice(Hand hand, GrabTypes grabType)
    {
        // クールダウンチェック
        if (Time.time - lastSpawnTime < cooldownTime)
        {
            Debug.Log($"[RiceContainer] Cooldown active. Wait {cooldownTime - (Time.time - lastSpawnTime):F2}s");
            return;
        }

        // 米のPrefabが設定されているか確認
        if (ricePrefab == null)
        {
            Debug.LogError("[RiceContainer] Error: Rice Prefab is NOT assigned in Inspector!");
            return;
        }

        // 米を生成する位置（手の位置 + オフセット）
        Vector3 spawnPosition = hand.transform.position + hand.transform.TransformDirection(riceSpawnOffset);
        Quaternion spawnRotation = hand.transform.rotation;

        // 米を生成
        GameObject newRice = Instantiate(ricePrefab, spawnPosition, spawnRotation);

        // お釜と米の衝突を無視する（即座に衝突して変な挙動になるのを防ぐ）
        Collider containerCollider = GetComponent<Collider>();
        Collider riceCollider = newRice.GetComponent<Collider>();
        if (containerCollider != null && riceCollider != null)
        {
            Physics.IgnoreCollision(containerCollider, riceCollider);
        }

        Debug.Log($"[RiceContainer] Rice instantiated successfully at {spawnPosition}");

        // 効果音を再生
        if (riceGrabSound != null)
        {
            AudioSource.PlayClipAtPoint(riceGrabSound, spawnPosition);
        }

        // 生成した米を手にアタッチ
        // お釜を経由せず、直接米を持たせる
        hand.AttachObject(newRice, grabType);
        Debug.Log($"[RiceContainer] Attached Rice to {hand.name}");

        // クールダウン時間を記録
        lastSpawnTime = Time.time;
    }

    // OnAttachedToHandはもう使わないが、念のため空にして残しておく（インターフェース用）
    private void OnAttachedToHand(Hand hand)
    {
        // 処理なし
    }
}
