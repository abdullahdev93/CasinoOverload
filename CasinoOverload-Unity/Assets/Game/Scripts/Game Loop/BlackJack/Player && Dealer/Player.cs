using System.Collections.Generic;
using UnityEngine;

namespace BlackJack
{
    public class Player : MatchPlayer
    {
        [SerializeField] int testNum1 = 10;
        [SerializeField] int testNum2= 10;
        [SerializeField] bool testok;

        bool inGame = false;

        // Commands
        ICommand hitCommand;
        ICommand standCommand;
        ICommand doubleCommand;
        ICommand splitCommand;

        CommandInvoker invoker;

        public bool IsInGame => inGame;
        public MatchHandler Handler => handler;

        public bool CanSplitCurrentHand() {
            var hand = ActiveHand;
            if (hand == null || hand.Cards.Count != 2)
                return false;

            return hand.Cards[0].AddScore(0) == hand.Cards[1].AddScore(0); // or use your own split logic
        }

        protected override void OnMatchStart() {
            inGame = true;

            // Create commands
            hitCommand = new HitCommand(this);
            standCommand = new StandCommand(this);
            doubleCommand = new DoubleCommand(this);
            splitCommand = new SplitCommand(this);

            invoker = new CommandInvoker();
        }

        protected override void OnMatchComplete() {
            inGame = false;
        }

        private void Update() {
            if (!inGame || ActiveHand == null || !handler.CheckCurrentTurn(this))
                return;


            // ----- First card -----
            if (ActiveHand.Cards.Count == 0) {
                if (testok) {
                    Request(false, testNum1);
                }
                else {
                    TryTakeCard(false);
                }
                handler.EndTurn(this);
                return;
            }

            // ----- Second card -----
            if (ActiveHand.Cards.Count == 1) {
                if (testok) {
                    Request(false, testNum1);
                }
                else {
                    TryTakeCard(false);
                }

                return;
            }


            if (Input.GetKeyDown(KeyCode.W))
                invoker.SetCommand(hitCommand);

            if (Input.GetKeyDown(KeyCode.E))
                invoker.SetCommand(standCommand);

            if (Input.GetKeyDown(KeyCode.Q))
                invoker.SetCommand(doubleCommand);

            if (Input.GetKeyDown(KeyCode.S))
                invoker.SetCommand(splitCommand);

            invoker.ExecuteCommand();
        }

        public void CallHit() {
            if (!inGame || !handler.CheckCurrentTurn(this))
                return;

            if (TryTakeCard(false))
                Debug.Log("UI Hit");
        }

        public void CallStand() {
            if (!inGame || !handler.CheckCurrentTurn(this))
                return;

            if (TryStandHand())
                Debug.Log("UI Stand");
        }

        public void CallDoubleBet() {
            if (!inGame || !handler.CheckCurrentTurn(this))
                return;

            if (ActiveHand.CanDouble && TryDoubleDown())
                Debug.Log("UI Double");
        }

        public void CallSplitHand() {
            if (!inGame || !handler.CheckCurrentTurn(this))
                return;

            if (TryToSplit()) {
                Debug.Log("UI Split");

                // After split, both hands need a new card
                TryTakeCard(false);

                int index = activeHandIndex;

                activeHandIndex = hands.Count - 1;
                TryTakeCard(false);

                activeHandIndex = index;
            }
        }

        protected bool Request(bool faceDown , int value) {

            if (!handler.CheckCurrentTurn(this) || ActiveHand == null || ActiveHand.Stand)
                return false;

            var card = handler.TakeCardFromDeck(value);
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
    }
}
