namespace BlackJack
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class UIButtonTooltip : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerClickHandler
    {
        [TextArea] public string tooltipText;

        bool pointerInside = false;

        public void OnPointerEnter(PointerEventData eventData) {
            pointerInside = true;
            TooltipController.ShowTooltip(tooltipText);
        }

        public void OnPointerExit(PointerEventData eventData) {
            pointerInside = false;
            TooltipController.HideTooltip();
        }

        // TOUCH SUPPORT
        public void OnPointerClick(PointerEventData eventData) {
            if (pointerInside) {
                TooltipController.ShowTooltip(tooltipText);
            }
        }
    }
}