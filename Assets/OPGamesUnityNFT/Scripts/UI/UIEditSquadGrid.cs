using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UIEditSquadGrid : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	[SerializeField] private RectTransform gridButtonsParent;
	[SerializeField] private bool isOffense;

	private Image[,] cellImage = new Image[Constants.GridCols,Constants.GridRows];
	private int[,] cellIndex = new int[Constants.GridCols,Constants.GridRows];
	private bool initialized = false;
	private Sprite[] nftSprites = new Sprite[3];
	private Vector2Int prevCoord = new Vector2Int(-1,-1);
	private Color colorTransparent = new Color(0,0,0,0);
	private List<GameGlobals.SelectedInfo> selected;

	private void OnEnable()
	{
		if (isOffense) selected = GameGlobals.Offense;
		else           selected = GameGlobals.Defense;

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

			cellImage[x,y] = child.GetComponent<Image>();
			cellImage[x,y].sprite = null;
			cellIndex[x,y] = -1;

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
		var list = selected;
		for (int i=0; i<list.Count; i++)
		{
			var info = list[i];
			var nft = mgr.GetNFTItemDataById(info.Id);
			if (nft == null) continue;

			info.Pos.x = x;
			info.Pos.y = 0;
			
			var image = cellImage[x, 0];
			Utils.SetImageTexture(image, nft.Texture);

			cellIndex[x,0] = i;

			nftSprites[i] = image.sprite;

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

		Vector2Int coord = GetPosFromCellName(go.name);

		if (!IsValidCoord(coord))
			return;

		var imageCurr = cellImage[coord.x, coord.y];
		if (imageCurr.sprite == null)
			return;

		prevCoord = coord;
	}

	public void OnDrag(PointerEventData eventData)
	{
		OnDragInternal(eventData);
	}


	public void OnEndDrag(PointerEventData eventData)
	{
		OnDragInternal(eventData);
		prevCoord.Set(-1,-1);
	}

	private void OnDragInternal(PointerEventData eventData)
	{
		var go = GetCell(eventData);
		if (go == null) return;

		Vector2Int coord = GetPosFromCellName(go.name);
		if (prevCoord == coord)
			return;

		// error checking of index
		if (!IsValidCoord(prevCoord) || !IsValidCoord(coord))
			return;

		var imagePrev = cellImage[prevCoord.x, prevCoord.y];
		var imageCurr = cellImage[coord.x, coord.y];

		imageCurr.sprite = imagePrev.sprite;
		imageCurr.color = (imageCurr.sprite != null) ? Color.white : colorTransparent;

		imagePrev.sprite = null;
		imagePrev.color = colorTransparent;

		cellIndex[coord.x, coord.y] = cellIndex[prevCoord.x, prevCoord.y];
		cellIndex[prevCoord.x, prevCoord.y] = -1;

		prevCoord = coord;
	}

	private bool IsValidCoord(Vector2Int c)
	{
		return c.x >= 0 && c.x < Constants.GridCols &&
			   c.y >= 0 && c.y < Constants.GridRows;
	}

	private Vector2Int GetPosFromCellName(string n)
	{
		string temp = n.Replace("Cell ", "");
		var coords = temp.Split(',');

		int x = 0;
		int y = 0;
		Int32.TryParse(coords[0], out x);
		Int32.TryParse(coords[1], out y);

		return new Vector2Int(x,y);
	}

	public void AssignFinalPositions()
	{
		Debug.Log("AssignFinalPositions");
		for (int x=0; x<Constants.GridCols; x++)
		{
			for (int y=0; y<Constants.GridRows; y++)
			{
				if (cellIndex[x,y] == -1) continue;

				int index = cellIndex[x,y];

				selected[index].Pos.Set(x,y);
			}
		}
	}
}

}
