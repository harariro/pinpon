using UnityEngine;
using UnityEngine.UI;

namespace PingPongGame
{
    /// <summary>
    /// 返球タイミングのゲージUIを表示するクラス
    /// </summary>
    public class TimingGaugeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image gaugeBackground;
        [SerializeField] private Image gaugeFill;
        [SerializeField] private Image perfectZone;
        [SerializeField] private Image goodZone;
        [SerializeField] private RectTransform gaugeContainer;

        [Header("Settings")]
        [SerializeField] private float gaugeWidth = 40f;  // 縦向きなので幅は小さく
        [SerializeField] private float gaugeHeight = 400f; // 縦向きなので高さは大きく
        [SerializeField] private Vector2 gaugePosition = new Vector2(350, 0); // 画面右側に配置

        [Header("Colors")]
        [SerializeField] private Color perfectColor = Color.yellow;
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color failColor = Color.red;
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        private SimpleBallController ballController;
        private SimplePlayerRacket playerRacket;
        private bool isActive = false;

        private void Awake()
        {
            if (canvas == null)
            {
                CreateCanvas();
            }

            CreateGaugeUI();
            // 常時表示にするため、最初から表示
            if (gaugeContainer != null)
            {
                gaugeContainer.gameObject.SetActive(true);
            }
            isActive = true;
            Debug.Log("[TimingGauge] Initialized and showing");
        }

        private void Start()
        {
            ballController = FindObjectOfType<SimpleBallController>();
            playerRacket = FindObjectOfType<SimplePlayerRacket>();
        }

        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("TimingGaugeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateGaugeUI()
        {
            // コンテナ
            GameObject containerObj = new GameObject("GaugeContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            gaugeContainer = containerObj.AddComponent<RectTransform>();
            gaugeContainer.anchorMin = new Vector2(0.5f, 0.5f);
            gaugeContainer.anchorMax = new Vector2(0.5f, 0.5f);
            gaugeContainer.sizeDelta = new Vector2(gaugeWidth + 60, gaugeHeight + 60);
            gaugeContainer.anchoredPosition = gaugePosition;

            // タイトルラベル（上部）
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

            // 背景（縦向き）
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(gaugeContainer, false);
            gaugeBackground = bgObj.AddComponent<Image>();
            gaugeBackground.color = backgroundColor;
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(gaugeWidth, gaugeHeight);
            bgRect.anchoredPosition = new Vector2(0, 0);

            // Perfect Zone（Z=-4の位置、動的にサイズ計算）
            GameObject perfectObj = new GameObject("PerfectZone");
            perfectObj.transform.SetParent(bgObj.transform, false);
            perfectZone = perfectObj.AddComponent<Image>();
            perfectZone.color = new Color(perfectColor.r, perfectColor.g, perfectColor.b, 0.5f);
            RectTransform perfectRect = perfectObj.GetComponent<RectTransform>();
            perfectRect.anchorMin = new Vector2(0.5f, 0.5f);
            perfectRect.anchorMax = new Vector2(0.5f, 0.5f);
            // サイズと位置はUpdateで設定
            perfectRect.sizeDelta = new Vector2(gaugeWidth, 60);
            perfectRect.anchoredPosition = new Vector2(0, 0);

            // Good Zone（Perfectの外側）
            GameObject goodObj = new GameObject("GoodZone");
            goodObj.transform.SetParent(bgObj.transform, false);
            goodZone = goodObj.AddComponent<Image>();
            goodZone.color = new Color(goodColor.r, goodColor.g, goodColor.b, 0.3f);
            RectTransform goodRect = goodObj.GetComponent<RectTransform>();
            goodRect.anchorMin = new Vector2(0.5f, 0.5f);
            goodRect.anchorMax = new Vector2(0.5f, 0.5f);
            // サイズと位置はUpdateで設定
            goodRect.sizeDelta = new Vector2(gaugeWidth, 100);
            goodRect.anchoredPosition = new Vector2(0, 0);

            // Fill（現在のボール位置インジケーター）
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            gaugeFill = fillObj.AddComponent<Image>();
            gaugeFill.color = Color.white;
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.5f, 0.5f);
            fillRect.anchorMax = new Vector2(0.5f, 0.5f);
            fillRect.sizeDelta = new Vector2(gaugeWidth, 5); // 横線
            fillRect.anchoredPosition = new Vector2(0, 0);
        }

        private void Update()
        {
            if (isActive && ballController != null)
            {
                UpdateGauge();
            }
        }

        private void UpdateGauge()
        {
            // SimpleGameControllerから閾値を取得
            if (SimpleGameController.Instance == null) return;

            float perfectThreshold = SimpleGameController.Instance.PerfectThreshold;
            float goodThreshold = SimpleGameController.Instance.GoodThreshold;

            // Z位置ベースのゲージ（Z:0が上端、Z:-5が下端）
            float currentZ = ballController.CurrentPosition.z;

            // Z>0の場合は上端（Z=0）に張り付き
            float clampedZ = Mathf.Clamp(currentZ, -5f, 0f);

            // Z=0 → 1.0（上端）, Z=-5 → 0.0（下端）
            float normalizedZ = Mathf.InverseLerp(-5f, 0f, clampedZ);

            // ゲージの位置を更新（Y方向、-gaugeHeight/2からgaugeHeight/2）
            float yPosition = (normalizedZ - 0.5f) * gaugeHeight;

            RectTransform fillRect = gaugeFill.GetComponent<RectTransform>();
            fillRect.anchoredPosition = new Vector2(0, yPosition);

            // Perfect/Good Zoneの位置とサイズを更新
            // Z=-4の位置を計算
            float z4NormalizedPos = Mathf.InverseLerp(-5f, 0f, -4f); // -4は0～-5の範囲で0.2
            float z4YPos = (z4NormalizedPos - 0.5f) * gaugeHeight;

            // PerfectZoneのサイズ（Z=-4±perfectThreshold）
            float perfectZoneSize = (perfectThreshold * 2f / 5f) * gaugeHeight; // 5はZ範囲
            RectTransform perfectRect = perfectZone.GetComponent<RectTransform>();
            perfectRect.sizeDelta = new Vector2(gaugeWidth, perfectZoneSize);
            perfectRect.anchoredPosition = new Vector2(0, z4YPos);

            // GoodZoneのサイズ（Z=-4±goodThreshold）
            float goodZoneSize = (goodThreshold * 2f / 5f) * gaugeHeight;
            RectTransform goodRect = goodZone.GetComponent<RectTransform>();
            goodRect.sizeDelta = new Vector2(gaugeWidth, goodZoneSize);
            goodRect.anchoredPosition = new Vector2(0, z4YPos);

            // 色をZ位置に応じて変更
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

        public void ShowGauge()
        {
            if (gaugeContainer != null)
            {
                gaugeContainer.gameObject.SetActive(true);
                isActive = true;
                Debug.Log("[TimingGauge] Gauge shown");
            }
        }

        public void HideGauge()
        {
            // 常時表示にするため、非表示にしない
            // if (gaugeContainer != null)
            // {
            //     gaugeContainer.gameObject.SetActive(false);
            //     isActive = false;
            // }
            Debug.Log("[TimingGauge] HideGauge called but keeping visible");
        }

        /// <summary>
        /// RacketControllerから呼ばれる想定
        /// </summary>
        public void OnWaitingForReturn(bool waiting)
        {
            // 常時表示にするため、常にアクティブ
            if (gaugeContainer != null && !gaugeContainer.gameObject.activeSelf)
            {
                gaugeContainer.gameObject.SetActive(true);
            }
            isActive = true; // 常にアクティブ
            Debug.Log($"[TimingGauge] OnWaitingForReturn({waiting}) - Always active");
        }
    }
}
