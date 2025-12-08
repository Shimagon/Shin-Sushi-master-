using UnityEngine;

/// <summary>
/// 座席ポイント - お客さんが座る位置を管理する
/// </summary>
public class SeatPoint : MonoBehaviour
{
    [Header("座席情報")]
    [Tooltip("座席番号（1〜6など）")]
    public int seatNumber = 1;

    [Tooltip("この座席が現在使用中かどうか")]
    public bool isOccupied = false;

    [Tooltip("この座席に座っているお客さん（いない場合はnull）")]
    private GameObject currentCustomer = null;

    [Header("座席位置")]
    [Tooltip("お客さんが立つ位置")]
    public Transform standPosition;

    [Tooltip("お客さんが座る位置")]
    public Transform sitPosition;

    /// <summary>
    /// 座席を占有する
    /// </summary>
    public void Occupy(GameObject customer)
    {
        isOccupied = true;
        currentCustomer = customer;
        Debug.Log($"座席 {seatNumber} が占有されました");
    }

    /// <summary>
    /// 座席を解放する
    /// </summary>
    public void Release()
    {
        isOccupied = false;
        currentCustomer = null;
        Debug.Log($"座席 {seatNumber} が解放されました");
    }

    /// <summary>
    /// 現在座っているお客さんを取得
    /// </summary>
    public GameObject GetCurrentCustomer()
    {
        return currentCustomer;
    }

    /// <summary>
    /// 座る位置を取得（sitPositionが設定されていればそれを、なければ自分の位置を返す）
    /// </summary>
    public Vector3 GetSitPosition()
    {
        if (sitPosition != null)
            return sitPosition.position;
        return transform.position;
    }

    /// <summary>
    /// 立つ位置を取得（standPositionが設定されていればそれを、なければ自分の位置を返す）
    /// </summary>
    public Vector3 GetStandPosition()
    {
        if (standPosition != null)
            return standPosition.position;
        return transform.position;
    }

    // エディタで座席位置を視覚化（デバッグ用）
    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 2f, 1f));

        if (sitPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sitPosition.position, 0.3f);
        }

        if (standPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(standPosition.position, 0.3f);
        }
    }
}
