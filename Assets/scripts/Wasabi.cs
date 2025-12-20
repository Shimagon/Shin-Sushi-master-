using UnityEngine;

/// <summary>
/// わさび（投擲物）
/// 客に当たると爆発して追い出す
/// </summary>
public class Wasabi : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("爆発エフェクトのPrefab")]
    public GameObject explosionEffect;

    [Tooltip("爆発音")]
    public AudioClip explosionSound;

    [Tooltip("爆発半径（見た目用）")]
    public float explosionRadius = 1.0f;

    private bool hasExploded = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        string hitTag = collision.gameObject.tag;

        // 指定されたタグ（flooring, wall, customer）の場合のみ爆発
        // 念のため "Customer" (大文字) も許可
        // AngryCustomerコンポーネントを持っているかチェック（タグが設定されていなくても反応するようにする）
        if (hitTag == "flooring" || hitTag == "wall" || hitTag == "customer" || hitTag == "Customer" || collision.gameObject.GetComponent<AngryCustomer>() != null)
        {
            Customer customer = collision.gameObject.GetComponent<Customer>();
            
            // 親オブジェクトの確認
            if (customer == null && collision.transform.parent != null)
            {
                customer = collision.transform.parent.GetComponent<Customer>();
            }

            // AngryCustomerコンポーネントの確認
            AngryCustomer angryCustomer = collision.gameObject.GetComponent<AngryCustomer>(); // 追加

            // どちらかがあれば爆発
            Explode(customer, angryCustomer);
        }
    }

    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode(Customer target, AngryCustomer angryTarget = null) // 引数追加
    {
        hasExploded = true;

        // エフェクト生成
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f); // エフェクトは3秒後に消す
        }

        // 効果音
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 客への通知
        if (target != null)
        {
            target.HitByWasabi();
        }

        // 怒った客への通知（追加）
        if (angryTarget != null)
        {
            // AngryCustomer側の被弾処理を呼び出す（メソッドがpublicである必要あり）
            // AngryCustomer.cs に public void OnHitByWasabi() があるか確認してください
            // なければ SendMessage 等で対応するか、メソッドを公開してください
            angryTarget.SendMessage("OnHitByWasabi", SendMessageOptions.DontRequireReceiver);
        }

        // 自分自身を破棄
        Destroy(gameObject);
    }
}
