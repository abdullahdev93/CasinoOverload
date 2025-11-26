using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackJack
{
    public class MatchHandler : MonoBehaviour
    {
        [SerializeField] List<MatchPlayer> players;
        [SerializeField] GameObject[] set;

        private Queue<MatchPlayer> turns = new Queue<MatchPlayer>();
        private List<ICard> deck = new List<ICard>();

        private MatchPlayer current;
        private int currentRound;


        public event System.Action OnMatchStarted;
        public event System.Action<MatchPlayer> OnWinPlayer;


        private void Start() {
            //StartMatch();
        }

        public void StartMatch() {
            foreach (var item in set) {
                deck.Add(item.GetComponent<ICard>());
                item.GetComponent<ICard>().Hide();
            }

            currentRound = 0;

            foreach (var item in players) {
                item.CallOnMatchStart(this);
                turns.Enqueue(item);
            }

            current = turns.Peek();

            OnMatchStarted?.Invoke();
        }

        private void OnValidate() {
            for (int i = 0; i < set.Length; i++) {
                if (set[i] == null) {
                    continue;
                }

                if (set[i].GetComponent<ICard>() == null) {
                    Debug.LogError($"item at index == {i} is not a Icard");

                    set[i] = null;
                }
            }
        }
        public ICard TakeCardFromDeck() {
            var card = deck[Random.Range(0, deck.Count)];
            deck.Remove(card);

            return card;
        }
        public ICard TakeCardFromDeck(int value) {
            var card = deck.Find(c => c.AddScore(0) == value);
            deck.Remove(card);

            return card;
        }

        public bool CheckCurrentTurn(MatchPlayer player) {
            return turns.Count > 0 && turns.Peek() == player;
        }
        public void EndTurn(MatchPlayer player) {
            if (!CheckTurn(player))
                return;

            RotateTurn(); // next
        }

        public bool CheckTurn(MatchPlayer player) {
            if (!CheckCurrentTurn(player))
                return false;

            int score = player.GetScore();

            if (score == 21 || OthersDisqualified(player)) {
                OnWinPlayer?.Invoke(player);
                FinishMatch();
                return false;
            }

            if (OthersCommited(player) && IsScoreGreater(player)) {
                OnWinPlayer?.Invoke(player);
                FinishMatch();
                return false;
            }

            if (players.All(p => p.HasStand())) {
                MatchPlayer winner = players
                    .Where(p => p.GetScore() <= 21)
                    .OrderByDescending(p => p.GetScore())
                    .FirstOrDefault();

                if (winner != null)
                    OnWinPlayer?.Invoke(winner);

                FinishMatch();
                return false;
            }

            return true;
        }

        void FinishMatch() {
            turns.Clear();
            current = null;

            foreach (var item in players) {
                item.CallOnMatchComplete();
            }
        }
        void RotateTurn() {
            turns.Dequeue();

            if (turns.Count == 0) {
                foreach (var p in players)
                    turns.Enqueue(p);
                currentRound++;
            }

            current = turns.Peek();

            if (current.HasStand())
                EndTurn(current);
        }

        bool IsScoreGreater(MatchPlayer player) {
            int score = player.GetScore();
            if (score > 21)
                return false; // get gud

            foreach (var p in players) {
                if (p == player) continue;

                int s = p.GetScore();
                if (s <= 21 && s >= score)
                    return false;
            }

            return true;
        }
        bool OthersCommited(MatchPlayer player) {
            foreach (var p in players) {
                if (p == player) continue;
                if (!p.HasStand()) return false;
            }
            return true;
        }
        bool OthersDisqualified(MatchPlayer player) {
            foreach (var p in players) {
                if (p == player) continue;
                if (!(p.GetScore() > 21)) return false;
            }
            return true;
        }
    }
}
