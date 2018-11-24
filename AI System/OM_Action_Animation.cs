using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OM_Action_Animation : OM_Action
{

	//Fields
	float timer;
	string animName;
	float animLength;

	/// <summary>
	/// Create an action where an animation plays for the old man
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="name">The name of this action</param>
	/// <param name="animName">The name of the animation</param>
	/// <param name="animLength">How long the animation lasts for. 0 for infinite</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public OM_Action_Animation(float priority, string name, string animName, float animLength, float discardTime = 0)
		: base(priority, name, OM_ActionType.StandStill, null, discardTime)
	{
		this.animLength = animLength;
		this.animName = animName;
		timer = 0;
	}

	/// <summary>
	/// Start this stand action
	/// </summary>
	/// <param name="om">The OldMan object</param>
	public override void StartAction(OldMan om)
	{
		base.StartAction(om);
		om.animator.Play(animName);
		timer = 0;
	}

	/// <summary>
	/// Stop this action
	/// </summary>
	/// <param name="om">The OldMan's object</param>
	public override void StopAction(OldMan om)
	{
		base.StopAction(om);
	}

	/// <summary>
	/// Code for the action that gets called in OldMan's update
	/// </summary>
	/// <param name="om">The Old Man script</param>
	public override void ExecuteAction(OldMan om)
	{
		if (!Active || Completed ||  animLength == 0)
			return;

		//Wait till animation is over
		if (timer < animLength)
		{
			timer += Time.deltaTime;
		}
		else CompleteAction();
	}
}
