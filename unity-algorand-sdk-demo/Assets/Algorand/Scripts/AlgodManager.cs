using System.Linq;
using AlgoSdk;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class AlgodManager : MonoBehaviour
{
    public string AlgodAddress = "http://localhost:4001";
    public string AlgodToken = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    public AlgorandAccount AssetCreator;

    public AlgorandAssetParams GameToken;

    public WeaponAsset[] Weapons;

    public UnityEvent OnAssetsSetup = new UnityEvent();

    public AlgodClient algod;

    public void Awake()
    {
        algod = new AlgodClient(AlgodAddress, AlgodToken);
        SetupAssets().Forget();
    }

    public async UniTaskVoid SetupAssets()
    {
        if (AssetCreator == null || Weapons == null || GameToken == null)
        {
            Debug.LogError("Invalid AlgodManager. Please set AssetCreator, Weapons, and GameToken.");
            return;
        }

        if (!GameToken.AssetParams.Manager.Equals(AssetCreator.Address))
        {
            Debug.LogError("Invalid AlgodManager. Please set AssetCreator Address to manager field.");
            return;
        }

        if (Weapons.Any(w => !w.assetParams.AssetParams.Manager.Equals(AssetCreator.Address)))
        {
            Debug.LogError("Invalid AlgodManager. Please set Weapons Managers to AssetCreator Address.");
            return;
        }

        using var k = AssetCreator.GetKeyPair();
        var (txnParamsErr, txnParams) = await algod.GetSuggestedParams();
        if (txnParamsErr.IsError) Debug.LogError(txnParamsErr.Message);

        ErrorResponse error = default;
        TransactionId txid = default;

        var (creatorInfoErr, creatorInfo) = await algod.GetAccountInformation(AssetCreator.Address);
        if (creatorInfoErr.IsError) Debug.LogError(creatorInfoErr.Message);
        if (creatorInfo.CreatedAssets == null || creatorInfo.CreatedAssets.Length == 0)
        {
            var createTxn = Transaction.AssetCreate(k.PublicKey, txnParams, GameToken).Sign(k.SecretKey);
            (error, txid) = await algod.SendTransaction(createTxn);
            if (error.IsError) Debug.LogError(error.Message);
            SetAssetIndex(GameToken, txid).Forget();
            foreach (var weapon in Weapons)
            {
                createTxn = Transaction.AssetCreate(
                    k.PublicKey,
                    txnParams,
                    weapon.assetParams
                ).Sign(k.SecretKey);
                (error, txid) = await algod.SendTransaction(createTxn);
                if (error.IsError) Debug.LogError(error.Message);
                SetAssetIndex(weapon.assetParams, txid).Forget();
            }
            await WaitForPendingTransactions();
        }

        (error, creatorInfo) = await algod.GetAccountInformation(AssetCreator.Address);
        if (error.IsError) Debug.LogError(error.Message);
        Debug.Log($"Created {creatorInfo.CreatedAssets?.Length ?? 0} assets");
        OnAssetsSetup?.Invoke();
    }

    async UniTask WaitForPendingTransactions()
    {
        var (_, pending) = await algod.GetPendingTransactionsByAccount(AssetCreator.Address);
        while (pending.TotalTransactions > 0)
        {
            await UniTask.Delay(100);
            (_, pending) = await algod.GetPendingTransactionsByAccount(AssetCreator.Address);
        }
    }

    async UniTaskVoid SetAssetIndex(AlgorandAssetParams assetParams, TransactionId txid)
    {
        var (error, pending) = await algod.GetPendingTransaction(txid);
        if (error.IsError)
        {
            Debug.LogError(error.Message);
            return;
        }

        while (pending.AssetIndex == 0)
        {
            await UniTask.Delay(100);
            (error, pending) = await algod.GetPendingTransaction(txid);
            if (error.IsError)
            {
                Debug.LogError(error.Message);
                return;
            }
        }

        assetParams.Index = pending.AssetIndex;
    }
}
