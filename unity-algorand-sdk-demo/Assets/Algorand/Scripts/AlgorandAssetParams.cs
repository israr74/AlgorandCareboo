using System.Linq;
using AlgoSdk;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAssetParams", menuName = "Algorand/Create Asset Params")]
public class AlgorandAssetParams : ScriptableObject
{
    [SerializeField]
    string assetName;

    [SerializeField]
    string unitName;

    [SerializeField]
    [Range(0, 32)]
    uint decimals;

    [SerializeField]
    ulong total;

    [SerializeField]
    AlgorandAccount manager;

    [SerializeField]
    AlgorandAccount clawback;

    [SerializeField]
    AlgorandAccount reserve;

    [SerializeField]
    AlgorandAccount freeze;

    [SerializeField]
    bool defaultFrozen;

    public ulong Index;

    public Algod Algod;

    public AlgorandAccount CreatorAccount;

    public AlgorandAccount Player;

    public AssetParams AssetParams => new AssetParams
    {
        Name = assetName,
        UnitName = unitName,
        Decimals = decimals,
        Total = total,
        Manager = manager?.Address ?? default,
        Clawback = clawback?.Address ?? default,
        Reserve = reserve?.Address ?? default,
        Freeze = freeze?.Address ?? default,
        DefaultFrozen = defaultFrozen
    };

    public static implicit operator AssetParams(AlgorandAssetParams a)
    {
        return a.AssetParams;
    }

    [ContextMenu(nameof(GetOrCreateAsset))]
    public void GetOrCreateAsset()
    {
        GetOrCreateAssetAsync().Forget();
    }

    [ContextMenu(nameof(MoveAssetsToClawBack))]
    public void MoveAssetsToClawBack()
    {
        MoveAssetsToClawBackAsync().Forget();
    }

    public async UniTask EnsureOptedIn(AlgorandAccount account)
    {
        Debug.Log($"Checking if account is opted in to asset...");
        var (err, accountInfo) = await Algod.Client.GetAccountInformation(account.Address);
        if (accountInfo.Assets?.Any(a => a.AssetId == Index) ?? false)
        {
            Debug.Log($"Account already is opted in to asset {name}");
            return;
        }

        Debug.Log($"Account is not opted in to asset {name}. Opting them in...");
        using var kp = account.GetKeyPair();
        var (_, txnParams) = await Algod.Client.GetSuggestedParams();
        var assetOptInTxn = Transaction.AssetAccept(
            kp.PublicKey,
            txnParams,
            Index
        );
        assetOptInTxn.AssetReceiver = kp.PublicKey;
        var signedTxn = assetOptInTxn.Sign(kp.SecretKey);
        var (txnErr, txid) = await Algod.Client.SendTransaction(signedTxn);
        if (txnErr.IsError) Debug.LogError(txnErr.Message);
        var pending = await Algod.WaitForTransaction(txid);
        Debug.Log($"Opted in account to asset id: {Index}");
    }

    async UniTaskVoid GetOrCreateAssetAsync()
    {
        using var kp = CreatorAccount.GetKeyPair();
        Debug.Log($"Getting account info for {CreatorAccount.Address}");
        var (accountError, accountInfo) = await Algod.Client.GetAccountInformation(kp.PublicKey);
        if (accountError.IsError) Debug.LogError(accountError.Message);
        Debug.Log($"Got account info!");

        Debug.Log($"Getting assets for account {CreatorAccount.Address}");
        Index = accountInfo.CreatedAssets?.FirstOrDefault(a => a.Params.UnitName.Equals(unitName)).Index ?? 0L;
        if (Index > 0L)
        {
            Debug.Log($"Found existing asset with Index: {Index}");
            return;
        }

        Debug.Log($"Did not find this asset. Making one now...");
        var (_, txnParams) = await Algod.Client.GetSuggestedParams();
        var createTxn = Transaction.AssetCreate(kp.PublicKey, txnParams, AssetParams).Sign(kp.SecretKey);
        var (createError, createTxid) = await Algod.Client.SendTransaction(createTxn);
        if (createError.IsError) Debug.LogError(createError.Message);
        var pending = await Algod.WaitForTransaction(createTxid);
        Index = pending.AssetIndex;
        Debug.Log($"Made an asset with Index: {Index}");
    }

    async UniTaskVoid MoveAssetsToClawBackAsync()
    {
        using var kp = CreatorAccount.GetKeyPair();

        var (playerInfoErr, playerInfo) = await Algod.Client.GetAccountInformation(Player.Address);
        if (playerInfoErr.IsError)
        {
            Debug.LogError(playerInfoErr.Message);
            return;
        }

        var assetAmount = playerInfo.Assets?.FirstOrDefault(a => a.AssetId == Index).Amount ?? 0;
        if (assetAmount == 0)
            return;

        var (_, txnParams) = await Algod.Client.GetSuggestedParams();
        var clawbackTxn = Transaction.AssetClawback(
            kp.PublicKey,
            txnParams,
            Index,
            assetAmount,
            Player.Address,
            kp.PublicKey
        ).Sign(kp.SecretKey);

        var (txnErr, txid) = await Algod.Client.SendTransaction(clawbackTxn);
        if (txnErr.IsError)
        {
            Debug.LogError(txnErr.Message);
            return;
        }
        var pending = await Algod.WaitForTransaction(txid);
        Debug.Log($"Clawed back asset {Index}");
    }
}
