using UnityEngine;

namespace PingPongGame
{
    /// <summary>
    /// ゲーム制御（シンプル版 - サーブローテーション＋失敗カウント）
    /// </summary>
    public class SimpleGameController : MonoBehaviour
    {
        public static SimpleGameController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SimpleBallController ball;
        [SerializeField] private BallStateManager stateManager;

        [Header("Settings")]
        [SerializeField] private Vector3 playerServeStart = new Vector3(0f, 1.2f, -4f);
        [SerializeField] private Vector3 aiServeStart = new Vector3(0f, 1.2f, 4f);
        [SerializeField] private int maxFailures = 3;

        [Header("Timing Settings")]
        [Tooltip("Perfect判定の範囲（Z=-4の±この値）")]
        [SerializeField] private float perfectThreshold = 0.3f;
        [Tooltip("Good判定の範囲（Z=-4の±この値、Perfect範囲の外側）")]
        [SerializeField] private float goodThreshold = 0.5f;

        // サーブローテーション
        private int serveCount = 0; // 0 or 1
        private bool isPlayerServe = true; // true=Player, false=AI

        // 失敗カウント
        private int failureCount = 0;
        private bool isGameOver = false;

        // ゲーム状態
        private bool gameStarted = false;

        // 公開プロパティ（UI用）
        public int FailureCount => failureCount;
        public int MaxFailures => maxFailures;
        public bool IsGameOver => isGameOver;

        // タイミング設定の公開プロパティ
        public float PerfectThreshold => perfectThreshold;
        public float GoodThreshold => goodThreshold;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 自動参照取得
            if (ball == null) ball = FindObjectOfType<SimpleBallController>();
            if (stateManager == null) stateManager = FindObjectOfType<BallStateManager>();

            if (ball == null) Debug.LogError("[GameController] SimpleBallController not found!");
            if (stateManager == null) Debug.LogError("[GameController] BallStateManager not found!");

            Debug.Log("[GameController] Initialized - Right-click to start");
            Debug.Log($"[GameController] Serve: PLAYER (0/2) | Failures: 0/{maxFailures}");
        }

        private void Update()
        {
            // ゲームオーバー時はRキーでリスタート
            if (isGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
                return;
            }

            // 右クリック検出（プレイヤーサーブ時のみ、それ以外はSimplePlayerRacketが処理）
            if (Input.GetMouseButtonDown(1))
            {
                // ボールが停止していて、プレイヤーのサーブターンの時のみサーブ処理
                if (!ball.IsMoving && isPlayerServe)
                {
                    PlayerServe();
                }
                // それ以外の右クリックは無視（SimplePlayerRacketでカーブ処理）
            }
        }

        /// <summary>
        /// プレイヤーサーブ
        /// </summary>
        private void PlayerServe()
        {
            if (ball == null || stateManager == null) return;

            // ランダムなX座標ターゲット（-1 ~ 1）
            Vector3 randomTarget = new Vector3(
                Random.Range(-1f, 1f),
                1.2f,
                4f
            );

            Debug.Log($"[GameController] ★ PLAYER SERVE (Serve {serveCount + 1}/2)");
            Debug.Log($"[GameController] Start: {playerServeStart} → Target: {randomTarget}");

            // 状態リセット
            stateManager.ResetState();

            // ボール発射
            ball.StartMove(playerServeStart, randomTarget);

            gameStarted = true;
        }

        /// <summary>
        /// AIサーブ
        /// </summary>
        private void AIServe()
        {
            if (ball == null || stateManager == null) return;

            // ランダムなX座標ターゲット（-1 ~ 1）
            Vector3 randomTarget = new Vector3(
                Random.Range(-1f, 1f),
                1.2f,
                -4f
            );

            Debug.Log($"[GameController] ★ AI SERVE (Serve {serveCount + 1}/2)");
            Debug.Log($"[GameController] Start: {aiServeStart} → Target: {randomTarget}");

            // 状態リセット
            stateManager.ResetState();

            // ボール発射
            ball.StartMove(aiServeStart, randomTarget);
        }

        /// <summary>
        /// プレイヤー失敗時に呼ばれる（BallStateManagerから）
        /// </summary>
        public void OnPlayerFailed()
        {
            if (isGameOver) return;

            failureCount++;
            Debug.Log($"[GameController] ━━━ PLAYER FAILED ━━━");
            Debug.Log($"[GameController] Failures: {failureCount}/{maxFailures}");

            // ゲームオーバー判定
            if (failureCount >= maxFailures)
            {
                GameOver();
                return;
            }

            // サーブローテーション
            serveCount++;
            if (serveCount >= 2)
            {
                serveCount = 0;
                isPlayerServe = !isPlayerServe;
                Debug.Log($"[GameController] ★ SERVE ROTATION! Now serving: {(isPlayerServe ? "PLAYER" : "AI")}");
            }

            Debug.Log($"[GameController] Next serve: {(isPlayerServe ? "PLAYER" : "AI")} ({serveCount + 1}/2)");

            // 次のサーブ準備
            if (isPlayerServe)
            {
                Debug.Log("[GameController] → Right-click to serve");
            }
            else
            {
                Debug.Log("[GameController] → AI will serve in 2 seconds");
                Invoke("AIServe", 2.0f);
            }
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        private void GameOver()
        {
            isGameOver = true;
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("        ★★★ GAME OVER ★★★");
            Debug.Log($"        Total Failures: {failureCount}/{maxFailures}");
            Debug.Log("        Press R to Restart");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// ゲームリスタート
        /// </summary>
        private void RestartGame()
        {
            Debug.Log("[GameController] ★ RESTARTING GAME ★");

            // 状態リセット
            failureCount = 0;
            serveCount = 0;
            isPlayerServe = true;
            isGameOver = false;
            gameStarted = false;

            // ボール停止
            if (ball != null)
            {
                ball.Stop();
            }

            // 状態マネージャーリセット
            if (stateManager != null)
            {
                stateManager.ResetState();
            }

            Debug.Log($"[GameController] Ready - Serve: PLAYER (1/2) | Failures: 0/{maxFailures}");
            Debug.Log("[GameController] Right-click to serve");
        }
    }
}
