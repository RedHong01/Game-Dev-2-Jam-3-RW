using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public Enemy enemy;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBetweenCampingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    void Start()
    {
        nextCampCheckTime = timeBetweenCampingChecks + Time.time;

        // 如果玩家存在，初始化玩家位置
        if (GameManager.CurrentPlayerTransform != null)
        {
            campPositionOld = GameManager.CurrentPlayerTransform.position;
        }

        map = FindObjectOfType<MapGenerator>();

        // 初始敌人数量和生成逻辑
        enemiesRemainingToSpawn = 10; // 设置默认生成数量
        enemiesRemainingAlive = enemiesRemainingToSpawn;
    }

    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                // 检查玩家是否存活
                if (GameManager.IsPlayerAlive && GameManager.CurrentPlayerTransform != null)
                {
                    isCamping = (Vector3.Distance(GameManager.CurrentPlayerTransform.position, campPositionOld) < campThresholdDistance);
                    campPositionOld = GameManager.CurrentPlayerTransform.position;
                }
                else
                {
                    isCamping = false; // 如果玩家不存在，设为非露营状态
                }
            }

            if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + 1f; // 默认生成间隔
                StartCoroutine(SpawnEnemy());
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        Transform spawnTile = map.GetRandomOpenTile();

        // 如果玩家存在，可能使用玩家位置作为生成逻辑
        if (GameManager.IsPlayerAlive && GameManager.CurrentPlayerTransform != null && isCamping)
        {
            spawnTile = map.GetTileFromPosition(GameManager.CurrentPlayerTransform.position);
        }

        // 直接生成敌人
        yield return new WaitForSeconds(1f); // 默认生成延迟
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

        if (enemiesRemainingAlive == 0)
        {
            Debug.Log("All enemies defeated!");
        }
    }
}