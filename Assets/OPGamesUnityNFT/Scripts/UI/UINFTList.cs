using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace OPGames.NFT
{

public class UINFTList : MonoBehaviour
{
	[SerializeField] private GameObject loadingParent;
	[SerializeField] private Text loadingStatus;

	[SerializeField] private RectTransform gridButtonsParent;
	[SerializeField] private RectTransform contentParent;
	[SerializeField] private GameObject prefabNFT;

	[SerializeField] private Button btnNext;
	[SerializeField] private int numNFTs = 3;

	[SerializeField] private UINFTItem[] selectedUI;
	[SerializeField] private Toggle[] selectedToggle;

	[SerializeField] private GameObject panelNFT;
	[SerializeField] private GameObject panelGrid;

	private Dictionary<string, GameObject> listItems   = new Dictionary<string, GameObject>();
	private Dictionary<string, Toggle>     listToggles = new Dictionary<string, Toggle>();

	private Button[] gridButtonOccupied;

	private int step = 0;

	public void OnBtnNext()
	{
		if (step == 0)
		{
			step++;
			panelNFT.SetActive(false);
			panelGrid.SetActive(true);

			foreach (var t in selectedToggle)
				t.gameObject.SetActive(true);
		}
		else
		{
			UIManager.Open(UIType.Matchmaking);
			step = 0;
		}
	}
	
	public void OnBtnBack()
	{
		UIManager.Close();
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
		Debug.Log($"OnGridBtnClick {x}, {y}");

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

		if (gridButtonOccupied[index] != null)
		{
			var prev = gridButtonOccupied[index];
			prev.image.sprite = null;
			prev.image.color = new Color(0,0,0,0);
		}

		var nftItem = selectedUI[index];
		image.sprite = nftItem.GetSprite();
		image.color = Color.white;

		GameGlobals.Selected[index].Pos = new Vector2Int(x,y);

		gridButtonOccupied[index] = b;
	}

	private void OnEnable()
	{
		step = 0;
		panelNFT.SetActive(true);
		panelGrid.SetActive(false);
	}

	private void Start()
	{
		gridButtonOccupied = new Button[numNFTs];

		NFTManager nft = NFTManager.Instance;
		if (nft != null)
		{
			nft.OnQueryChainBegin += OnQueryChainBegin;
			nft.OnQueryChainEnd   += OnQueryChainEnd;
			nft.OnNFTItemFound    += OnNFTItemFound;
			nft.OnNFTItemLoaded   += OnNFTItemLoaded;
			nft.OnNFTListComplete += OnNFTListComplete;
		}

		LoadFromManager();

		foreach (var s in selectedUI)
			s.gameObject.SetActive(false);

		foreach (var t in selectedToggle)
			t.gameObject.SetActive(false);

		InitGridButtons();
	}

	private void InitGridButtons()
	{
		int x = 0;
		int y = 2;
		int len = gridButtonsParent.childCount;
		for (int i=0; i<len; i++)
		{
			var child = gridButtonsParent.GetChild(i);
			var b = child.GetComponent<Button>();
			if (b == null) continue;

			int xTemp = x;
			int yTemp = y;
			b.onClick.AddListener(() => OnGridBtnClick(xTemp, yTemp, b));

			x = (x + 1) % 6;
			if (x == 0)
				y--;
		}
	}

	private void LoadFromManager()
	{
		NFTManager nft = NFTManager.Instance;
		if (nft == null)
			return;

		if (nft.IsNFTListComplete == false)
			return;

		var list = nft.LoadedNFTs;
		OnNFTListComplete(list);

		foreach (var n in list)
			OnNFTItemLoaded(n);
	}

	private void OnQueryChainBegin(string chain)
	{
		if (loadingStatus != null)
		{
			loadingStatus.text = string.Format("Query {0} begin", chain);
		}
	}

	private void OnQueryChainEnd(string chain)
	{
		if (loadingStatus != null)
		{
			loadingStatus.text = string.Format("Query {0} end", chain);
		}
	}

	private void OnNFTItemFound(int count)
	{
		if (loadingStatus != null)
		{
			loadingStatus.text = string.Format("Found {0} NFTs", count);
		}
	}

	private void OnNFTListComplete(List<NFTItemData> list)
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

			Debug.LogFormat("OnNFTListComplete - add to list {0}", n.UniqueId);
		}
	}

	private void OnNFTItemLoaded(NFTItemData n)
	{
		if (n == null)
			return;

		if (loadingParent != null)
			loadingParent.SetActive(false);

		if (listItems.ContainsKey(n.UniqueId) == false)
		{
			Debug.LogErrorFormat("UI list items does not have this key {0}", n.UniqueId);
			return;
		}

		GameObject clone = listItems[n.UniqueId];
		if (clone == null)
		{
			Debug.LogErrorFormat("UI list items gameobject is null {0}", n.UniqueId);
			return;
		}

		UINFTItem item = clone.GetComponent<UINFTItem>();
		if (item != null)
			item.Fill(n);

		RefreshTogglesAndNext();
	}

	private void RefreshTogglesAndNext(bool dontCare = false)
	{
		int count = 0;
		GameGlobals.Selected.Clear();
		foreach (var kvp in listToggles)
		{
			if (kvp.Value.isOn) 
			{
				count++;
				var info = new GameGlobals.SelectedInfo();
				info.Id = kvp.Key;
				GameGlobals.Selected.Add(info);
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
		foreach (var info in GameGlobals.Selected)
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
