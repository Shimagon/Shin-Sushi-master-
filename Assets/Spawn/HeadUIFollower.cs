using UnityEngine;

public class HeadUIFollower : MonoBehaviour
{
    [Tooltip("캐릭터의 Animator (Humanoid)")]
    public Animator animator;

    [Tooltip("머리에서 얼마나 위로 띄울지")]
    public Vector3 offset = new Vector3(0f, 0.25f, 0f);

    [Tooltip("카메라 방향을 항상 바라볼지 여부")]
    public bool lookAtCamera = true;

    private Transform head;

    void Start()
    {
        if (animator == null)
        {
            // 부모에서 Animator 자동 찾기 (캐릭터에 붙어있으면 편함)
            animator = GetComponentInParent<Animator>();
        }

        if (animator != null)
        {
            // 휴머노이드 아바타의 Head 뼈 Transform 가져오기
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        if (head == null)
        {
            Debug.LogWarning("[HeadUIFollower] Head 본을 찾지 못했습니다.");
        }
    }

    void LateUpdate()
    {
        if (head == null) return;

        // 머리 위치 + 오프셋
        transform.position = head.position + offset;

        // 항상 카메라를 바라보게 할지 (선택)
        if (lookAtCamera && Camera.main != null)
        {
            // 카메라 쪽을 향하도록
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}
