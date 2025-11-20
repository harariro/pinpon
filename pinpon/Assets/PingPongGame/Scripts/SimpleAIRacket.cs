using UnityEngine;

namespace PingPongGame
{
    /// <summary>
    /// AIラケット（シンプル版）
    /// </summary>
    public class SimpleAIRacket : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5f;

        private const float FIXED_Z = 4f;
        private const float FIXED_Y = 1.2f;

        private float targetX = 0f;
        private bool isMoving = false;

        private void Start()
        {
            // 初期位置
            transform.position = new Vector3(0f, FIXED_Y, FIXED_Z);
            Debug.Log($"[AIRacket] Initialized at {transform.position}");
        }

        private void Update()
        {
            if (!isMoving) return;

            // ターゲットX座標に移動
            Vector3 currentPos = transform.position;
            float newX = Mathf.MoveTowards(currentPos.x, targetX, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, FIXED_Y, FIXED_Z);

            // 到達判定
            if (Mathf.Abs(newX - targetX) < 0.01f)
            {
                isMoving = false;
            }
        }

        /// <summary>
        /// 指定X座標に移動
        /// </summary>
        public void MoveTo(float x)
        {
            targetX = x;
            isMoving = true;
            Debug.Log($"[AIRacket] Moving to X={targetX:F2}");
        }
    }
}
