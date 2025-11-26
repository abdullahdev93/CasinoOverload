using UnityEngine.UI;

namespace BlackJack
{
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerUIController : MonoBehaviour
    {
        [Header("References")]
        public Player player;

        [Header("Buttons")]
        public Button hitButton;
        public Button standButton;
        public Button doubleButton;
        public Button splitButton;

        private CommandInvoker invoker;

        void Start() {
            invoker = new CommandInvoker();

            SetupButton(hitButton, new HitCommand(player), "Take another card.");
            SetupButton(standButton, new StandCommand(player), "End your turn and keep your current total.");
            SetupButton(doubleButton, new DoubleCommand(player), "Double your bet; receive exactly one more card.");
            SetupButton(splitButton, new SplitCommand(player), "Split your pair into two separate hands.");
        }

        void Update() {
            UpdateButtonState();
        }

        void SetupButton(Button btn, ICommand command, string tooltip) {
            var cmdBtn = btn.gameObject.AddComponent<UICommandButton>();
            cmdBtn.Init(command, invoker);

            var tip = btn.gameObject.AddComponent<UIButtonTooltip>();
            tip.tooltipText = tooltip;

            btn.onClick.AddListener(cmdBtn.OnClick);
        }

        void UpdateButtonState() {

            bool inGame = player.IsInGame;

            if (!inGame) {

                hitButton.interactable = inGame;
                standButton.interactable = inGame;
                doubleButton.interactable = inGame;
                splitButton.interactable = inGame;

                return;
            }

            bool myTurn = player.Handler.CheckCurrentTurn(player);

            var hand = player.ActiveHand;

            bool baseEnabled = inGame && myTurn && hand != null && hand.Cards.Count >= 2;

            hitButton.interactable = baseEnabled && !hand.Stand;
            standButton.interactable = baseEnabled;
            doubleButton.interactable = baseEnabled && hand.CanDouble;
            splitButton.interactable = baseEnabled && player.CanSplitCurrentHand();
        }
    }
}