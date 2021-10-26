using UnityEngine;
using System.Collections.Generic;

// TODO: public members should start with Capital letter
public class Grid : MonoBehaviour
{
	[System.Serializable]
	public class Node
	{
		public Vector2Int parent = new Vector2Int(-1,-1);
		public Vector2Int pos = new Vector2Int(-1,-1);
		public int gCost = 0;
		public int hCost = 0;
		public int fCost { get { return gCost + hCost; } }
		public int occupied = 0;

		public void ResetForSearch()
		{
			parent.x = -1;
			parent.y = -1;
			gCost = 0;
			hCost = 0;
		}
	}

	public Vector2 gridSize = new Vector2(1,1);
	public int width = 0;
	public int height = 0;
	public int costAdjacent = 10;
	public int costDiagonal = 14;

	private Node[,] search;
	private List<Node> open = new List<Node>();
	private List<Node> close = new List<Node>();

	public void Awake()
	{
		search = new Node[width, height];

		for (int i=0; i<search.GetLength(0); i++)
		{
			for (int j=0; j<search.GetLength(1); j++)
			{
				search[i,j] = new Node() { pos = new Vector2Int(i,j) };
			}
		}

//		search[1,3].occupied = 0;
//		search[1,2].occupied = 1;
//		search[1,1].occupied = 1;
//		search[1,0].occupied = 1;
//
//		search[2,3].occupied = 0;
//		search[2,2].occupied = 0;
//		search[2,1].occupied = 0;
//		search[2,0].occupied = 0;
//
//		search[3,3].occupied = 0;
//		search[3,2].occupied = 1;
//		search[3,1].occupied = 1;
//		search[3,0].occupied = 0;
//
//		search[4,3].occupied = 1;
//		search[4,2].occupied = 1;
//		search[4,1].occupied = 1;
//		search[4,0].occupied = 0;
//
//		FindPath(new Vector2Int(0,0), new Vector2Int(5,3));
	}

	public void SetOccupied(Vector3 world, int id)
	{
		SetOccupied(WorldToGrid(world), id);
	}

	public void SetOccupied(Vector2Int g, int id)
	{
		if (g.x < 0 || g.x >= width || g.y < 0 || g.y >= height)
		{
			Debug.LogErrorFormat("Index out of range {0}", g);
			return;
		}
		search[g.x, g.y].occupied = id;
	}

	public void ClearOccupied(Vector3 world)
	{
		SetOccupied(world, 0);
	}

	public void ClearOccupied(Vector2Int g)
	{
		SetOccupied(g, 0);
	}

	public int GetOccupied(Vector3 world)
	{
		return GetOccupied(WorldToGrid(world));
	}

	public int GetOccupied(Vector2Int g)
	{
		if (g.x < 0 || g.x >= width || g.y < 0 || g.y >= height)
		{
			Debug.LogErrorFormat("Index out of range {0}", g);
			return 0;
		}
		return search[g.x, g.y].occupied;
	}

	private void OnDrawGizmos()
	{
		for (int i=0; i<width; i++)
		{
			for (int j=0; j<height; j++)
			{
				if (search != null)
					Gizmos.color = search[i,j].occupied == 0 ? Color.white : Color.black;

				Gizmos.DrawSphere(new Vector3(i*gridSize.x, j*gridSize.y, 0), 0.1f);
			}
		}
	}

	public Vector2Int WorldToGrid(Vector3 world)
	{
		return new Vector2Int(
				(int)Mathf.Round(world.x / gridSize.x),
				(int)Mathf.Round(world.y / gridSize.y));
	}

	public Vector3 GridToWorld(Vector2Int grid)
	{
		return new Vector3( grid.x * gridSize.x, grid.y * gridSize.y, 0);
	}

	public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
	{
		List<Node> path = FindPath(WorldToGrid(startWorld), WorldToGrid(endWorld));
		if (path == null)
			return null;

		List<Vector3> pathWorld = new List<Vector3>();

		foreach (Node n in path)
		{
			pathWorld.Add(GridToWorld(n.pos));
		}

		return pathWorld;
	}

	// A* pathfinding
	public List<Node> FindPath(Vector2Int start, Vector2Int end)
	{
		//Debug.LogFormat("FindPath {0}, {1}", start, end);
		foreach (Node n in search)
		{
			n.ResetForSearch();
		}

		List<Node> children = new List<Node>();
		List<Node> path = new List<Node>();

		open.Clear();
		close.Clear();

		Node nStart = search[start.x, start.y];
		nStart.hCost = GetHCost(start, end);

		open.Add(nStart);

		int iterations = 0;
		while (open.Count > 0 && iterations < 100)
		{
			// get least f cost node from open
			int cost = System.Int32.MaxValue;
			Node nCurrent = null;
			foreach (Node n in open)
			{
				if (cost > n.fCost)
				{
					cost = n.fCost;
					nCurrent = n;
				}
			}
			
			open.Remove(nCurrent);
			close.Add(nCurrent);

			//Debug.LogFormat("Finding... current {0}, fCost {1}", nCurrent.pos, nCurrent.fCost);

			if (nCurrent.pos == end)
			{
				int iterations2 = 0;

				// found end goal. backtrack to get final path
				//Debug.Log("Found end, backtrack to start");
				while (nCurrent.pos != start && iterations2 < 100)
				{
					path.Insert(0, nCurrent);
					nCurrent = search[nCurrent.parent.x, nCurrent.parent.y];
					//Debug.LogFormat("Backtrack... current {0}", nCurrent.pos);
					iterations++;
				}

				string text = "";
				foreach (Node n in path)
				{
					text += n.pos.ToString() + ",";
				}

				//Debug.Log("Final path: " + text);
				return path;
			}

			// create a list of children from current node
			children.Clear();
			GetChildren(nCurrent, end, ref children);

			// insert valid children in front so they are prioritized
			foreach (Node n in children)
			{
				open.Insert(0, n);
			}

			iterations++;
		}
		return null;
	}

	private int GetHCost(Vector2Int a, Vector2Int b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	private void GetChildren(Node parent, Vector2Int end, ref List<Node> result)
	{
		result.Clear();

		Vector2Int pPos = parent.pos;

		int costA = parent.gCost+costAdjacent;
		int costD = parent.gCost+costDiagonal;

		Vector2Int[] positions = new Vector2Int[] 
		{
			// north, south, east, west
			new Vector2Int(pPos.x,   pPos.y+1),
			new Vector2Int(pPos.x,   pPos.y-1),
			new Vector2Int(pPos.x+1, pPos.y  ),
			new Vector2Int(pPos.x-1, pPos.y  ),

//			// northeast, northwest, southeast, southwest
			new Vector2Int(pPos.x+1, pPos.y+1),
			new Vector2Int(pPos.x-1, pPos.y+1),
			new Vector2Int(pPos.x+1, pPos.y-1),
			new Vector2Int(pPos.x-1, pPos.y-1),
		};
		
		for (int i=0; i<positions.Length; i++)
		{
			Vector2Int p = positions[i];
			if (p.x < 0 || p.x >= width) continue;
			if (p.y < 0 || p.y >= height) continue;

			int gCost = i < 4 ? costA : costD;

			Node n = search[p.x, p.y];
			if (n.occupied != 0) continue;
			if (close.Contains(n)) continue;
			if (open.Contains(n) && n.gCost < gCost) continue;

			n.parent = parent.pos;
			n.gCost = i < 4 ? costA : costD;
			n.hCost = GetHCost(n.pos, end);
			result.Add(n);
		}
	}
}
