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
    }

    void Update()
    {
        if (agent != null && animator != null && !hasArrived)
        {
            // 実際の移動速度から歩行アニメーションを制御
            bool moving = agent.velocity.sqrMagnitude > 0.05f;
            animator.SetBool(walkBool, moving);
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

    void OnHitByWasabi()
    {
        isHit = true;

        // 念のため移動を停止
        if (agent != null)
        {
            agent.isStopped = true;
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
