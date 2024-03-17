

using GameNetcodeStuff;
using LethalCompanyTemplate.Behaviours;
using Unity.Netcode;
using UnityEngine;

public class CrolardBehaviour : PhysicsProp
{
    
    private InteractTrigger[] triggers;
    private BoxCollider container;
    private AudioSource ganjaSource;
    public AudioClip ganjaCrolard;
    public AudioClip ganjaNoiseLow;
    public AudioClip ganjaNoiseMedium;
    public AudioClip ganjaNoiseHigh;
    public AudioClip ganjaExplosion;
    
    private float w1 = 1.0f;
    private float w2 = 3.0f;
    private float chanceMin = 0.005f;
    private float chanceMax = 0.2f;
    private float addedWeight = 0f;
    
    
    public override void Start()
    {
        base.Start();
        
        triggers = GetComponentsInChildren<InteractTrigger>();
        ganjaSource = GetComponentInChildren<AudioSource>();
        foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
        {
            Debug.LogError($"Box collider found : {collider.name} !");
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
            Debug.Log($"The tag is {nameof(InteractTrigger)}");
            trigger.interactCooldown = false;
            trigger.cooldownTime = 0;
            trigger.timeToHold = (float) 1.2;
            trigger.oneHandedItemAllowed = true;
            trigger.twoHandedItemAllowed = true;
            trigger.disableTriggerMesh = true;
            trigger.holdInteraction = true;
        }
    }
    
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (buttonDown)
        {
            if (playerHeldBy != null)
            {
                if (w1 < addedWeight && addedWeight <= w2)
                {
                    ganjaSource.PlayOneShot(ganjaNoiseMedium, 0.8f);
                } else if (addedWeight <= w1)
                {
                    ganjaSource.PlayOneShot(ganjaNoiseLow, 0.6f);
                }
                else
                {
                    ganjaSource.PlayOneShot(ganjaNoiseHigh, 1.0f);
                }
                
                RoundManager.Instance.PlayAudibleNoise(transform.position, 60, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            }
                
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
            //SetInteractTriggers(false, "No item to feed");
            return;
        }

        if (!player.isHoldingObject)
        {
            SetInteractTriggers(false, "No item to feed");
            return;
        }

        GrabbableObject holdingItem = player.currentlyHeldObjectServer;
        if (holdingItem.GetComponent<CrolardBehaviour>() != null)
        {
            //SetInteractTriggers(false, "Crolard is not recursive");
            return;
        }
        if (!holdingItem.itemProperties.isScrap)
        {
            SetInteractTriggers(false, "You can only feed scrap to Crolard");
            return;
        }
        
        if (RoundManager.Instance == null || RoundManager.Instance.dungeonGenerator == null || !RoundManager.Instance.dungeonCompletedGenerating)
        {
            SetInteractTriggers(false, "You're not on a planet");
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
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateInternalWeightServerRpc(float newWeight)
    {
        UpdateInternalWeightClientRpc(newWeight);
    }

    [ClientRpc]
    public void UpdateInternalWeightClientRpc(float newWeight)
    {
        
        addedWeight = newWeight;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateValueServerRpc(int newValue)
    {
        UpdateValueClientRpc(newValue);
    }
    
    [ClientRpc]
    public void UpdateValueClientRpc(int newValue)
    {
        scrapValue = newValue;
        SetScrapValue(scrapValue);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ExplodeServerRpc(Vector3 location)
    {
        ExplodeClientRpc(location);
    }
    
    [ClientRpc]
    public void ExplodeClientRpc(Vector3 location)
    {
        ganjaSource.pitch = Random.Range(0.93f, 1.07f);
        ganjaSource.PlayOneShot(this.ganjaExplosion, 1.2f);
        SpawnExplosion(location, killRange: 5.7f, damageRange: 6.4f);
    }
    
    private void InteractCrolard(PlayerControllerB playerInteractor)
    {
        
        if (!playerInteractor.isHoldingObject) return;
        
        if (playerInteractor.currentlyHeldObjectServer == null)
        {
            Debug.LogError("Item is null");
            return;
        }
        
        if (!playerInteractor.currentlyHeldObjectServer.itemProperties.isScrap)
        {
            Debug.LogError("This is not scrap");
            return;
        }
        
        int addedValue = (int) (playerInteractor.currentlyHeldObjectServer.scrapValue * 1.30);
        float objectWeight = (playerInteractor.currentlyHeldObjectServer.itemProperties.weight-1)*1.25f;
        

        if (playerInteractor.currentlyHeldObjectServer.GetComponent<SaucisseBehaviour>() != null)
        {
            ganjaSource.PlayOneShot(ganjaCrolard, 0.8f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, 800, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            UpdateValueServerRpc(scrapValue + 70);
            playerInteractor.DespawnHeldObject();
        }
        else
        {
            playerInteractor.DespawnHeldObject();
            UpdateInternalWeightServerRpc(addedWeight + objectWeight);
            UpdateValueServerRpc(scrapValue + addedValue);
                
            float explosionChance;
            if (w1 < addedWeight && addedWeight <= w2)
            {
                explosionChance = chanceMin + (addedWeight - w1) * (chanceMax - chanceMin) / (w2 - w1);
            }
            else if (addedWeight <= w1)
            {
                explosionChance = chanceMin;
            }
            else
            {
                explosionChance = chanceMax;
            }

            if (!(Random.Range(0f, 1f) < explosionChance)) return;
            // Explosion
            ExplodeServerRpc(this.transform.position + Vector3.up);
            UpdateInternalWeightServerRpc(0);
            UpdateValueServerRpc((scrapValue-addedValue)/2);
            
        }
    }
    
    public static void SpawnExplosion(
        Vector3 explosionPosition,
        bool spawnExplosionEffect = false,
        float killRange = 1f,
        float damageRange = 1f)
      {
        Debug.Log((object) "Spawning explosion at pos: {explosionPosition}");
        if (spawnExplosionEffect)
          Object.Instantiate<GameObject>(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0.0f, 0.0f), RoundManager.Instance.mapPropsContainer.transform).SetActive(true);
        float num1 = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
        if ((double) num1 < 14.0)
          HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        else if ((double) num1 < 25.0)
          HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
        Collider[] colliderArray = Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);
        for (int index = 0; index < colliderArray.Length; ++index)
        {
          float num2 = Vector3.Distance(explosionPosition, colliderArray[index].transform.position);
          if ((double) num2 <= 4.0 || !Physics.Linecast(explosionPosition, colliderArray[index].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
          {
            if (colliderArray[index].gameObject.layer == 3)
            {
              PlayerControllerB component = colliderArray[index].gameObject.GetComponent<PlayerControllerB>();
              if ((Object) component != (Object) null && component.IsOwner)
              {
                if ((double) num2 < (double) killRange)
                {
                  Vector3 bodyVelocity = (component.gameplayCamera.transform.position - explosionPosition) * 80f / Vector3.Distance(component.gameplayCamera.transform.position, explosionPosition);
                  component.KillPlayer(bodyVelocity, causeOfDeath: CauseOfDeath.Blast);
                }
                else if ((double) num2 < (double) damageRange)
                  component.DamagePlayer(50);
              }
            }
            else if (colliderArray[index].gameObject.layer == 19)
            {
              EnemyAICollisionDetect componentInChildren = colliderArray[index].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
              if ((Object) componentInChildren != (Object) null && componentInChildren.mainScript.IsOwner && (double) num2 < 4.5)
                componentInChildren.mainScript.HitEnemyOnLocalClient(6);
            }
          }
        }
        int num3 = ~LayerMask.GetMask("Room");
        int layerMask = ~LayerMask.GetMask("Colliders");
        foreach (Component component1 in Physics.OverlapSphere(explosionPosition, 10f, layerMask))
        {
          Rigidbody component2 = component1.GetComponent<Rigidbody>();
          if ((Object) component2 != (Object) null)
            component2.AddExplosionForce(70f, explosionPosition, 10f);
        }
      }

    public override void GrabItem()
    {
        base.GrabItem();
        if (playerHeldBy && GameNetworkManager.Instance.localPlayerController == playerHeldBy)
        {
            playerHeldBy.carryWeight += Mathf.Clamp(addedWeight, 0, 10f);
        }
    }

    public override void DiscardItem()
    {
        if (playerHeldBy && GameNetworkManager.Instance.localPlayerController == playerHeldBy)
        {
            playerHeldBy.carryWeight -= Mathf.Clamp(addedWeight, 0, 10f);
        }
        base.DiscardItem();
    }

    
    public override void OnBroughtToShip()
    {
        base.OnBroughtToShip();
        ganjaSource.PlayOneShot(ganjaCrolard, 1.0f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 300, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
    }
}