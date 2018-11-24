using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for a point the Old Man should move to
/// </summary>
public class OM_MoveData
{
	/// <summary>
	/// The point to move to
	/// </summary>
	public Vector3 Point { get; set; }
	/// <summary>
	/// Amount of time the OM waits after reaching the point
	/// </summary>
	public float WaitAfterReaching { get; set; }

	/// <summary>
	/// Create a point for the old man to move to
	/// </summary>
	/// <param name="point">The point to move to</param>
	/// <param name="waitAfterReaching">Amount of time the OM waits after reaching the point</param>
	public OM_MoveData(Vector3 point, float waitAfterReaching = 0)
	{
		Point = point;
		WaitAfterReaching = waitAfterReaching;
	}
}
