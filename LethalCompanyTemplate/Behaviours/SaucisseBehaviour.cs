using UnityEngine;

namespace LethalCompanyTemplate.Behaviours;

public class SaucisseBehaviour : PhysicsProp
{
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (buttonDown)
        {
            if(playerHeldBy!=null)
                playerHeldBy.DamagePlayer(20);
            else
                Debug.Log("PlayerHeldBy is null");
        }
    }
}