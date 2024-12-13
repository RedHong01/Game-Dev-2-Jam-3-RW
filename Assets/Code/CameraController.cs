using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 控制摄像机移动的速度
    public float speed = 5f;

    // 控制屏幕边缘的检测范围
    public float edgeThreshold = 0.05f;

    // 摄像头 Y 轴滚动灵敏度
    public float scrollSensitivity = 5f;

    // 摄像头的死区
    public Vector3 minPosition;
    public Vector3 maxPosition;

    // 摄像机初始位置，用于重置
    private Vector3 initialPosition;

    // 摄像机锁定状态
    private bool isLocked = false;

    // 标记是否在拖拽游戏盘
    private bool isDraggingGameBoard = false;

    void Start()
    {
        // 记录摄像头初始位置
        initialPosition = transform.position;
    }

    void Update()
    {
        // 处理拖拽操作标志，防止与游戏盘操作冲突
        if (Input.GetMouseButtonDown(0))
        {
            isDraggingGameBoard = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDraggingGameBoard = false;
        }

        // 切换锁定状态
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isLocked = !isLocked;
        }

        // 如果没有锁定并且没有拖拽游戏盘，处理移动和滚轮缩放
        if (!isLocked && !isDraggingGameBoard)
        {
            // 处理鼠标屏幕边缘的移动
            HandleEdgeMovement();

            // 处理鼠标滚轮Y轴调整
            HandleScrollZoom();
        }

        // 检测键盘 C 键重置摄像头位置
        if (Input.GetKeyDown(KeyCode.C))
        {
            ResetCameraPosition();
        }

        // 限制摄像机的位置在指定范围内
        ClampCameraPosition();
    }

    // 处理屏幕边缘的移动逻辑
    void HandleEdgeMovement()
    {
        if (Input.mousePosition.x >= Screen.width * (1 - edgeThreshold))
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }
        if (Input.mousePosition.x <= Screen.width * edgeThreshold)
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
        }
        if (Input.mousePosition.y >= Screen.height * (1 - edgeThreshold))
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
        }
        if (Input.mousePosition.y <= Screen.height * edgeThreshold)
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
        }
    }

    // 鼠标滚轮控制摄像头Y轴高度
    void HandleScrollZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Vector3 newPosition = transform.position;
        newPosition.y += scrollInput * scrollSensitivity;
        transform.position = newPosition;
    }

    // 限制摄像机的位置在死区范围内
    void ClampCameraPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minPosition.x, maxPosition.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minPosition.y, maxPosition.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minPosition.z, maxPosition.z);
        transform.position = clampedPosition;
    }

    // 重置摄像头位置到初始状态
    void ResetCameraPosition()
    {
        transform.position = initialPosition;
    }
}