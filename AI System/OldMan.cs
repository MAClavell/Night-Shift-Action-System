using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

enum ThoughtState { None, Bub1, Bub2, Bub3 }

[RequireComponent(typeof(NavMeshAgent))]
public class OldMan : MonoBehaviour {

    //Public Fields
    public OM_Action currentAction;
	public Animator animator;
	public SpriteHelper sprite;
	public SpriteHelper shadowSprite;
    public Vector3 spriteOffset = new Vector3(0, -0.422f, 0.855f);

    //Thought bubbles
    public GameObject[] thoughtBubbles; //must be a size of three!!!
	public Image thoughtItem;
    public Vector3 bubbleOffset = new Vector3(0, 0, 0.85f);
    Vector3[] rightOgBubPos; //based on direction the old man is walking, not the side the sprites are on
	Vector3[] leftOgBubPos;
	ThoughtState thoughtState;
	Coroutine thoughtCor;
	
	//Radii
	public float detectionRadius = 3f;

	//Properties                                         
	public NavMeshAgent Agent { get; set; }

	// Called before Start
	void Awake()
	{
		//Initialize variabless
		Agent = GetComponent<NavMeshAgent>();
		thoughtState = ThoughtState.None;
		thoughtCor = null;

        //Initialize thought bubbles. Always 3!
        rightOgBubPos = new Vector3[3];
        leftOgBubPos = new Vector3[3];
		for (int i = 0; i < 3; i++)
        {
            rightOgBubPos[i] = thoughtBubbles[i].transform.localPosition;
			leftOgBubPos[i] = thoughtBubbles[i].transform.localPosition;
			leftOgBubPos[i].x = -leftOgBubPos[i].x;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
        //Don't update if game paused
        if (GameManager.Instance.State != GameState.Playing)
            return;

		//Action execution
        ExecuteAction();

		//Put sprites in correct position
		sprite.transform.position = transform.position + spriteOffset;
		shadowSprite.transform.position = transform.position + spriteOffset;

		//Animator updates
		Vector3 direction = Vector3.Normalize(Agent.velocity);
		animator.SetFloat("horizontal", direction.x);
		for (int i = 0; i < 3; i++)
		{
			if (direction.x < -0.01f)
			{
				thoughtBubbles[i].transform.position = transform.position + leftOgBubPos[i] + bubbleOffset;
			}
			else thoughtBubbles[i].transform.position = transform.position + rightOgBubPos[i] + bubbleOffset;
		}
	}

    /// <summary>
    /// Execute the old man's action
    /// </summary>
	void ExecuteAction()
    {
		if (currentAction == null)
			return;

		//Start the action if it is not started
		if (!currentAction.Active)
			currentAction.StartAction(this);

		//Execute the action
		currentAction.ExecuteAction(this);
	}

	/// <summary>
	/// Starts a new thought for the old man
	/// </summary>
	public void StartThought()
	{
		//Don't need to animate if we are already at the end state
		if (thoughtCor != null)
			StopCoroutine(thoughtCor);

        thoughtCor = StartCoroutine(ThoughtIn(0.05f));
	}

	/// <summary>
	/// Stops the old man's current thought
	/// </summary>
	public void EndThought()
	{
		//Don't need to animate if we are already at the end state
		if (thoughtCor != null)
			StopCoroutine(thoughtCor);

		thoughtCor = StartCoroutine(ThoughtOut(0.05f));
	}

	/// <summary>
	/// Animate in the thought bubbles
	/// </summary>
	/// <param name="stepLength">The amount of time between bubbles appearing</param>
	IEnumerator ThoughtIn(float stepLength)
	{
		float timer = 0;
		//Loop until we get the final thought bubble inside
		while(thoughtState != ThoughtState.Bub3)
		{
			//Increment timer and state
			timer += Time.deltaTime;
			if(timer > stepLength)
			{
				timer -= stepLength;
				thoughtState++;

				//Set the correct thought bubble to active
				thoughtBubbles[(int)thoughtState-1].SetActive(true);
			}
			yield return null;
		}
	}

	/// <summary>
	/// Animate out the thought bubbles
	/// </summary>
	/// <param name="stepLength">The amount of time between bubbles appearing</param>
	IEnumerator ThoughtOut(float stepLength)
	{
		float timer = 0;
		//Loop until we get the final thought bubble inside
		while (thoughtState != ThoughtState.None)
		{
			//Increment timer and state
			timer += Time.deltaTime;
			if (timer > stepLength)
			{
				timer -= stepLength;
				thoughtState--;

				//Set the correct thought bubble to active
				thoughtBubbles[(int)thoughtState].SetActive(false);
			}
			yield return null;
		}
	}

	//Draw gizmos in the editor
#if UNITY_EDITOR
	void OnDrawGizmos()
    {
		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, detectionRadius);
	}
#endif
}
