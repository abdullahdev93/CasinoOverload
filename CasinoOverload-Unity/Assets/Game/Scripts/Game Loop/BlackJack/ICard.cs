using UnityEngine;


namespace BlackJack
{
    public abstract class ICard : MonoBehaviour
    {
        public bool IsShowing { get; set; }
        public abstract int AddScore(int score);

        public abstract void Show();
        public abstract void Hide();
    }
}
