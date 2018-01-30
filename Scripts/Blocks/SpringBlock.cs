using UnityEngine;

public class SpringBlock : Block
{
    // References
    public Rigidbody movingPart;

    // State
    private Vector3 startLocalPos;

    protected override void HandleSetup()
    {
        startLocalPos = movingPart.transform.localPosition;
    }

    protected override void HandleStartTrigger()
    {
        movingPart.transform.SetParent(null);
    }

    protected override void HandleReset()
    {
        movingPart.transform.SetParent(transform);
        movingPart.position = transform.position + startLocalPos;
    }

    protected override void HandleStepTrigger()
    {
        // @note: for this to work, the parent should not be moving
        movingPart.MovePosition(transform.position + startLocalPos + transform.up * animCurve.Evaluate(triggerTime));
        //movingPart.transform.localPosition = startLocalPos + transform.up * animCurve.Evaluate(t);
        //Debug.Log("MOVING " + t + " to " + (transform.position + startLocalPos + transform.up * animCurve.Evaluate(t)));

        if (triggerTime >= 1.0f)
        {
            triggerTime = 1.0f;
            movingPart.position = transform.position + startLocalPos;
            isTriggered = false;
            movingPart.transform.SetParent(transform);
        }
    }

    protected override void HandleEndTrigger()
    {
        movingPart.transform.SetParent(transform);
        movingPart.position = transform.position + startLocalPos;
    }

}
