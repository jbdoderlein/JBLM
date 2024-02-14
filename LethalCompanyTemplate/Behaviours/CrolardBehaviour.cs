
using GameNetcodeStuff;
using UnityEngine;
namespace LethalCompanyTemplate.Behaviours;

public class CrolardBehaviour : PhysicsProp
{
    
    private InteractTrigger[] triggers;
    private BoxCollider container;

    public override void Start()
    {
        base.Start();
        triggers = GetComponentsInChildren<InteractTrigger>();
        foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
        {
            Debug.LogError($"Cox collider found : {collider.name} !");
            if (collider.name != "PlaceableBounds") continue;

            container = collider;
            break;
        }
        if (container == null) Debug.LogError($"Couldn't find {nameof(BoxCollider)} component in the prefab...");
        if (triggers == null) Debug.LogError($"Couldn't find {nameof(InteractTrigger)} components in the prefab...");
        foreach (InteractTrigger trigger in triggers)
        {
            trigger.onInteract.AddListener(InteractCrolard);
            trigger.tag = nameof(InteractTrigger); // Necessary for the interact UI to appear
            trigger.interactCooldown = false;
            trigger.cooldownTime = 0;
        }
    }

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

    public override void Update()
    {
        base.Update();
        UpdateInteractTriggers();
    }

    private void UpdateInteractTriggers()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
        {
            SetInteractTriggers(false, "No item");
            return;
        }

        if (!player.isHoldingObject)
        {
            SetInteractTriggers(false, "No item");
            return;
        }

        GrabbableObject holdingItem = player.currentlyHeldObjectServer;
        if (holdingItem.GetComponent<CrolardBehaviour>() != null)
        {
            SetInteractTriggers(false, "Crolard is not recursive");
            return;
        }
        SetInteractTriggers(true, "Feed Crolard [LMB]");
    }
    
    private void SetInteractTriggers(bool interactable = false, string hoverTip = "Default text")
    {
        foreach (InteractTrigger trigger in triggers)
        {
            trigger.interactable = interactable;
            if (interactable) trigger.hoverTip = hoverTip;
            else trigger.disabledHoverTip = hoverTip;
        }
    }
    
    private void InteractCrolard(PlayerControllerB playerInteractor)
    {
        if (playerInteractor.isHoldingObject)
        {
            Debug.LogError("Item was fed in crolard !");
            return;
        }
    }
}