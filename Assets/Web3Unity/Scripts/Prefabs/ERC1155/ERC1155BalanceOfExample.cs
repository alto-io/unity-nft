using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;

public class ERC1155BalanceOfExample : MonoBehaviour
{
    public GameObject sprite;

    async void Start()
    {
        string chain = "ethereum";
        string network = "rinkeby";
        string contract = "0x0ECb00e4f1A671AB36fDe83EA5bEF66928993FD3";
        string account = "0x3233f67E541444DDbbf9391630D92D7F7Aaf508D";
        string tokenId = "5";

        BigInteger balanceOf = await ERC1155.BalanceOf(chain, network, contract, account, tokenId);
        print(balanceOf);

        if (balanceOf > 0)
        {
            sprite.GetComponent<SpriteRenderer>().color = Color.red;

            string uri = await ERC1155.URI(chain, network, contract, tokenId);
            print(uri);
        }
    }
}
