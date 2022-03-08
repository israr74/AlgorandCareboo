using AlgoSdk;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.FPS.Game;

public class ObjectiveRewards : MonoBehaviour
{
    [SerializeField]
    ulong rewardAmount = 1;

    [SerializeField]
    AlgorandAccount playerAccount;

    [SerializeField]
    AlgorandAccount assetCreator;

    [SerializeField]
    AlgorandAssetParams gameTokenAsset;

    [SerializeField]
    Algod algod;

    public void Awake()
    {
        Objective.OnObjectiveCompleted += OnObjectiveCompleted;
    }

    public void OnDestroy()
    {
        Objective.OnObjectiveCompleted -= OnObjectiveCompleted;
    }

    void OnObjectiveCompleted(Objective objective)
    {
        RewardToken();
    }

    public void RewardToken()
    {
        RewardTokenAsync().Forget();
    }

    async UniTaskVoid RewardTokenAsync()
    {
        Debug.Log($"Rewarding player {rewardAmount}...");
        await gameTokenAsset.EnsureOptedIn(playerAccount);
        using var kp = assetCreator.GetKeyPair();
        var (_, txnParams) = await algod.Client.GetSuggestedParams();
        var assetXferTxn = Transaction.AssetTransfer(
            kp.PublicKey,
            txnParams,
            gameTokenAsset.Index,
            rewardAmount,
            playerAccount.Address
        ).Sign(kp.SecretKey);

        var (txnErr, txid) = await algod.Client.SendTransaction(assetXferTxn);
        if (txnErr.IsError) Debug.LogError(txnErr.Message);
        var pendingTxn = await algod.WaitForTransaction(txid);
        Debug.Log($"Player received {rewardAmount} of {gameTokenAsset.name}");
    }
}
