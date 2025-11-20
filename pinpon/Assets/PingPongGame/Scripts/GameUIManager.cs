using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PingPongGame
{
    /// <summary>
    /// 統合UIマネージャー（すべてのUI要素を1つのCanvasで管理）
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Gauge Settings")]
        [SerializeField] private float gaugeWidth = 40f;
        [SerializeField] private float gaugeHeight = 400f;
        [SerializeField] private Vector2 gaugePosition = new Vector2(350, 0);

        [Header("Colors")]
        [SerializeField] private Color perfectColor = Color.yellow;
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color failColor = Color.red;
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Canvas
        private Canvas canvas;

        // 失敗カウント（左上）
        private Text failureCountText;

        // タイミングゲージ（右側）
        private RectTransform gaugeContainer;
        private Image gaugeBackground;
        private Image gaugeFill;
        private Image perfectZone;
        private Image goodZone;

        // ゲームオーバーパネル（中央）
        private GameObject gameOverPanel;
        private Text gameOverText;

        // タイミング判定フィードバック（中央）
        private Text timingFeedbackText;
        private float feedbackTimer = 0f;
        private const float FEEDBACK_DISPLAY_TIME = 1.0f;

        // 参照
        private SimpleBallController ballController;
        private bool isActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            CreateCanvas();
            CreateFailureCountUI();
            CreateTimingGaugeUI();
            CreateGameOverUI();
            CreateTimingFeedbackUI();

            Debug.Log("[GameUIManager] All UI created on unified Canvas");
        }

        private void Start()
        {
            ballController = FindObjectOfType<SimpleBallController>();
            if (ballController == null)
            {
                Debug.LogError("[GameUIManager] SimpleBallController not found!");
            }

            isActive = true;
        }

        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("GameUICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        #region 失敗カウントUI

        private void CreateFailureCountUI()
        {
            GameObject failCountObj = new GameObject("FailureCountText");
            failCountObj.transform.SetParent(canvas.transform, false);
            failureCountText = failCountObj.AddComponent<Text>();
            failureCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            failureCountText.fontSize = 24;
            failureCountText.color = Color.white;
            failureCountText.alignment = TextAnchor.UpperLeft;
            failureCountText.text = "Failures: 0/3";

            RectTransform failRect = failCountObj.GetComponent<RectTransform>();
            failRect.anchorMin = new Vector2(0, 1);
            failRect.anchorMax = new Vector2(0, 1);
            failRect.pivot = new Vector2(0, 1);
            failRect.anchoredPosition = new Vector2(20, -20);
            failRect.sizeDelta = new Vector2(300, 50);
        }

        #endregion

        #region タイミングゲージUI

        private void CreateTimingGaugeUI()
        {
            // コンテナ
            GameObject containerObj = new GameObject("GaugeContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            gaugeContainer = containerObj.AddComponent<RectTransform>();
            gaugeContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gaugeContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gaugeContainer.sizeDelta = new Vector2(gaugeWidth + 60, gaugeHeight + 60);
            gaugeContainer.anchoredPosition = gaugePosition;

            // タイトルラベル
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(gaugeContainer, false);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "TIMING";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.sizeDelta = new Vector2(100, 30);
            labelRect.anchoredPosition = new Vector2(0, -15);

            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(gaugeContainer, false);
            gaugeBackground = bgObj.AddComponent<Image>();
            gaugeBackground.color = backgroundColor;
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(gaugeWidth, gaugeHeight);
            bgRect.anchoredPosition = new Vector2(0, 0);

            // Perfect Zone
            GameObject perfectObj = new GameObject("PerfectZone");
            perfectObj.transform.SetParent(bgObj.transform, false);
            perfectZone = perfectObj.AddComponent<Image>();
            perfectZone.color = new Color(perfectColor.r, perfectColor.g, perfectColor.b, 0.5f);
            RectTransform perfectRect = perfectObj.GetComponent<RectTransform>();
            perfectRect.anchorMin = new Vector2(0.5f, 0.5f);
            perfectRect.anchorMax = new Vector2(0.5f, 0.5f);
            perfectRect.sizeDelta = new Vector2(gaugeWidth, 60);
            perfectRect.anchoredPosition = new Vector2(0, 0);

            // Good Zone
            GameObject goodObj = new GameObject("GoodZone");
            goodObj.transform.SetParent(bgObj.transform, false);
            goodZone = goodObj.AddComponent<Image>();
            goodZone.color = new Color(goodColor.r, goodColor.g, goodColor.b, 0.3f);
            RectTransform goodRect = goodObj.GetComponent<RectTransform>();
            goodRect.anchorMin = new Vector2(0.5f, 0.5f);
            goodRect.anchorMax = new Vector2(0.5f, 0.5f);
            goodRect.sizeDelta = new Vector2(gaugeWidth, 100);
            goodRect.anchoredPosition = new Vector2(0, 0);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            gaugeFill = fillObj.AddComponent<Image>();
            gaugeFill.color = Color.white;
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.5f, 0.5f);
            fillRect.anchorMax = new Vector2(0.5f, 0.5f);
            fillRect.sizeDelta = new Vector2(gaugeWidth, 5);
            fillRect.anchoredPosition = new Vector2(0, 0);
        }

        #endregion

        #region ゲームオーバーUI

        private void CreateGameOverUI()
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(canvas.transform, false);

            Image panelBg = gameOverPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);

            RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(400, 200);

            GameObject gameOverTextObj = new GameObject("GameOverText");
            gameOverTextObj.transform.SetParent(gameOverPanel.transform, false);
            gameOverText = gameOverTextObj.AddComponent<Text>();
            gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            gameOverText.fontSize = 36;
            gameOverText.color = Color.red;
            gameOverText.alignment = TextAnchor.MiddleCenter;
            gameOverText.text = "GAME OVER\n\nPress R to Restart";

            RectTransform textRect = gameOverTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            gameOverPanel.SetActive(false);
        }

        #endregion

        #region タイミング判定フィードバックUI

        private void CreateTimingFeedbackUI()
        {
            GameObject feedbackObj = new GameObject("TimingFeedback");
            feedbackObj.transform.SetParent(canvas.transform, false);
            timingFeedbackText = feedbackObj.AddComponent<Text>();
            timingFeedbackText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timingFeedbackText.fontSize = 60;
            timingFeedbackText.color = Color.yellow;
            timingFeedbackText.alignment = TextAnchor.MiddleCenter;
            timingFeedbackText.text = "";

            RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
            feedbackRect.anchorMin = new Vector2(0.5f, 0.5f);
            feedbackRect.anchorMax = new Vector2(0.5f, 0.5f);
            feedbackRect.pivot = new Vector2(0.5f, 0.5f);
            feedbackRect.anchoredPosition = new Vector2(0, 50);
            feedbackRect.sizeDelta = new Vector2(400, 100);

            // 初期状態は非表示
            timingFeedbackText.enabled = false;
        }

        #endregion

        private void Update()
        {
            if (!isActive) return;

            UpdateFailureCount();
            UpdateGameOverPanel();
            UpdateTimingGauge();
            UpdateTimingFeedback();
        }

        private void UpdateFailureCount()
        {
            if (SimpleGameController.Instance != null)
            {
                var controller = SimpleGameController.Instance;
                failureCountText.text = $"Failures: {controller.FailureCount}/{controller.MaxFailures}";
            }
        }

        private void UpdateGameOverPanel()
        {
            if (SimpleGameController.Instance != null)
            {
                var controller = SimpleGameController.Instance;
                gameOverPanel.SetActive(controller.IsGameOver);
            }
        }

        private void UpdateTimingGauge()
        {
            if (ballController == null || SimpleGameController.Instance == null) return;

            float perfectThreshold = SimpleGameController.Instance.PerfectThreshold;
            float goodThreshold = SimpleGameController.Instance.GoodThreshold;

            float currentZ = ballController.CurrentPosition.z;
            float clampedZ = Mathf.Clamp(currentZ, -5f, 0f);
            float normalizedZ = Mathf.InverseLerp(-5f, 0f, clampedZ);
            float yPosition = (normalizedZ - 0.5f) * gaugeHeight;

            RectTransform fillRect = gaugeFill.GetComponent<RectTransform>();
            fillRect.anchoredPosition = new Vector2(0, yPosition);

            // Perfect/Good Zoneの位置とサイズ
            float z4NormalizedPos = Mathf.InverseLerp(-5f, 0f, -4f);
            float z4YPos = (z4NormalizedPos - 0.5f) * gaugeHeight;

            float perfectZoneSize = (perfectThreshold * 2f / 5f) * gaugeHeight;
            RectTransform perfectRect = perfectZone.GetComponent<RectTransform>();
            perfectRect.sizeDelta = new Vector2(gaugeWidth, perfectZoneSize);
            perfectRect.anchoredPosition = new Vector2(0, z4YPos);

            float goodZoneSize = (goodThreshold * 2f / 5f) * gaugeHeight;
            RectTransform goodRect = goodZone.GetComponent<RectTransform>();
            goodRect.sizeDelta = new Vector2(gaugeWidth, goodZoneSize);
            goodRect.anchoredPosition = new Vector2(0, z4YPos);

            // 色の変更
            float distanceFromPlayerRacket = Mathf.Abs(currentZ - (-4f));
            if (distanceFromPlayerRacket <= perfectThreshold)
            {
                gaugeFill.color = perfectColor;
            }
            else if (distanceFromPlayerRacket <= goodThreshold)
            {
                gaugeFill.color = goodColor;
            }
            else
            {
                gaugeFill.color = failColor;
            }
        }

        private void UpdateTimingFeedback()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;

                // フェードアウト
                float alpha = Mathf.Clamp01(feedbackTimer / FEEDBACK_DISPLAY_TIME);
                Color color = timingFeedbackText.color;
                color.a = alpha;
                timingFeedbackText.color = color;

                if (feedbackTimer <= 0f)
                {
                    timingFeedbackText.enabled = false;
                }
            }
        }

        /// <summary>
        /// タイミング判定フィードバックを表示
        /// </summary>
        public void ShowTimingFeedback(string timing, string curveDirection = "")
        {
            // カーブ方向を含めて表示
            if (!string.IsNullOrEmpty(curveDirection))
            {
                timingFeedbackText.text = $"{timing}\n{curveDirection}";
            }
            else
            {
                timingFeedbackText.text = timing;
            }

            timingFeedbackText.enabled = true;

            // 色を設定
            if (timing == "PERFECT")
            {
                Color color = perfectColor;
                color.a = 1f;
                timingFeedbackText.color = color;
            }
            else if (timing == "GOOD")
            {
                Color color = goodColor;
                color.a = 1f;
                timingFeedbackText.color = color;
            }

            feedbackTimer = FEEDBACK_DISPLAY_TIME;

            Debug.Log($"[GameUIManager] Showing timing feedback: {timing} {curveDirection}");
        }
    }
}
