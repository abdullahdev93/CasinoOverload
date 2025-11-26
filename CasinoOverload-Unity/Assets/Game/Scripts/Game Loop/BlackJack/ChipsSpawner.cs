using UnityEngine;

namespace BlackJack
{
    public class ChipsSpawner : MonoBehaviour
    {
        [SerializeField] Transform spawnPoint;
        [SerializeField] PokerChip pokerChip;

        private static ChipsSpawner Instance;

        private void Awake() {
            Instance = this;
        }

        public static void SpawnChip(int score, out PokerChip pokerChip) {

            if (!Instance) {
                pokerChip = null;
                return;
            }

            pokerChip = Instantiate(Instance.pokerChip);
            pokerChip.transform.position = Instance.spawnPoint.position;

            pokerChip.UpdateDisplay(score, Instance.spawnPoint.position);
        }
    }
}