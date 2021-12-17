using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OPGames.NFT
{

public enum UIType
{
	Null = 0,
	MainMenu = 1,
	EditSquadDef = 2,
	Settings = 3,
	Leaderboard = 4,
	History = 5,
	BattleResult = 6,
	Matchmaking = 7,
	EditSquadOff = 8,
	NFTList = 9,
	EnterName = 10,
	Loading = 11,
}

public class UIManager : MonoBehaviour
{
	static private UIManager _instance = null;

	[System.Serializable]
	public class Item
	{
		public UIType Type;
		public GameObject Go;
	}

	public List<Item> Items = new List<Item>();
	public Stack<Item> UIStack = new Stack<Item>();

	private void Awake()
	{
		if (_instance != null)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;

		foreach (var i in Items)
			i.Go.SetActive(false);

		Open(UIType.Loading);
	}

	private void OnDestroy()
	{
		if (_instance == this)
			_instance = null;
	}

	static public void Open(UIType t)
	{
		foreach (var i in _instance.Items)
		{
			if (i.Type == t)
			{
				i.Go.SetActive(true);
				var canvas = i.Go.GetComponent<Canvas>();
				if (canvas != null)
					canvas.sortingOrder = _instance.UIStack.Count;

				if (_instance.UIStack.Count > 0)
				{
					var top = _instance.UIStack.Peek();
					top.Go.SetActive(false);
				}

				_instance.UIStack.Push(i);
				break;
			}
		}
	}

	static public void Close()
	{
		Item top = null;
		Item next = null;

		if (_instance.UIStack.Count > 0)
			top = _instance.UIStack.Pop();

		if (_instance.UIStack.Count > 0)
			next = _instance.UIStack.Peek();

		if (next != null)
			next.Go.SetActive(true);

		if (top != null)
			top.Go.SetActive(false);
	}
}

}
