using DG.Tweening;
using System;
using TMPro;
using UnityEngine;


namespace BlackJack
{
    public class MatchVisualsHandler : MonoBehaviour
    {
        [SerializeField] MatchHandler matchHandler;

        [SerializeField] Transform deck;

        [SerializeField] GameObject mainPanel;
        [SerializeField] GameObject winPanel;
        [SerializeField] TextMeshProUGUI title;


        void Awake() {
            mainPanel.SetActive(false);
            winPanel.SetActive(false);

            matchHandler.OnMatchStarted += OnMatchStarted;
            matchHandler.OnWinPlayer += OnWinPlayer;
        }

        public void Exit() {
            mainPanel.SetActive(false);
        }

        private void OnWinPlayer(MatchPlayer obj) {

            winPanel.SetActive(true);

            title.text = $"{obj.gameObject.name} won";
        }
        private void OnMatchStarted() {

            mainPanel.SetActive(true);
        }

        public GameObject SpawnNTakeCard(ICard card, Vector3 targetPoint, Action<ICard> OnComplete) {

            var newCard = Instantiate(card.gameObject, deck.position, Quaternion.identity);

            newCard.transform.DOMove(targetPoint, 2/3f).OnComplete(() => OnComplete?.Invoke(newCard.GetComponent<ICard>()));

            return newCard;
        }
    }
}
