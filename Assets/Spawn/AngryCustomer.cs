using UnityEngine;
using UnityEngine.AI;

public class AngryCustomer : MonoBehaviour
{
    [Header("移動先（怒る場所）")]
    public Transform angrySpot;

    [Header("到着判定距離")]
    public float arriveDistance = 0.3f;

    [Header("Animator")]
    public Animator animator;
    public string walkBool = "IsWalking";
    public string tauntTrigger = "Taunt";

    [Header("Wasabi 判定")]
    public string wasabiTag = "Wasabi";
    public string wasabiName = "wasabi";

    [Header("被弾後の消滅設定")]
    [Tooltip("wasabiに当たってから消えるまでの時間（秒）")]
    public float destroyDelay = 0.1f;   // ★ Inspectorで変更可能

    [Header("オーディオ")]
    public AudioClip defeatSound;       // 撃退された時の音
    public AudioClip ambienceSound;     // 滞在中のループ音
    private AudioSource audioSource;


    private NavMeshAgent agent;
    private bool hasArrived = false;
    private bool isHit = false;          // 二重判定防止

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    void Start()
    {
        if (agent != null && angrySpot != null)
        {
            agent.isStopped = false;
            agent.SetDestination(angrySpot.position);
        }

        // 滞在音（アンビエンス）の再生
        if (ambienceSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = ambienceSound;
            audioSource.loop = true;
            audioSource.spatialBlend = 1.0f; // 3Dサウンドにする
            audioSource.Play();
        }

        // 初期状態でプレイヤーの方を向く
        LookAtPlayer();

        // スコアマネージャーに登録（来店通知）
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterAngryCustomer();
        }
    }

    void OnDestroy()
    {
        // スコアマネージャーから登録解除（退店通知）
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UnregisterAngryCustomer();
        }
    }

    void Update()
    {
        if (agent != null && animator != null && !hasArrived)
        {
            // 実際の移動速度から歩行アニメーションを制御
            bool moving = agent.velocity.sqrMagnitude > 0.05f;
            animator.SetBool(walkBool, moving);
        }

        // 到着後もプレイヤーの方を向き続ける
        if (hasArrived)
        {
            LookAtPlayer();
        }

        if (hasArrived || agent == null || angrySpot == null) return;
        if (agent.pathPending) return;

        if (agent.remainingDistance <= arriveDistance)
        {
            hasArrived = true;
            agent.isStopped = true;

            if (animator != null)
            {
                animator.SetBool(walkBool, false);
                animator.SetTrigger(tauntTrigger);
            }
        }
    }

    // プレイヤーの方を向く処理
    void LookAtPlayer()
    {
        if (Camera.main != null)
        {
            Vector3 targetPosition = Camera.main.transform.position;
            targetPosition.y = transform.position.y; // 高さは合わせる（上を向かないように）
            transform.LookAt(targetPosition);
        }
    }

    // -----------------------
    // wasabi に当たった判定
    // -----------------------
    private void OnTriggerEnter(Collider other)
    {
        if (isHit) return;

        if (IsWasabi(other.gameObject))
        {
            OnHitByWasabi();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isHit) return;

        if (IsWasabi(collision.gameObject))
        {
            OnHitByWasabi();
        }
    }

    public void OnHitByWasabi()
    {
        isHit = true;

        // 念のため移動を停止
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // 撃退音を再生
        if (defeatSound != null)
        {
            AudioSource.PlayClipAtPoint(defeatSound, transform.position);
        }

        // 指定時間後に消滅
        Destroy(gameObject, destroyDelay);
    }

    bool IsWasabi(GameObject obj)
    {
        if (obj == null) return false;

        // Tag優先
        if (!string.IsNullOrEmpty(wasabiTag) && obj.CompareTag(wasabiTag))
            return true;

        // 名前判定（wasabi / wasabi(Clone) 対応）
        return obj.name.ToLower().Contains(wasabiName.ToLower());
    }
}
