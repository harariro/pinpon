using UnityEngine;

namespace PingPongGame
{
    /// <summary>
    /// ボールの物理移動のみを担当（シンプル版）
    /// </summary>
    public class SimpleBallController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 2.5f;

        // 卓球台の範囲
        public const float TABLE_MIN_X = -1.5f;
        public const float TABLE_MAX_X = 1.5f;
        public const float TABLE_SAFE_MARGIN = 0.1f; // 安全マージン

        private Vector3 currentPosition;
        private Vector3 direction;
        private bool isMoving = false;

        // 公開プロパティ
        public Vector3 CurrentPosition => currentPosition;
        public Vector3 Direction => direction;
        public bool IsMoving => isMoving;

        private void Start()
        {
            // 初期位置: Z=-4
            currentPosition = new Vector3(0f, 1.2f, -4f);
            transform.position = currentPosition;
            transform.localScale = Vector3.one * 0.3f;

            Debug.Log($"[Ball] Initialized at {currentPosition}");
        }

        private void Update()
        {
            if (!isMoving) return;

            // 線形移動
            float distance = moveSpeed * Time.deltaTime;
            currentPosition += direction * distance;
            transform.position = currentPosition;
        }

        /// <summary>
        /// 移動開始
        /// </summary>
        public void StartMove(Vector3 startPos, Vector3 targetPos)
        {
            currentPosition = startPos;
            transform.position = currentPosition;

            direction = (targetPos - startPos).normalized;
            isMoving = true;

            Debug.Log($"[Ball] StartMove: {startPos} → {targetPos}, Direction: {direction}");
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            isMoving = false;
            Debug.Log($"[Ball] Stopped at {currentPosition}");
        }

        /// <summary>
        /// 方向を設定（反射用、Perfect補正用）
        /// </summary>
        public void SetDirection(Vector3 newDirection)
        {
            direction = newDirection.normalized;
            Debug.Log($"[Ball] ★ Direction set to {direction} (X={direction.x:F3}, Z={direction.z:F3})");
        }

        /// <summary>
        /// Y位置を設定
        /// </summary>
        public void SetYPosition(float y)
        {
            currentPosition.y = y;
            transform.position = currentPosition;
        }

        /// <summary>
        /// 指定Z座標到達時の位置を予測
        /// </summary>
        /// <param name="targetZ">目標Z座標</param>
        /// <returns>予測位置</returns>
        public Vector3 PredictPositionAtZ(float targetZ)
        {
            if (Mathf.Abs(direction.z) < 0.001f)
            {
                // Z方向の移動がほぼない場合は現在位置を返す
                return currentPosition;
            }

            // targetZまでの移動時間を計算
            float deltaZ = targetZ - currentPosition.z;
            float time = deltaZ / direction.z;

            // 予測位置を計算
            Vector3 predictedPos = currentPosition + direction * time;

            return predictedPos;
        }

        /// <summary>
        /// 返球（Z方向反転 + X方向角度計算 + カーブ効果）
        /// </summary>
        /// <param name="racketX">ラケットのX座標</param>
        /// <param name="curveEffect">カーブ効果（デフォルト0）</param>
        public void ReturnBall(float racketX, float curveEffect = 0f)
        {
            // ボールとラケットのX位置差で基本角度を決定
            float ballX = currentPosition.x;
            float xOffset = racketX - ballX;

            // 角度係数（0.5 = 適度な角度変化）
            float angleMultiplier = 0.5f;
            float baseDirectionX = xOffset * angleMultiplier;

            // カーブ効果を加算
            float directionX = baseDirectionX + curveEffect;

            // 新しい方向ベクトル（X方向 + Z方向）
            Vector3 newDir = new Vector3(directionX, 0, 1f);
            direction = newDir.normalized;

            Debug.Log($"[Ball] ★ PLAYER RETURN (Good/Normal)");
            Debug.Log($"[Ball]   Ball X: {ballX:F2}, Racket X: {racketX:F2}, Offset: {xOffset:F2}");
            Debug.Log($"[Ball]   Curve Effect: {curveEffect:F2}, Total Direction X: {directionX:F2}");
            Debug.Log($"[Ball]   New Direction: {direction} (X={direction.x:F3}, Z={direction.z:F3})");
        }
    }
}
