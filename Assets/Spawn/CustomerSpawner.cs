using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomerSpawner : MonoBehaviour
{
    [Header("客のプレハブ")]
    public GameObject customerPrefab;

    [Header("生成時の効果音")]
    public AudioClip spawnSound;

    [Header("客のスポーン位置（入口など）")]
    public Transform spawnPoint;

    [Header("椅子 SeatPoint 一覧（1〜6）")]
    public SeatPoint[] seats;

    [Header("客の生成間隔（秒）")]
    public float spawnInterval = 30f;   // 30秒ごとに生成

    [Header("Difficulty Settings")]
    public float minSpawnInterval = 5f;
    public float difficultyIncreaseInterval = 20f;
    public float difficultyDecreaseAmount = 1f;



    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        // 難易度調整：時間経過で生成間隔を短くする機能を停止（ずっと30秒間隔にするため）
        /*
        timer += Time.deltaTime;
        if (timer >= difficultyIncreaseInterval)
        {
            timer = 0f;
            if (spawnInterval > minSpawnInterval)
            {
                spawnInterval -= difficultyDecreaseAmount;
                if (spawnInterval < minSpawnInterval) spawnInterval = minSpawnInterval;
                Debug.Log($"難易度アップ！生成間隔が {spawnInterval}秒 になりました");
            }
        }
        */
    }

    /// <summary>
    /// 一定間隔で客を生成するループ処理
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            TrySpawnCustomer();
            Debug.Log($"[CustomerSpawner] 次の生成まで {spawnInterval} 秒待機します...");
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 空いている席があれば客を1人生成して、その席に向かわせる
    /// </summary>
    private void TrySpawnCustomer()
    {
        // 1) 空いている席を探す
        SeatPoint freeSeat = GetFreeSeat();
        if (freeSeat == null)
        {
            Debug.Log("空いている椅子がないため、これ以上客を生成しません。");
            return;
        }

        // 2) スポーン位置で客プレハブを生成
        // NavMesh上の正しい位置を探す（空中に浮いたり埋まったりしないように）
        Vector3 finalSpawnPos = spawnPoint.position;
        NavMeshHit hit;
        // 半径20m以内で一番近いNavMeshを探す（範囲を拡大）
        if (NavMesh.SamplePosition(spawnPoint.position, out hit, 20.0f, NavMesh.AllAreas))
        {
            finalSpawnPos = hit.position;
        }
        else
        {
            Debug.LogError($"[CustomerSpawner] スポーン地点 ( {spawnPoint.position} ) の近く(20m以内)にNavMeshが見つかりません！床の上か、青いメッシュの近くに配置してください。");
            return; // そもそも生成しない
        }

        GameObject obj = Instantiate(
            customerPrefab,
            finalSpawnPos,
            spawnPoint.rotation
        );

        // 効果音再生
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, finalSpawnPos);
        }

        // 3) 生成した客を目標の席に向かわせる（到着後に座る）
        CustomerSitting customer = obj.GetComponent<CustomerSitting>();
        if (customer != null)
        {
            // NavMeshAgentを一旦Warpさせて確実にNavMeshに乗せる
            NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                // エージェントを有効化する前に位置を強制補正
                agent.Warp(finalSpawnPos);
                
                if (!agent.isOnNavMesh)
                {
                    Debug.LogError("致命的エラー: 生成した客がNavMeshに乗っていません。NavMeshをBakeしてください！");
                    Destroy(obj);
                    return;
                }
            }

            customer.GoToSeat(freeSeat);
        }
        else
        {
            Debug.LogWarning("CustomerSitting スクリプトが客プレハブにアタッチされていません！");
        }
    }

    /// <summary>
    /// 配列 seats の中から「未使用の席」を1つ返す。なければ null。
    /// </summary>
    private SeatPoint GetFreeSeat()
    {
        if (seats == null) return null;

        foreach (var seat in seats)
        {
            // seat自体がnull（未設定など）の場合はスキップ
            if (seat != null && !seat.isOccupied)
            {
                return seat;
            }
        }
        // 全ての席が埋まっている場合
        return null;
    }
}
