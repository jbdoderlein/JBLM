using LethalCompanyTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrolardItem : GrabbableObject
{
    private static ILogger logger = Debug.unityLogger;
    
    private RoundManager roundManager;

    public override void Start()
    {
        this.grabbable = true;
        this.isInFactory = true;
        base.Start();
        
        roundManager = FindObjectOfType<RoundManager>();
        
    }

    public override void Update()
    {
        base.Update();
    }
}