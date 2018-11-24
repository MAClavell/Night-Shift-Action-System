using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum StandState { Walking, Standing }

public class OM_Action_StandStill : OM_Action
{

	//Fields
	float timer;
	float standLength;
	Vector3? standPoint = null;
	StandState state;

	/// <summary>
	/// Create an action where the Old Man stands in place
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="name">The name of this action</param>
	/// <param name="standLength">How long the stand lasts for, x lessthan/= 0 for infinite</param>
	/// <param name="standPoint">The Vector3 to stand at. If null is passed in then there is no movement</param>
	/// <param name="thoughtItem">The sprite to put in the thought bubble. Can be left null for no item to be shown</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public OM_Action_StandStill(float priority, string name, float standLength = 0, Vector3? standPoint = null, 
									Sprite thoughtItem = null, float discardTime = 0) 
		: base(priority, name, OM_ActionType.StandStill, thoughtItem, discardTime)
	{
		this.standLength = standLength;
		timer = 0;

		//Move to the point if one is passed in
		if (standPoint != null)
		{
			this.standPoint = standPoint.Value;
			state = StandState.Walking;
		}
		else state = StandState.Standing;
	}

	/// <summary>
	/// Start this stand action
	/// </summary>
	/// <param name="om">The OldMan object</param>
	public override void StartAction(OldMan om)
	{
		base.StartAction(om);
        if (standPoint != null)
        {
            om.Agent.autoBraking = true;
            om.Agent.destination = standPoint.Value;
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
			case StandState.Walking:
				if (!om.Agent.pathPending && om.Agent.remainingDistance < 0.3f)
				{
					state = StandState.Standing;
				}
				break;

			case StandState.Standing:
				//Infinite stand
				if (standLength <= 0)
					return;

				//Timed stand
				timer += Time.deltaTime;
				if (timer > standLength)
				{
					CompleteAction();
				}
				break;
		}
	}
}
