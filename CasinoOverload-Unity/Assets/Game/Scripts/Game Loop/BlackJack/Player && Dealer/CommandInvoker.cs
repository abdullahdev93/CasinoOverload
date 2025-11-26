namespace BlackJack
{
    public class CommandInvoker
    {
        private ICommand currentCommand;

        public void SetCommand(ICommand command) {
            currentCommand = command;
        }

        public void ExecuteCommand() {
            if (currentCommand != null) {
                currentCommand.Execute();
                currentCommand = null;
            }
        }
    }
    public interface ICommand
    {
        void Execute();
    }

    public class HitCommand : ICommand
    {
        Player player;
        public HitCommand(Player p) { player = p; }

        public void Execute() {
            player.CallHit();
        }
    }
    public class StandCommand : ICommand
    {
        Player player;
        public StandCommand(Player p) { player = p; }

        public void Execute() {
            player.CallStand();
        }
    }
    public class DoubleCommand : ICommand
    {
        Player player;
        public DoubleCommand(Player p) { player = p; }

        public void Execute() {
            player.CallDoubleBet();
        }
    }
    public class SplitCommand : ICommand
    {
        Player player;
        public SplitCommand(Player p) { player = p; }

        public void Execute() {
            player.CallSplitHand();
        }
    }
}