using UnityEngine;
using UnityEngine.UI;

namespace PingPongGame
{
    /// <summary>
    /// シンプルなゲームUI（失敗カウント＋ゲームオーバー表示）
    /// </summary>
    public class SimpleGameUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private int fontSize = 24;

        private Canvas canvas;
        private Text failureCountText;
        private GameObject gameOverPanel;
        private Text gameOverText;

        private void Awake()
        {
            CreateUI();
        }

        private void CreateUI()
        {
            // Canvas作成
            GameObject canvasObj = new GameObject("GameUICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 失敗カウントテキスト（画面左上）
            GameObject failCountObj = new GameObject("FailureCountText");
            failCountObj.transform.SetParent(canvas.transform, false);
            failureCountText = failCountObj.AddComponent<Text>();
            failureCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            failureCountText.fontSize = fontSize;
            failureCountText.color = textColor;
            failureCountText.alignment = TextAnchor.UpperLeft;
            failureCountText.text = "Failures: 0/3";

            RectTransform failRect = failCountObj.GetComponent<RectTransform>();
            failRect.anchorMin = new Vector2(0, 1);
            failRect.anchorMax = new Vector2(0, 1);
            failRect.pivot = new Vector2(0, 1);
            failRect.anchoredPosition = new Vector2(20, -20);
            failRect.sizeDelta = new Vector2(300, 50);

            // ゲームオーバーパネル（中央）
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

            // ゲームオーバーテキスト
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

            // 初期状態：ゲームオーバーパネルは非表示
            gameOverPanel.SetActive(false);

            Debug.Log("[GameUI] UI created - Failure count and Game Over panel");
        }

        private void Update()
        {
            // SimpleGameControllerから状態を取得して表示更新
            if (SimpleGameController.Instance != null)
            {
                UpdateFailureCount();
                UpdateGameOverPanel();
            }
        }

        private void UpdateFailureCount()
        {
            var controller = SimpleGameController.Instance;
            failureCountText.text = $"Failures: {controller.FailureCount}/{controller.MaxFailures}";
        }

        private void UpdateGameOverPanel()
        {
            var controller = SimpleGameController.Instance;
            gameOverPanel.SetActive(controller.IsGameOver);
        }
    }
}
