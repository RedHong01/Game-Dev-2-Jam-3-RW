using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : Lives
{
    public float moveSpeed = 5f; // 玩家移动速度
    private Camera viewCamera;
    private PlayerController controller;
    private GunController gunController;

    public static PlayerData LastPlayerData { get; private set; } // 静态字段用于存储最后一次玩家数据

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        health = startingHealth; // 初始化血量


        // 如果有上一次保存的玩家数据，恢复到当前玩家
        if (LastPlayerData != null)
        {
            RestoreData(LastPlayerData);
        }
    }

    void Update()
    {
        // 玩家移动输入
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // 玩家面朝方向
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            controller.LookAt(point);
        }

        // 开火
        if (Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }
    }

    protected override void Die()
    {
        SavePlayerData(); // 保存当前玩家数据
        base.Die(); // 调用基类的死亡逻辑
    }

    private void SavePlayerData()
    {
        LastPlayerData = new PlayerData
        {
            health = health,
            moveSpeed = moveSpeed,
            position = transform.position,
            rotation = transform.rotation
        };
    }

    private void RestoreData(PlayerData data)
    {
        health = Mathf.Clamp(data.health, 0, startingHealth); // 恢复血量，确保在范围内
        moveSpeed = data.moveSpeed;
        transform.position = data.position;
        transform.rotation = data.rotation;
    }
}

[System.Serializable]
public class PlayerData
{
    public float health; // 当前血量
    public float moveSpeed; // 移动速度
    public Vector3 position; // 位置信息
    public Quaternion rotation; // 旋转信息
}