using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MoveToState { WaitingBefore, WalkingTo, WaitingAt }

public class OM_Action_MoveTo : OM_Action
{

	//Fields
	OM_MoveData goalPoint;
	float waitBeforeMoving;
	MoveToState state;
	float waitTimer;

	/// <summary>
	/// Create an action where the Old Man walks to a set point
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="name">The name of this action</param>
	/// <param name="goalPoint">The point the OM is walking to</param>
	/// <param name="waitBeforeMoving">How long the OM should wait before moving to the goalPoint</param>
	/// <param name="thoughtItem">The sprite to put in the thought bubble. Can be left null for no item to be shown</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public OM_Action_MoveTo(float priority, string name, OM_MoveData goalPoint, float waitBeforeMoving, 
								Sprite thoughtItem = null, float discardTime = 0) 
		: base(priority, name, OM_ActionType.MoveTo, thoughtItem, discardTime)
	{
		this.goalPoint = goalPoint;
		this.waitBeforeMoving = waitBeforeMoving;

		if (waitBeforeMoving > 0)
			state = MoveToState.WaitingBefore;
		else
			state = MoveToState.WalkingTo;
		waitTimer = 0;
	}

	/// <summary>
	/// Start this action
	/// </summary>
	/// <param name="om">The OldMan object</param>
	public override void StartAction(OldMan om)
	{
		base.StartAction(om);
		if(state == MoveToState.WalkingTo)
		{
			SetNextPoint(om, goalPoint);
		}
	}

	/// <summary>
	/// Stop this action
	/// </summary>
	/// <param name="om">The OldMan's object</param>
	public override void StopAction(OldMan om)
	{
		base.StopAction(om);
		om.Agent.ResetPath();
	}

	/// <summary>
	/// Code for the action that gets called in OldMan's update
	/// </summary>
	/// <param name="om">The Old Man script</param>
	public override void ExecuteAction(OldMan om)
	{
		if (!Active || Completed)
			return;

		//State control
		switch (state)
		{
			//Wait for a set amount of seconds before walking to the point
			case MoveToState.WaitingBefore:
				waitTimer += Time.deltaTime;
				if (waitTimer > waitBeforeMoving)
				{
					state = MoveToState.WalkingTo;
					SetNextPoint(om, goalPoint);
				}
				break;

			//Walk to the point
			case MoveToState.WalkingTo:
				if (!om.Agent.pathPending && om.Agent.remainingDistance < 0.3f)
				{
                    //Wait if there is a value passed in, otherwise start walking back
                    if (goalPoint.WaitAfterReaching > 0)
                    {
                        waitTimer = 0;
                        state = MoveToState.WaitingAt;
                    }
                    else CompleteAction();
				}
				break;

			//Wait for a set amount of seconds before walking back
			case MoveToState.WaitingAt:
				waitTimer += Time.deltaTime;
				if (waitTimer > goalPoint.WaitAfterReaching)
				{
                    CompleteAction();
				}
				break;
		}
	}

	void SetNextPoint(OldMan om, OM_MoveData point)
	{
		//Only slow down the Old Man if they have to wait at the point
		if (point.WaitAfterReaching > 0)
			om.Agent.autoBraking = true;
		else om.Agent.autoBraking = false;

		// Send the NavMesh agent to that point
		om.Agent.destination = point.Point;
	}
}
