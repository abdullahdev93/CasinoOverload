using UnityEngine;
using TMPro;
using DG.Tweening;

namespace BlackJack
{
    public class PokerChip : MonoBehaviour
    {
        [SerializeField] TextMeshPro text;

        public void UpdateDisplay(int score, Vector3 point) {
            // Auto fill text
            text.text = score.ToString();

            // Kill previous tweens to avoid stacking 
            transform.DOKill();
            text.DOKill();

            // Move chip to point
            transform.DOMove(point, 0.4f)
                .SetEase(Ease.OutBack);

        }

        public void UpdateDisplay( Vector3 point) {

            // Kill previous tweens to avoid stacking 
            transform.DOKill();
            text.DOKill();

            // Move chip to point
            transform.DOMove(point, 0.4f)
                .SetEase(Ease.OutBack);

        }
    }
}
