using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UINFTSelect : MonoBehaviour
{
	[SerializeField] private RectTransform contentParent;
	[SerializeField] private GameObject prefabNFT;

	[SerializeField] private Button btnNext;
	[SerializeField] private int numNFTs = 3;

	[SerializeField] private UINFTItem[] selectedUI;
	[SerializeField] private Toggle[] selectedToggle;

	[SerializeField] private bool isOffense = true;

	private Dictionary<string, GameObject> listItems   = new Dictionary<string, GameObject>();
	private Dictionary<string, Toggle>     listToggles = new Dictionary<string, Toggle>();

	private List<GameGlobals.SelectedInfo> selected;

	public void OnBtnNext()
	{
	}
	
	public void OnBtnReset()
	{
		foreach (var kvp in listToggles)
		{
			kvp.Value.isOn = false;
		}

		RefreshSelection();
	}

	public void OnGridBtnClick(int x, int y, Button b)
	{
		var image = b.GetComponent<Image>();
		if (image == null)
			return;

		int index = 0;
		for (int i=0; i<selectedToggle.Length; i++)
		{
			if (selectedToggle[i].isOn == false)
				continue;

			index = i;
			break;
		}

		var nftItem = selectedUI[index];
		image.sprite = nftItem.GetSprite();
		image.color = Color.white;

		selected[index].Pos = new Vector2Int(x,y);
	}

	private void OnEnable()
	{
		if (isOffense) selected = GameGlobals.Offense;
		else           selected = GameGlobals.Defense;
	}

	private void Start()
	{
		LoadFromManager();

		foreach (var s in selectedUI)
			s.gameObject.SetActive(false);

		foreach (var t in selectedToggle)
			t.gameObject.SetActive(false);
	}

	private void LoadFromManager()
	{
		NFTManager nft = NFTManager.Instance;
		if (nft == null)
			return;

		if (nft.IsNFTListComplete == false)
			return;

		var list = nft.LoadedNFTs;
		AddNFTListItems(list);
		RefreshTogglesAndNext();
	}

	private void AddNFTListItems(List<NFTItemData> list)
	{
		foreach (var n in list)
		{
			GameObject clone = Instantiate(prefabNFT);
			clone.transform.SetParent(contentParent);
			clone.transform.localScale = UnityEngine.Vector3.one;

			listItems.Add(n.UniqueId, clone);

			var toggle = clone.GetComponentInChildren<Toggle>();
			if (toggle != null)
			{
				listToggles.Add(n.UniqueId, toggle);
				toggle.onValueChanged.AddListener(RefreshTogglesAndNext);
			}

			UINFTItem item = clone.GetComponent<UINFTItem>();
			if (item != null)
				item.Fill(n);
		}
	}

	private void RefreshTogglesAndNext(bool dontCare = false)
	{
		int count = 0;
		selected.Clear();
		foreach (var kvp in listToggles)
		{
			if (kvp.Value.isOn) 
			{
				count++;
				var info = new GameGlobals.SelectedInfo();
				info.Id = kvp.Key;
				selected.Add(info);
			}
		}

		if (count >= numNFTs)
		{
			foreach (var t in listToggles.Values)
			{
				if (t.isOn == false) 
				{
					t.interactable = false;
				}
			}
			btnNext.interactable = true;
		}
		else
		{
			foreach (var t in listToggles.Values)
			{
				t.interactable = true;
			}

			btnNext.interactable = false;
		}

		RefreshSelection();
	}

	private void RefreshSelection()
	{
		NFTManager nft = NFTManager.Instance;
		if (nft == null) return;

		int selectedCount = 0;
		foreach (var info in selected)
		{
			string key = info.Id;
			if (selectedUI[selectedCount] != null)
			{
				NFTItemData d = nft.GetNFTItemDataById(key);
				selectedUI[selectedCount].Fill(d);
				selectedUI[selectedCount].gameObject.SetActive(true);
				selectedCount++;
			}
		}

		for (int i=selectedCount; i<selectedUI.Length; i++)
			selectedUI[i].gameObject.SetActive(false);
	}
}

}
