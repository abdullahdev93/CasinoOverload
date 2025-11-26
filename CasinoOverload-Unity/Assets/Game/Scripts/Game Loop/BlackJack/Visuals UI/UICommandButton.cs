using UnityEngine;

namespace BlackJack
{
    public class UICommandButton : MonoBehaviour
    {
        private ICommand command;
        private CommandInvoker invoker;

        public void Init(ICommand cmd, CommandInvoker inv) {
            command = cmd;
            invoker = inv;
        }

        public void OnClick() {
            invoker.SetCommand(command);
            invoker.ExecuteCommand();
        }
    }

}