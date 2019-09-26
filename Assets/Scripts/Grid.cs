using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

	#region Fields

	//creates a bool variable w. id onlydisplaypathgizmos, which gives the user the option to only display the found path OR the foundpath and the nodegrid
	public bool onlyDisplayPathGizmos;

	//creates a public Transform w. id player, which holds the seeker in the game which job is to find the target
	public Transform Seeker;

	//creates a public layermask w. id unwalkable, which gives the user to set which gameobjects in the game that are unwalkable for the seeker 
	public LayerMask unwalkable;

	//creates a public vector2 w. id gridworldsize, which let´s the user to set the size of the grid in the editor
	public Vector2 gridWorldSize;

	//creates a public float variable w. id noderadius, which also lets the user decided the radius of each node in the nodegrid within the editor 
	public float nodeRadius;

	//creates a public list of nodes w. id path, which holds all the nodes that forms the found path
	public List<Node> path;

	//creates a two dimensional Node array w. id grid, which will form the nodegrid
	Node[,] grid;

	//creates a private float variable w. id nodediameter, which will hold the diameter of specific node
	private float nodeDiameter;

	//creates two private int variables w. id gridsizeX and -Y, which will hold the acctual size of the nodegrid 
	private int gridSizeX, gridSizeY;

	#endregion


	private void Start()
	{
		//calculates the diameter of a node, by multiplying the noderadius by 2
		nodeDiameter = nodeRadius * 2;

		//calculates the gridsizeX and -Y, by dividing the gridworldsize of each axis by the diameter of a node
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		//calls the method creategrid, which creates the nodegrid
		CreateGrid();
	}


	#region Grid methods

	/// <summary>
	/// Creates the nodegrid
	/// </summary>
	private void CreateGrid()
	{
		//sets the two dimensional grid array to be equal the size of gridsizeX and -Y
		grid = new Node[gridSizeX, gridSizeY];

		//creates a new vector2 w. id worldbottomleft, which calculates and gets the bottom left corner of the nodegrid
		//by subtracting both half of the grid´s size in x and y
		Vector2 worldBottomLeft = - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

		//two for-loops that goes through both gridsizes
		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				//creates a vector2 w. id worldpoint which gets calculated by,
				//adding the bottom left corner in the world with each node on x- and y-axis
				Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);

				//creates a bool variable w. id walkable that checks if the worldpoint is colliding w. a node that is unwalkable
				bool walkable = !Physics2D.Raycast(worldPoint, Vector2.zero, 1, unwalkable).collider;

				//fills the nodegrid w. nodes on specific x- and y position
				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}


	/// <summary>
	/// Gets a node from a position in the world
	/// </summary>
	/// <param name="worldPosition"> to get node from </param>
	/// <returns> node from specific position in the world </returns>
	public Node NodeFromWorldPoint(Vector2 worldPosition)
	{
		//creates vector2 w. id worldbottomleft, which gets the bottom left corner of the nodegrid
		Vector2 worldBottomLeft = -Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

		//gets the distance between the world´s x-position and bottom left x-position, as the same w. the distance on y-axis
		int xDistance = (int)worldPosition.x - (int)worldBottomLeft.x;
		int yDistance = (int)worldPosition.y - (int)worldBottomLeft.y;

		//IF the x- and y-distance is inside the nodegrid the node on that position is returned otherwise null
		if (xDistance >= 0 && xDistance < gridSizeX && yDistance >= 0 && yDistance < gridSizeY)
		{
			return grid[xDistance, yDistance];
		}
		return null;
	}


	/// <summary>
	/// Gets the neighbouringnodes to specific node
	/// </summary>
	/// <param name="node"> to get neighbouring nodes of </param>
	/// <returns> a list of neighbouringnodes </returns>
	public List<Node> GetNeighbour(Node node)
	{
		//creates a list of node w. id neighbour, which will hold the neighbouringnodes of a specific node
		List<Node> neighbour = new List<Node>();

		//two for-loops that searches through a block of 3 by 3 nodes
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				//IF the x- and y-value both are 0 then we have the position of the node we are searching neighbouringnodes from, so we don´t want to search this node so we skipp it
				if(x == 0 && y == 0)
				{
					continue;
				}

				//creates two int variables w. id checkX and -Y, which gets the value of specified node plus x- and y-value
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				//IF that int variables for checkX and -Y is inside the nodegrid, then the neighbouringnode is added to the neighbouring list 
				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbour.Add(grid[checkX, checkY]);
				}
			}
		}
		//returns the neighbouring list filled with neighbours to specific node
		return neighbour;
	}


	/// <summary>
	/// Draws the nodegrid, walkable or not, and the node where our player is positioned
	/// </summary>
	private void OnDrawGizmos()
	{
		//draws the nodegrid from it´s position and with the size from gridWorldSize set by the user in the editor
		Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

		//IF the tick box onlydisplaypathgizmos is ticked, then only the path is drawn by the gizmo
		if (onlyDisplayPathGizmos)
		{
			//only draws a path as long as the path has a path of nodes within it self
			if (path != null)
			{
				//loops through the path list and draws the nodes forming the path with black color
				foreach (Node n in path)
				{
					Gizmos.color = Color.black;
					Gizmos.DrawCube(n.worldPosition, Vector2.one * (nodeDiameter - .1f));
				}
			}
		}
		else //if the tick box of onlydisplaypathgizmo is not ticked, then both the path and the nodegrid is drawn
		{
			//draws the nodegrid as long as the grid array is filled with nodes
			if (grid != null)
			{
				//creates a node w. id seekerNode, which will store the node in the nodegrid by using Nodefromworldpoint method
				Node seekerNode = NodeFromWorldPoint(Seeker.position);

				//foreach-loop that loops through the grid array and draws the walkablenodes w. white and unwalkable nodes w. red
				foreach (Node n in grid)
				{
					//if n is walkable we set the node to color white otherwise to red
					Gizmos.color = (n.walkable) ? Color.white : Color.red;

					//only draws a path as long as the path has a path of nodes within it self
					if (path != null)
					{
						//so if the path contains the node n, then the pathnode is drawn black
						if (path.Contains(n))
						{
							Gizmos.color = Color.black;
						}
					}

					//so if the seekernode is equal to the node n, then the seekernode is drawn with cyan
					if (seekerNode == n)
					{
						Gizmos.color = Color.cyan;
					}

					//for each node in the grid we draw a cube
					Gizmos.DrawCube(n.worldPosition, Vector2.one * (nodeDiameter - .1f));
				}
			}
		}
	}

	#endregion


}

