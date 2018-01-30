using UnityEngine;

public abstract class Block : MonoBehaviour
{
    // Parameters
	public AnimationCurve animCurve;
	public float duration = 0.5f;

    // State
    protected bool isTriggered = false;
    protected float triggerTime = 0.0f;

    public Vector3 accel;
    public Vector3 speed;
    public float maxSpeed = 1f;

    void Awake ()
	{
	    HandleSetup();
		AgentReset();
	}

	public void AgentReset()
    {
        var rb = gameObject.GetComponent<Rigidbody>();

        transform.localEulerAngles = Vector3.zero;

        // Resetting block's base
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // TODO: place out of range, will be placed at the correct position with an action instead
        transform.position = Vector3.zero;

        speed = Vector3.zero;
        accel = Vector3.zero;

        triggerTime = 0.0f;
		isTriggered = false;
        triggerPressed = false;

        HandleReset();
	}

    private bool triggerPressed = false;

    public void UnpressTrigger()
    {
        triggerPressed = false;
    }

    public float CurrentTriggerTime
    {
        get { return triggerTime; }
    }

    public bool Trigger()
	{
		//Debug.Log("TRY TO TRIGGER");
	    if (triggerPressed) return false;
		if (isTriggered) return false;
        triggerPressed = true;

        //Debug.Log("TRIGGER!");

        isTriggered = true;
		triggerTime = 0.0f;

	    HandleStartTrigger();

	    return true;
	}

	void FixedUpdate()
    {
        // Test triggering
        if (Input.GetKeyDown(KeyCode.T)) Trigger();

        if (isTriggered)
		{
			triggerTime += Time.fixedDeltaTime / duration;
            HandleStepTrigger();

            if (triggerTime >= 1.0f)
            {
                triggerTime = 0.0f;
                isTriggered = false;
                HandleEndTrigger();
            }
        }

        // Movement
        //accel *= Time.fixedDeltaTime * 0.1f;

        speed += accel * Time.fixedDeltaTime;
        speed *= 0.9f;  // drag
        if (speed.sqrMagnitude > maxSpeed * maxSpeed)
            speed = speed.normalized * maxSpeed; 

        transform.position += speed * Time.fixedDeltaTime;
    }

    protected abstract void HandleSetup();

    protected abstract void HandleReset();

    protected abstract void HandleStartTrigger();

    protected abstract void HandleStepTrigger();

    protected abstract void HandleEndTrigger();
}
