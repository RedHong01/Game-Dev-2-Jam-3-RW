using System.Collections.Generic;
using UnityEngine;
using TMPro;  // 用于更新 TextMeshPro 的血量显示
using UnityEngine.UI;  // 引入传统 UI 组件

public class GameResetWithBackups : MonoBehaviour
{
    public List<GameObject> gameObjectsToBackup;  // List of important game objects to reset
    private List<GameObject> backupObjects;  // Stores inactive backups of these game objects
    public Button Button;  // 引用 UI 按钮

    void Start()
    {
        CreateBackups();  // Create backups at the start of the game
        if (Button != null)
        {
            Button.onClick.AddListener(OnButtonClicked);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            DestroyAllEnemies();
            ResetGame();
        }
    }

    void OnButtonClicked()
    {
        DestroyAllEnemies();
        ResetGame();
    }

    // Create inactive backups of all important game objects
    void CreateBackups()
    {
        backupObjects = new List<GameObject>();

        foreach (GameObject obj in gameObjectsToBackup)
        {
            GameObject backup = Instantiate(obj, obj.transform.position, obj.transform.rotation);
            backup.SetActive(false);  // Keep the backup inactive
            backupObjects.Add(backup);
        }
    }

    // Reset the game by deactivating current objects and activating backups
    void ResetGame()
    {
        // Deactivate current game objects
        foreach (GameObject obj in gameObjectsToBackup)
        {
            obj.SetActive(false);
        }

        // Activate backup objects
        foreach (GameObject backup in backupObjects)
        {
            backup.SetActive(true);
        }

        // Clear the current active objects list and create new backups from the newly activated objects
        gameObjectsToBackup = new List<GameObject>(backupObjects);
        CreateBackups();
    }
    void DestroyAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  // 找到所有带有 "Enemy" 标签的对象

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);  // 销毁这些对象
        }
    }
}
