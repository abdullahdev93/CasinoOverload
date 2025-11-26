using DG.Tweening;
using UnityEngine;


namespace BlackJack
{
    public class AceCard : ICard
    {
        private void Start() {
            transform.eulerAngles = Vector3.up * 180;
        }

        public override int AddScore(int score) {
            return score + ((score + 11 <= 21) ? 11 : 1);
        }

        public override void Hide() {
            transform.eulerAngles = (Vector3.up * 180);
            IsShowing = false;
        }

        public override void Show() {
            transform.DORotate(Vector3.zero, 1 / 3f);

            IsShowing = true;
        }
    }

}
