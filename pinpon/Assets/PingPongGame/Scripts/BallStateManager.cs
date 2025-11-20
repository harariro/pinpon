using UnityEngine;

namespace PingPongGame
{
    /// <summary>
    /// ボールのZ座標を監視し、状態管理を行う（シンプル版）
    /// </summary>
    public class BallStateManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleBallController ball;
        [SerializeField] private SimplePlayerRacket playerRacket;
        [SerializeField] private SimpleAIRacket aiRacket;

        // Z座標の境界値
        private const float Z_AI_RACKET = 4.0f;
        private const float Z_AI_TABLE = 2.0f;
        private const float Z_NET = 0.0f;
        private const float Z_PLAYER_TABLE = -2.0f;
        private const float Z_PLAYER_RACKET = -4.0f;
        private const float Z_FAIL_BOUNDARY = -5.0f;

        // Y座標の設定値
        private const float Y_AT_AI_RACKET = 1.1f;
        private const float Y_AT_AI_TABLE = 0.8f;
        private const float Y_AT_NET = 1.2f;
        private const float Y_AT_PLAYER_TABLE = 0.8f;
        private const float Y_AT_PLAYER_RACKET = 1.1f;
        private const float Y_AT_FAIL = 1.2f;

        // 境界通過フラグ
        private bool passedAIRacket = false;
        private bool passedAITable = false;
        private bool passedPlayerTable = false;
        private bool passedPlayerRacket = false;
        private bool passedFailBoundary = false;

        private float previousZ = 0f;

        private void Start()
        {
            Debug.Log("[BallState] BallStateManager initialized");

            // 自動参照取得
            if (ball == null) ball = FindObjectOfType<SimpleBallController>();
            if (playerRacket == null) playerRacket = FindObjectOfType<SimplePlayerRacket>();
            if (aiRacket == null) aiRacket = FindObjectOfType<SimpleAIRacket>();

            if (ball == null) Debug.LogError("[BallState] SimpleBallController not found!");
            if (playerRacket == null) Debug.LogError("[BallState] SimplePlayerRacket not found!");
            if (aiRacket == null) Debug.LogError("[BallState] SimpleAIRacket not found!");
        }

        private void Update()
        {
            if (ball == null || !ball.IsMoving) return;

            float currentZ = ball.CurrentPosition.z;
            CheckBoundaries(currentZ, previousZ);
            previousZ = currentZ;
        }

        private void CheckBoundaries(float currentZ, float prevZ)
        {
            Vector3 direction = ball.Direction;

            // === Z=4: AI反射 ===
            if (!passedAIRacket && prevZ < Z_AI_RACKET && currentZ >= Z_AI_RACKET && direction.z > 0)
            {
                passedAIRacket = true;
                float gaugeValue = Mathf.InverseLerp(-4f, 4f, currentZ);
                Debug.Log($"[BallState] ★ Z=4 AI RACKET - Triggering reflection (Gauge: {gaugeValue:F3})");

                // Y位置設定
                ball.SetYPosition(Y_AT_AI_RACKET);
                Debug.Log($"[BallState] → Y position set to {Y_AT_AI_RACKET}");

                // Z方向反転（AI反射）
                Vector3 newDir = ball.Direction;
                newDir.z = -Mathf.Abs(newDir.z); // 必ずマイナス方向

                // ★ 軌道予測と修正（Z=-2での着地点を卓球台内に収める）
                // 反転後のnewDirを使って予測
                float deltaZ = Z_PLAYER_TABLE - ball.CurrentPosition.z; // -2 - 4 = -6
                float time = deltaZ / newDir.z;
                Vector3 predictedPos = ball.CurrentPosition + newDir * time;
                float predictedX = predictedPos.x;

                // 卓球台範囲外なら修正
                if (predictedX < SimpleBallController.TABLE_MIN_X || predictedX > SimpleBallController.TABLE_MAX_X)
                {
                    // 安全マージンを考慮した目標X座標
                    float targetX = Mathf.Clamp(predictedX,
                        SimpleBallController.TABLE_MIN_X + SimpleBallController.TABLE_SAFE_MARGIN,
                        SimpleBallController.TABLE_MAX_X - SimpleBallController.TABLE_SAFE_MARGIN);

                    // 目標X座標に到達するための必要なX方向成分を再計算
                    float requiredDirX = (targetX - ball.CurrentPosition.x) / time;

                    // 方向ベクトルを更新
                    newDir.x = requiredDirX;
                    newDir = newDir.normalized;

                    Debug.Log($"[BallState] → Trajectory corrected!");
                    Debug.Log($"[BallState]   Predicted X at Z=-2: {predictedX:F2} → Target X: {targetX:F2}");
                    Debug.Log($"[BallState]   New Direction: {newDir} (X={newDir.x:F3}, Z={newDir.z:F3})");
                }
                else
                {
                    Debug.Log($"[BallState] → Trajectory OK: Will land at X={predictedX:F2} (within table)");
                }

                ball.SetDirection(newDir);

                // AIラケットを移動
                if (aiRacket != null)
                {
                    aiRacket.MoveTo(ball.CurrentPosition.x);
                }

                // プレイヤーラケットに追跡開始を通知
                if (playerRacket != null)
                {
                    playerRacket.StartTracking(ball.CurrentPosition.x);
                }

                // プレイヤー側のフラグをリセット（次の検出を有効化）
                passedPlayerTable = false;
                passedPlayerRacket = false;
                passedFailBoundary = false;
                Debug.Log("[BallState] → Player-side flags reset for next detection");
            }

            // === Z=2: AIテーブルバウンド ===
            if (!passedAITable && prevZ < Z_AI_TABLE && currentZ >= Z_AI_TABLE && direction.z > 0)
            {
                passedAITable = true;
                float gaugeValue = Mathf.InverseLerp(-4f, 4f, currentZ);

                // ★ バウンド位置が卓球台外ならプレイヤー失敗
                float ballX = ball.CurrentPosition.x;
                if (ballX < SimpleBallController.TABLE_MIN_X || ballX > SimpleBallController.TABLE_MAX_X)
                {
                    Debug.Log($"[BallState] ★ Z=2 OUT OF BOUNDS - Ball landed at X={ballX:F2} (outside table, Gauge: {gaugeValue:F3})");
                    Debug.Log($"[BallState] → PLAYER FAILED: Return went off-table");

                    // ボール停止
                    ball.SetYPosition(Y_AT_AI_TABLE);
                    ball.Stop();

                    // プレイヤーラケット追跡停止
                    if (playerRacket != null)
                    {
                        playerRacket.StopTracking();
                    }

                    // プレイヤー失敗処理
                    if (SimpleGameController.Instance != null)
                    {
                        SimpleGameController.Instance.OnPlayerFailed();
                    }
                    return;
                }

                // 卓球台内なら通常処理
                Debug.Log($"[BallState] ★ Z=2 AI TABLE - Bounce should occur (Gauge: {gaugeValue:F3})");
                Debug.Log($"[BallState] → Ball landed at X={ballX:F2} (within table)");

                // Y位置設定
                ball.SetYPosition(Y_AT_AI_TABLE);
                Debug.Log($"[BallState] → Y position set to {Y_AT_AI_TABLE}");
            }

            // === Z=-2: プレイヤーテーブルバウンド ===
            if (!passedPlayerTable && prevZ > Z_PLAYER_TABLE && currentZ <= Z_PLAYER_TABLE && direction.z < 0)
            {
                passedPlayerTable = true;
                float gaugeValue = Mathf.InverseLerp(-4f, 4f, currentZ);
                Debug.Log($"[BallState] ★ Z=-2 PLAYER TABLE - Bounce should occur (Gauge: {gaugeValue:F3})");

                // Y位置設定
                ball.SetYPosition(Y_AT_PLAYER_TABLE);
                Debug.Log($"[BallState] → Y position set to {Y_AT_PLAYER_TABLE}");
            }

            // === Z=-4: プレイヤーラケット位置通過（返球待ち） ===
            if (!passedPlayerRacket && prevZ > Z_PLAYER_RACKET && currentZ <= Z_PLAYER_RACKET && direction.z < 0)
            {
                passedPlayerRacket = true;
                float gaugeValue = Mathf.InverseLerp(-4f, 4f, currentZ);
                Debug.Log($"[BallState] ★ Z=-4 PLAYER RACKET POSITION - Waiting for input (Gauge: {gaugeValue:F3})");

                // Y位置設定
                ball.SetYPosition(Y_AT_PLAYER_RACKET);
                Debug.Log($"[BallState] → Y position set to {Y_AT_PLAYER_RACKET}");

                // プレイヤー入力待ち
                // SimplePlayerRacketが左クリックを検出して返球処理を行う
                // 返球失敗時はZ=-5到達で自動失敗
            }

            // === Z=-5: 失敗境界 ===
            if (!passedFailBoundary && currentZ < Z_FAIL_BOUNDARY && direction.z < 0)
            {
                passedFailBoundary = true;
                float gaugeValue = Mathf.InverseLerp(-4f, 4f, currentZ);
                Debug.LogWarning($"[BallState] ★ Z=-5 FAIL BOUNDARY - AUTO FAIL! (Gauge: {gaugeValue:F3})");

                // Y位置設定
                ball.SetYPosition(Y_AT_FAIL);
                Debug.Log($"[BallState] → Y position set to {Y_AT_FAIL}");

                // ボール停止
                ball.Stop();

                // プレイヤーラケットに追跡停止を通知
                if (playerRacket != null)
                {
                    playerRacket.StopTracking();
                }

                // SimpleGameControllerに失敗を通知
                if (SimpleGameController.Instance != null)
                {
                    SimpleGameController.Instance.OnPlayerFailed();
                }
            }

            // === Z=5 または Z=-6: ゲーム破綻（強制リセット） ===
            if (currentZ > 5.0f || currentZ < -6.0f)
            {
                Debug.LogError($"[BallState] ★★★ GAME BROKEN! Ball at Z={currentZ:F2} - FORCING RESET ★★★");

                // ボール停止
                ball.Stop();

                // プレイヤーラケット追跡停止
                if (playerRacket != null)
                {
                    playerRacket.StopTracking();
                }

                // 状態リセット
                ResetState();

                // ボールを初期位置に戻す
                ball.StartMove(new Vector3(0f, 1.2f, -4f), new Vector3(0f, 1.2f, -4f));
                ball.Stop();

                Debug.Log("[BallState] → Ball reset to starting position. Right-click to restart.");
            }
        }

        /// <summary>
        /// 状態リセット（新しいサーブ時に呼ぶ）
        /// </summary>
        public void ResetState()
        {
            passedAIRacket = false;
            passedAITable = false;
            passedPlayerTable = false;
            passedPlayerRacket = false;
            passedFailBoundary = false;
            previousZ = 0f;
            Debug.Log("[BallState] State reset");
        }

        /// <summary>
        /// プレイヤー返球成功時に呼ばれる（SimplePlayerRacketから）
        /// </summary>
        public void OnPlayerReturnSuccess()
        {
            // AI側のフラグをリセット（次の検出を有効化）
            passedAIRacket = false;
            passedAITable = false;
            Debug.Log("[BallState] → AI-side flags reset (player return success)");
        }
    }
}
