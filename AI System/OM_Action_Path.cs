using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PathState { Walking, Waiting }

public class OM_Action_Path : OM_Action {

	//Fields
	OM_MoveData[] points;
	int target;
	PathState state;
	float waitTimer;

	/// <summary>
	/// Create an action where the Old Man walks on a set path
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="name">The name of this action</param>
	/// <param name="points">Array of all points</param>
	/// <param name="startPoint">Start index for this point</param>
	/// <param name="thoughtItem">The sprite to put in the thought bubble. Can be left null for no item to be shown</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public OM_Action_Path(float priority, string name, OM_MoveData[] points, int startPoint = 0, 
							Sprite thoughtItem = null, float discardTime = 0) 
				: base(priority, name, OM_ActionType.Path, thoughtItem, discardTime)
	{
		this.points = points;

		//Keep the start point in bounds
		if (startPoint >= points.Length || startPoint < 0)
			target = 0;
		else
			target = startPoint;

		state = PathState.Walking;
		waitTimer = 0;
	}

	public override void StartAction(OldMan om)
	{
		base.StartAction(om);
		SetNextPoint(om, false);
	}

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
			case PathState.Walking:
				if (!om.Agent.pathPending && om.Agent.remainingDistance < 0.3f)
				{
					//Wait if there is a value passed in, otherwise go to the next point
					if (points[target].WaitAfterReaching > 0)
					{
						waitTimer = 0;
						state = PathState.Waiting;
					}
					else SetNextPoint(om);
				}
				break;

			//Wait for a set amount of seconds
			case PathState.Waiting:
				waitTimer += Time.deltaTime;
				if(waitTimer > points[target].WaitAfterReaching)
				{
					state = PathState.Walking;
					SetNextPoint(om);
				}
				break;
		}
	}

	void SetNextPoint(OldMan om, bool incrementTarget = true)
	{
		//Increment target, but keep it in bounds
		if(incrementTarget)
			target = (target + 1) % points.Length;

		//Only slow down the Old Man if they have to wait at the point
		if (points[target].WaitAfterReaching > 0)
			om.Agent.autoBraking = true;
		else om.Agent.autoBraking = false;

		// Send the NavMesh agent to that point
		om.Agent.destination = points[target].Point;
	}

	/// <summary>
	/// Returns the current point this action is walking to
	/// </summary>
	public OM_MoveData GetCurrentPoint()
	{
		if (state == PathState.Walking)
			return points[target];
		else
			return points[(target + 1) % points.Length];
	}
}
