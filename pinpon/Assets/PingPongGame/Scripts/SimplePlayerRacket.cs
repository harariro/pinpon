using UnityEngine;

namespace PingPongGame
{
    /// <summary>
    /// プレイヤーラケット（マウス操作版）
    /// </summary>
    public class SimplePlayerRacket : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float dragCurveMultiplier = 1.5f; // ドラッグからカーブへの変換係数
        [SerializeField] private float swingSpeed = 5f; // 右クリック時のスイング速度
        [SerializeField] private float swingCurveMultiplier = 0.3f; // スイング速度からカーブへの変換係数
        [SerializeField] private float hitDistance = 0.3f; // ボールとの接触判定距離

        private const float FIXED_Z = -4f;
        private const float FIXED_Y = 1.2f;
        private const float SWING_START_Z = -4.5f; // スイング開始位置
        private const float SWING_END_Z = -3.5f;   // スイング終了位置

        // タイミング判定範囲（固定）
        private const float VALID_RANGE_MIN = -4.5f;
        private const float VALID_RANGE_MAX = -3.5f;

        private SimpleBallController ball;
        private BallStateManager stateManager;
        private bool canReturn = false; // 返球可能フラグ
        private bool isWaitingForReturn = false; // 返球待機中

        // 左クリック&ドラッグ用（従来のタイミング返球）
        private bool isLeftClickHeld = false;
        private Vector3 leftClickStartWorldPos;

        // 右クリックカーブ用
        private bool isRightClickHeld = false;
        private float racketSwingZ; // スイング中のZ座標
        private Vector3 previousMouseWorldPos; // 前フレームのマウス位置
        private float mouseVelocityX; // マウスX方向速度

        // 移動範囲（カメラビューポートから計算）
        private float minX;
        private float maxX;

        private void Start()
        {
            // カメラビューポートから移動範囲を計算
            CalculateMovementBounds();

            // 初期位置
            transform.position = new Vector3(0f, FIXED_Y, FIXED_Z);
            Debug.Log($"[PlayerRacket] Initialized at {transform.position}");
            Debug.Log($"[PlayerRacket] Movement range: X={minX:F2} to {maxX:F2}");

            // 参照取得
            ball = FindObjectOfType<SimpleBallController>();
            stateManager = FindObjectOfType<BallStateManager>();

            if (ball == null) Debug.LogError("[PlayerRacket] SimpleBallController not found!");
            if (stateManager == null) Debug.LogError("[PlayerRacket] BallStateManager not found!");
        }

        private void CalculateMovementBounds()
        {
            // カメラのビューポート端をワールド座標に変換
            Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 10f));
            Vector3 bottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 10f));

            minX = bottomLeft.x + 0.3f;  // 少しマージンを取る
            maxX = bottomRight.x - 0.3f;
        }

        private void Update()
        {
            // 返球可能範囲のチェック
            if (ball != null && ball.IsMoving && isWaitingForReturn)
            {
                float ballZ = ball.CurrentPosition.z;
                canReturn = (ballZ >= VALID_RANGE_MIN && ballZ <= VALID_RANGE_MAX);
            }
            else
            {
                canReturn = false;
            }

            // 右クリック優先（カーブ返球）
            // ボールが移動中の場合のみスイングモード開始（停止中はSimpleGameControllerがサーブ処理）
            if (Input.GetMouseButtonDown(1))
            {
                bool isBallMoving = ball != null && ball.IsMoving;
                float ballZ = isBallMoving ? ball.CurrentPosition.z : -999f;
                Debug.Log($"[PlayerRacket] ★★★ RIGHT CLICK DETECTED! Ball moving={isBallMoving}, ballZ={ballZ:F2}, isRightClickHeld={isRightClickHeld}");

                if (isBallMoving && !isRightClickHeld)
                {
                    StartSwingMode();
                }
                else
                {
                    Debug.Log($"[PlayerRacket] → RIGHT CLICK IGNORED (Ball moving={isBallMoving}, isRightClickHeld={isRightClickHeld})");
                }
            }

            if (isRightClickHeld)
            {
                UpdateSwingMode();

                // 右クリックリリース検出
                if (Input.GetMouseButtonUp(1))
                {
                    EndSwingMode(false); // リリースで終了
                }
            }
            else
            {
                // 通常モード：マウス操作でラケット移動
                UpdateRacketPosition();

                // 左クリック押下検出（従来のタイミング返球）
                if (Input.GetMouseButtonDown(0) && canReturn && !isLeftClickHeld)
                {
                    isLeftClickHeld = true;
                    leftClickStartWorldPos = GetMouseWorldPosition();
                }

                // 左クリックリリース検出（ドラッグ終了→返球）
                if (Input.GetMouseButtonUp(0) && isLeftClickHeld && canReturn)
                {
                    isLeftClickHeld = false;
                    TryReturnWithDrag();
                }

                // 返球範囲外になったらクリック状態をリセット
                if (!canReturn && isLeftClickHeld)
                {
                    isLeftClickHeld = false;
                }
            }
        }

        /// <summary>
        /// マウス位置に応じてラケットを移動
        /// </summary>
        private void UpdateRacketPosition()
        {
            Vector3 worldPos = GetMouseWorldPosition();

            // X座標を画面範囲内に制限
            float targetX = Mathf.Clamp(worldPos.x, minX, maxX);

            // ラケットを移動（スムーズ移動）
            Vector3 currentPos = transform.position;
            float newX = Mathf.Lerp(currentPos.x, targetX, moveSpeed * Time.deltaTime);

            transform.position = new Vector3(newX, FIXED_Y, FIXED_Z);
        }

        /// <summary>
        /// マウスのワールド座標を取得
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // カメラからの距離
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// スイングモード開始（右クリック）
        /// </summary>
        private void StartSwingMode()
        {
            isRightClickHeld = true;
            racketSwingZ = SWING_START_Z; // Z=-4.5からスタート
            previousMouseWorldPos = GetMouseWorldPosition();
            mouseVelocityX = 0f;

            Debug.Log($"[PlayerRacket] ★ SWING MODE START - Racket at Z={racketSwingZ:F2}");
        }

        /// <summary>
        /// スイングモード更新（右クリック中）
        /// </summary>
        private void UpdateSwingMode()
        {
            Vector3 currentMouseWorldPos = GetMouseWorldPosition();

            // マウスX移動でラケットX位置を変更
            float targetX = Mathf.Clamp(currentMouseWorldPos.x, minX, maxX);

            // マウスY移動（下→上）でラケットを前進（Z=-4.5 → -3.5）
            float mouseDeltaY = currentMouseWorldPos.y - previousMouseWorldPos.y;
            racketSwingZ += mouseDeltaY * swingSpeed;
            racketSwingZ = Mathf.Clamp(racketSwingZ, SWING_START_Z, SWING_END_Z);

            // ラケット位置を更新
            transform.position = new Vector3(targetX, FIXED_Y, racketSwingZ);

            // マウスX速度を計算
            float mouseDeltaX = currentMouseWorldPos.x - previousMouseWorldPos.x;
            mouseVelocityX = mouseDeltaX / Time.deltaTime;

            previousMouseWorldPos = currentMouseWorldPos;

            // ボールとの接触判定
            if (ball != null && ball.IsMoving)
            {
                Vector3 ballPos = ball.CurrentPosition;
                float distance = Vector3.Distance(transform.position, ballPos);

                if (distance <= hitDistance)
                {
                    // 接触！返球処理
                    TryReturnWithSwing(ballPos.z);
                }
            }
        }

        /// <summary>
        /// スイングモード終了（右クリックリリース）
        /// </summary>
        private void EndSwingMode(bool hitSuccessful)
        {
            isRightClickHeld = false;

            // ラケットを通常位置に戻す
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            float targetX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
            transform.position = new Vector3(targetX, FIXED_Y, FIXED_Z);

            if (!hitSuccessful)
            {
                Debug.Log($"[PlayerRacket] SWING MODE END - No hit");
            }
        }

        /// <summary>
        /// スイング返球（右クリック、接触時）
        /// </summary>
        private void TryReturnWithSwing(float contactZ)
        {
            if (ball == null || !ball.IsMoving) return;

            // SimpleGameControllerから閾値を取得
            if (SimpleGameController.Instance == null)
            {
                Debug.LogError("[PlayerRacket] SimpleGameController.Instance is null!");
                return;
            }

            float perfectThreshold = SimpleGameController.Instance.PerfectThreshold;
            float goodThreshold = SimpleGameController.Instance.GoodThreshold;

            // タイミング判定（接触時のZ座標で判定）
            float distanceFromCenter = Mathf.Abs(contactZ - FIXED_Z);

            string timing = "MISS";
            if (distanceFromCenter <= perfectThreshold)
            {
                timing = "PERFECT";
            }
            else if (distanceFromCenter <= goodThreshold)
            {
                timing = "GOOD";
            }

            Debug.Log($"[PlayerRacket] ★ SWING HIT! Timing: {timing} (Contact Z={contactZ:F2}, Distance={distanceFromCenter:F2})");

            // 返球成功
            if (distanceFromCenter <= goodThreshold)
            {
                // スイングモード終了
                EndSwingMode(true);

                // 返球待機停止
                StopTracking();

                // ラケット位置を取得
                float racketX = transform.position.x;

                // マウス速度からカーブ効果を計算（右移動=左カーブ）
                float curveEffect = -mouseVelocityX * swingCurveMultiplier;

                // Perfect判定の場合、カーブ方向の端を狙う補正を追加
                if (timing == "PERFECT" && Mathf.Abs(curveEffect) > 0.01f)
                {
                    float targetX = curveEffect > 0 ? SimpleBallController.TABLE_MAX_X : SimpleBallController.TABLE_MIN_X;
                    float targetZ = 2.0f;

                    Vector3 currentBallPos = ball.CurrentPosition;
                    Vector3 targetPos = new Vector3(targetX, currentBallPos.y, targetZ);
                    Vector3 directionToTarget = (targetPos - currentBallPos).normalized;

                    ball.SetDirection(directionToTarget);

                    Debug.Log($"[PlayerRacket] → PERFECT CURVE CORRECTION (Swing)!");
                    Debug.Log($"[PlayerRacket]   Target: ({targetX:F2}, {targetZ:F2}), Direction: {directionToTarget}");
                }
                else
                {
                    ball.ReturnBall(racketX, curveEffect);
                }

                // BallStateManagerに通知
                if (stateManager != null)
                {
                    stateManager.OnPlayerReturnSuccess();
                }

                // タイミング判定フィードバックを表示（カーブ方向付き）
                if (GameUIManager.Instance != null)
                {
                    string curveDirection = "";
                    if (Mathf.Abs(curveEffect) > 0.01f)
                    {
                        curveDirection = curveEffect > 0 ? "Curve to Right" : "Curve to Left";
                    }
                    GameUIManager.Instance.ShowTimingFeedback(timing, curveDirection);
                }

                Debug.Log($"[PlayerRacket] → SWING RETURN SUCCESS! Timing: {timing}, Racket X: {racketX:F2}, Mouse Velocity X: {mouseVelocityX:F2}, Curve: {curveEffect:F2}");
            }
            else
            {
                Debug.LogWarning($"[PlayerRacket] → SWING MISS! Ball will continue to Z=-5");
                EndSwingMode(false);
            }
        }

        /// <summary>
        /// ドラッグ返球（左クリック）
        /// </summary>
        private void TryReturnWithDrag()
        {
            if (ball == null || !ball.IsMoving) return;

            // SimpleGameControllerから閾値を取得
            if (SimpleGameController.Instance == null)
            {
                Debug.LogError("[PlayerRacket] SimpleGameController.Instance is null!");
                return;
            }

            float perfectThreshold = SimpleGameController.Instance.PerfectThreshold;
            float goodThreshold = SimpleGameController.Instance.GoodThreshold;

            float ballZ = ball.CurrentPosition.z;

            // タイミング判定
            float distanceFromCenter = Mathf.Abs(ballZ - FIXED_Z);

            string timing = "MISS";
            if (distanceFromCenter <= perfectThreshold)
            {
                timing = "PERFECT";
            }
            else if (distanceFromCenter <= goodThreshold)
            {
                timing = "GOOD";
            }

            Debug.Log($"[PlayerRacket] ★ RETURN ATTEMPT: {timing} (Z={ballZ:F2}, Distance={distanceFromCenter:F2})");

            // 返球成功
            if (distanceFromCenter <= goodThreshold)
            {
                // 返球待機停止
                StopTracking();

                // ラケット位置を取得
                float racketX = transform.position.x;

                // クリック開始位置からのドラッグ量を計算（X方向）
                Vector3 currentMouseWorldPos = GetMouseWorldPosition();
                float dragX = currentMouseWorldPos.x - leftClickStartWorldPos.x;

                // カーブ方向を反転（右移動=左カーブ、左移動=右カーブ）
                float curveEffect = -dragX * dragCurveMultiplier;

                // Perfect判定の場合、カーブ方向の端を狙う補正を追加
                if (timing == "PERFECT" && Mathf.Abs(curveEffect) > 0.01f)
                {
                    // カーブ方向に応じてターゲットX座標を決定
                    // curveEffect > 0 = 右カーブ → 右端（X=1.5）
                    // curveEffect < 0 = 左カーブ → 左端（X=-1.5）
                    float targetX = curveEffect > 0 ? SimpleBallController.TABLE_MAX_X : SimpleBallController.TABLE_MIN_X;
                    float targetZ = 2.0f; // AIテーブル

                    // 現在のボール位置からターゲットへの方向ベクトルを計算
                    Vector3 currentBallPos = ball.CurrentPosition;
                    Vector3 targetPos = new Vector3(targetX, currentBallPos.y, targetZ);
                    Vector3 directionToTarget = (targetPos - currentBallPos).normalized;

                    // ボールの方向を直接設定（Perfect補正）
                    ball.SetDirection(directionToTarget);

                    Debug.Log($"[PlayerRacket] → PERFECT CURVE CORRECTION!");
                    Debug.Log($"[PlayerRacket]   Target: ({targetX:F2}, {targetZ:F2}), Direction: {directionToTarget}");
                }
                else
                {
                    // Good判定または直進の場合、通常返球（カーブ効果のみ）
                    ball.ReturnBall(racketX, curveEffect);
                }

                // BallStateManagerに通知（AI側フラグをリセット）
                if (stateManager != null)
                {
                    stateManager.OnPlayerReturnSuccess();
                }

                // タイミング判定フィードバックを表示（カーブ方向付き）
                if (GameUIManager.Instance != null)
                {
                    string curveDirection = "";
                    if (Mathf.Abs(curveEffect) > 0.01f)
                    {
                        curveDirection = curveEffect > 0 ? "Curve to Right" : "Curve to Left";
                    }
                    GameUIManager.Instance.ShowTimingFeedback(timing, curveDirection);
                }

                Debug.Log($"[PlayerRacket] → Return SUCCESS! Timing: {timing}, Racket X: {racketX:F2}, Drag X: {dragX:F2}, Curve: {curveEffect:F2}");
            }
            else
            {
                Debug.LogWarning($"[PlayerRacket] → Return MISS! Ball will continue to Z=-5");
            }
        }

        /// <summary>
        /// ボール追跡開始（返球待機開始）
        /// </summary>
        public void StartTracking(float ballX)
        {
            isWaitingForReturn = true;
            canReturn = false; // リセット
            Debug.Log($"[PlayerRacket] ★ START TRACKING - Ball coming to X={ballX:F2}");
            Debug.Log($"[PlayerRacket] → Move mouse to position racket!");
        }

        /// <summary>
        /// ボール追跡停止
        /// </summary>
        public void StopTracking()
        {
            isWaitingForReturn = false;
            canReturn = false;
            Debug.Log($"[PlayerRacket] ★ STOP TRACKING");
        }
    }
}
