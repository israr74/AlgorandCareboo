using UnityEngine;
using AlgoSdk;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "NewAlgod", menuName = "Algorand/Create Algod Client")]
public class Algod : ScriptableObject
{
    public AlgodClient Client;

    [ContextMenu(nameof(TestConnection))]
    public void TestConnection()
    {
        TestConnectionAsync().Forget();
    }

    async UniTaskVoid TestConnectionAsync()
    {
        var resp = await Client.GetHealth();
        if (resp.Error.IsError)
            Debug.LogError(resp.Error.Message);
        else
            Debug.Log($"Algod connection is {resp.Status}");
    }

    public async UniTask<PendingTransaction> WaitForTransaction(TransactionId txid)
    {
        var (error, pending) = await Client.GetPendingTransaction(txid);
        if (error.IsError)
        {
            Debug.LogError(error.Message);
            return pending;
        }

        var timeWaited = 0;
        var checkTime = 200;
        while (pending.ConfirmedRound == 0 && timeWaited < 10_000)
        {
            await UniTask.Delay(checkTime);
            (error, pending) = await Client.GetPendingTransaction(txid);
            if (error.IsError)
            {
                Debug.LogError(error.Message);
                return pending;
            }
            timeWaited += checkTime;
        }

        return pending;
    }
}
