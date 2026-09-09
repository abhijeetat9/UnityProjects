using UnityEngine;

namespace CardGame.UI
{
    /// <summary>
    /// Scales a full-screen background sprite so it always covers the camera's
    /// orthographic view, regardless of window resolution or aspect ratio.
    /// Attach to a GameObject with a SpriteRenderer positioned behind everything else.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundFitter : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        private SpriteRenderer _spriteRenderer;
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private float _lastOrthographicSize;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Start()
        {
            Fit();
        }

        private void LateUpdate()
        {
            // Only recompute when something that affects the fit has actually
            // changed, so this stays cheap even though it runs every frame.
            if (Screen.width != _lastScreenWidth ||
                Screen.height != _lastScreenHeight ||
                (targetCamera != null && !Mathf.Approximately(targetCamera.orthographicSize, _lastOrthographicSize)))
            {
                Fit();
            }
        }

        private void Fit()
        {
            if (targetCamera == null || _spriteRenderer == null || _spriteRenderer.sprite == null)
            {
                return;
            }

            float halfHeight = targetCamera.orthographicSize;
            float halfWidth = halfHeight * targetCamera.aspect;

            Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return;
            }

            // Scale to cover the full view (may overflow slightly on one axis),
            // matching an "aspect fill" behaviour so there are never gaps.
            float scaleX = (halfWidth * 2f) / spriteSize.x;
            float scaleY = (halfHeight * 2f) / spriteSize.y;
            float scale = Mathf.Max(scaleX, scaleY);

            transform.localScale = new Vector3(scale, scale, 1f);

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastOrthographicSize = targetCamera.orthographicSize;
        }
    }
}
