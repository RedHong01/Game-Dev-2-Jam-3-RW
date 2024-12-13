using UnityEngine;
using System;

public class Lives : MonoBehaviour, Damageable
{
    public float startingHealth; // 初始血量
    public float health; // 当前血量
    protected bool dead; // 是否已死亡

    public event Action OnDeath; // 死亡事件

    protected virtual void Start()
    {
        health = startingHealth; // 初始化血量
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        // 此处可以扩展命中逻辑（如特效、声音等）
        TakeDamage(damage); // 直接调用伤害处理
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage; // 扣减血量

        if (health <= 0 && !dead)
        {
            Die(); // 血量为零且未死亡时调用死亡逻辑
        }
    }

    [ContextMenu("Self Destruct")]
    protected virtual void Die()
    {
        dead = true;
        OnDeath?.Invoke(); // 触发死亡事件
        Destroy(gameObject); // 销毁当前对象
    }
}