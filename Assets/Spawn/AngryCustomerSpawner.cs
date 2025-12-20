using UnityEngine;
using UnityEngine.AI;

public class AngryCustomerSpawner : MonoBehaviour
{
    [Header("怒った客のPrefab")]
    public GameObject angryCustomerPrefab;

    [Header("スポーン地点（通常客と別）")]
    public Transform angrySpawnPoint;

    [Header("怒る場所（複数設定可）")]
    public Transform[] angrySpots;       // ← 配列に変更

    [Header("オーディオ")]
    public AudioClip spawnSound;         // スポーン時の効果音



    [Header("スポーン設定")]
    [Tooltip("ゲーム開始から最初のスポーンまでの待機時間（秒）")]
    public float firstSpawnDelay = 25f;   // 初回スポーンは25秒後

    [Tooltip("2回目以降のスポーン間隔（秒）")]
    public float spawnInterval = 25f;

    [Tooltip("ゲーム開始時から自動でスポーンするか")]
    public bool autoSpawn = true;

    // 内部タイマー
    private float timer = 0f;

    // 初回スポーン済みかどうか
    private bool hasSpawnedOnce = false;

    void Start()
    {
        // タイマー初期化
        timer = 0f;
        hasSpawnedOnce = false;
    }

    void Update()
    {
        if (!autoSpawn) return;

        // 経過時間を加算
        timer += Time.deltaTime;

        // -----------------------
        // 初回スポーン処理
        // -----------------------
        if (!hasSpawnedOnce)
        {
            if (timer >= firstSpawnDelay)
            {
                timer = 0f;
                hasSpawnedOnce = true;
                SpawnAngryCustomer();
            }
            return;
        }

        // -----------------------
        // 2回目以降のスポーン処理
        // -----------------------
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnAngryCustomer();
        }
    }

    /// <summary>
    /// 怒った客を1人スポーンさせる
    /// </summary>
    public void SpawnAngryCustomer()
    {
        // 必要な参照が設定されているかチェック
        if (angryCustomerPrefab == null || angrySpawnPoint == null)
        {
            Debug.LogError("[AngryCustomerSpawner] Prefab または SpawnPoint が設定されていません。");
            return;
        }

        // NavMesh上の安全な位置に補正
        Vector3 spawnPos = angrySpawnPoint.position;
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }

        // 客を生成
        GameObject obj = Instantiate(
            angryCustomerPrefab,
            spawnPos,
            angrySpawnPoint.rotation
        );

        // NavMeshAgent を確実に NavMesh 上に配置
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPos);
        }

        // スポーン音を再生
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, spawnPos);
        }


        // 怒った客に目的地（怒る場所）を設定
        AngryCustomer angry = obj.GetComponent<AngryCustomer>();
        if (angry != null)
        {
            // 複数の候補からランダムに1つ選ぶ
            if (angrySpots != null && angrySpots.Length > 0)
            {
                int randIndex = Random.Range(0, angrySpots.Length);
                angry.angrySpot = angrySpots[randIndex];
            }
            else
            {
                 Debug.LogWarning("[AngryCustomerSpawner] AngrySpots が設定されていません。");
            }
        }
    }

    /// <summary>
    /// 外部から即時スポーンさせたい場合用
    /// </summary>
    public void SpawnNow()
    {
        SpawnAngryCustomer();
    }

    /// <summary>
    /// スポーンタイマーをリセットする
    /// </summary>
    public void ResetSpawnTimer()
    {
        timer = 0f;
        hasSpawnedOnce = false;
    }
}
