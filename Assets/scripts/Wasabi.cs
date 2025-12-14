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
        if (hitTag == "flooring" || hitTag == "wall" || hitTag == "customer" || hitTag == "Customer")
        {
            Customer customer = collision.gameObject.GetComponent<Customer>();
            
            // 親オブジェクトの確認
            if (customer == null && collision.transform.parent != null)
            {
                customer = collision.transform.parent.GetComponent<Customer>();
            }

            Explode(customer);
        }
    }

    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode(Customer target)
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

        // 自分自身を破棄
        Destroy(gameObject);
    }
}
