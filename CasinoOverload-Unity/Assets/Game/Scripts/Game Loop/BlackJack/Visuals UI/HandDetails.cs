using UnityEngine;
using TMPro;
using System.Collections.Generic;


namespace BlackJack
{
    public class HandDetails : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI score;
        [SerializeField] Transform chipsPoint;

        List<PokerChip> chips = new List<PokerChip>();


        public void OnSpawn(int totalValue) {
            ChipsSpawner.SpawnChip(totalValue, out PokerChip chip);

            chips.Add(chip);
        }
        public void UpdateDisplay(PlayerHand hand) {
            score.text = $"{hand.GetScore()}/21";

            foreach (var item in chips) {
                item.UpdateDisplay(chipsPoint.position);
            }
        }
    }
}