using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// 投げられる寿司オブジェクト
/// お客さんに当たったらスコアを追加する
/// </summary>
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VelocityEstimator))]
public class SushiThrowable : MonoBehaviour
{
    [Header("Sushi Settings")]
    [Tooltip("この寿司の種類")]
    public string sushiType = "Maguro";

    [Tooltip("この寿司の得点")]
    public int pointValue = 100;

    [Header("Throw Settings")]
    [Tooltip("投げる力の倍率")]
    public float throwMultiplier = 1.5f;

    [Tooltip("投げた後に自動的に消えるまでの時間（秒）")]
    public float autoDestroyTime = 10f;

    [Header("Effects")]
    [Tooltip("お客さんに当たったときのエフェクト")]
    public GameObject hitEffect;

    [Tooltip("お客さんに当たったときの効果音")]
    public AudioClip hitSound;

    [Tooltip("間違った寿司が当たったときのエフェクト")]
    public GameObject wrongHitEffect;

    [Tooltip("間違った寿司が当たったときの「ブブー」効果音")] // 新規追加
    public AudioClip wrongHitSound;

    [Tooltip("お客さん以外（床や壁）に当たった時の「べちゃ」効果音")]
    public AudioClip splatSound;

    [Tooltip("投げたときの効果音")]
    public AudioClip throwSound;

    private Interactable interactable;
    private Rigidbody rb;
    private VelocityEstimator velocityEstimator;
    private bool hasBeenThrown = false;
    private bool hasHitTarget = false;
    private float throwTime = 0f;

    private bool hasSplatted = false;


    void Awake()
    {
        interactable = GetComponent<Interactable>();
        rb = GetComponent<Rigidbody>();
        velocityEstimator = GetComponent<VelocityEstimator>();

        // SushiTypeコンポーネントがあれば、そこから種類を取得
        SushiType sushiTypeComponent = GetComponent<SushiType>();
        if (sushiTypeComponent != null)
        {
            sushiType = sushiTypeComponent.GetSushiType();
            pointValue = sushiTypeComponent.GetScore();
        }

        // Interactableのイベントに登録
        interactable.onAttachedToHand += OnAttachedToHand;
        interactable.onDetachedFromHand += OnDetachedFromHand;

        // Rigidbodyの設定
        if (rb != null)
        {
            rb.maxAngularVelocity = 50.0f;
        }
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.onAttachedToHand -= OnAttachedToHand;
            interactable.onDetachedFromHand -= OnDetachedFromHand;
        }
    }

    void Update()
    {
        // 投げられた後、一定時間経過したら自動的に消す
        if (hasBeenThrown)
        {
            throwTime += Time.deltaTime;
            if (throwTime >= autoDestroyTime)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnAttachedToHand(Hand hand)
    {
        Debug.Log($"{sushiType}が{hand.name}に掴まれました");
        hasBeenThrown = false;
        hasSplatted = false; // フラグをリセット
        throwTime = 0f;

        // 投げるための速度計測を開始
        if (velocityEstimator != null)
        {
            velocityEstimator.BeginEstimatingVelocity();
        }
    }

    private void OnDetachedFromHand(Hand hand)
    {
        Debug.Log($"{sushiType}が{hand.name}から離されました（投げられました）");

        // 速度計測を終了
        if (velocityEstimator != null)
        {
            velocityEstimator.FinishEstimatingVelocity();
        }

        // 投げる処理
        ThrowObject(hand);
    }

    private void ThrowObject(Hand hand)
    {
        hasBeenThrown = true;
        throwTime = 0f;

        // 手の速度を取得して寿司に適用
        if (velocityEstimator != null && rb != null)
        {
            Vector3 velocity = velocityEstimator.GetVelocityEstimate() * throwMultiplier;
            Vector3 angularVelocity = velocityEstimator.GetAngularVelocityEstimate();

            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;

            Debug.Log($"寿司を投げました！速度: {velocity.magnitude:F2} m/s");
        }

        // 投げた音を再生
        if (throwSound != null)
        {
            AudioSource.PlayClipAtPoint(throwSound, transform.position);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // まだ投げられていない、または既に当たっている場合は無視
        if (!hasBeenThrown || hasHitTarget)
        {
            return;
        }

        GameObject hitObject = collision.gameObject;

        // お客さんに当たったかチェック
        Customer customer = hitObject.GetComponent<Customer>();
        CustomerOrderWithTimer customerWithTimer = hitObject.GetComponent<CustomerOrderWithTimer>();

        if (customer != null)
        {
            // お客さんに当たった！(古いScript)
            OnHitCustomer(customer, collision.contacts[0].point);
        }
        else if (customerWithTimer != null)
        {
            // お客さんに当たった！(新しいScript)
            OnHitCustomerWithTimer(customerWithTimer, collision.contacts[0].point);
        }
        else if (hitObject.CompareTag("Customer"))
        {
            // タグでもチェック
            OnHitCustomerSimple(hitObject, collision.contacts[0].point);
        }
        else if (hitObject.CompareTag("flooring") && !hasSplatted)
        {
            // 床に当たった場合（1回のみ再生）
            hasSplatted = true;
            PlaySplatSound(collision.contacts[0].point);
        }
    }

    /// <summary>
    /// 「べちゃ」音を再生
    /// </summary>
    private void PlaySplatSound(Vector3 position)
    {
        if (splatSound != null)
        {
            AudioSource.PlayClipAtPoint(splatSound, position);
        }
    }

    /// <summary>
    /// お客さんに当たったときの処理
    /// </summary>
    private void OnHitCustomer(Customer customer, Vector3 hitPoint)
    {
        hasHitTarget = true;

        // お客さんが求めている寿司かチェック
        bool isCorrectSushi = customer.WantsSushi(sushiType);
        
        int earnedPoints = 0;
        if (isCorrectSushi)
        {
            earnedPoints = pointValue;
            Debug.Log($"お客さんに{sushiType}が当たった！正解！ {earnedPoints}点獲得！");
        }
        else
        {
            earnedPoints = -1; // 間違いはマイナス1点
            Debug.Log($"お客さんに{sushiType}が当たった！間違い... {earnedPoints}点...");
        }

        // スコアを追加
        // ScoreManager.Instance?.AddScore(earnedPoints);

        // お客さんに通知
        customer.ReceiveSushi(sushiType, isCorrectSushi);

        // エフェクトと音を再生
        PlayHitEffects(hitPoint, isCorrectSushi);

        // 寿司を消す
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// お客さん（Timer付き）に当たったときの処理
    /// </summary>
    private void OnHitCustomerWithTimer(CustomerOrderWithTimer customer, Vector3 hitPoint)
    {
        hasHitTarget = true;

        // お客さんが求めている寿司かチェック
        bool isCorrectSushi = customer.WantsSushi(sushiType);
        
        int earnedPoints = 0;
        if (isCorrectSushi)
        {
            earnedPoints = pointValue;
            Debug.Log($"お客さん(Timer)に{sushiType}が当たった！正解！ {earnedPoints}点獲得！");
        }
        else
        {
            earnedPoints = -1; // 間違いはマイナス1点
            Debug.Log($"お客さん(Timer)に{sushiType}が当たった！間違い... {earnedPoints}点...");
        }

        // スコアを追加
        // ScoreManager.Instance?.AddScore(earnedPoints);

        // お客さんに通知
        customer.ReceiveSushi(sushiType, isCorrectSushi);

        // エフェクトと音を再生
        PlayHitEffects(hitPoint, isCorrectSushi);

        // 寿司を消す
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// お客さんに当たったときの処理（シンプル版）
    /// </summary>
    private void OnHitCustomerSimple(GameObject customerObject, Vector3 hitPoint)
    {
        hasHitTarget = true;

        // スコアを追加
        ScoreManager.Instance?.AddScore(pointValue);

        Debug.Log($"お客さんに{sushiType}が当たった！ {pointValue}点獲得！");

        // エフェクトと音を再生
        PlayHitEffects(hitPoint, true);

        // 寿司を消す
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// ヒットエフェクトと音を再生
    /// </summary>
    private void PlayHitEffects(Vector3 position, bool isCorrect)
    {
        // エフェクト
        if (isCorrect)
        {
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
                Destroy(effect, 3f);
            }
            // 正解音
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, position);
            }
        }
        else
        {
            if (wrongHitEffect != null)
            {
                GameObject effect = Instantiate(wrongHitEffect, position, Quaternion.identity);
                Destroy(effect, 3f);
            }
            // 間違い音（不正解音）
            if (wrongHitSound != null)
            {
                AudioSource.PlayClipAtPoint(wrongHitSound, position);
            }
        }
    }

    /// <summary>
    /// 投げられたかどうか
    /// </summary>
    public bool HasBeenThrown()
    {
        return hasBeenThrown;
    }

    /// <summary>
    /// 寿司の種類を取得
    /// </summary>
    public string GetSushiType()
    {
        return sushiType;
    }
}
