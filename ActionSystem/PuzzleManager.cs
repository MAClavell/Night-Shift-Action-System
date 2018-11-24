using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

/*
 * 
 * THE GUIDE TO OLD MAN ACTION PRIORITIES:
 * priority <= 0 - Highest priority puzzle specific actions
 * priority == 1 - The old man dying action
 * 10 < priority < 19 - High priority puzzle specific actions
 * 19 < priority < 20 - The seeking the light sizzle action
 * 20 < priority < 50 - Puzzle specific actions
 */

/// <summary>
/// The controlling class for a level.
/// While the 'Puzzle' script controls the specifics of a level, the 'PuzzleManager' makes sure
///		those actions occur.
/// </summary>
/// 
public class PuzzleManager : Singleton<PuzzleManager> {

    //Events
    public delegate void OnActionCompleted(OM_Action action);
    public event OnActionCompleted onActionCompleted;

    public delegate void OnItemPlacedInCart(Interactible item);
    public event OnItemPlacedInCart onItemPlacedInCart;

	public delegate void OnPickupDropped(Interactible item);
	public event OnPickupDropped onPickupDropped;

	public delegate void OnInteractibleUsed(Interactible item);
    public event OnInteractibleUsed onInteractibleUsed;

	public delegate void OnItemTakenFromCart(Interactible item);
	public event OnItemTakenFromCart onItemTakenFromCart;

	//Public Fields
	public OldMan oldMan;
    public CartScript cart;
    public Level levelFile;
	public AnimationClip oldManHeartAttack;

	//Sprites
	public Sprite questionMark;
	public Sprite exclamationPoint;

	/// <summary>
	/// The action queue
	/// </summary>
	public SimplePriorityQueue<OM_Action> OM_ActionQueue { get; private set; }

	//Private fields
	List<OM_Action> actionsToRemove;

    /// <summary>
    /// If the Old Man is in danger of dying
    /// </summary>
    public bool OM_inDanger { get; set; }

    /// <summary>
    /// OldMan is close to the cart
    /// </summary>
    public bool OM_nearCart { get { return Vector3.Distance(cart.transform.position, oldMan.transform.position) < oldMan.detectionRadius + cart.interactibleRange; } }

	//Called before Start()
	void Awake()
	{
		OM_ActionQueue = new SimplePriorityQueue<OM_Action>();
		actionsToRemove = new List<OM_Action>();

		//There is always in infinite standStill action at the bottom of the queue
		OM_Action_StandStill defaultAction = new OM_Action_StandStill(float.MaxValue, "DefaultStandStill");
		OM_ActionQueue.Enqueue(defaultAction, defaultAction.Priority);

        levelFile.SetupLevel();
	}

	// Use this for initialization
	void Start ()
	{
		AssignNextAction();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Early return if Old Man is dead
		if (GameManager.Instance.OldManDead)
			return;

		//Error if queue is empty
		if (OM_ActionQueue.Count <= 0)
		{
			Debug.LogError("Action queue is empty. This should never happen! Always have an infinite StandStill action at the bottom of the queue.");
			return;
		}

		if(GameManager.Instance.player.IsNearOldMan && !OM_inDanger)
		{
			OM_inDanger = true;
			OM_ActionQueue.EnqueueWithoutDuplicates(
				new OM_Action_NearPlayer(PuzzleManager.Instance.exclamationPoint),
				0);
		}

		//Assign a new action to the old man if necessary
		if (oldMan.currentAction == null || oldMan.currentAction.Completed || oldMan.currentAction > OM_ActionQueue.First)
        {
            AssignNextAction();
        }

		//Tick the discard time and remove the action if necessary
		foreach (OM_Action action in OM_ActionQueue)
		{
			action.TickDiscardTime();
			if (action.ToDiscard)
			{
				actionsToRemove.Add(action);
			}
		}
		for(int i = actionsToRemove.Count-1; i >= 0; i--)
		{
			Debug.Log("<color=#FDF212>The action \"" + actionsToRemove[i].Name + "\" has been removed from the queue.</color>");
			OM_ActionQueue.Remove(actionsToRemove[i]);
			actionsToRemove.RemoveAt(i);
		}
	}

	/// <summary>
	/// Add a MoveTo action to the queue
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="goalPoint">The point the OM is walking to</param>
	/// <param name="waitBeforeMoving">How long the OM should wait before moving to the goalPoint</param>
	/// <param name="thoughtItem">The sprite to put in the thought bubble. Can be left null for no item to be shown</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public void AddMoveToAction(float priority, string name, OM_MoveData goalPoint, float waitBeforeMoving, 
									Sprite thoughtItem = null, float discardTime = 0)
	{
		OM_Action_MoveTo action = new OM_Action_MoveTo(priority, name, goalPoint, waitBeforeMoving, thoughtItem, discardTime);
		OM_ActionQueue.Enqueue(action, action.Priority);
	}

    /// <summary>
    /// Create a path action to the queue
    /// </summary>
    /// <param name="priority">How important the action is</param>
    /// <param name="name">The name of this action</param>
    /// <param name="points">Array of all points</param>
    /// <param name="startPoint">Start index for this point</param>
    /// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
    public void AddPathAction(float priority, string name, OM_MoveData[] points, int startPoint = 0, 
		Sprite thoughtItem = null, float discardTime = 0)
    {
        OM_Action_Path action = new OM_Action_Path(priority, name, points, startPoint, thoughtItem, discardTime);
        OM_ActionQueue.Enqueue(action, action.Priority);
    }

    /// <summary>
    /// Create an action where the Old Man stands in place
    /// </summary>
    /// <param name="priority">How important the action is</param>
    /// <param name="name">The name of this action</param>
    /// <param name="standLength">How long the stand lasts for, x lessthan/= 0 for infinite</param>
    /// <param name="standPoint">The Vector3 to stand at. If null is passed in then there is no movement</param>
    /// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
    public void AddStandStillAction(float priority, string name, float standLength = 0, Vector3? standPoint = null, 
										Sprite thoughtItem = null, float discardTime = 0)
    {
        OM_Action_StandStill action = new OM_Action_StandStill(priority, name, standLength, standPoint, thoughtItem, discardTime);
        OM_ActionQueue.Enqueue(action, action.Priority);
    }

	/// <summary>
	/// Create an action where an animation plays for the old man
	/// </summary>
	/// <param name="priority">How important the action is</param>
	/// <param name="name">The name of this action</param>
	/// <param name="animName">The name of the animation</param>
	/// <param name="animLength">How long the animation lasts for. 0 for infinite</param>
	/// <param name="discardTime">The amount of time this action can spend in the queue without being discarded. 0 for infinite.</param>
	public void AddAnimationAction(float priority, string name, string animName, float animLength, float discardTime = 0)
	{
		OM_Action_Animation action = new OM_Action_Animation(priority, name, animName, animLength, discardTime);
		OM_ActionQueue.Enqueue(action, action.Priority);
	}

	/// <summary>
	/// Remove an action from the queue by name. Pretty inefficient so only use if necessary
	/// </summary>
	/// <param name="name">The name of the action</param>
	public void RemoveActionByName(string name)
    {
        OM_Action toRemove = null;
        foreach (OM_Action action in OM_ActionQueue)
        {
            if (action.Name == name)
            {
                toRemove = action;
				break;
            }
        }

        if (toRemove != null)
            OM_ActionQueue.Remove(toRemove);
    }

    /// <summary>
    /// Hand a new action for the old man to complete
    /// </summary>
    void AssignNextAction()
	{
		//If the old man doesn't have an action
		if (oldMan.currentAction == null)
		{
			oldMan.currentAction = OM_ActionQueue.Dequeue();
			return;
		}

		//Stop the current action
		oldMan.currentAction.StopAction(oldMan);

		//If the old man hasn't completed this action, add it back
		if(!oldMan.currentAction.Completed && !OM_ActionQueue.Contains(oldMan.currentAction))
		{
			Debug.Log("<color=#FDF212>The action \"" + oldMan.currentAction.Name + "\" has been put back into the queue.</color>");
			OM_ActionQueue.Enqueue(oldMan.currentAction, oldMan.currentAction.Priority);
		}

		//Keep the default action inside the queue
		if (OM_ActionQueue.Count == 1)
		{
			oldMan.currentAction = OM_ActionQueue.First;
			return;
		}

		//Get the new action
		oldMan.currentAction = OM_ActionQueue.Dequeue();
		Debug.Log("<color=#FDF212>The action \"" + oldMan.currentAction.Name + "\" has become the old man's current action.</color>");
	}

	/// <summary>
	/// Check if the old man is near a passed in object
	/// </summary>
	/// <param name="obj">The object to check</param>
	/// <param name="range">The range on the object</param>
	/// <returns></returns>
	public bool OM_nearObject(Transform obj, float range)
	{
		return Vector3.Distance(obj.position, oldMan.transform.position) < oldMan.detectionRadius + range;
	}

    /// <summary>
    /// Trigger the OnActionCompleted event
    /// </summary>
    /// <param name="action">The action that triggered it</param>
    public void TriggerOnActionCompleted(OM_Action action)
    {
        if (onActionCompleted != null)
        {
            onActionCompleted.Invoke(action);
        }
    }

    /// <summary>
    /// Trigger the OnItemPlacedInCart event
    /// </summary>
    /// <param name="action">The item that triggered it</param>
    public void TriggerOnItemPlacedInCart(Interactible item)
    {
        if (onItemPlacedInCart != null)
        {
            onItemPlacedInCart.Invoke(item);
        }
    }

	/// <summary>
	/// Trigger the OnPickupDropped event
	/// </summary>
	/// <param name="action">The item that triggered it</param>
	public void TriggerOnPickupDropped(Interactible item)
	{
		if (onPickupDropped != null)
		{
			onPickupDropped.Invoke(item);
		}
	}

	/// <summary>
	/// Trigger the OnInteractibleUsed event
	/// </summary>
	/// <param name="item">The item that triggered it</param>
	public void TriggerOnInteractibleUsed(Interactible item)
    {
        if (onInteractibleUsed != null)
        {
            onInteractibleUsed.Invoke(item);
        }
    }

	/// <summary>
	/// Trigger the OnItemTakenFromCart event
	/// </summary>
	/// <param name="item">The item that triggered it</param>
	public void TriggerOnItemTakenFromCart(Interactible item)
	{
		if (onItemTakenFromCart != null)
		{
			onItemTakenFromCart.Invoke(item);
		}
	}
}
