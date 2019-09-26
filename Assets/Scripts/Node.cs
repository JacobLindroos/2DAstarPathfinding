using UnityEngine;

public class Node
{
	#region Fields

	//creates a bool variable w. id walkable, which checks if it´s true/false that specific node is walkable or not
	public bool walkable;

	//creates a vector2 variable w. id worldposition, which holds the specific node x and y-position in the world 
	public Vector2 worldPosition;

	//creates two int variables that stores both the Gcost and Hcost of specific node
	public int gCost, hCost;

	//creates two int variables w. id gridX and -Y, which stores the specific node x- and y-position in the nodegrid
	public int gridX, gridY;

	//creates a Node variable w. id parent, which stores the parent node to specific node
	public Node parent;

	#endregion


	#region Property

	/// <summary>
	/// Property that calculates the Fcost of specific node
	/// </summary>
	public int fCost
	{
		//gets and returns the Fcost of specific node
		get { return gCost + hCost; }
	}

	#endregion


	/// <summary>
	/// Constructor to Node class, which setup a specific node when created
	/// </summary>
	/// <param name="_walkable"> true/false </param>
	/// <param name="_worldPosition"> of specific node </param>
	/// <param name="_gridX"> position of specific node </param>
	/// <param name="_gridY"> position of specific node </param>
	public Node(bool _walkable, Vector2 _worldPosition, int _gridX, int _gridY)
	{
		//sets the Node class variables to the parameters of the node constructor
		walkable = _walkable;
		worldPosition = _worldPosition;
		gridX = _gridX;
		gridY = _gridY;
	}
}

