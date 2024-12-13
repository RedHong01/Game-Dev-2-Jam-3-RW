using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static Transform CurrentPlayerTransform { get; private set; } // 公共静态变量，保存当前玩家的 Transform
    
    private bool IsPlayerRespawning = false; // 标记玩家是否处于重生状态
    [Header("Timer Settings")]
    public float countdownTime = 60f; // 倒计时设置
    public string countdownEndSceneName; // 倒计时结束时的场景名称
    public TextMeshProUGUI countdownText; // TextMeshPro UI 元素

    [Header("Player Settings")]
    public GameObject playerPrefab; // 玩家预制件
    public Transform playerSpawnPoint; // 玩家生成点
    private Vector3 spawnPosition; // 记录玩家生成点的位置
    private Quaternion spawnRotation; // 记录玩家生成点的旋转
    private GameObject playerInstance; // 玩家实例
    public string playerDeathSceneName; // 玩家死亡后的场景名称

    [Header("Destroy Zone Settings")]
    public Collider destroyZone; // 摧毁区域的 Collider
    public string playerTag = "Player"; // 用于检测的玩家标签

    [Header("Score Settings")]
    public int score = 0; // 游戏分数
    public static bool IsPlayerAlive { get; private set; } = true; // 标记玩家是否存活

   IEnumerator RespawnPlayerAfterDestroy()
{
    yield return new WaitForSeconds(2f); // 重生延迟，可根据需求调整

    // 生成新的玩家
    SpawnPlayer();

    // 更新状态变量
    IsPlayerRespawning = false; // 重生完成
    IsPlayerAlive = true; // 玩家恢复活跃状态
}

    void Start()
    {
        // 初始化生成点位置
        if (playerSpawnPoint != null)
        {
            spawnPosition = playerSpawnPoint.position;
            spawnRotation = playerSpawnPoint.rotation;
        }
        else if (playerPrefab != null)
        {
            Debug.LogWarning("PlayerSpawnPoint is not set. Using playerPrefab's default position.");
            spawnPosition = playerPrefab.transform.position;
            spawnRotation = playerPrefab.transform.rotation;
        }
        else
        {
            Debug.LogError("PlayerSpawnPoint and PlayerPrefab are both not set. Cannot initialize spawn position.");
            return; // 无法生成玩家，退出逻辑
        }

        // 初始化玩家
        SpawnPlayer();
    }

    void Update()
    {
        HandleCountdown();
        CheckPlayerHealth();
    }

    void HandleCountdown()
    {
        if (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;

            if (countdownText != null)
            {
                countdownText.text = "Time Remaining: " + Mathf.CeilToInt(countdownTime).ToString();
            }
        }
        else
        {
            SwitchToScene(countdownEndSceneName);
        }
    }

    void CheckPlayerHealth()
{
    if (IsPlayerRespawning)
    {
        Debug.Log("Player is respawning, skipping health check.");
        return; // 玩家正在重生，跳过健康检查
    }

    else if (!IsPlayerAlive || (CurrentPlayerTransform != null && CurrentPlayerTransform.GetComponent<Lives>().health <= 0))
    {
        Debug.Log("Player HP is zero or not alive, triggering game over.");
        SwitchToScene(playerDeathSceneName); // 切换到失败场景
    }
}

    void SwitchToScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is not set.");
        }
    }

    public void AddScore()
    {
        score += 1;
        Debug.Log("Score: " + score);
    }

    void SpawnPlayer()
{
    // 查找所有带有 "Player" 标签的对象
    GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(playerTag);

    GameObject activePlayer = null;
    GameObject inactivePlayer = null;

    // 分类活跃和非活跃玩家
    foreach (GameObject obj in playerObjects)
    {
        if (obj.activeSelf)
        {
            activePlayer = obj; // 找到活跃的玩家
        }
        else
        {
            inactivePlayer = obj; // 找到非活跃的玩家
        }
    }

    // 如果已经有一个活跃玩家并且其 health > 0，则不生成新的玩家
    if (activePlayer != null)
    {
        var lives = activePlayer.GetComponent<Lives>();
        if (lives != null && lives.health > 0)
        {
            Debug.Log("Active player already exists with sufficient health. No new player spawned.");
            return;
        }
    }

    // 如果存在非激活玩家，则激活它
    if (inactivePlayer != null)
    {
        inactivePlayer.transform.position = spawnPosition;
        inactivePlayer.transform.rotation = spawnRotation;
        inactivePlayer.SetActive(true);

        // 更新当前玩家的 Transform
        CurrentPlayerTransform = inactivePlayer.transform;
        Debug.Log("Inactive player reactivated: " + CurrentPlayerTransform.name);
        return;
    }

    // 如果没有找到任何玩家对象，则生成一个新玩家
    if (playerPrefab != null)
    {
        playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);

        // 更新当前玩家的 Transform
        CurrentPlayerTransform = playerInstance.transform;

        playerInstance.SetActive(true);
        Debug.Log("Player spawned and activated at: " + spawnPosition);
    }
    else
    {
        Debug.LogError("PlayerPrefab is not set. Cannot spawn player.");
    }
}

    private void OnTriggerEnter(Collider other)
    {
        if (destroyZone != null && other.CompareTag(playerTag))
        {
            Debug.Log($"Player entered DestroyZone: {other.gameObject.name}");
            DestroyPlayer(other.gameObject);
        }
    }

    void DestroyPlayer(GameObject player)
{
    if (player != null)
    {
        IsPlayerAlive = false; // 标记玩家不活跃
        IsPlayerRespawning = true; // 标记玩家正在重生

        // 获取 Lives 组件并触发死亡逻辑
        var lives = player.GetComponent<Lives>();
        if (lives != null)
        {
            lives.TakeDamage(lives.health); // 让玩家触发死亡逻辑
        }

        Destroy(player); // 销毁当前玩家对象
        CurrentPlayerTransform = null; // 清空引用
    }

    // 启动重生流程
    StartCoroutine(RespawnPlayerAfterDestroy());
}
}