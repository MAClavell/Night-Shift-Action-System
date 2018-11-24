using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OM_Action_NearPlayer : OM_Action
{
	//Fields
	float timer;
	float deathTotal = 0.75f;

	/// <summary>
	/// Create an action where the Old Man stands in place
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="name">The name of this action</param>
	/// <param name="standLength">How long the stand lasts for, x lessthan/= 0 for infinite</param>
	/// <param name="standPoint">The Vector3 to stand at. If null is passed in then there is no movement</param>
	/// <param name="thoughtItem">The sprite to put in the thought bubble. Can be left null for no item to be shown</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public OM_Action_NearPlayer(Sprite thoughtItem = null)
		: base(0, "NearPlayer", OM_ActionType.NearPlayer, thoughtItem, 0)
	{
		timer = 0;
	}

	/// <summary>
	/// Start this near player action
	/// </summary>
	/// <param name="om">The OldMan object</param>
	public override void StartAction(OldMan om)
	{
		base.StartAction(om);
		om.Agent.ResetPath();
	}

	/// <summary>
	/// Code for the action that gets called in OldMan's update
	/// </summary>
	/// <param name="om">The Old Man script</param>
	public override void ExecuteAction(OldMan om)
	{
		if (!Active || Completed || GameManager.Instance.OldManDead)
			return;

		timer += Time.deltaTime;

		if (!GameManager.Instance.player.IsNearOldMan)
		{
            CompleteAction();
			PuzzleManager.Instance.OM_inDanger = false;
		}
		if(timer >= deathTotal)
		{
			GameManager.Instance.KillOldMan();
		}
	}
}
