
using TMPro;
using UnityEngine;

namespace BlackJack
{
    using UnityEngine;
    using TMPro;

    public class TooltipController : MonoBehaviour
    {

        [SerializeField] TextMeshProUGUI text;
        [SerializeField] RectTransform rect;

        private Camera cam;          // camera that looks at the world-space canvas
        private Canvas canvas;       // reference to the world-space canvas

        private bool showing = false;
        private static TooltipController Instance;


        void Awake() {
            Instance = this;
            HideTooltip();

            canvas = GetComponentInParent<Canvas>();
            cam = canvas.worldCamera;
        }

        void Update() {
            if (!showing)
                return;

            Vector3 screenPos = GetPointerPosition();
            PositionTooltip(screenPos);
        }

        Vector3 GetPointerPosition() {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).position;

            return Input.mousePosition;
        }

        void PositionTooltip(Vector3 screenPosition) {
            Vector2 worldPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition + new Vector3(50f, 50f, 0),
                cam,
                out worldPos
            );

            rect.localPosition = worldPos;
        }

        public static void ShowTooltip(string msg) {
            if (!Instance) {
                return;
            }

            Instance.text.text = msg;
            Instance.gameObject.SetActive(true);
            Instance.showing = true;

            // Immediately move to pointer
            Instance.PositionTooltip(Instance.GetPointerPosition());
        }

        public static void HideTooltip() {
            if (!Instance) {
                return;
            }

            Instance.gameObject.SetActive(false);
            Instance.showing = false;
        }
    }
}