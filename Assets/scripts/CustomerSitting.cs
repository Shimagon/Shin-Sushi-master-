using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// お客さんの座る動作を管理するスクリプト
/// NavMeshAgentを使って座席まで移動し、到着したら座る
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerSitting : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動速度")]
    public float moveSpeed = 3.5f;

    [Tooltip("到着判定距離")]
    public float arrivalDistance = 0.5f;

    [Header("座る設定")]
    [Tooltip("座っているときのY座標オフセット")]
    public float sittingYOffset = -0.5f;

    private NavMeshAgent navAgent;
    private SeatPoint targetSeat;
    private bool isWalking = false;
    private bool isSitting = false;
    private Animator animator;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }
    }

    void Update()
    {
        // 座席に向かって歩いている場合、到着をチェック
        if (isWalking && targetSeat != null && navAgent != null)
        {
            // NavMeshAgentが目的地に到達したかチェック
            if (!navAgent.pathPending && navAgent.remainingDistance <= arrivalDistance)
            {
                ArrivedAtSeat();
            }
        }
    }

    /// <summary>
    /// 指定された座席に向かう
    /// </summary>
    public void GoToSeat(SeatPoint seat)
    {
        if (seat == null)
        {
            Debug.LogWarning("座席がnullです");
            return;
        }

        targetSeat = seat;
        targetSeat.Occupy(gameObject);

        if (navAgent != null)
        {
            Vector3 targetPosition = targetSeat.GetStandPosition();
            navAgent.SetDestination(targetPosition);
            isWalking = true;

            Debug.Log($"{gameObject.name}が座席 {seat.seatNumber} に向かっています");

            // 歩きアニメーション開始（Animatorがあれば）
            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            Debug.LogError("NavMeshAgentが見つかりません！");
        }
    }

    /// <summary>
    /// 座席に到着したときの処理
    /// </summary>
    private void ArrivedAtSeat()
    {
        isWalking = false;
        isSitting = true;

        Debug.Log($"{gameObject.name}が座席に到着しました");

        // NavMeshAgentを停止
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }

        // 歩きアニメーション停止
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSitting", true);
        }

        // 座席の位置に移動して座る
        Vector3 sitPos = targetSeat.GetSitPosition();
        transform.position = sitPos;

        // 座席の方向を向く（必要に応じて）
        transform.rotation = targetSeat.transform.rotation;

        // Customerスクリプトがあれば要求を表示開始
        Customer customer = GetComponent<Customer>();
        if (customer != null)
        {
            // Customerスクリプトは既にStartで要求を表示する仕組みがあるのでそのまま
        }
    }

    /// <summary>
    /// お客さんが退店するときに座席を解放する
    /// </summary>
    void OnDestroy()
    {
        if (targetSeat != null)
        {
            targetSeat.Release();
        }
    }

    /// <summary>
    /// 現在座っているかどうか
    /// </summary>
    public bool IsSitting()
    {
        return isSitting;
    }

    /// <summary>
    /// 現在歩いているかどうか
    /// </summary>
    public bool IsWalking()
    {
        return isWalking;
    }
}
