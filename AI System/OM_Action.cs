using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the types an Old Man action can be
/// </summary>
public enum OM_ActionType { StandStill, Path, MoveTo, NearPlayer }

/// <summary>
/// The base class for an action the old man can take.
/// </summary>
public class OM_Action 
{
	/// <summary>
	/// How important the action is. Lower is a higher priority
	/// </summary>
	public float Priority { get; private set; }
	/// <summary>
	/// If this action is currently active
	/// </summary>
	public bool Active { get; private set; }
	/// <summary>
	/// Whether this action has been activated at all before
	/// </summary>
	public bool Fresh { get; private set; }
	/// <summary>
	/// If this action is completed
	/// </summary>
	public bool Completed { get; set; }
	/// <summary>
	/// The type of action this action is
	/// </summary>
	public OM_ActionType ActionType { get; private set; }
	/// <summary>
	/// Whether this action should be discarded
	/// </summary>
	public bool ToDiscard { get { return discardTimer > discardTotal; } }
    /// <summary>
    /// The name of this action
    /// </summary>
    public string Name { get; set; }

    //Private fields
    private float discardTotal;
	private float discardTimer;
	private Sprite thoughtItem;

    /// <summary>
    /// Create a base action for the old man
    /// </summary>
    /// <param name="priority">How important this action is</param>
    /// <param name="name">The name of this action</param>
    /// <param name="type">The type of action this is</param>
	/// <param name="thoughtItem">The sprite to put in the thought bubble. Can be left null for no item to be shown</param>
    /// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite</param>
    public OM_Action(float priority, string name, OM_ActionType type, Sprite thoughtItem = null, float discardTime = 0)
	{
		Priority = priority;
        Name = name;
		Completed = false;
		ActionType = type;
		Fresh = true;
		discardTotal = discardTime;
		discardTimer = 0;
		this.thoughtItem = thoughtItem;
	}

	/// <summary>
	/// Tick the discard time
	/// </summary>
	public void TickDiscardTime()
	{
		if (discardTotal == 0)
			return;

		discardTimer += Time.deltaTime;
	}

	/// <summary>
	/// Code for the action that gets called in OldMan's update
	/// </summary>
	/// <param name="om">The Old Man object</param>
	public virtual void ExecuteAction(OldMan om)
	{ }

	/// <summary>
	/// Start this action
	/// </summary>
	/// <param name="om">The OldMan object</param>
	public virtual void StartAction(OldMan om)
	{
		Active = true;
		Fresh = false;

        //Set the correct thought bubble status
        if (thoughtItem != null)
        {
            om.thoughtItem.sprite = thoughtItem;
            om.StartThought();
        }
        else om.EndThought();
	}

	/// <summary>
	/// Stop this action
	/// </summary>
	/// <param name="om">The OldMan's object</param>
	public virtual void StopAction(OldMan om)
	{
		Active = false;
        if(thoughtItem != null)
        {
            om.EndThought();
        }
    }

    /// <summary>
    /// Complete this action
    /// </summary>
    public void CompleteAction()
    {
        Completed = true;
        PuzzleManager.Instance.TriggerOnActionCompleted(this);
    }

    /// <summary>
    /// Override for the '<' operator. Compares the 'Priority' field
    /// </summary>
    public static bool operator <(OM_Action first, OM_Action second)
    {
        return first.Priority < second.Priority;
    }

    /// <summary>
    /// Override for the '<=' operator. Compares the 'Priority' field
    /// </summary>
    public static bool operator <=(OM_Action first, OM_Action second)
    {
        return first.Priority <= second.Priority;
    }

    /// <summary>
    /// Override for the '>' operator. Compares the 'Priority' field
    /// </summary>
    public static bool operator >(OM_Action first, OM_Action second)
    {
        return first.Priority > second.Priority;
    }

    /// <summary>
    /// Override for the '>=' operator. Compares the 'Priority' field
    /// </summary>
    public static bool operator >=(OM_Action first, OM_Action second)
    {
        return first.Priority >= second.Priority;
    }
}
