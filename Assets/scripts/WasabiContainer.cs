using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// わさび入れ - 掴んだらわさびを手に生成する
/// RiceContainerをベースに作成
/// </summary>
[RequireComponent(typeof(Interactable))]
public class WasabiContainer : MonoBehaviour
{
    [Header("Wasabi Settings")]
    [Tooltip("生成するわさびのPrefab")]
    public GameObject wasabiPrefab;

    [Tooltip("わさびの生成位置オフセット（手からの相対位置）")]
    public Vector3 spawnOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Audio")]
    [Tooltip("わさびを掴んだときの効果音")]
    public AudioClip grabSound;

    [Header("Settings")]
    [Tooltip("連続生成のクールダウン時間（秒）")]
    public float cooldownTime = 0.5f;

    private Interactable interactable;
    private float lastSpawnTime = 0f;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

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
                SpawnWasabi(hand, startingGrabType);
            }
        }
    }

    private void SpawnWasabi(Hand hand, GrabTypes grabType)
    {
        // クールダウンチェック
        if (Time.time - lastSpawnTime < cooldownTime)
        {
            return;
        }

        // Prefabチェック
        if (wasabiPrefab == null)
        {
            Debug.LogError("[WasabiContainer] Wasabi Prefab is NOT assigned!");
            return;
        }

        // 生成位置
        Vector3 spawnPosition = hand.transform.position + hand.transform.TransformDirection(spawnOffset);
        Quaternion spawnRotation = hand.transform.rotation;

        // 生成
        GameObject newWasabi = Instantiate(wasabiPrefab, spawnPosition, spawnRotation);

        // コンテナとの衝突無視
        Collider containerCollider = GetComponent<Collider>();
        Collider wasabiCollider = newWasabi.GetComponent<Collider>();
        if (containerCollider != null && wasabiCollider != null)
        {
            Physics.IgnoreCollision(containerCollider, wasabiCollider);
        }

        // 効果音
        if (grabSound != null)
        {
            AudioSource.PlayClipAtPoint(grabSound, spawnPosition);
        }

        // 手にアタッチ
        hand.AttachObject(newWasabi, grabType);

        lastSpawnTime = Time.time;
    }
}
