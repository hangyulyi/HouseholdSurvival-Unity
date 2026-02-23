using UnityEngine;

public class BudgetSystem
{
    PlayerState player;

    public BudgetSystem(PlayerState player )
    {
        this.player = player;
    }

    public void ApplyBudget()
    {
        player.health += player.healthcareBudget / 10;

        if(player.foodBudget < 50)
        {
            player.stress += 10;
        }

        player.money -= TotalBudget();
    }

    int TotalBudget()
    {
        return player.healthcareBudget +
            player.foodBudget +
            player.housingBudget +
            player.educationBudget +
            player.savingsBudget;
    }
}
