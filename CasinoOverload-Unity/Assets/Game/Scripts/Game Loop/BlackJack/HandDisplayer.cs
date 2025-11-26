using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.XR;

namespace BlackJack
{
    public class HandDisplayer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MatchVisualsHandler handler;
        [SerializeField] private MatchPlayer matchPlayer;
        [SerializeField] private TextMeshProUGUI score;

        [Header("Layout")]
        [SerializeField] private float rowSpacing = 1.75f;
        [SerializeField] private float cardSpacing = 1.75f;
        [SerializeField] private float verticalSpacing = 1f;

        [Header("UI")]
        [SerializeField] private HandDetails detailsPrefab;
        [SerializeField] private Transform detailsContainer;

        private List<Hand> handCardRows = new();

        [System.Serializable]
        private class Hand
        {
            public List<ICard> Cards = new();
            public HandDetails handDetails;
            public PlayerHand playerHand;
        }

        #region Unity

        private void Awake() {
            if (score) score.text = "0/21";

            matchPlayer.OnStart += OnStart;
            matchPlayer.OnSplitHand += OnSplitHand;
            matchPlayer.OnCardTaken += OnCardTaken;
            matchPlayer.OnDoubleBet += OnDoubleBet;
            matchPlayer.OnCardUpdated += OnCardUpdated;
        }

        private void LateUpdate() {
            FormatCards();
            UpdateScore();
        }

        #endregion

        #region Event Handlers

        private void OnStart() {
            handCardRows.Clear();

            var newUIHand = new Hand {
                handDetails = detailsPrefab ? Instantiate(detailsPrefab, detailsContainer) : null,
                playerHand = matchPlayer.ActiveHand
            };


            newUIHand.handDetails?.OnSpawn(newUIHand.playerHand.BetAmount);

            handCardRows.Add(newUIHand);

            OnDoubleBet();
            UpdateScore();
        }

        private void OnSplitHand(int handIndex) {

            var originalRow = handCardRows[handIndex];

            Hand newUIHand = new Hand {
                handDetails = Instantiate(detailsPrefab, detailsContainer),
                playerHand = matchPlayer.GetHand(handIndex + 1) // correct new hand
            };

            newUIHand.handDetails?.OnSpawn(newUIHand.playerHand.BetAmount);

            // Move card visually
            newUIHand.Cards.Add(originalRow.Cards[1]);
            originalRow.Cards.RemoveAt(1);

            handCardRows.Add(newUIHand);

            RefreshAfterSplit();
        }



        private void OnCardUpdated() {
            var hand = matchPlayer.ActiveHand;
            var row = handCardRows[matchPlayer.ActiveHandIndex()];

            for (int i = 0; i < hand.Cards.Count; i++) {
                if (i >= row.Cards.Count) continue;

                if (hand.Cards[i].IsShowing)
                    row.Cards[i].Show();
                else
                    row.Cards[i].Hide();
            }

            UpdateScore();
        }

        private void OnCardTaken(ICard card) {
            int handIndex = matchPlayer.ActiveHandIndex();
            var row = handCardRows[handIndex];

            var newCard = handler.SpawnNTakeCard(
                card,
                GetNextCardPosition(row.Cards.Count, handIndex),
                spawned => {
                    row.Cards.Add(spawned);
                    if (card.IsShowing) spawned.Show();
                    FormatCards();
                });

            newCard.transform.parent = transform;

            UpdateScore();
        }

        private void OnDoubleBet() {
            // Implement logic or remove when not needed.
        }

        #endregion

        #region UI & Layout

        private void UpdateScore() {
            if (score)
                score.text = $"{matchPlayer.GetScore()}/21";

            foreach (var entry in handCardRows) {
                if (entry.handDetails)
                    entry.handDetails.UpdateDisplay(entry.playerHand);
            }
        }

        private void FormatCards() {
            int setCount = handCardRows.Count;
            if (setCount == 0) return;

            float totalSetWidth = (setCount - 1) * rowSpacing;
            float leftMostSet = -totalSetWidth * 0.5f;

            for (int setIndex = 0; setIndex < setCount; setIndex++) {

                var row = handCardRows[setIndex];
                int cardCount = row.Cards.Count;

                float setCenterX = leftMostSet + setIndex * rowSpacing;
                float rowY = verticalSpacing * ((setIndex % 2 == 0) ? 1 : -1);

                // Move the hand details label
                if (row.handDetails) {
                    Vector3 target = new Vector3(setCenterX, rowY, 0);
                    row.handDetails.transform.DOMove(transform.position + target, 0.35f);
                }

                if (cardCount == 0) continue;

                float totalCardWidth = (cardCount - 1) * cardSpacing;
                float leftMostCard = setCenterX - totalCardWidth * 0.5f;

                for (int i = 0; i < cardCount; i++) {
                    float x = leftMostCard + i * cardSpacing;
                    Vector3 target = new Vector3(x, rowY, 0);

                    row.Cards[i].transform.DOLocalMove(target, 0.35f);
                }
            }
        }

        private Vector3 GetNextCardPosition(int cardIndex, int handIndex) {
            float startX = -(cardSpacing / 2f) * cardIndex;
            float yOffset = verticalSpacing * ((handIndex % 2 == 0) ? 1 : -1);
            return transform.position + new Vector3(startX + cardSpacing * cardIndex, yOffset, 0);
        }

        public void RefreshAfterSplit() {
            while (handCardRows.Count < matchPlayer.TotalHands())
                handCardRows.Add(new Hand());

            FormatCards();
            UpdateScore();
        }

        #endregion
    }
}
