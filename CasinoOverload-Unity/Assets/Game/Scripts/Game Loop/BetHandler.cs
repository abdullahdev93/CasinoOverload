using UnityEngine;
using BlackJack;
using System.Collections.Generic;
using System;

public class BetHandler : MonoBehaviour
{
    [SerializeField] MatchHandler matchHandler;

    private static BetHandler Instance;

    private List<IBetable> bets = new List<IBetable>();
    private int selectedAmount = 10;


    private void Awake() {
        Instance = this;
    }

    public static void StartMatch(int betAmmount) {

        Instance.selectedAmount = betAmmount;

        Instance.matchHandler.StartMatch();
    }

    public static bool TryPlaceBet(IBetable bet) {

        if (Instance == null || CurrencyHandler.Cash < Instance.selectedAmount) {

            return false;
        }

        Debug.Log("Placed");

        bet.OnBetEnded += Instance.OnBetEnded;
        bet.BetAmount = Instance.selectedAmount;

        Instance.bets.Add(bet);

        return true;
    }

    public static bool CanPlaceBet(IBetable bet , int ammont) {

        if (CurrencyHandler.Cash < ammont) {
            return false;
        }

        return true;
    }

    private void OnBetEnded(IBetable bet , bool won) {

        if (won) {
            // 
        }
        else {
            //
        }
    }
}


public interface IBetable
{
    public int BetAmount {
        get; 
        set;
    }

    public event Action<IBetable , bool> OnBetEnded;
}
