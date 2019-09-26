using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{

	#region Fields

	//holds two transforms w. id seeker and target, in our case the seeker and the targets transform in the game
	public Transform seeker, target;
	//float variable w. id seekerTime, holds the time until next movement of seeker
	private float seekerTime;
	//float variable w. id seekerTimerMax, holds the time which whitin the next movement should happen
	private float seekerTimeMax = 0.2f;
	//int variable w. id index, holds the element index in list 
	private int index = 0;
	//bool variable w. id foundpath, that checks if it´s true/false if the path is found or not
	private bool foundPath = true;

	//creates an instance of Grid class w. id grid, which helps us to get methods from the grid class
	Grid grid;

	//creates a new random w. id rnd, which will be used to get a new random position within the nodegrid for the target
	System.Random rnd = new System.Random();

	//creates a list of nodes w. id path, which will hold the nodes that together shapes the path for the seeker
	List<Node> path;
	//creates a list of nodes w. id newpath, which will be used to get the nodes in the path list to then give the seeker positions of the shortest way to target and then walking that path
	List<Node> newPath;

	//creates a vector2 w. id newTargetPos which will hold the new random position for the target within the nodegrid when found by seeker
	Vector2 newTargetPos;

	#endregion


	private void Awake()
	{
		//gets the reference from the Grid class
		grid = GetComponent<Grid>();
	}

	private void Update()
	{
		//as long it´s true that the position of targetnode is on a unwalkable position in the nodegrid, targetnode will get a new position until it gets the position "right"
		while (Physics2D.Raycast(target.position, Vector2.zero, 1, grid.unwalkable).collider)
		{
			//calls the method moveTargetToRandomPos, which gives the targetnode a new random position within in the nodegrid
			MoveTargetToRandomPos();
		}

		//calls the method findpath, finds the path between seekernodes position to targetnode position
		FindPath(seeker.position, target.position);

		//calls the method moveSeeker, which moves the seeker to the target after the path is found
		MoveSeeker();
	}


	#region Pathfinding methods

	/// <summary>
	/// Finds the path between two nodes
	/// </summary>
	/// <param name="startPos"> position of node we are searching from </param>
	/// <param name="targetPos"> position of node we are searching to </param>
	private void FindPath(Vector2 startPos, Vector2 targetPos)
	{
		//creates a stopwatch w. id sw, will keep track of time until the path is found
		Stopwatch sw = new Stopwatch();
		//starts the stopwatch clock timer
		sw.Start();

		//creates two nodes w. id startnode & targetned and sets thier both´s position in the nodegrid by calling the method Nodefromworldpoint. Which gets their specific node they are ocuppying in the nodegrid
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		//creates a List of nodes w. id openset, which will hold the nodes in the nodegrid to be evaluated for getting the shortest path
		List<Node> openSet = new List<Node>();
		//creates a Hashset of nodes w. id closedset, which will hold the nodes in the nodegrid which has already been evaluated to form the shortest path 
		HashSet<Node> closedSet = new HashSet<Node>();

		//adds the node w. id startnode to the openset list as it´s first node in the list, gets the first node
		openSet.Add(startNode);

		//initiates a while-loop that keeps going as long it´s true that openset list size is greater then 0, beacuse if it´s 0 then we have found the path
		while (openSet.Count > 0)
		{
			//creates a node w. id currentnode and set´s it to be the first node in the openset list, so now the currentnode is equal to the startnode
			Node currentNode = openSet[0];

			//initiates a for-loop that loops through the openset list w. start from the second node in the list, beacuse we dont want to search the node we are searching from
			for (int i = 1; i < openSet.Count; i++)
			{
				//IF the node´s Fcost at index position "i" is lesser then the currentnode Fcost OR if they have equal Fcost AND the node at index position "i" has a lesser Hcost
				if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
				{
					//then we set the currentnode to that index in the openset list
					currentNode = openSet[i];
				}
			}
			//now a node w. lowest Fcost or Hcost have been found we remove it from openset and adds it to the closetset
			openSet.Remove(currentNode);
			closedSet.Add(currentNode);

			//IF that found currentnode is the targetnode
			if (currentNode == targetNode)
			{
				//the stopwatch timer is stopped and the time it took to find the targetnode is printed
				sw.Stop();
				print("Path found: " + sw.ElapsedMilliseconds + " ms");
				//the retracepath method is called, which retraces the path from the targetnode to the startnode
				RetracePath(startNode, targetNode);
				//we breack out of the while-loop beacuse the path was found
				return;
			}

			//if the currentnode wasn´t equal to the targetnode then a search through the neighbouringnodes is initiated
			//starts a foreach-loop that goes through the neighbouringnodes of the currentnode
			foreach (Node neighbour in grid.GetNeighbour(currentNode))
			{
				//IF the neighbouring node is not walkable OR already exsist in the closedset list, 
				//which means if a neighbouring node could be a wall or already is inside the closedset then we skipp that neighbouring node and search through the other neighbouring nodes
				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				//creates a int variable w. id newmovementcosttoneighbour, which calculates the movement cost to neighbouring node, 
				//by adding the Gcost of the currentnode w. the cost between the currentnode and neighbouringnode
				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

				//IF the newmovementcosttoneighbour is lesser then the neighbouring Gcost OR not already exsist in the openset 
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					//then we update the neighbouringnode´s G- and Hcost, and sets it´s parent node to the currentnode
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					//IF the neighbouring node isn´t in the openset we also add it to the openset
					if (!openSet.Contains(neighbour))
					{
						openSet.Add(neighbour);
					}
				}
			}
		}
	}


	/// <summary>
	/// Gets the distance between two nodes
	/// </summary>
	/// <param name="nodeA"> to get distance from </param>
	/// <param name="nodeB"> to get distance to </param>
	/// <returns> calculated int distance value between the two nodes </returns>
	private int GetDistance(Node nodeA, Node nodeB)
	{
		//creates a int variable w. id dstX, which gets the calculated x distancec between the two nodes 
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		//creates a int variable w. id dstY, which gets the calculated y distancec between the two nodes
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		//IF the x distance is greater then the y distance
		if (dstX > dstY)
		{
			//return this calculated distance value 
			return 14 * dstY + 10 * (dstX - dstY);
		}
		//otherwise return this calculated distance value
		return 14 * dstX + 10 * (dstY - dstX);
	}


	/// <summary>
	/// Retraces the path between seeker- and targetnode
	/// </summary>
	/// <param name="startNode"> where to retrace path to </param>
	/// <param name="endNode"> where to retrace path from </param>
	private void RetracePath(Node startNode, Node endNode)
	{
		//creates the path list, which will hold the nodes of the found path and which will be retraced
		path = new List<Node>();

		//creates a node w. id currentnode and set it to be the endnode, gets the end node
		Node currentNode = endNode;

		//while-loop that goes on as long as the endnode is not equal to the startnode
		while (currentNode != startNode)
		{
			//adds the currentnode to the path list
			path.Add(currentNode);
			//sets the currentnode to the currentnode´s parent, a new currentnode is set
			currentNode = currentNode.parent;
		}
		//reverses through the path node list
		path.Reverse();
		//sets the path in the grid class to be eqaul to this path list, which will draw out the path in gizmos in the grid class
		grid.path = path;
	}


	/// <summary>
	/// Gives targetnode a new random position
	/// </summary>
	private void MoveTargetToRandomPos()
	{
		//creates a vector2 w. id newtargetpos, which is set to be equal a new vector2 w. a random position on the x- and y-axis
		Vector2 newTargetPos = new Vector2(rnd.Next(Mathf.RoundToInt((-grid.gridWorldSize.x / 2)+1), Mathf.RoundToInt((grid.gridWorldSize.x / 2))-1), rnd.Next(Mathf.RoundToInt((-grid.gridWorldSize.y / 2)+1), Mathf.RoundToInt((grid.gridWorldSize.y / 2))-1));
		//sets the targetnode´s position to the new random target position
		target.position = newTargetPos;
	}


	/// <summary>
	/// Moves the seeker to the target
	/// </summary>
	private void MoveSeeker()
	{
		//sets the seekertime to time since game is started
		seekerTime += Time.deltaTime;

		//IF foundpath is true the newpath list is set to be equal the path list and the foundpath is set to be false
		if (foundPath)
		{
			//newpath list gets the current path list of nodes 
			newPath = path;
			//we only want to access this if statement if the seeker found the path to the target, 
			//so as long the path to target isn´t found we want to keep going through newpath list until the target is found.
			//Then path list is updated and we want the newpath list to be updated w. it
			foundPath = false;
		}

		//IF the time for the seeker to move towards target is greater then the seekertimemax 
		if (seekerTime > seekerTimeMax)
		{
			//then the seekers position is set to a new position, which it gets from the node at index value in newpath list
			seeker.transform.position = newPath[index].worldPosition;
			//we want all nodes of the newpath list so then after the seeker has moved to it´s new position we increase the index value to get the next node´s position in the newpath list
			index++;
			//resets the timer so that the seeker always moves each and other second it has been set to move
			seekerTime = 0f;
		}

		//IF seeker´s position in the nodegrid is equal to the target´s position, which means that the seeker has now walked up to the target
		if (grid.NodeFromWorldPoint(seeker.position) == grid.NodeFromWorldPoint(target.position))
		{
			//the targets position is moved to a random position within the nodegrid, calling the movetargettorandompos method
			MoveTargetToRandomPos();
			//index count value is set to be 0 beacuse now there is a new path to find and follow
			index = 0;
			//resets the foundpath variable to true beacuse now newpath list needs to change to the new path list
			foundPath = true;
		}
	}

	#endregion
}

