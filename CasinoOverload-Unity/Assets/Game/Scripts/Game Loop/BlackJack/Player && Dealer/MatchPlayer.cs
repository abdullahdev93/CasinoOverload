using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BlackJack
{
    public abstract class MatchPlayer : MonoBehaviour
    {
        protected List<PlayerHand> hands = new List<PlayerHand>();
        protected int activeHandIndex = 0;

        protected MatchHandler handler;

        public event Action<ICard> OnCardTaken;
        public event Action<int> OnSplitHand;
        public event Action OnCardUpdated;
        public event Action OnDoubleBet;
        public event Action OnStart;

        protected abstract void OnMatchStart();
        protected abstract void OnMatchComplete();

        public PlayerHand ActiveHand
            => hands.Count > activeHandIndex && activeHandIndex >= 0 ? hands[activeHandIndex] : null;

        public void CallOnMatchComplete() =>
            OnMatchComplete();
        public int GetScore()
            => ActiveHand != null ? ActiveHand.GetScore() : 0;
        public bool HasStand()
            => hands.All(hand => hand.Stand);
        public int TotalHands()
            => hands.Count; 
        public int ActiveHandIndex()
            => activeHandIndex;
        public PlayerHand GetHand(int index)
            => hands[index];

        public void CallOnMatchStart(MatchHandler handler) {

            this.handler = handler;

            var hand = new PlayerHand();

            if (BetHandler.TryPlaceBet(hand)) {

                hands.Clear();
                activeHandIndex = 0;

                hands.Add(hand);

                OnMatchStart();
                OnStart?.Invoke();
            }
            else {

                Debug.Log("???");
            }

        }

        protected bool TryTakeCard(bool faceDown) {

            if (!handler.CheckCurrentTurn(this) || ActiveHand == null|| ActiveHand.Stand)
                return false;

            var card = handler.TakeCardFromDeck();
            card.IsShowing = !faceDown;

            ActiveHand.Cards.Add(card);

            if (ActiveHand.GetScore() > 21)
                ActiveHand.Stand = true;

            if (!handler.CheckTurn(this))
                ActiveHand.Stand = true;

            ActiveHand.CanDouble = false;

            CardTaken(card);

            return true;
        }

        protected bool TryDoubleDown() {

            if (!handler.CheckCurrentTurn(this) || ActiveHand.Stand || !ActiveHand.CanDouble) {
                return false;
            }

            TryTakeCard(false);

            ActiveHand.BetAmount = ActiveHand.BetAmount * 2;
            OnDoubleBet?.Invoke();

            ActiveHand.CanDouble = false;

            handler.EndTurn(this);

            return true;
        }

        protected bool TryStandHand() {

            var hand = ActiveHand;
            hand.Stand = true;

            activeHandIndex++;
            activeHandIndex = Mathf.Clamp(activeHandIndex, 0, hands.Count - 1);
            
            if (HasStand())
                handler.EndTurn(this);

            return true;
        }

        protected bool TryToSplit() {

            var hand = ActiveHand;

            if (!handler.CheckCurrentTurn(this))
                return false;

            if (!hand.IsPair())
                return false;

            PlayerHand newHand = new PlayerHand();

            if (BetHandler.TryPlaceBet(newHand)) {

                newHand.Cards.Add(hand.Cards[1]);
                hand.Cards.RemoveAt(1);

                hand.CanDouble = true;
                newHand.CanDouble = true;

                hands.Add(newHand);
                CardUpdate();

                OnSplitHand?.Invoke(activeHandIndex);
            }

            return true;
        }

        protected void CardUpdate() {

            OnCardUpdated?.Invoke();
        }

        protected void CardTaken(ICard card) {

            OnCardTaken?.Invoke(card);
        }
    }
}
