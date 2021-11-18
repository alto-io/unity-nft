using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UIEditSquadGrid : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	[SerializeField]
	private RectTransform gridButtonsParent;

	private Image[,] cells = new Image[6,3];
	private bool initialized = false;

	public void OnBtnConfirm()
	{
		SceneManager.LoadScene(2);
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		InitGridButtons();
		InitCharacters();
	}

	private void InitGridButtons()
	{
		if (initialized)
			return;

		int x = 0;
		int y = 2;
		int len = gridButtonsParent.childCount;
		for (int i=0; i<len; i++)
		{
			var child = gridButtonsParent.GetChild(i);

			int xTemp = x;
			int yTemp = y;

			cells[x,y] = child.GetComponent<Image>();

			UIGridCell c = child.gameObject.AddComponent<UIGridCell>();
			c.SetParent(this);

			child.gameObject.name = $"Cell {x},{y}";

			x = (x + 1) % 6;
			if (x == 0)
				y--;
		}

		initialized = true;
	}

	private void InitCharacters()
	{
		NFTManager mgr = NFTManager.Instance;

		int x = 0;
		var list = GameGlobals.Selected;
		foreach (var info in list)
		{
			Debug.Log($"set nft 1 - {info.Id}");
			var nft = mgr.GetNFTItemDataById(info.Id);
			if (nft == null) continue;
			
			Debug.Log($"set nft 2 - {info.Id}");
			var image = cells[x, 0];
			Utils.SetImageTexture(image, nft.Texture);

			image.color = Color.white;
			x++;
		}
	}

	private GameObject GetCell(PointerEventData eventData)
	{
		foreach(var go in eventData.hovered)
		{
			if (go.name.Contains("Cell"))
			{
				return go;
			}
		}
		return null;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		var go = GetCell(eventData);
		if (go == null) return;

		Debug.Log($"OnBeginDrag {go.name}");
	}

	public void OnDrag(PointerEventData eventData)
	{
		var go = GetCell(eventData);
		if (go == null) return;
		Debug.Log($"OnDrag {go.name}");
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		var go = GetCell(eventData);
		if (go == null) return;
		Debug.Log($"OnEndDrag {go.name}");
	}
}

}
