using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// 左手で持つ米オブジェクト
/// 魚（ネタ）と衝突したときに寿司を生成する
/// </summary>
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
public class GrabbableRice : MonoBehaviour
{
    [Header("Sushi Settings")]
    [Tooltip("マグロ寿司のPrefab")]
    public GameObject maguroSushiPrefab;

    [Tooltip("たまご寿司のPrefab")]
    public GameObject tamagoSushiPrefab;

    [Tooltip("サーモン寿司のPrefab")]
    public GameObject salmonSushiPrefab;

    [Tooltip("寿司の生成位置オフセット")]
    public Vector3 sushiSpawnOffset = Vector3.zero;

    [Tooltip("寿司の回転（Eulerアングル）")]
    public Vector3 sushiRotation = new Vector3(-90f, 0f, 0f);

    [Header("VR Hand Settings")]
    [Tooltip("このオブジェクトをアタッチする手（左手を推奨）")]
    public Hand preferredHand;

    [Header("Audio")]
    [Tooltip("寿司生成時の効果音")]
    public AudioClip sushiMakeSound;

    [Header("Effects")]
    [Tooltip("寿司生成時のエフェクト（CFXR Magic Poofなど）")]
    public GameObject sushiMakeEffect;

    [Header("Debug")]
    [Tooltip("デバッグモード：手で持たなくても寿司を作れる")]
    public bool allowWithoutHand = false;

    private Interactable interactable;
    private Rigidbody rb;
    private bool isHeldByHand = false;
    private Hand currentHand;
    private bool hasCreatedSushi = false; // 寿司を作ったかどうか

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        rb = GetComponent<Rigidbody>();

        // Interactableのイベントに登録
        interactable.onAttachedToHand += OnAttachedToHand;
        interactable.onDetachedFromHand += OnDetachedFromHand;
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.onAttachedToHand -= OnAttachedToHand;
            interactable.onDetachedFromHand -= OnDetachedFromHand;
        }
    }

    private void OnAttachedToHand(Hand hand)
    {
        isHeldByHand = true;
        currentHand = hand;
        Debug.Log($"米が{hand.name}に掴まれました");
    }

    private void OnDetachedFromHand(Hand hand)
    {
        isHeldByHand = false;
        currentHand = null;
        Debug.Log($"米が{hand.name}から離されました");
    }

    void OnCollisionEnter(Collision collision)
    {
        // デバッグログ: 何かと衝突した
        Debug.Log($"[Rice] Collision detected with: {collision.gameObject.name}. IsHeld: {isHeldByHand}");

        // 既に寿司を作っていたら無視（重複防止）
        if (hasCreatedSushi)
        {
            return;
        }

        // 手で持っているときのみチェック（デバッグモードではスキップ）
        if (!isHeldByHand && !allowWithoutHand)
        {
            return;
        }

        // 衝突相手が魚かチェック
        GameObject otherObject = collision.gameObject;
        GrabbableFish fish = otherObject.GetComponent<GrabbableFish>();

        if (fish != null)
        {
            Debug.Log($"[Rice] Collided with Fish! Fish IsHeld: {fish.IsHeldByHand()}");
            
            // GrabbableFishがある場合
            // デバッグモードなら手で持ってなくてもOK
            if (allowWithoutHand || fish.IsHeldByHand())
            {
                Debug.Log($"[Rice] Merging with {fish.GetFishType()}...");
                hasCreatedSushi = true; // フラグを立てる
                MakeSushi(fish, collision.contacts[0].point);
            }
        }
        else if (otherObject.CompareTag("Fish"))
        {
            // タグだけでもチェック（後方互換性）
            hasCreatedSushi = true; // フラグを立てる
            MakeSushiSimple(otherObject, collision.contacts[0].point);
        }
    }

    void Update()
    {
        // 手で持っているときのみ、近くの魚を探す（物理衝突に頼らないマージ処理）
        if (isHeldByHand && !hasCreatedSushi)
        {
            // 半径10cm以内のコライダーを探す
            Collider[] colliders = Physics.OverlapSphere(transform.position, 0.1f);
            foreach (var col in colliders)
            {
                GrabbableFish fish = col.GetComponent<GrabbableFish>();
                if (fish != null && fish.IsHeldByHand())
                {
                    Debug.Log($"[Rice] Overlap detected with {fish.GetFishType()}! Merging...");
                    hasCreatedSushi = true;
                    MakeSushi(fish, transform.position);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 寿司を生成する（GrabbableFish使用）
    /// </summary>
    void MakeSushi(GrabbableFish fish, Vector3 collisionPoint)
    {
        // エラー回避: ThrowableとVelocityEstimatorを無効化（NaNエラー防止）
        var throwable = GetComponent<Throwable>();
        if (throwable != null) throwable.enabled = false;
        var velEst = GetComponent<VelocityEstimator>();
        if (velEst != null) velEst.enabled = false;

        if (fish != null)
        {
            var fishThrowable = fish.GetComponent<Throwable>();
            if (fishThrowable != null) fishThrowable.enabled = false;
            var fishVelEst = fish.GetComponent<VelocityEstimator>();
            if (fishVelEst != null) fishVelEst.enabled = false;
        }

        // 重要: デタッチする前に、現在持っている手の情報を保存しておく
        Hand myHand = currentHand;
        if (myHand == null && interactable.attachedToHand != null)
        {
            myHand = interactable.attachedToHand;
        }

        Hand fishHand = null;
        if (fish != null)
        {
            fishHand = fish.GetCurrentHand();
            // 念のためInteractableからも確認
            if (fishHand == null)
            {
                var fishInteractable = fish.GetComponent<Interactable>();
                if (fishInteractable != null && fishInteractable.attachedToHand != null)
                {
                    fishHand = fishInteractable.attachedToHand;
                }
            }
        }

        // 1. 両方の手からオブジェクトを離す
        if (myHand != null)
        {
            myHand.DetachObject(gameObject);
        }
        
        if (fishHand != null && fish != null)
        {
            fishHand.DetachObject(fish.gameObject);
        }

        // 2. 魚の種類に応じた寿司Prefabを取得
        GameObject sushiPrefab = GetSushiPrefabForFish(fish.gameObject);

        if (sushiPrefab == null)
        {
            Debug.LogWarning("寿司のPrefabが設定されていません！");
            return;
        }

        // 3. 寿司を生成
        Vector3 spawnPosition = collisionPoint + sushiSpawnOffset;
        Quaternion spawnRotation = Quaternion.Euler(sushiRotation);
        GameObject newSushi = Instantiate(sushiPrefab, spawnPosition, spawnRotation);

        // 4. エフェクトと音
        if (sushiMakeEffect != null)
        {
            GameObject effect = Instantiate(sushiMakeEffect, newSushi.transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 2f;
            Destroy(effect, 3f);
        }

        if (sushiMakeSound != null)
        {
            AudioSource.PlayClipAtPoint(sushiMakeSound, spawnPosition);
        }

        // 5. 寿司を手に持たせる（魚を持っていた手に優先的に持たせる）
        // 保存しておいた手の情報を使う
        Hand targetHand = fishHand != null ? fishHand : myHand;
        
        if (targetHand != null)
        {
            Debug.Log($"寿司を{targetHand.name}に持たせようとしています...");
            // 少し遅延させて持たせる（物理演算の干渉を防ぐため）
            StartCoroutine(AttachSushiDelayed(targetHand, newSushi));
        }
        else
        {
            Debug.LogWarning("寿司を持たせる手が見つかりませんでした（両方null）");
        }

        // 6. 古いオブジェクトを破棄（遅延させる）
        StartCoroutine(DestroyObjectsDelayed(fish.gameObject));
    }

    private System.Collections.IEnumerator AttachSushiDelayed(Hand hand, GameObject sushi)
    {
        // 1フレームではなく、少しだけ待つ（手のステートがリセットされるのを待つ）
        yield return new WaitForSeconds(0.05f);
        
        if (hand != null && sushi != null)
        {
            // 強制的にアタッチ
            hand.AttachObject(sushi, GrabTypes.Grip);
            Debug.Log($"寿司を{hand.name}にアタッチしました（遅延実行）");
        }
    }

    private System.Collections.IEnumerator DestroyObjectsDelayed(GameObject fishObject)
    {
        // 見た目を消す
        HideObject(gameObject);
        if (fishObject != null) HideObject(fishObject);

        // 少し待ってから破棄（デタッチが確実に完了するのを待つ）
        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
        if (fishObject != null)
        {
            Destroy(fishObject);
        }
    }

    private void HideObject(GameObject obj)
    {
        // レンダラーを無効化
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        // コライダーを無効化
        foreach (var c in obj.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
    }

    /// <summary>
    /// 寿司を生成する（シンプル版）
    /// </summary>
    void MakeSushiSimple(GameObject fishObject, Vector3 collisionPoint)
    {
        // 魚の種類に応じた寿司Prefabを取得
        GameObject sushiPrefab = GetSushiPrefabForFish(fishObject);

        if (sushiPrefab == null)
        {
            Debug.LogError("寿司のPrefabが設定されていません！");
            return;
        }

        Vector3 spawnPosition = collisionPoint + sushiSpawnOffset;
        Quaternion spawnRotation = Quaternion.Euler(sushiRotation);

        GameObject newSushi = Instantiate(sushiPrefab, spawnPosition, spawnRotation);

        if (sushiMakeSound != null)
        {
            AudioSource.PlayClipAtPoint(sushiMakeSound, spawnPosition);
        }

        if (sushiMakeEffect != null)
        {
            GameObject effect = Instantiate(sushiMakeEffect, newSushi.transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 2f;

            // エフェクトを3秒後に自動削除
            Destroy(effect, 3f);
        }

        Destroy(gameObject);
        Destroy(fishObject);
    }

    /// <summary>
    /// 魚の種類に応じた寿司のPrefabを取得
    /// </summary>
    private GameObject GetSushiPrefabForFish(GameObject fishObject)
    {
        // 魚オブジェクトにFishTypeコンポーネントがあれば、それを使用
        FishType fishType = fishObject.GetComponent<FishType>();
        if (fishType != null)
        {
            switch (fishType.fishTypeName)
            {
                case "Maguro":
                    return maguroSushiPrefab;
                case "Tamago":
                    return tamagoSushiPrefab;
                case "Salmon":
                    return salmonSushiPrefab;
                default:
                    Debug.LogWarning("不明な魚の種類: " + fishType.fishTypeName);
                    return maguroSushiPrefab; // デフォルトはマグロ
            }
        }

        // GrabbableFishコンポーネントから種類を取得
        GrabbableFish grabbableFish = fishObject.GetComponent<GrabbableFish>();
        if (grabbableFish != null)
        {
            string fishTypeName = grabbableFish.GetFishType();
            if (fishTypeName == "Tamago")
                return tamagoSushiPrefab;
            else if (fishTypeName == "Salmon")
                return salmonSushiPrefab;
            else
                return maguroSushiPrefab;
        }

        // FishTypeコンポーネントがない場合は名前で判定
        string fishName = fishObject.name.ToLower();
        if (fishName.Contains("maguro") || fishName.Contains("tuna"))
        {
            return maguroSushiPrefab;
        }
        else if (fishName.Contains("tamago") || fishName.Contains("egg"))
        {
            return tamagoSushiPrefab;
        }
        else if (fishName.Contains("salmon") || fishName.Contains("sake"))
        {
            return salmonSushiPrefab;
        }

        // デフォルトはマグロ
        Debug.LogWarning("魚の種類を判定できませんでした。マグロとして扱います: " + fishObject.name);
        return maguroSushiPrefab;
    }

    /// <summary>
    /// 反対の手を取得
    /// </summary>
    private Hand GetOtherHand(Hand hand)
    {
        if (hand == null) return null;

        // Playerオブジェクトから両手を取得
        Player player = hand.GetComponentInParent<Player>();
        if (player != null)
        {
            if (player.leftHand == hand)
                return player.rightHand;
            else if (player.rightHand == hand)
                return player.leftHand;
        }

        return null;
    }

    /// <summary>
    /// 現在手で持っているかどうか
    /// </summary>
    public bool IsHeldByHand()
    {
        return isHeldByHand;
    }

    /// <summary>
    /// 現在持っている手を取得
    /// </summary>
    public Hand GetCurrentHand()
    {
        return currentHand;
    }
}
