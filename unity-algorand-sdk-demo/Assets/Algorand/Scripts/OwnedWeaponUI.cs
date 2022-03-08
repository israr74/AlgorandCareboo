using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OwnedWeaponUI : MonoBehaviour
{
    public Algod algod;

    public AlgorandAccount playerAccount;

    public Transform contentParent;

    public OwnedWeaponItem ownedWeaponItemPrefab;

    public WeaponStoreUI store;

    Dictionary<ulong, OwnedWeaponItem> items = new Dictionary<ulong, OwnedWeaponItem>();

    bool shouldPollWeapons;

    public void Awake()
    {
        for (var i = 0; i < store.weapons.Length; i++)
        {
            var ownedWeaponItem = GameObject.Instantiate(ownedWeaponItemPrefab, Vector3.zero, Quaternion.identity, contentParent);
            items[store.weapons[i].assetParams.Index] = ownedWeaponItem;
            ownedWeaponItem.asset = store.weapons[i].assetParams.AssetParams;
            ownedWeaponItem.UpdateWeaponAmount(0);
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
        var (_, info) = await algod.Client.GetAccountInformation(playerAccount.Address);
        if (info.Assets == null) return;
        var weapons = info.Assets.Where(a => items.ContainsKey(a.AssetId));

        foreach (var weaponHolding in weapons)
        {
            items[weaponHolding.AssetId].UpdateWeaponAmount((int)weaponHolding.Amount);
        }
    }
}
