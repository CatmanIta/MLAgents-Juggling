using UnityEngine;

public class FlipperBlock : Block
{

    // Parameters
    public float magnitude = -45;

    // References
    public Transform movingTr;

    // State
    private float startAngle;

    protected override void HandleSetup()
    {
        startAngle = movingTr.localEulerAngles.x;
    }

    protected override void HandleReset()
    {
        movingTr.localEulerAngles = new Vector3(startAngle, 0, 0);
    }

    protected override void HandleStartTrigger()
    {
        movingTr.localEulerAngles = new Vector3(startAngle, 0, 0);
    }

    protected override void HandleStepTrigger()
    {
        movingTr.localEulerAngles = new Vector3(startAngle + animCurve.Evaluate(triggerTime) * magnitude, 0, 0);
    }

    protected override void HandleEndTrigger()
    {
        movingTr.localEulerAngles = new Vector3(startAngle, 0, 0);
    }
}
