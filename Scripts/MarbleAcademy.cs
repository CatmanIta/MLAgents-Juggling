using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleAcademy : Academy
{
    [Header("Parameters")]
    public Brain discreteBrain;
    public Brain continuousBrain;

    public MarbleAgent marbleAgent;

    public StateType actionSpaceType = StateType.continuous;
    public BrainType defaultEditorBrainType = BrainType.Internal;

    private Brain brainToUse;

    protected override void GetBrains(GameObject gameObject, List<Brain> brains)
    {
        brainToUse = actionSpaceType == StateType.continuous ? continuousBrain : discreteBrain;
        brains.Add(brainToUse);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        discreteBrain.gameObject.SetActive(false);
        continuousBrain.gameObject.SetActive(false);
        brainToUse = actionSpaceType == StateType.continuous ? continuousBrain : discreteBrain;
        brainToUse.gameObject.SetActive(true);
        marbleAgent.brain = brainToUse;
    }
#endif

    public override void InitializeAcademy()
    {

#if UNITY_EDITOR
        discreteBrain.brainType = defaultEditorBrainType;
        continuousBrain.brainType = defaultEditorBrainType;
#endif
        brainToUse = actionSpaceType == StateType.continuous ? continuousBrain : discreteBrain;
        marbleAgent.brain = brainToUse;

        brainToUse.brainParameters.stateSize = marbleAgent.GetStateSize ();
        brainToUse.brainParameters.actionSize = marbleAgent.GetActionSize ();

        base.InitializeAcademy();
    }

    [Header("Curriculum")]
    public int n_marbles = 1;
    //public int n_blocks = 1;
    public int grid_size = 3;

    public override void AcademyReset()
    {
        n_marbles = (int)resetParameters["n_marbles"];
        //n_blocks = (int)resetParameters["n_blocks"];
        grid_size = (int)resetParameters["grid_size"];
    }

    public override void AcademyStep()
    {

    }
}
