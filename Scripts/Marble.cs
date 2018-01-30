using UnityEngine;

public class Marble : MonoBehaviour
{
	//[HideInInspector]
	public float currentStillTime = 0.0f;
    
	public void AgentReset(MarbleAgent agent, int marbleSequentialIndex, float startDelay = 0)
	{
		currentStillTime = 0.0f;

		var rb = GetComponent<Rigidbody> ();
		rb.velocity = new Vector3(0f, 0f, 0f);
		rb.angularVelocity = Vector3.zero;
	    onForceCooldown = 0.0f;

        // Re-init position
        rb.transform.position = agent.startMarbleSpawnPos;
	    rb.transform.position += Vector3.up * marbleSequentialIndex * 2f;    // next one is up
        rb.transform.position += Vector3.right * marbleSequentialIndex;    // next one is right

        if (agent.randomizeStartMarbles)
	    {
            rb.transform.position += new Vector3(
                Random.Range(-agent.startMarbleRandomRange.x, agent.startMarbleRandomRange.x),
                Random.Range(-agent.startMarbleRandomRange.y, agent.startMarbleRandomRange.y),
                Random.Range(-agent.startMarbleRandomRange.z, agent.startMarbleRandomRange.z)) * (agent.gridSize / 2f);
        }

	    CancelInvoke("StartMoving");
        rb.isKinematic = false;
        if (startDelay > 0.0f)
	    {
            rb.isKinematic = true;
            Invoke("StartMoving", startDelay);
        }
    }

    void StartMoving()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }

    public void SetShown(bool b)
    {
        gameObject.SetActive(b);
    }


    void FixedUpdate()
    {
        if (onForceCooldown > 0.0f)
            onForceCooldown -= Time.fixedDeltaTime;
    }

    private float onForceCooldown = 0.0f;

    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        if (onForceCooldown > 0.0f) return; // Not on cooldown
        gameObject.GetComponent<Rigidbody>().AddForce(force, forceMode);
        onForceCooldown = 1f;
    }
}