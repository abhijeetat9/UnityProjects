using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CardGame.UI
{
    // Shown once, right after a player's very first successful login (see
    // AuthController.DoLogin, which checks the same PlayerPrefs flag this
    // script sets before deciding whether to route here or straight to
    // Lobby). Purely a UI/navigation screen - no gameplay or network code.
    //
    // Layout: a 3-section paged carousel (Lobby / Cards / Scoring),
    // navigated with Prev/Next arrow buttons and a dot indicator. The Cards
    // section has a clickable grid of special cards; tapping one updates a
    // shared power-info panel underneath. Continue only appears on the
    // last section.
    public class InstructionsController : MonoBehaviour
    {
        [Header("Next scene")]
        [SerializeField] private string lobbySceneName = "Lobby";

        [Header("Live UI References (auto-filled by 'Build UI Now')")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private GameObject[] sectionRoots = new GameObject[SectionCount];
        [SerializeField] private Image[] dotImages = new Image[SectionCount];
        [SerializeField] private TextMeshProUGUI powerInfoText;
        [SerializeField] private Image[] cardFrames = new Image[8];

        [Header("Theme (used only by the 'Build UI Now' generator tool)")]
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color panelColor = new Color(0.06f, 0.13f, 0.21f, 0.92f);
        [SerializeField] private Color dotInactiveColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color cardFrameSelectedColor = new Color(0.95f, 0.91f, 0.85f, 1f);
        [SerializeField] private Color cardFrameNormalColor = new Color(1f, 1f, 1f, 0f);

        [Header("Card Illustrations (Cards section grid)")]
        [Tooltip("Pulled from PixelCardsSheet - these 8 populate the clickable card grid, in this order.")]
        [SerializeField] private Sprite sevenIllustration;
        [SerializeField] private Sprite eightIllustration;
        [SerializeField] private Sprite nineIllustration;
        [SerializeField] private Sprite tenIllustration;
        [SerializeField] private Sprite jackIllustration;
        [SerializeField] private Sprite queenIllustration;
        [SerializeField] private Sprite redKingIllustration;
        [SerializeField] private Sprite blackKingIllustration;

        private const int SectionCount = 3;
        private const string SeenFlagPrefix = "cabboo_seen_instructions_";

        private int _currentSection;
        private int _selectedCard;
        private (string label, string power)[] _cardInfo;

        private void Awake()
        {
            BuildCardInfo();

            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (prevButton != null) prevButton.onClick.AddListener(() => GoToSection(_currentSection - 1));
            if (nextButton != null) nextButton.onClick.AddListener(() => GoToSection(_currentSection + 1));

            for (int i = 0; i < cardFrames.Length; i++)
            {
                if (cardFrames[i] == null) continue;
                int capturedIndex = i;
                var button = cardFrames[i].GetComponent<Button>();
                if (button != null) button.onClick.AddListener(() => SelectCard(capturedIndex));
            }

            GoToSection(0);
            SelectCard(0);
        }

        private void BuildCardInfo()
        {
            _cardInfo = new (string, string)[]
            {
                ("7", "Look at one of your own cards."),
                ("8", "Look at one of your own cards."),
                ("9", "Look at one of another player's cards."),
                ("10", "Look at one of another player's cards."),
                ("Jack", "Skip the next player's turn."),
                ("Queen", "Blind Swap - trade a card with another player's, without looking."),
                ("Red King", "Look at another player's card, then decide whether to swap it."),
                ("Black King", "No special power."),
            };
        }

        private void GoToSection(int index)
        {
            _currentSection = Mathf.Clamp(index, 0, SectionCount - 1);

            for (int i = 0; i < sectionRoots.Length; i++)
            {
                if (sectionRoots[i] != null) sectionRoots[i].SetActive(i == _currentSection);
            }

            for (int i = 0; i < dotImages.Length; i++)
            {
                if (dotImages[i] != null) dotImages[i].color = i == _currentSection ? textColor : dotInactiveColor;
            }

            if (prevButton != null) prevButton.gameObject.SetActive(_currentSection > 0);
            if (nextButton != null) nextButton.gameObject.SetActive(_currentSection < SectionCount - 1);
            if (continueButton != null) continueButton.gameObject.SetActive(_currentSection == SectionCount - 1);
        }

        private void SelectCard(int index)
        {
            if (_cardInfo == null || index < 0 || index >= _cardInfo.Length) return;
            _selectedCard = index;

            for (int i = 0; i < cardFrames.Length; i++)
            {
                if (cardFrames[i] != null) cardFrames[i].color = i == _selectedCard ? cardFrameSelectedColor : cardFrameNormalColor;
            }

            if (powerInfoText != null)
            {
                var (label, power) = _cardInfo[_selectedCard];
                powerInfoText.text = $"<b>{label}:</b> {power}";
            }
        }

        private void OnContinueClicked()
        {
            string username = PlayerPrefs.GetString("auth_username", "");
            if (!string.IsNullOrEmpty(username))
            {
                PlayerPrefs.SetInt(SeenFlagPrefix + username, 1);
                PlayerPrefs.Save();
            }

            SceneManager.LoadScene(lobbySceneName);
        }

        // Call this from AuthController (or anywhere else) before deciding
        // whether to route to Instructions or straight to Lobby.
        public static bool HasSeenInstructions(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            return PlayerPrefs.GetInt(SeenFlagPrefix + username, 0) == 1;
        }

        // ---------- Editor-only UI generator (same pattern as AuthController) ----------
        // With this scene open in Edit Mode: right-click the component header
        // in the Inspector -> "Build UI Now". Rebuilds the whole panel from
        // scratch under the scene's Canvas and wires every button.
        private Canvas _canvas;
        private GameObject _panelRoot;
        private const float PanelWidth = 720f;
        private const float PanelHeight = 520f;
        private const float SidePadding = 24f;
        private const float TitleHeight = 50f;
        private const float ArrowAreaWidth = 44f;
        private const float DotsAreaHeight = 30f;
        private const float ButtonAreaHeight = 60f;

        [ContextMenu("Build UI Now")]
        private void BuildUI()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("InstructionsController: no Canvas found in scene. Add a Canvas first.");
                return;
            }

            BuildCardInfo();
            ClearGeneratedUI();
            BuildPanel();
        }

        private void ClearGeneratedUI()
        {
            var existing = _canvas.transform.Find("InstructionsPanel");
            if (existing == null) return;
#if UNITY_EDITOR
            DestroyImmediate(existing.gameObject);
#else
            Destroy(existing.gameObject);
#endif
        }

        private void BuildPanel()
        {
            _panelRoot = new GameObject("InstructionsPanel", typeof(RectTransform));
            _panelRoot.transform.SetParent(_canvas.transform, false);
            var panelRt = _panelRoot.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            panelRt.anchoredPosition = Vector2.zero;

            var bg = _panelRoot.AddComponent<Image>();
            if (panelSprite != null)
            {
                bg.sprite = panelSprite;
                bg.type = Image.Type.Sliced;
            }
            bg.color = panelColor;

            CreateTitle(_panelRoot.transform, "HOW TO PLAY CABBOO");

            float carouselTop = TitleHeight;
            float carouselBottom = DotsAreaHeight + ButtonAreaHeight;
            var carouselRow = CreateCarouselRow(_panelRoot.transform, carouselTop, carouselBottom);

            sectionRoots[0] = BuildLobbySection(carouselRow);
            sectionRoots[1] = BuildCardsSection(carouselRow);
            sectionRoots[2] = BuildScoringSection(carouselRow);

            // Dots sit centered in a DotsAreaHeight-tall band directly above
            // the button area, both measured up from the panel's bottom edge.
            CreateDots(_panelRoot.transform, ButtonAreaHeight + DotsAreaHeight * 0.5f);
            continueButton = CreateButton(_panelRoot.transform, "Got it, let's play!");

            GoToSection(0);
            SelectCard(0);
        }

        private RectTransform CreateCarouselRow(Transform parent, float topInset, float bottomInset)
        {
            var rowGo = new GameObject("CarouselRow", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 0f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.offsetMin = new Vector2(SidePadding, bottomInset);
            rowRt.offsetMax = new Vector2(-SidePadding, -topInset);

            prevButton = CreateArrowButton(rowRt, "PrevButton", true);
            nextButton = CreateArrowButton(rowRt, "NextButton", false);

            var sectionArea = new GameObject("SectionArea", typeof(RectTransform));
            sectionArea.transform.SetParent(rowRt, false);
            var sectionAreaRt = sectionArea.GetComponent<RectTransform>();
            sectionAreaRt.anchorMin = new Vector2(0f, 0f);
            sectionAreaRt.anchorMax = new Vector2(1f, 1f);
            sectionAreaRt.offsetMin = new Vector2(ArrowAreaWidth + 12f, 0f);
            sectionAreaRt.offsetMax = new Vector2(-(ArrowAreaWidth + 12f), 0f);

            return sectionAreaRt;
        }

        private Button CreateArrowButton(Transform parent, string name, bool isLeft)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rt.anchorMax = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rt.pivot = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rt.sizeDelta = new Vector2(ArrowAreaWidth, ArrowAreaWidth);
            rt.anchoredPosition = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.08f);
            var button = go.AddComponent<Button>();

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = isLeft ? "<" : ">";
            tmp.font = fontAsset;
            tmp.fontSize = 22;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private GameObject BuildLobbySection(RectTransform parent)
        {
            var sectionGo = CreateSectionRoot(parent, "SectionLobby");

            CreateSectionHeading(sectionGo.transform, "Creating a lobby");

            string[] steps =
            {
                "Tap Create Lobby to get a 6-character invite code.",
                "Share the code with friends so they can join.",
                "Once everyone's in, the host taps Start to begin.",
            };

            float y = 44f;
            for (int i = 0; i < steps.Length; i++)
            {
                CreateStepRow(sectionGo.transform, (i + 1).ToString(), steps[i], y);
                y += 56f;
            }

            return sectionGo;
        }

        private GameObject BuildCardsSection(RectTransform parent)
        {
            var sectionGo = CreateSectionRoot(parent, "SectionCards");

            CreateSectionHeading(sectionGo.transform, "Tap a card to see its power");
            CreateIntroLine(sectionGo.transform, "On your turn: draw and swap it into your grid or discard it, or draw a special card and use its power.", 40f);

            var sprites = new[] { sevenIllustration, eightIllustration, nineIllustration, tenIllustration, jackIllustration, queenIllustration, redKingIllustration, blackKingIllustration };

            // Fixed card size (not derived from available width) so cards
            // stay a sensible playing-card aspect ratio and the whole grid
            // fits comfortably within the section's vertical budget - only
            // the horizontal centering adapts to the available width.
            const int columns = 4;
            const float gap = 10f;
            const float cellWidth = 64f;
            const float cellHeight = 90f;
            float gridTop = 96f;

            float sectionWidth = PanelWidth - SidePadding * 2f - (ArrowAreaWidth + 12f) * 2f;
            float gridTotalWidth = columns * cellWidth + (columns - 1) * gap;
            float startX = (sectionWidth - gridTotalWidth) * 0.5f;

            for (int i = 0; i < sprites.Length; i++)
            {
                int col = i % columns;
                int row = i / columns;
                float x = startX + col * (cellWidth + gap);
                float y = gridTop + row * (cellHeight + gap);
                cardFrames[i] = CreateCardCell(sectionGo.transform, sprites[i], x, y, cellWidth, cellHeight);
            }

            float panelY = gridTop + 2 * cellHeight + gap + 16f;
            powerInfoText = CreatePowerInfoPanel(sectionGo.transform, panelY);

            return sectionGo;
        }

        private GameObject BuildScoringSection(RectTransform parent)
        {
            var sectionGo = CreateSectionRoot(parent, "SectionScoring");

            CreateSectionHeading(sectionGo.transform, "Scoring a round");

            var rows = new (string label, string value)[]
            {
                ("Ace / number cards", "face value"),
                ("Jack", "11"),
                ("Queen", "12"),
                ("Red King", "13"),
                ("Black King", "0"),
                ("Joker", "-1"),
            };

            float y = 44f;
            foreach (var row in rows)
            {
                CreateScoreRow(sectionGo.transform, row.label, row.value, y);
                y += 32f;
            }

            CreateIntroLine(sectionGo.transform, "Call Kaboo when you think you have the lowest hand. Guess right: -5 bonus. Guess wrong: +5 penalty.", y + 12f);

            return sectionGo;
        }

        private GameObject CreateSectionRoot(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private void CreateSectionHeading(Transform parent, string text)
        {
            var go = new GameObject("Heading", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 28f);
            rt.anchoredPosition = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = fontAsset;
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.TopLeft;
        }

        private void CreateIntroLine(Transform parent, string text, float topY)
        {
            var go = new GameObject("IntroLine", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 48f);
            rt.anchoredPosition = new Vector2(0f, -topY);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = fontAsset;
            tmp.fontSize = 13;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        private void CreateStepRow(Transform parent, string number, string text, float topY)
        {
            var rowGo = new GameObject("Step", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.sizeDelta = new Vector2(0f, 48f);
            rowRt.anchoredPosition = new Vector2(0f, -topY);

            var badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(rowGo.transform, false);
            var badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0f, 1f);
            badgeRt.anchorMax = new Vector2(0f, 1f);
            badgeRt.pivot = new Vector2(0f, 1f);
            badgeRt.sizeDelta = new Vector2(24f, 24f);
            badgeRt.anchoredPosition = Vector2.zero;
            badgeGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

            var badgeTextGo = new GameObject("Text", typeof(RectTransform));
            badgeTextGo.transform.SetParent(badgeGo.transform, false);
            var badgeTextRt = badgeTextGo.GetComponent<RectTransform>();
            badgeTextRt.anchorMin = Vector2.zero;
            badgeTextRt.anchorMax = Vector2.one;
            badgeTextRt.offsetMin = Vector2.zero;
            badgeTextRt.offsetMax = Vector2.zero;
            var badgeTmp = badgeTextGo.AddComponent<TextMeshProUGUI>();
            badgeTmp.text = number;
            badgeTmp.font = fontAsset;
            badgeTmp.fontSize = 13;
            badgeTmp.color = textColor;
            badgeTmp.alignment = TextAlignmentOptions.Center;

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(rowGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            // Horizontal: stretch-fill the row minus a 34px left inset (for
            // the number badge), governed entirely by offsetMin/offsetMax.
            // Vertical: fixed 48px height anchored to the row's top edge.
            textRt.anchorMin = new Vector2(0f, 1f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.pivot = new Vector2(0f, 1f);
            textRt.offsetMin = new Vector2(34f, -48f);
            textRt.offsetMax = new Vector2(0f, 0f);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = fontAsset;
            tmp.fontSize = 14;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        private Image CreateCardCell(Transform parent, Sprite sprite, float x, float y, float width, float height)
        {
            var cellGo = new GameObject("CardCell", typeof(RectTransform), typeof(Image), typeof(Button));
            cellGo.transform.SetParent(parent, false);
            var cellRt = cellGo.GetComponent<RectTransform>();
            cellRt.anchorMin = new Vector2(0f, 1f);
            cellRt.anchorMax = new Vector2(0f, 1f);
            cellRt.pivot = new Vector2(0f, 1f);
            cellRt.sizeDelta = new Vector2(width, height);
            cellRt.anchoredPosition = new Vector2(x, -y);

            var frameImage = cellGo.GetComponent<Image>();
            frameImage.color = cardFrameNormalColor;

            var cardImgGo = new GameObject("CardArt", typeof(RectTransform), typeof(Image));
            cardImgGo.transform.SetParent(cellGo.transform, false);
            var cardImgRt = cardImgGo.GetComponent<RectTransform>();
            cardImgRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardImgRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardImgRt.pivot = new Vector2(0.5f, 0.5f);
            cardImgRt.sizeDelta = new Vector2(width - 8f, height - 8f);
            cardImgRt.anchoredPosition = Vector2.zero;
            var cardImg = cardImgGo.GetComponent<Image>();
            cardImg.sprite = sprite;
            cardImg.preserveAspect = true;

            return frameImage;
        }

        private TextMeshProUGUI CreatePowerInfoPanel(Transform parent, float topY)
        {
            var panelGo = new GameObject("PowerInfoPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(parent, false);
            var rt = panelGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 48f);
            rt.anchoredPosition = new Vector2(0f, -topY);
            panelGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(panelGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10f, 6f);
            textRt.offsetMax = new Vector2(-10f, -6f);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.font = fontAsset;
            tmp.fontSize = 13;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;

            return tmp;
        }

        private void CreateScoreRow(Transform parent, string label, string value, float topY)
        {
            var rowGo = new GameObject("ScoreRow", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.sizeDelta = new Vector2(0f, 26f);
            rowRt.anchoredPosition = new Vector2(0f, -topY);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(0.6f, 1f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.font = fontAsset;
            labelTmp.fontSize = 13;
            labelTmp.color = textColor;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var valueGo = new GameObject("Value", typeof(RectTransform));
            valueGo.transform.SetParent(rowGo.transform, false);
            var valueRt = valueGo.GetComponent<RectTransform>();
            valueRt.anchorMin = new Vector2(0.6f, 0f);
            valueRt.anchorMax = new Vector2(1f, 1f);
            valueRt.offsetMin = Vector2.zero;
            valueRt.offsetMax = Vector2.zero;
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = value;
            valueTmp.font = fontAsset;
            valueTmp.fontSize = 13;
            valueTmp.fontStyle = FontStyles.Bold;
            valueTmp.color = textColor;
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
        }

        private void CreateTitle(Transform parent, string text)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, TitleHeight);
            rt.anchoredPosition = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = fontAsset;
            tmp.fontSize = 28;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // bottomOffsetToCenter is measured from the panel's bottom edge up
        // to the vertical center of the dots row.
        private void CreateDots(Transform parent, float bottomOffsetToCenter)
        {
            var rowGo = new GameObject("Dots", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0f);
            rowRt.anchorMax = new Vector2(0.5f, 0f);
            rowRt.pivot = new Vector2(0.5f, 0.5f);
            rowRt.sizeDelta = new Vector2(SectionCount * 20f, DotsAreaHeight);
            rowRt.anchoredPosition = new Vector2(0f, bottomOffsetToCenter);

            for (int i = 0; i < SectionCount; i++)
            {
                var dotGo = new GameObject("Dot" + i, typeof(RectTransform), typeof(Image));
                dotGo.transform.SetParent(rowGo.transform, false);
                var dotRt = dotGo.GetComponent<RectTransform>();
                dotRt.sizeDelta = new Vector2(8f, 8f);
                dotRt.anchoredPosition = new Vector2((i - (SectionCount - 1) * 0.5f) * 20f, 0f);
                dotImages[i] = dotGo.GetComponent<Image>();
                dotImages[i].color = dotInactiveColor;
            }
        }

        private Button CreateButton(Transform parent, string label)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(260f, 44f);
            rt.anchoredPosition = new Vector2(0f, 8f);

            var image = go.AddComponent<Image>();
            if (buttonSprite != null)
            {
                image.sprite = buttonSprite;
                image.type = Image.Type.Sliced;
            }

            var button = go.AddComponent<Button>();

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.font = fontAsset;
            tmp.fontSize = 20;
            tmp.color = new Color(0.15f, 0.1f, 0.05f);
            tmp.alignment = TextAlignmentOptions.Center;

            return button;
        }
    }
}
