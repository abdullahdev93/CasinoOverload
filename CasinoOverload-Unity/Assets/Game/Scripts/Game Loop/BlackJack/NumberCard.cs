using DG.Tweening;
using UnityEngine;


namespace BlackJack
{
    public class NumberCard : ICard
    {
        [SerializeField] private int score;

        private void Start() {
            IsShowing = false;
            transform.eulerAngles = Vector3.up * 180;
        }

        public override int AddScore(int score) {
            return this.score + score;
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
