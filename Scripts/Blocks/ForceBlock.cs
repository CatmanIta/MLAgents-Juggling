using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Will push anything in the trigger area with a force in the direction the block is facing
/// </summary>
public class ForceBlock : Block
{
    public int forceStrength = 5;
    public ForceMode forceMode = ForceMode.Impulse;

    public MeshRenderer forceAreaMR;

    protected override void HandleSetup()
    {
        overlappingObjects.Clear();
    }

    protected override void HandleReset()
    {
        overlappingObjects.Clear();
    }

    protected override void HandleStartTrigger()
    {
        //Debug.Log("Applying force to: " + overlappingObjects.Count);
        foreach (var obj in overlappingObjects)
        {
            var marble = obj.GetComponent<Marble>();
            marble.AddForce(transform.up * forceStrength, forceMode);
          
            //Debug.Log("FORCE!");
        }

        forceAreaMR.material.color = Color.yellow * 0.5f;
    }

    protected override void HandleStepTrigger()
    {
    }

    protected override void HandleEndTrigger()
    {
        forceAreaMR.material.color = Color.cyan*0.5f;
    }

    #region Overlap

    private List<Collider> overlappingObjects = new List<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<Marble>()) return;  // TODO: optimize
        overlappingObjects.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<Marble>()) return;
        overlappingObjects.Remove(other);
    }

    #endregion
}
