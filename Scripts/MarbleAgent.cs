using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MarbleAgent : Agent
{
    public enum Test
    {
        SingleMover,
        StillForce,
        MovingForce,
        MovingForces,
        MovingRotatingForce,
        MovingRotatingForces
    }

	private Academy academy;

    public bool useCurriculum = false;

    [Header("Common Tests")]
    public Test test;

    void OnValidate()
    {
        switch (test)
        {
            // Moves a single block around to catch falling marbles
            case Test.SingleMover:
                canTriggerBlocks = false;
                canSpeedMoveBlocks = true;
                canPlaceBlocks = false;
                canRotateBlocks = false;

                stateUseMarblePosVelXZ = true;
                stateUseMarblePosVelY = true;
                stateUseBlockPos = true;
                stateUseBlockVel = true;
                stateUseBlockRot = false;

                endIfStill = false;

                nBlocks = 1;
                startMarbleRandomRange = new Vector3(1f, 0f, 1f);
                break;

            // Throws the ball up when falling, does not move
            case Test.StillForce:
                canTriggerBlocks = true;
                canSpeedMoveBlocks = false;
                canPlaceBlocks = false;
                canRotateBlocks = false;

                stateUseMarblePosVelXZ = false;
                stateUseMarblePosVelY = true;
                stateUseBlockPos = false;
                stateUseBlockVel = false;
                stateUseBlockRot = false;

                endIfStill = true;

                nBlocks = 1;
                startMarbleRandomRange = Vector3.zero;
                break;

            // Throws the ball up and moves as well (juggling)
            case Test.MovingForce:
                canTriggerBlocks = true;
                canSpeedMoveBlocks = true;
                canPlaceBlocks = false;
                canRotateBlocks = false;

                stateUseMarblePosVelXZ = true;
                stateUseMarblePosVelY = true;
                stateUseBlockPos = true;
                stateUseBlockVel = true;
                stateUseBlockRot = false;

                endIfStill = true;

                nBlocks = 1;
                startMarbleRandomRange = new Vector3(1f, 0f, 1f);
                break;

            // Throws the ball up and moves and rotates as well (juggling)
            case Test.MovingRotatingForce:
                canTriggerBlocks = true;
                canSpeedMoveBlocks = true;
                canPlaceBlocks = false;
                canRotateBlocks = true;

                stateUseMarblePosVelXZ = true;
                stateUseMarblePosVelY = true;
                stateUseBlockPos = true;
                stateUseBlockVel = true;
                stateUseBlockRot = true;

                endIfStill = true;

                nBlocks = 1;
                startMarbleRandomRange = new Vector3(1f, 0f, 1f);
                break;

            // Can move force triggers and trigger, marbles fall anywhere
            case Test.MovingForces:
                canTriggerBlocks = true;
                canSpeedMoveBlocks = false;
                canPlaceBlocks = true;
                canRotateBlocks = false;

                stateUseMarblePosVelXZ = true;
                stateUseMarblePosVelY = true;
                stateUseBlockPos = true;
                stateUseBlockVel = true;
                stateUseBlockRot = false;

                endIfStill = true;

                nBlocks = 2;
                startMarbleRandomRange = new Vector3(1f, 0f, 1f);
                break;

            // Can move and rotate force triggers and trigger, marbles fall anywhere
            case Test.MovingRotatingForces:
                canTriggerBlocks = true;
                canSpeedMoveBlocks = true;
                canPlaceBlocks = false;
                canRotateBlocks = true;

                stateUseMarblePosVelXZ = true;
                stateUseMarblePosVelY = true;
                stateUseBlockPos = true;
                stateUseBlockVel = true;
                stateUseBlockRot = true;

                endIfStill = true;

                nBlocks = 2;
                startMarbleRandomRange = Vector3.zero;  // easier
                //startMarbleRandomRange = new Vector3(1f, 0f, 1f);
                break;
        }
    }

    [Header("Debug")]
    public bool verboseAction = false;
    public bool verboseState = false;
    public bool verboseReward = false;
    public bool verboseEnd = false;

    [Header("Blocks")]
    public int nBlocks = 1;
    public int gridSize = 3;
    //public GameObject[,] gridSlots;
    public Block[] blockPrefabs;    // types of blocks we can instantiate
    public float blockMaxAcceleration = 1f;
    public float blockRotationSpeed = 1f;
    public float blockRotationAngleLimit = 45;
    public bool limitBlockMovement = true;

    public bool autoPlaceBlockAtMidpoint = true;
    private List<Block> blocks = new List<Block>(); // blocks we spawn and use	

    [Header("Marbles")]
	public int nMarbles = 1;
    public int maxMarbles = 5;
    public Marble marblePrefab;
    public Vector3 startMarbleSpawnPos = new Vector3(0f, 4f, 0f);
    public bool randomizeStartMarbles = false;
    public Vector3 startMarbleRandomRange = new Vector3(0f, 0f, 0f);     // Relative to the Grid Size (max is gridSize/2)
	private List<Marble> marbles = new List<Marble>();    // marbles we look at

    [Header("End")]
    public float fallingHeight = 0;
    public bool endIfOutOfRange = false;
    public bool endIfFalling = false;
    public float maxStillSeconds = 1f;
    public float velocityForStillMarble = 0.1f;
    public bool endIfStill = false;
    private float lastStepTime;
    private float outOfRangeSpan { get { return gridSize / 2f; } }

    [Header("Rewards")]
    // @note: these should be normalized
    public float rewardForEnding = -1f;

    public float rewardForStep = 0.01f;
    public float rewardForAction_Trigger = -0.02f;
	public float rewardForAction_Place = -0.2f;

    public float minVelocityScore = 0.0f;
    public float maxVelocityScore = 1.0f;
    public float minVelocityForScore = 5.0f;
    public float maxVelocityForScore = 10.0f;

    public float minHeightScore = 0.0f;
    public float maxHeightScore = 1.0f;
    public float minHeightForScore = 2.0f;
    public float maxHeightForScore = 5.0f;

    [Header("Weights")]
    [Range(0,1)]
    public float rewardStepWeight = 0.0f;
    [Range(0, 1)]
    public float rewardVelocityWeight = 0.5f;
    [Range(0, 1)]
    public float rewardVelocityUpWeight = 0.2f;
    [Range(0, 1)]
    public float rewardHeightWeight = 0.5f;

	private float velocityRange
    {
        get { return maxVelocityForScore - minVelocityForScore; }
    }

    private float heightRange
    {
        get { return maxHeightForScore - minHeightForScore; }
    }

	[Header("State")]
	public bool stateUseMarblePosVelXZ = true;
    public bool stateUseMarblePosVelY = true;
    public bool stateUseBlockTriggerTime = false;
    public bool stateUseBlockPos = false;
    public bool stateUseBlockVel = false;
    public bool stateUseBlockRot = false;
    public bool stateUseAllMarbles = true;

    [Header("Normalization")]
    public float maxMarbleVelocity = 10f;
    public float maxBlockVelocity = 5f;
    public float areaHeight = 10f; // height of the movement area (for normalization)

    [Header("Actions")]
    public bool canTriggerBlocks = true;
    public bool canSpeedMoveBlocks = true;
    public bool canRotateBlocks = false;
    public bool canPlaceBlocks = false;

	public int GetStateSize()
	{
		int nStates = 0;
        int marbleStates = 0;
        marbleStates += stateUseMarblePosVelXZ ? 6 : 0;
	    marbleStates += stateUseMarblePosVelY ? 3 : 0;
        if (stateUseAllMarbles) marbleStates *= nMarbles;

	    int blockStates = 0;
        blockStates += stateUseBlockTriggerTime ? 1 : 0;
        blockStates += stateUseBlockPos ? 2 : 0;
        blockStates += stateUseBlockRot ? 3 : 0;
        blockStates += stateUseBlockVel ? 2 : 0;
	    blockStates *= nBlocks;
	    nStates = marbleStates + blockStates;
        return nStates;
	}

	public int GetActionSize()
	{
		int nActions = 0;
		if (brain.brainParameters.actionSpaceType == StateType.continuous) {
			nActions += canTriggerBlocks ? 1 : 0;
			nActions += canPlaceBlocks ? 2 : 0;
			nActions += canSpeedMoveBlocks ? 2 : 0;
		    nActions += canRotateBlocks ? 2 : 0;
            nActions *= nBlocks;
		} else {
			nActions += 1; // trigger
			nActions *= nBlocks;
			nActions += 1; // empty discrete action
		}
		return nActions;
	}

    public override void InitializeAgent()
    {	
		academy = FindObjectOfType<Academy>();

        // Spawn N marbles from the prefab
        // TODO: set different positions
        // TODO: prepare listening for marbles spawning instead of spawning them here, spawn into the Academy)
        for (int i = 0; i < maxMarbles; i++) 
		{
			var marbleGo = Instantiate (marblePrefab);
			var marble = marbleGo.GetComponent<Marble> ();
			marble.AgentReset (this, i);
		    marble.SetShown(false);
			marbles.Add (marble);
		}

		// TODO: instead of spawning the same block, spawn different ones
		// TODO: spawn initial empty blocks, delete and spawn new ones when creating new blocks
		for (int i = 0; i < nBlocks; i++) {
			var block = Instantiate (blockPrefabs [0]);
			blocks.Add (block);
		}

        //gridSlots = new GameObject[gridSize, gridSize];

        // Remove heavy stuff during training
        if (!academy.isInference)
        {
            verboseAction = false;
            verboseState = false;
            verboseReward = false;
        }

        if (verboseReward)
        {
            DrawArea();
        }
    }

    void DrawArea()
    {
        Debug.DrawLine(
            new Vector3(outOfRangeSpan, fallingHeight, outOfRangeSpan),
            new Vector3(-outOfRangeSpan, fallingHeight, outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(-outOfRangeSpan, fallingHeight, outOfRangeSpan),
            new Vector3(-outOfRangeSpan, fallingHeight, -outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(-outOfRangeSpan, fallingHeight, -outOfRangeSpan),
            new Vector3(outOfRangeSpan, fallingHeight, -outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(outOfRangeSpan, fallingHeight, -outOfRangeSpan),
            new Vector3(outOfRangeSpan, fallingHeight, outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(outOfRangeSpan, fallingHeight + 5, outOfRangeSpan),
            new Vector3(-outOfRangeSpan, fallingHeight + 5, outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(-outOfRangeSpan, fallingHeight + 5, outOfRangeSpan),
            new Vector3(-outOfRangeSpan, fallingHeight + 5, -outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(-outOfRangeSpan, fallingHeight + 5, -outOfRangeSpan),
            new Vector3(outOfRangeSpan, fallingHeight + 5, -outOfRangeSpan),
            Color.green,
            100.0f
            );

        Debug.DrawLine(
            new Vector3(outOfRangeSpan, fallingHeight + 5, -outOfRangeSpan),
            new Vector3(outOfRangeSpan, fallingHeight + 5, outOfRangeSpan),
            Color.green,
            100.0f
            );
    }

    // Returns a list of float defining the state of the agent
    public override List<float> CollectState()
    {
        List<float> state = new List<float>();

        // @note: we handle changing state size (i.e. more marbles) by applying state size to the brain based on the number of marbles

        // we get the bottomest marble only with negative velocity (i.e. the one that is going down!)
        // this is because we cannot change state size with the curriculum, so we cannot just add more marbles and check all of them
        // also, we are not interested in high marbles, after all
        // @note: we could check the two bottomest marbles if needed
        Marble riskyestMarble = marbles[0];
        if (!stateUseAllMarbles)
        {
            for (var mi = 1; mi < nMarbles; mi++) // Only the first nMarbles are considered
            {
                var marble = marbles[mi];
                var velOld = riskyestMarble.GetComponent<Rigidbody>().velocity.y;
                var velNew = marble.GetComponent<Rigidbody>().velocity.y;
                // TODO: cache RB into marble itself so we do not call GetComponent
                if (velNew < velOld)
                {
                    // going down-er!
                    riskyestMarble = marble;
                }
                else if (Math.Abs(velNew - velOld) < 0.01f)
                {
                    if (marble.transform.position.y < riskyestMarble.transform.position.y)
                        riskyestMarble = marble;
                }
            }
            foreach (var marble in marbles)
            {
                marble.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
            riskyestMarble.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        for (var mi = 0; mi < nMarbles; mi++) // Only the first nMarbles are considered
        {
            var marble = marbles[mi];
            var usedMarble = marble;
            if (!stateUseAllMarbles)
            {
                usedMarble = riskyestMarble;
            }
            // Marble Position (+3)
            // @note: normalized and relative to agent's position
            if (stateUseMarblePosVelXZ)
                state.Add((usedMarble.transform.position.x - transform.position.x) / (gridSize / 2f));
            if (stateUseMarblePosVelY)
                state.Add((usedMarble.transform.position.y - transform.position.y) / (areaHeight));
            if (stateUseMarblePosVelXZ)
                state.Add((usedMarble.transform.position.z - transform.position.z) / (gridSize / 2f));

            // TODO: marbles should have a MAX SPEED
            // Marble Speed (+3)
            // @note: normalized based on max velocity
            var marbleRB = usedMarble.transform.GetComponent<Rigidbody>();
            var marbleVelocity = marbleRB.velocity;
            if (stateUseMarblePosVelXZ)
                state.Add(Mathf.Clamp(marbleVelocity.x, -maxMarbleVelocity, maxMarbleVelocity) / maxMarbleVelocity);
            if (stateUseMarblePosVelY)
                state.Add(Mathf.Clamp(marbleVelocity.y, -maxMarbleVelocity, maxMarbleVelocity) / maxMarbleVelocity);
            if (stateUseMarblePosVelXZ)
                state.Add(Mathf.Clamp(marbleVelocity.z, -maxMarbleVelocity, maxMarbleVelocity) / maxMarbleVelocity);

            // TODO: marbles should have a MAX angular velocity
            // Marble Angular Velocity (+3)
            var marbleAngularVelocity = marbleRB.angularVelocity;
            if (stateUseMarblePosVelXZ)
                state.Add(Mathf.Clamp(marbleAngularVelocity.x, -maxMarbleVelocity, maxMarbleVelocity) /
                          maxMarbleVelocity);
            if (stateUseMarblePosVelY)
                state.Add(Mathf.Clamp(marbleAngularVelocity.y, -maxMarbleVelocity, maxMarbleVelocity) /
                          maxMarbleVelocity);
            if (stateUseMarblePosVelXZ)
                state.Add(Mathf.Clamp(marbleAngularVelocity.z, -maxMarbleVelocity, maxMarbleVelocity) /
                          maxMarbleVelocity);

            if (!stateUseAllMarbles) break; // Only one
        }


        foreach (var block in blocks) 
		{
            // Block trigger time (+1)
		    if (stateUseBlockTriggerTime)
		    {
		        state.Add(block.CurrentTriggerTime);
		    }

			// Block position (+2)
			if (stateUseBlockPos) 
			{
                state.Add((block.transform.position.x - transform.position.x) / (gridSize / 2f));
                //state.Add((block.transform.position.y - transform.position.y) / (gridSize / 2f));
                state.Add((block.transform.position.z - transform.position.z) / (gridSize / 2f));
			}

			// Block rotation (+3)
			if (stateUseBlockRot) 
			{
				state.Add(block.transform.localEulerAngles.x / blockRotationAngleLimit);
                state.Add(block.transform.localEulerAngles.y / blockRotationAngleLimit);
                state.Add(block.transform.localEulerAngles.z / blockRotationAngleLimit);
			}

            // Block velocity (+2)
            if (stateUseBlockVel)
            {
                // TODO: range -1,1
                //var blockRb = block.GetComponent<Rigidbody>();
                //var blockVel = blockRb.velocity;
                state.Add(Mathf.Clamp(block.speed.x, -maxBlockVelocity, maxBlockVelocity) / maxBlockVelocity);
                //state.Add(Mathf.Clamp(blockVel.y, -maxBlockVelocity, maxBlockVelocity) / maxBlockVelocity);
                state.Add(Mathf.Clamp(block.speed.z, -maxBlockVelocity, maxBlockVelocity) / maxBlockVelocity);
            }
        }

        if (verboseState)
        {
            string statesString = "State:";
            for (int i = 0; i < state.Count; i++)
            {
                statesString += " " + state[i] + "\n";
            }
            Debug.Log(statesString);
        }

        return state;
    }


    // Steps the simulation based on the actions
    // Sets done when finished
    // Sets the reward
	// @note: this is called at each FixedUpdate if we are not frame-skipping
    public override void AgentStep(float[] act)
    {
        if (verboseAction)
        {
            string actionsString = "Actions:";
            for (int i = 0; i < act.Length; i++)
            {
                actionsString += " " + act[i];
            }
            Debug.Log(actionsString);
        }

        // ---- Actions we can perform ----
        // TODO: - create a new block of a given type (costs a lot, -reward!)
        // TODO: - move a block on the grid (-reward, but not too much)
        // - activate a placed block (no reward malus)

        // Action space is dependent on the number of blocks we have
        float actionReward = 0.0f;
		for (int bi = 0; bi < blocks.Count; bi++) 
		{
			var block = blocks [bi];

		    if (brain.brainParameters.actionSpaceType == StateType.continuous)
		    {
		        int ai = 0;

                // Use action [0] to trigger blocks
                if (canTriggerBlocks)
                {
                    float triggerAction = act[bi];
                    ai++;
                    //Debug.Log(triggerAction);
                    if (triggerAction > 0.2f && triggerAction < 0.5f) // trigger inside this threshold
                    {
                        if (block.Trigger())
                        {
                            actionReward += rewardForAction_Trigger;

                            // Always unpress (can keep doing it!)
                            block.UnpressTrigger();
                        }
                    }
                    else // untrigger outside the threshold
                    {
                        block.UnpressTrigger();
                    }
		        }

		        // Use actions [N,2N] to place an unplaced block
				if (canPlaceBlocks) 
				{
					float placeXAction = act [bi + nBlocks * ai];
				    ai++;
                    float placeZAction = act [bi + nBlocks * ai];
                    ai++;

                    int grid_x = Mathf.RoundToInt(placeXAction / gridSize);	
					int grid_z = Mathf.RoundToInt(placeZAction / gridSize);

					grid_x = Mathf.Clamp(grid_x, 0, gridSize - 1);
					grid_z = Mathf.Clamp(grid_z, 0, gridSize - 1);

					block.transform.position = new Vector3(grid_x, 0, grid_z);
					actionReward += rewardForAction_Place;
				}

                // Use actions [N,2N] to speed-move blocks (cannot work with canPlaceBlocks)
                if (canSpeedMoveBlocks) 
				{
					float moveXAction = act [bi + nBlocks * ai];
                    ai++;
                    float moveZAction = act [bi + nBlocks * ai];
                    ai++;

                    //Debug.Log("Move: " + new Vector2(moveXAction,moveZAction));

				    //var blockRB = block.gameObject.GetComponent<Rigidbody>();
				    //blockRB.isKinematic = true;
                    //blockRB.AddForce(new Vector3(moveXAction, 0f, moveZAction) * blockMoveSpeed, ForceMode.Acceleration);
				    block.accel = new Vector3(moveXAction, 0f, moveZAction) * blockMaxAcceleration;

                    /*
                    // TODO: do the same for marbles
                    if (blockRB.velocity.sqrMagnitude > maxBlockVelocity*maxBlockVelocity)
                    {
                        blockRB.velocity = blockRB.velocity.normalized * maxBlockVelocity;// * 0.9f;
                        //Debug.Log("MAX REACHED: " + blockRB.velocity);
                    }
				    Debug.Log(name + " speed: " + blockRB.velocity.magnitude);*/

                    // Stop inside the grid's area
				    var tmpPos = block.transform.position;
				    if (limitBlockMovement)
                    {
                        if (tmpPos.x > outOfRangeSpan)
                            tmpPos.x = outOfRangeSpan;
                        if (tmpPos.x < -outOfRangeSpan)
                            tmpPos.x = -outOfRangeSpan;
                        if (tmpPos.z > outOfRangeSpan)
                            tmpPos.z = outOfRangeSpan;
                        if (tmpPos.z < -outOfRangeSpan)
                            tmpPos.z = -outOfRangeSpan;
                    }

                    block.transform.position = tmpPos;
				    //block.speed = Vector3.zero;
				    //blockRB.velocity = Vector3.zero;

				    // TODO: move this logic to the block itself!
				    // @note: we use MovePosition for better precision
				    // @note: we may avoid the delta here? The 3dball example does not do it!
				    /*block.gameObject.GetComponent<Rigidbody> ().MovePosition (
                        block.transform.position +
                        new Vector3 (moveXAction, 0, moveZAction) * blockMoveSpeed); // * Time.fixedDeltaTime */
				}


                // Use actions to rotate the block around X and Z
                if (canRotateBlocks)
                {
                    float rotateXAction = act[bi + nBlocks * ai];
                    ai++;
                    float rotateZAction = act[bi + nBlocks * ai];
                    ai++;
                    //Debug.Log("Rotate: " + new Vector2(rotateXAction, rotateZAction));

                    block.transform.Rotate(Vector3.right, rotateXAction * blockRotationSpeed);
                    block.transform.Rotate(Vector3.forward, rotateZAction * blockRotationSpeed);

                    // Has limits!
                    var tmpEul = block.transform.localEulerAngles;
                    int xDirection = 1;
                    var xAroundZero = tmpEul.x;
                    if (tmpEul.x > 180)
                    {
                        xAroundZero = 360 - tmpEul.x;
                        xDirection = -1;
                    }
                    var zAroundZero = tmpEul.z;
                    int zDirection = 1;
                    if (tmpEul.z > 180)
                    {
                        zAroundZero = 360 - tmpEul.z;
                        zDirection = -1;
                    }

                    //Debug.Log("vX " + tmpEul.x + " vZ " + tmpEul.z);
                    //Debug.Log("X " + xAroundZero + " Z " + zAroundZero);

                    if (xAroundZero > blockRotationAngleLimit)
                        tmpEul.x = blockRotationAngleLimit * xDirection;
                    if (zAroundZero > blockRotationAngleLimit)
                        tmpEul.z = blockRotationAngleLimit * zDirection;
                    block.transform.localEulerAngles = tmpEul;
                }
            }
            else 
			{
				// Discrete, easier to define
				// @note: action at bi == nBlocks means 'no trigger'
				int discreteAction = Mathf.FloorToInt(act[0]);
				if (discreteAction == bi)
				{
					block.Trigger();
					actionReward += rewardForAction_Trigger;
				}
			}
		}

					
		// ------- Reward -------
        // Define the reward
		done = false;
        reward = 0;
        reward += rewardStepWeight * rewardForStep;
        for(var mi = 0; mi < nMarbles; mi++)  // Only the first nMarbles are considered
        {
            var marble = marbles[mi];

            var velocity = marble.transform.GetComponent<Rigidbody>().velocity;
            var position = marble.transform.position;
            var velocityMagnitude = velocity.magnitude;

            // Fast marble UP -> good
            float rewardVelocityUp = 0.0f;
            if (velocityMagnitude > minVelocityForScore && velocity.y > 0)
            {
                float velocityRatio = Mathf.Clamp(velocity.y, minVelocityForScore, maxVelocityForScore) / velocityRange;
                // [0,1]
                rewardVelocityUp = minVelocityScore + velocityRatio;
            }

            // Fast marble ANY -> good
            float rewardVelocityAbs = 0.0f;
            if (velocityMagnitude > minVelocityForScore)
            {
                float velocityRatio = Mathf.Clamp(velocityMagnitude, minVelocityForScore, maxVelocityForScore) /
                                      velocityRange; // [0,1]
                rewardVelocityAbs = minVelocityScore + velocityRatio * (maxVelocityScore - minVelocityScore);
            }

            // Marble UP -> good (only if going up!)
            float rewardHeight = 0;
            if (position.y > minHeightForScore && velocity.y > 0)
            {
                float heightRatio = Mathf.Clamp(position.y, minHeightForScore, maxHeightForScore) / heightRange;
                    // [0,1]
                rewardHeight = minHeightScore + heightRatio * (maxHeightScore - minHeightScore);
            }

            // Sum rewards
            reward += rewardVelocityUpWeight * rewardVelocityUp
                     + rewardVelocityWeight * rewardVelocityAbs
                     + rewardHeightWeight * rewardHeight;
            if (reward > 1.0f) reward = 1.0f;

            // Check current speed
            if (velocityMagnitude < velocityForStillMarble
                && position.y < 2.5f) // avoid considering stopping when too high
            {
                marble.currentStillTime += Time.time - lastStepTime;
            }
            else
            {
                marble.currentStillTime = 0.0f;
            }
            lastStepTime = Time.time;

            // Marble outside my grid -> end
            // (we aim at having the marble never get out of range!)
            if (endIfOutOfRange && (marble.transform.position.x < -outOfRangeSpan
                                    || marble.transform.position.x > outOfRangeSpan
                                    || marble.transform.position.z < -outOfRangeSpan
                                    || marble.transform.position.z > outOfRangeSpan))
            {
                if (verboseEnd) Debug.Log("END: out of range");
                done = true;
                reward = rewardForEnding;
                break;
            }
            else if (endIfFalling && marble.transform.position.y <= fallingHeight)
            {
                if (verboseEnd) Debug.Log("END: fallen");
                done = true;
                reward = rewardForEnding;
                break;
            }
            else if (endIfStill && marble.currentStillTime > maxStillSeconds)
            {
                if (verboseEnd) Debug.Log("END: still (marble has time " + marble.currentStillTime + ")");
                done = true;
                reward = rewardForEnding;
                break;
            }
        }

        reward += actionReward; // will be a malus

        //Debug.Log(marbleStillTime);
        Monitor.Log("Reward", reward, MonitorType.slider, transform);

        if (verboseReward)
        {
			Debug.Log ("Reward: " + reward + " (tot " + CumulativeReward + ")");
//                      + "\nVelocity: " + velocityMagnitude);
        }
    }


    // Resets the state of the agent
    public override void AgentReset()
    {
        // Reset parameters based on the curriculum
        if (useCurriculum)
        {
            var marbleAcademy = academy as MarbleAcademy;
            gridSize = marbleAcademy.grid_size;
            nMarbles = marbleAcademy.n_marbles;
            //Debug.Log("Curriculum says: grid size is " + gridSize);
            //DrawArea();
        }
        /*
        this.gridSize = marbleAcademy.grid_size;
        this.nMarbles = marbleAcademy.n_marbles;
        this.nBlocks = marbleAcademy.n_blocks;
        */

        foreach (var block in blocks) {
			block.AgentReset ();
            block.maxSpeed = maxBlockVelocity;
        }

        // Activate only some marbles
        for (var mi = 0; mi < nMarbles; mi++)
        {
            var marble = marbles[mi];
		    float perMarbleDelay = 1f;
            marble.AgentReset(this, mi, mi* perMarbleDelay);
            marble.SetShown(false);
            marble.SetShown(true);
        }

        if (autoPlaceBlockAtMidpoint) {
            for (var index = 0; index < blocks.Count; index++)
            {
                var block = blocks[index];
                block.transform.position = Vector3.zero + Vector3.right * index;
            }
        }
    }
}
