using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UIType
{
	MainMenu = 0,
	EditSquad = 1,
	Settings = 2,
	Leaderboard = 3,
	History = 4,
	BattleResult = 5,
	Matchmaking = 6,
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

		Show(UIType.MainMenu);
	}

	static public void Show(UIType t)
	{
		foreach (var i in _instance.Items)
		{
			if (i.Type == t)
			{
				i.Go.SetActive(true);
				var canvas = i.Go.GetComponent<Canvas>();
				if (canvas != null)
					canvas.sortingOrder = _instance.UIStack.Count;
				_instance.UIStack.Push(i);
				break;
			}
		}
	}

	static public void Hide(UIType t)
	{
	}
}
