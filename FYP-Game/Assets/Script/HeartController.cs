using UnityEngine;

public class HeartbeatController : MonoBehaviour
{
    [Header("Heartbeat Settings")]
    public AudioSource heartbeat;      // 心跳 AudioSource（挂在 Player 身上）
    public float maxDistance = 20f;    // 超过这个距离就不播放
    public float minPitch = 0.8f;      // 最慢心跳
    public float maxPitch = 2.0f;      // 最近时最快心跳
    public float minVolume = 0.0f;     // 最小音量（远处）
    public float maxVolume = 1.0f;     // 最大音量（近处）

    [Header("Game Over")]
    public GameObject gameOverPanel;   // 拖你的 Game Over Panel 进来

    [Header("Enemies")]
    public Transform[] enemies;        // ✅ 手动拖拽敌人到这里

    [Header("Player Reference")]
    public Transform player;           // 玩家（即使隐藏也要引用）
    private Vector3 lastKnownPosition; // 玩家最后出现的位置

    void Start()
    {
        if (player != null)
            lastKnownPosition = player.position; // 初始化
    }

    void Update()
    {
        if (heartbeat == null || enemies.Length == 0 || player == null) return;

        // 🔴 Game Over 时，心跳立即停止
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            if (heartbeat.isPlaying)
                heartbeat.Stop();
            return;
        }

        // 玩家激活时，更新最后位置
        if (player.gameObject.activeInHierarchy)
        {
            lastKnownPosition = player.position;
        }

        float closestDistance = Mathf.Infinity;

        // 遍历所有手动拖拽的敌人
        foreach (Transform enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(lastKnownPosition, enemy.position);
            if (dist < closestDistance)
                closestDistance = dist;
        }

        if (closestDistance <= maxDistance)
        {
            if (!heartbeat.isPlaying)
                heartbeat.Play();

            // 越近，心跳越快、声音越大
            float t = 1 - (closestDistance / maxDistance);
            heartbeat.volume = Mathf.Lerp(minVolume, maxVolume, t);
            heartbeat.pitch = Mathf.Lerp(minPitch, maxPitch, t);
        }
        else
        {
            if (heartbeat.isPlaying)
                heartbeat.Stop();
        }
    }
}
