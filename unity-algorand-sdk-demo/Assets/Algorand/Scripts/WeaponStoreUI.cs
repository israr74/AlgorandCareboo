using AlgoSdk;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

public class WeaponStoreUI : MonoBehaviour
{
    public Algod algod;
    public AlgorandAccount creator;
    public AlgorandAccount player;
    public AlgorandAssetParams gameTokenAsset;
    public WeaponAsset[] weapons;

    public Transform listingContentParent;

    public WeaponStoreListingUI listingPrefab;

    public UnityEvent<int> OnStartBuy = new UnityEvent<int>();

    public UnityEvent<int> OnFinishBuy = new UnityEvent<int>();

    public UnityEvent<int> OnBought = new UnityEvent<int>();

    public UnityEvent<int[]> OnWeaponAmountUpdate = new UnityEvent<int[]>();

    bool shouldPollWeapons;

    Dictionary<ulong, int> weaponMap = new Dictionary<ulong, int>();

    public void Awake()
    {
        listingPrefab.gameObject.SetActive(false);
        var startingAmounts = new int[weapons.Length];
        for (var i = 0; i < weapons.Length; i++)
        {
            var newListing = GameObject.Instantiate(listingPrefab, Vector3.zero, Quaternion.identity, listingContentParent);
            newListing.Store = this;
            newListing.WeaponStoreIndex = i;
            newListing.gameObject.SetActive(true);
            newListing.OnWeaponAmountUpdate(startingAmounts);
            weaponMap[weapons[i].assetParams.Index] = i;
            Debug.Log($"Mapping asset {weapons[i].assetParams.Index} to weapon {i}");
        }
    }

    public void OnEnable()
    {
        PollWeapons().Forget();
    }

    public void OnDisable()
    {
        shouldPollWeapons = false;
    }

    public void BuyWeapon(int i)
    {
        BuyWeaponAsync(i).Forget();
    }

    void FinishBuy(int i)
    {
        FetchWeapons().Forget();
        OnFinishBuy.Invoke(i);
    }

    async UniTaskVoid PollWeapons()
    {
        shouldPollWeapons = true;
        while (shouldPollWeapons)
        {
            await FetchWeapons();
            await UniTask.Delay(500);
        }
    }

    async UniTask FetchWeapons()
    {
        var (err, info) = await algod.Client.GetAccountInformation(creator.Address);
        if (err.IsError)
        {
            Debug.LogError(err.Message);
            shouldPollWeapons = false;
            return;
        }

        if (info.Assets == null) return;

        var amounts = new int[weapons.Length];
        foreach (var asset in info.Assets)
        {
            if (weaponMap.TryGetValue(asset.AssetId, out var i))
                amounts[i] = (int)asset.Amount;
        }
        OnWeaponAmountUpdate.Invoke(amounts);
    }

    async UniTaskVoid BuyWeaponAsync(int i)
    {
        OnStartBuy.Invoke(i);
        var weapon = weapons[i];
        await weapon.assetParams.EnsureOptedIn(player);
        var index = weapon.assetParams.Index;
        var (_, txnParams) = await algod.Client.GetSuggestedParams();

        using var creatorKp = creator.GetKeyPair();
        using var playerKp = player.GetKeyPair();
        var assetSellTxn = Transaction.AssetTransfer(
            creatorKp.PublicKey,
            txnParams,
            weapon.assetParams.Index,
            1,
            playerKp.PublicKey
        );
        var assetBuyTxn = Transaction.AssetTransfer(
            playerKp.PublicKey,
            txnParams,
            gameTokenAsset.Index,
            1,
            creatorKp.PublicKey
        );
        var groupId = Transaction.GetGroupId(assetSellTxn.GetId(), assetBuyTxn.GetId());
        assetSellTxn.Group = groupId;
        assetBuyTxn.Group = groupId;
        var signedAssetSell = assetSellTxn.Sign(creatorKp.SecretKey);
        var signedAssetBuy = assetBuyTxn.Sign(playerKp.SecretKey);

        var (txnErr, txid) = await algod.Client.SendTransactions(signedAssetSell, signedAssetBuy);

        if (txnErr.IsError)
        {
            Debug.LogError(txnErr.Message);
            OnFinishBuy.Invoke(i);
            return;
        }

        var pending = await algod.WaitForTransaction(txid);

        if (pending.ConfirmedRound > 0)
            OnBought.Invoke(i);
        OnFinishBuy.Invoke(i);
    }
}
