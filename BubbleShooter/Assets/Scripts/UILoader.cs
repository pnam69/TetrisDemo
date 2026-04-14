//using UnityEngine;
//using UnityEngine.UI;

//public class UILoader : MonoBehaviour
//{
//    public GameManager gameManager;

//    void Awake()
//    {
//        if (gameManager == null)
//        {
//            gameManager = FindObjectOfType<GameManager>();
//        }

//        if (gameManager == null) return;

//        Canvas canvas = FindObjectOfType<Canvas>();
//        if (canvas == null)
//        {
//            GameObject canvasGO = new GameObject("Canvas");
//            canvas = canvasGO.AddComponent<Canvas>();
//            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//            canvasGO.AddComponent<CanvasScaler>();
//            canvasGO.AddComponent<GraphicRaycaster>();
//        }

//        if (gameManager.scoreText == null)
//        {
//            gameManager.scoreText = CreateText("ScoreText", canvas.transform, new Vector2(120f, -30f));
//        }

//        if (gameManager.levelText == null)
//        {
//            gameManager.levelText = CreateText("LevelText", canvas.transform, new Vector2(120f, -70f));
//        }

//        if (gameManager.shotsText == null)
//        {
//            gameManager.shotsText = CreateText("ShotsText", canvas.transform, new Vector2(120f, -110f));
//        }

//        if (gameManager.victoryPanel == null)
//        {
//            gameManager.victoryPanel = CreatePanel("VictoryPanel", canvas.transform, "VICTORY");
//            gameManager.victoryPanel.SetActive(false);
//        }

//        if (gameManager.gameOverPanel == null)
//        {
//            gameManager.gameOverPanel = CreatePanel("GameOverPanel", canvas.transform, "GAME OVER");
//            gameManager.gameOverPanel.SetActive(false);
//        }
//    }

//    Text CreateText(string name, Transform parent, Vector2 anchoredPos)
//    {
//        GameObject go = new GameObject(name);
//        go.transform.SetParent(parent, false);

//        RectTransform rt = go.AddComponent<RectTransform>();
//        rt.anchorMin = new Vector2(0f, 1f);
//        rt.anchorMax = new Vector2(0f, 1f);
//        rt.pivot = new Vector2(0f, 1f);
//        rt.anchoredPosition = anchoredPos;
//        rt.sizeDelta = new Vector2(300f, 40f);

//        Text text = go.AddComponent<Text>();
//        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        text.fontSize = 28;
//        text.color = Color.white;
//        text.alignment = TextAnchor.MiddleLeft;
//        return text;
//    }

//    GameObject CreatePanel(string name, Transform parent, string label)
//    {
//        GameObject panel = new GameObject(name);
//        panel.transform.SetParent(parent, false);

//        RectTransform rt = panel.AddComponent<RectTransform>();
//        rt.anchorMin = new Vector2(0.5f, 0.5f);
//        rt.anchorMax = new Vector2(0.5f, 0.5f);
//        rt.pivot = new Vector2(0.5f, 0.5f);
//        rt.anchoredPosition = Vector2.zero;
//        rt.sizeDelta = new Vector2(500f, 180f);

//        Image image = panel.AddComponent<Image>();
//        image.color = new Color(0f, 0f, 0f, 0.7f);

//        Text text = CreateText(name + "Label", panel.transform, new Vector2(-180f, 30f));
//        text.text = label;
//        text.alignment = TextAnchor.MiddleCenter;

//        RectTransform txtRt = text.GetComponent<RectTransform>();
//        txtRt.anchorMin = new Vector2(0.5f, 0.5f);
//        txtRt.anchorMax = new Vector2(0.5f, 0.5f);
//        txtRt.pivot = new Vector2(0.5f, 0.5f);
//        txtRt.anchoredPosition = Vector2.zero;
//        txtRt.sizeDelta = new Vector2(400f, 80f);

//        return panel;
//    }
//}
