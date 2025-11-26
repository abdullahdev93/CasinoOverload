using System.Collections;
using System.Linq;
using UnityEngine;


namespace BlackJack
{
    public class Dealer : MatchPlayer
    {
        bool inGame = false;
        DealerState state = DealerState.FirstTurn;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        private void Start() {
            StartCoroutine(DealerStateMachine());
        }

        protected override void OnMatchStart() {
            state = DealerState.FirstTurn;

            inGame = true;
        }
        protected override void OnMatchComplete() {

            inGame = false;
        }

        private IEnumerator DealerStateMachine() {
            while (true) {
                if (!inGame) {
                    yield return wait;
                    continue;
                }
                if (handler.CheckCurrentTurn(this)) {
                    switch (state) {
                        case DealerState.FirstTurn:

                            yield return waitForSeconds;

                            if (handler.CheckCurrentTurn(this)) {
                                TryTakeCard(true);
                                handler.EndTurn(this);
                                state = DealerState.SecondTurn;
                            }

                            break;
                        case DealerState.SecondTurn:

                            yield return waitForSeconds;

                            if (handler.CheckCurrentTurn(this)) {
                                TryTakeCard(false);
                                handler.EndTurn(this);
                                state = DealerState.LastTurn;
                            }

                            break;

                        case DealerState.LastTurn:

                            yield return waitForSeconds;

                            if (handler.CheckCurrentTurn(this)) {
                                if (hands[0].Cards.Any(h => !h.IsShowing)) {

                                    foreach (var card in hands[activeHandIndex].Cards)
                                        card.IsShowing = true;

                                    CardUpdate();
                                }
                            }

                            handler.EndTurn(this);
                            state = DealerState.End;

                            break;

                        case DealerState.End:

                            yield return waitForSeconds;

                            if (handler.CheckCurrentTurn(this)) {
                                if (handler.CheckTurn(this)) {
                                    TryTakeCard(false);
                                    handler.EndTurn(this);
                                }
                            }

                            break;
                    }
                }

                yield return wait;
            }
        }

        enum DealerState
        {
            FirstTurn,
            SecondTurn,
            LastTurn,
            End
        }
    }
}
