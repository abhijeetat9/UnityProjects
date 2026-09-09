using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CardGame.UI
{
    public class AuthController : MonoBehaviour
    {
        [Header("Backend")]
        [SerializeField] private string backendBaseUrl = "https://cardgameprototype.onrender.com";
        [SerializeField] private string nextSceneName = "Lobby";

        [Header("Live UI References (drag the prefab-instantiated panel's children here)")]
        [SerializeField] private GameObject loginPanelRoot;
        [SerializeField] private TMP_InputField loginUsernameField;
        [SerializeField] private TMP_InputField loginPasswordField;
        [SerializeField] private TextMeshProUGUI loginStatusText;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button loginToSignupLinkButton;

        [SerializeField] private GameObject signupPanelRoot;
        [SerializeField] private TMP_InputField signupUsernameField;
        [SerializeField] private TMP_InputField signupEmailField;
        [SerializeField] private TMP_InputField signupPasswordField;
        [SerializeField] private TextMeshProUGUI signupStatusText;
        [SerializeField] private Button signupButton;
        [SerializeField] private Button signupToLoginLinkButton;

        [Header("Theme (used only by the 'Build UI Now' generator tool)")]
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color panelColor = new Color(0.06f, 0.13f, 0.21f, 0.85f);

        [Header("Hero (Option B split-screen) - used only by the generator tool")]
        [Tooltip("3 card sprites to fan out on the left-hand hero side. Pick any from the PixelCardsSheet.")]
        [SerializeField] private Sprite[] heroCards;
        [SerializeField] private Color heroOverlayColor = new Color(0.04f, 0.08f, 0.13f, 0.47f);
        [SerializeField] private float horizontalSpacing;
        [SerializeField] private float verticalSpacing;

        private Canvas _canvas;
        private GameObject _heroPanel;

        [Serializable] private struct LoginData
        {
            public string username;
            public string password;
        }

        [Serializable]
        private struct SignupData
        {
            public string email;
            public string username;
            public string password;
        }

        [Serializable]
        private struct ResponseData
        {
            public string token;
        }

        private const string TokenKey = "auth_token";

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            WireUI();
            ShowSignup();
        }
        
        private void WireUI()
        {
            loginButton.onClick.AddListener(() => StartCoroutine(DoLogin()));
            loginToSignupLinkButton.onClick.AddListener(() =>
            {
                loginStatusText.text = "";
                ShowSignup();
            });

            signupButton.onClick.AddListener(() => StartCoroutine(DoSignup()));
            signupToLoginLinkButton.onClick.AddListener(() =>
            {
                signupStatusText.text = "";
                ShowLogin();
            });
        }

        private bool ValidateReferences()
        {
            bool ok = true;
            void Check(UnityEngine.Object obj, string fieldName)
            {
                if (obj != null) return;
                Debug.LogError($"AuthController: '{fieldName}' is not assigned in the Inspector. " +
                                "Drag the matching child from LoginPanel/SignupPanel into this slot.", this);
                ok = false;
            }

            Check(loginPanelRoot, nameof(loginPanelRoot));
            Check(loginUsernameField, nameof(loginUsernameField));
            Check(loginPasswordField, nameof(loginPasswordField));
            Check(loginStatusText, nameof(loginStatusText));
            Check(loginButton, nameof(loginButton));
            Check(loginToSignupLinkButton, nameof(loginToSignupLinkButton));

            Check(signupPanelRoot, nameof(signupPanelRoot));
            Check(signupUsernameField, nameof(signupUsernameField));
            Check(signupEmailField, nameof(signupEmailField));
            Check(signupPasswordField, nameof(signupPasswordField));
            Check(signupStatusText, nameof(signupStatusText));
            Check(signupButton, nameof(signupButton));
            Check(signupToLoginLinkButton, nameof(signupToLoginLinkButton));

            return ok;
        }

        private void ShowLogin()
        {
            loginPanelRoot.SetActive(true);
            signupPanelRoot.SetActive(false);
        }

        private void ShowSignup()
        {
            loginPanelRoot.SetActive(false);
            signupPanelRoot.SetActive(true);
        }

        private IEnumerator DoLogin()
        {
            LoginData _loginData = new LoginData();
            _loginData.username = loginUsernameField.text;
            _loginData.password = loginPasswordField.text;
            var data = JsonUtility.ToJson(_loginData);

            var req = new UnityWebRequest(backendBaseUrl + "/auth/login", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (UnityWebRequest.Result.Success == req.result)
            {
                var responseData = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);
                PlayerPrefs.SetString(TokenKey, responseData.token);
                PlayerPrefs.SetString("auth_username", _loginData.username);
                PlayerPrefs.Save();
                loginStatusText.text = "Successfully logged in!";
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                loginStatusText.text = req.error;
            }
        }

        private IEnumerator DoSignup()
        {
            SignupData _signupData = new SignupData();
            _signupData.email = signupEmailField.text;
            _signupData.username = signupUsernameField.text;
            _signupData.password = signupPasswordField.text;
            var data = JsonUtility.ToJson(_signupData);
            var req = new UnityWebRequest(backendBaseUrl + "/auth/signup", "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (UnityWebRequest.Result.Success == req.result)
            {
                loginUsernameField.text = signupUsernameField.text;
                loginPasswordField.text = signupPasswordField.text;
                signupStatusText.text = "Successfully created user!";

                yield return StartCoroutine(DoLogin());
            }
            else
            {
                signupStatusText.text = req.error;
            }
        }

        // ---------- Editor-only UI generator (kept for future redesigns) ----------
        // Callable from the Editor (Inspector -> right-click component header ->
        // "Build UI Now") with the scene open in Edit Mode. Rebuilds HeroPanel/
        // LoginPanel/SignupPanel from scratch under the Canvas and assigns the
        // results into the same serialized fields above, then wires them via
        // WireUI() -- so this stays a fully working standalone regenerate tool,
        // not a second parallel wiring path. Not called at runtime; the live
        // scene uses whatever's already dragged into those fields (the baked
        // prefab instances) instead.
        [ContextMenu("Build UI Now")]
        private void BuildUI()
        {
            _canvas = FindAnyObjectByType<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("AuthController: no Canvas found in scene.");
                return;
            }

            ClearGeneratedUI();

            CreateHeroSection();
            BuildLoginPanel();
            BuildSignupPanel();

            WireUI();
            ShowSignup();
        }

        // Removes any previously-generated panels (by name) before rebuilding,
        // so re-running "Build UI Now" in the Editor gives a clean regenerate
        // instead of stacking duplicate copies under the Canvas.
        private void ClearGeneratedUI()
        {
            foreach (var name in new[] { "HeroPanel", "LoginPanel", "SignupPanel" })
            {
                var existing = _canvas.transform.Find(name);
                if (existing == null) continue;
#if UNITY_EDITOR
                DestroyImmediate(existing.gameObject);
#else
                Destroy(existing.gameObject);
#endif
            }
        }

        private void CreateHeroSection()
        {
            _heroPanel = CreatePanel("HeroPanel");
            var rt = _heroPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            CreateTitle(_heroPanel.transform, "CABBOO");
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var image = _heroPanel.GetComponent<Image>();
            image.color = heroOverlayColor;

            for (int i = 0; i < heroCards.Length; i++)
            {
                GameObject stackImage = new GameObject("StackImages", typeof(RectTransform), typeof(Image));
                stackImage.transform.SetParent(_heroPanel.transform, false);

                var stackImageRt = stackImage.GetComponent<RectTransform>();
                var stackImageGetImage = stackImage.GetComponent<Image>();

                stackImageRt.anchorMin = new Vector2(0.5f, 0.5f);
                stackImageRt.anchorMax = new Vector2(0.5f, 0.5f);
                stackImageRt.sizeDelta = new Vector2(140f, 195f);
                stackImageRt.pivot = new Vector2(0.5f, 1f);

                float horizontalOffset = (i - 1) * horizontalSpacing;
                float verticalOffset = Mathf.Abs(i - 1) * verticalSpacing;

                stackImageRt.anchoredPosition = new Vector2(horizontalOffset, verticalOffset);

                stackImageGetImage.sprite = heroCards[i];
                float angle = -20f + (i * 20f);
                stackImageGetImage.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            }
        }

        // ---------- UI construction ----------

        private void BuildLoginPanel()
        {
            loginPanelRoot = CreatePanel("LoginPanel");
            CreateTitle(loginPanelRoot.transform, "Log In");

            loginUsernameField = CreateInputField(loginPanelRoot.transform, "Username", false, -75f);
            loginPasswordField = CreateInputField(loginPanelRoot.transform, "Password", true, -125f);

            loginStatusText = CreateStatusText(loginPanelRoot.transform, -170f);

            loginButton = CreateButton(loginPanelRoot.transform, "Log In", -210f);
            loginToSignupLinkButton = CreateLinkButton(loginPanelRoot.transform, "Need an account?", "Sign Up", -260f);
        }

        private void BuildSignupPanel()
        {
            signupPanelRoot = CreatePanel("SignupPanel");

            CreateTitle(signupPanelRoot.transform, "CREATE ACCOUNT");

            signupUsernameField = CreateInputField(signupPanelRoot.transform, "Username", false, -75f);
            signupEmailField = CreateInputField(signupPanelRoot.transform, "Email", false, -125f);
            signupPasswordField = CreateInputField(signupPanelRoot.transform, "Password", true, -175f);

            signupStatusText = CreateStatusText(signupPanelRoot.transform, -220f);

            signupButton = CreateButton(signupPanelRoot.transform, "Sign Up", -260f);
            signupToLoginLinkButton = CreateLinkButton(signupPanelRoot.transform, "Already have an account?", "Log In", -310f);
        }

        // ---------- small UI factory helpers ----------

        private GameObject CreatePanel(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.75f, 0.5f);
            rt.anchorMax = new Vector2(0.75f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 400f);
            rt.anchoredPosition = Vector2.zero;

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
            rt.sizeDelta = new Vector2(320f, 50f);
            rt.anchoredPosition = new Vector2(0f, -20f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = fontAsset;
            tmp.fontSize = 32;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private TMP_InputField CreateInputField(Transform parent, string placeholder, bool isPassword, float topY)
        {
            var fieldGo = new GameObject(placeholder + "Field", typeof(RectTransform));
            fieldGo.SetActive(false);
            fieldGo.transform.SetParent(parent, false);
            var fieldRt = fieldGo.GetComponent<RectTransform>();
            fieldRt.anchorMin = new Vector2(0.5f, 1f);
            fieldRt.anchorMax = new Vector2(0.5f, 1f);
            fieldRt.pivot = new Vector2(0.5f, 1f);
            fieldRt.sizeDelta = new Vector2(300f, 40f);
            fieldRt.anchoredPosition = new Vector2(0f, topY);

            var bgImage = fieldGo.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.12f);

            var inputField = fieldGo.AddComponent<TMP_InputField>();

            // Text Area (viewport with mask so text scrolls within bounds)
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
            if (isPassword)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
            }

            // Blinking caret: TMP_InputField already blinks the caret whenever
            // the field is focused -- it just needs a visible color/width, since
            // the defaults are easy to lose against a dark background.
            inputField.customCaretColor = true;
            inputField.caretColor = new Color(1f, 0.86f, 0.6f, 1f);
            inputField.caretWidth = 2;
            inputField.caretBlinkRate = 0.85f;
            inputField.selectionColor = new Color(1f, 0.86f, 0.6f, 0.35f);

            fieldGo.SetActive(true);

            return inputField;
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
            tmp.color = new Color(1f, 0.6f, 0.6f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = "";
            return tmp;
        }

        private Button CreateButton(Transform parent, string label, float topY)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(200f, 40f);
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

        private Button CreateLinkButton(Transform parent, string promptText, string actionText, float topY)
        {
            var go = new GameObject(actionText + "Link", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(320f, 28f);
            rt.anchoredPosition = new Vector2(0f, topY);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            // Rich text: only the actionable part is colored/underlined/bold,
            // so it's visually obvious what's clickable vs. plain label text.
            const string accentHex = "FFDA99";
            tmp.text = $"{promptText} <color=#{accentHex}><u><b>{actionText}</b></u></color>";
            tmp.font = fontAsset;
            tmp.fontSize = 16;
            tmp.color = new Color(0.85f, 0.9f, 0.98f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            var button = go.AddComponent<Button>();
            button.targetGraphic = tmp;

            return button;
        }
    }
}
