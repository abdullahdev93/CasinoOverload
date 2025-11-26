using System;
using System.Collections.Generic;


namespace BlackJack
{
    [System.Serializable]
    public class PlayerHand : IBetable
    {
        public List<ICard> Cards = new List<ICard>();
        public bool Stand = false;
        public bool CanDouble = true;

        public int BetAmount { 
            get; 
            set;
        }

        public event Action<IBetable, bool> OnBetEnded;

        public int GetScore() {
            int total = 0;

            foreach (var c in Cards) {
                if (c.IsShowing)
                    total = c.AddScore(total);
            }

            return total;
        }

        public bool IsPair() {

            return Cards.Count == 2 &&
                   Cards[0].AddScore(0) == Cards[1].AddScore(0);
        }

        public void CallBetEnded(bool won) {

            OnBetEnded?.Invoke(this , won);
        }
    }
}
