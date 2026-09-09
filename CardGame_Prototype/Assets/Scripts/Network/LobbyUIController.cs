using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGame.UI
{
    [RequireComponent(typeof(LobbyApiController))]
    public class LobbyUIController : MonoBehaviour
    {
        [Header("Live UI References (drag the prefab-instantiated panel's children here)")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private TextMeshProUGUI inviteCodeText;
        [SerializeField] private TMP_InputField joinCodeField;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Theme (used only by the 'Build UI Now' generator tool)")]
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color panelColor = new Color(0.06f, 0.13f, 0.21f, 0.85f);

        private LobbyApiController _api;
        private LobbyAuthenticator _authenticator;
        private Canvas _canvas;

        private void Awake()
        {
            _api = GetComponent<LobbyApiController>();
            _authenticator = FindAnyObjectByType<LobbyAuthenticator>();
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            WireUI();
        }

        private void WireUI()
        {
            createLobbyButton.onClick.AddListener(() =>
            {
                statusText.text = "";
                StartCoroutine(_api.CreateLobby(OnCreateComplete));
            });

            joinLobbyButton.onClick.AddListener(() =>
            {
                statusText.text = "";
                string code = joinCodeField.text.Trim();
                StartCoroutine(_api.JoinLobby(code, OnJoinComplete));
            });
        }

        private bool ValidateReferences()
        {
            bool ok = true;
            void Check(Object obj, string fieldName)
            {
                if (obj != null) return;
                Debug.LogError($"LobbyUIController: '{fieldName}' is not assigned in the Inspector. " +
                                "Drag the matching child from LobbyEntryPanel into this slot.", this);
                ok = false;
            }

            Check(panelRoot, nameof(panelRoot));
            Check(createLobbyButton, nameof(createLobbyButton));
            Check(inviteCodeText, nameof(inviteCodeText));
            Check(joinCodeField, nameof(joinCodeField));
            Check(joinLobbyButton, nameof(joinLobbyButton));
            Check(statusText, nameof(statusText));

            return ok;
        }
        
        private void OnCreateComplete(bool success, string result)
        {
            if (success)
            {
                _authenticator.SetInviteCode(result);
                _authenticator.SetAuthToken(PlayerPrefs.GetString("auth_token"));
                inviteCodeText.text = $"Invite code: {result}";
                statusText.text = "Lobby created! Share the code above.";
                NetworkManager.singleton.StartClient();
                joinCodeField.text = "";
                joinCodeField.interactable = false;
                joinLobbyButton.interactable = false;
            }
            else
            {
                statusText.text = result;
            }
        }

        private void OnJoinComplete(bool success, string result)
        {
            if (success)
            {
                var lobbyJoin = joinCodeField.text.Trim();
                _authenticator.SetInviteCode(lobbyJoin);
                _authenticator.SetAuthToken(PlayerPrefs.GetString("auth_token"));
                statusText.text = "Joined lobby!";
                NetworkManager.singleton.StartClient();
            }
            else
            {
                statusText.text = result;
            }
        }
        
        [ContextMenu("Build UI Now")]
        private void BuildUI()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("LobbyUIController: no Canvas found in scene.");
                return;
            }

            var existing = _canvas.transform.Find("LobbyEntryPanel");
            if (existing != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(existing.gameObject);
#else
                Destroy(existing.gameObject);
#endif
            }

            BuildPanel();

            _api = GetComponent<LobbyApiController>();
            WireUI();
        }

        private void BuildPanel()
        {
            panelRoot = CreatePanel("LobbyEntryPanel");

            CreateTitle(panelRoot.transform, "MULTIPLAYER LOBBY");

            createLobbyButton = CreateButton(panelRoot.transform, "Create Lobby", -70f);

            inviteCodeText = CreateStatusText(panelRoot.transform, -115f);
            inviteCodeText.color = new Color(1f, 0.86f, 0.6f, 1f);
            inviteCodeText.fontSize = 20;

            joinCodeField = CreateInputField(panelRoot.transform, "Enter invite code", -135f);

            joinLobbyButton = CreateButton(panelRoot.transform, "Join Lobby", -200f);

            statusText = CreateStatusText(panelRoot.transform, -260f);
        }

        private GameObject CreatePanel(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(360f, 260f);
            rt.anchoredPosition = new Vector2(0f, -10f);

            var image = go.AddComponent<Image>();
            if (panelSprite != null)
            {
                image.sprite = panelSprite;
                image.type = Image.Type.Sliced;
            }
            image.color = panelColor;

            return go;
        }

        private void CreateTitle(Transform parent, string text)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(320f, 40f);
            rt.anchoredPosition = new Vector2(0f, -20f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = fontAsset;
            tmp.fontSize = 26;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private Button CreateButton(Transform parent, string label, float topY)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(220f, 40f);
            rt.anchoredPosition = new Vector2(0f, topY);

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
            tmp.fontSize = 18;
            tmp.color = new Color(0.15f, 0.1f, 0.05f);
            tmp.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private TextMeshProUGUI CreateStatusText(Transform parent, float topY)
        {
            var go = new GameObject("StatusText", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(320f, 30f);
            rt.anchoredPosition = new Vector2(0f, topY);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = fontAsset;
            tmp.fontSize = 14;
            tmp.color = new Color(0.85f, 0.9f, 0.98f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = "";
            return tmp;
        }

        private TMP_InputField CreateInputField(Transform parent, string placeholder, float topY)
        {
            var fieldGo = new GameObject("JoinCodeField", typeof(RectTransform));
            fieldGo.SetActive(false);
            fieldGo.transform.SetParent(parent, false);
            var fieldRt = fieldGo.GetComponent<RectTransform>();
            fieldRt.anchorMin = new Vector2(0.5f, 1f);
            fieldRt.anchorMax = new Vector2(0.5f, 1f);
            fieldRt.pivot = new Vector2(0.5f, 1f);
            fieldRt.sizeDelta = new Vector2(280f, 40f);
            fieldRt.anchoredPosition = new Vector2(0f, topY);

            var bgImage = fieldGo.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.12f);

            var inputField = fieldGo.AddComponent<TMP_InputField>();

            var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(fieldGo.transform, false);
            var textAreaRt = textArea.GetComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(10f, 6f);
            textAreaRt.offsetMax = new Vector2(-10f, -6f);

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(textArea.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.font = fontAsset;
            textTmp.fontSize = 18;
            textTmp.color = textColor;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
            placeholderGo.transform.SetParent(textArea.transform, false);
            var placeholderRt = placeholderGo.GetComponent<RectTransform>();
            placeholderRt.anchorMin = Vector2.zero;
            placeholderRt.anchorMax = Vector2.one;
            placeholderRt.offsetMin = Vector2.zero;
            placeholderRt.offsetMax = Vector2.zero;
            var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholderTmp.font = fontAsset;
            placeholderTmp.fontSize = 18;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.color = new Color(0.78f, 0.83f, 0.9f, 0.65f);
            placeholderTmp.text = placeholder;
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            inputField.textViewport = textAreaRt;
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;
            inputField.fontAsset = fontAsset;
            inputField.characterLimit = 12;

            inputField.customCaretColor = true;
            inputField.caretColor = new Color(1f, 0.86f, 0.6f, 1f);
            inputField.caretWidth = 2;
            inputField.caretBlinkRate = 0.85f;
            inputField.selectionColor = new Color(1f, 0.86f, 0.6f, 0.35f);

            fieldGo.SetActive(true);

            return inputField;
        }
    }
}
