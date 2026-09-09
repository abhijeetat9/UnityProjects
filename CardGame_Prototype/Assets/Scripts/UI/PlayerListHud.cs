using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class PlayerListHud : MonoBehaviour
    {
        public static PlayerListHud Instance { get; private set; }

        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private Sprite panelSprite;

        [Header("Colors")]
        [SerializeField] private Color panelColor = new Color(0.0627451f, 0.1254902f, 0.21176471f, 0.85f);
        [SerializeField] private Color normalTextColor = new Color(0.85f, 0.9f, 0.98f, 1f);
        [SerializeField] private Color activeTextColor = new Color(1f, 0.86f, 0.6f, 1f);
        [SerializeField] private Color activeRowColor = new Color(1f, 0.86f, 0.6f, 0.18f);
        [SerializeField] private Color rowColor = new Color(1f, 1f, 1f, 0f);

        private const int MaxRows = 6;
        private const float RowHeight = 28f;
        private const float Padding = 10f;

        private Image[] _rowBackgrounds;
        private TextMeshProUGUI[] _rowLabels;
        private GameObject _panelGo;

        private void Awake()
        {
            Instance = this;
            if (NetworkServer.active && !NetworkClient.active) return;
            BuildHud();
        }

        private void BuildHud()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("PlayerListHud: no Canvas found in scene.");
                return;
            }

            _panelGo = new GameObject("PlayerListPanel", typeof(RectTransform));
            _panelGo.transform.SetParent(canvas.transform, false);
            var panelRt = _panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(12f, -12f);
            panelRt.sizeDelta = new Vector2(180f, Padding * 2 + RowHeight * MaxRows);
            panelRoot = panelRt;

            var panelImage = _panelGo.AddComponent<Image>();
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced;
            }
            panelImage.color = panelColor;

            _rowBackgrounds = new Image[MaxRows];
            _rowLabels = new TextMeshProUGUI[MaxRows];

            for (int i = 0; i < MaxRows; i++)
            {
                var rowGo = new GameObject($"Row{i}", typeof(RectTransform));
                rowGo.transform.SetParent(_panelGo.transform, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.anchorMin = new Vector2(0f, 1f);
                rowRt.anchorMax = new Vector2(1f, 1f);
                rowRt.pivot = new Vector2(0.5f, 1f);
                rowRt.anchoredPosition = new Vector2(0f, -Padding - i * RowHeight);
                rowRt.sizeDelta = new Vector2(0f, RowHeight);

                var rowImage = rowGo.AddComponent<Image>();
                rowImage.color = rowColor;
                _rowBackgrounds[i] = rowImage;

                var labelGo = new GameObject("Label", typeof(RectTransform));
                labelGo.transform.SetParent(rowGo.transform, false);
                var labelRt = labelGo.GetComponent<RectTransform>();
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(10f, 0f);
                labelRt.offsetMax = new Vector2(-10f, 0f);

                var label = labelGo.AddComponent<TextMeshProUGUI>();
                label.font = fontAsset;
                label.fontSize = 16;
                label.color = normalTextColor;
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.text = "";
                _rowLabels[i] = label;

                rowGo.SetActive(false);
            }
        }

        public void Render(NetworkGameController.BoardSnapshot snapshot)
        {
            if (_rowLabels == null || snapshot.Players == null)
            {
                return;
            }

            int count = Mathf.Min(snapshot.Players.Length, MaxRows);
            for (int i = 0; i < count; i++)
            {
                var player = snapshot.Players[i];
                bool isActive = i == snapshot.CurrentPlayerIndex;

                _rowLabels[i].gameObject.transform.parent.gameObject.SetActive(true);
                _rowLabels[i].text = player.PlayerName;
                _rowLabels[i].color = isActive ? activeTextColor : normalTextColor;
                _rowLabels[i].fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
                _rowBackgrounds[i].color = isActive ? activeRowColor : rowColor;
            }

            for (int i = count; i < MaxRows; i++)
            {
                _rowLabels[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
