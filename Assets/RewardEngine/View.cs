// Test class modeling the view

using UnityEngine;

public class View
{

	internal virtual void updateRank(int rank)
    {
        Debug.Log("View: Notify rank change: " + rank);
    }

	internal virtual void updateReward(int reward)
	{
        Debug.Log("View: Notify reward change: " + reward);
	}

	internal virtual void updatePositiveScore(int s, int rs)
	{
        Debug.Log("View: Notify new P score: " + s + ", " + rs);
	}
	internal virtual void updateNegativeScore(int s, int rs)
	{
        Debug.Log("View: Notify new N score: " + s + ", " + rs);
	}
}
