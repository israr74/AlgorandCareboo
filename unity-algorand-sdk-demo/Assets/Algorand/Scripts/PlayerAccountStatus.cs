using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using System.Linq;

public class PlayerAccountStatus : MonoBehaviour
{
    public Algod algod;
    public AlgorandAccount playerAccount;
    public AlgorandAssetParams gameToken;
    public int pollIntervalMs;

    public UnityEvent<string> OnUpdateAlgoAmount = new UnityEvent<string>();

    public UnityEvent<string> OnUpdateGameTokenAmount = new UnityEvent<string>();


    bool shouldPoll;

    public void Awake()
    {
        shouldPoll = true;
        PollPlayerInfo().Forget();
    }

    public void OnDestroy()
    {
        shouldPoll = false;
    }

    async UniTaskVoid PollPlayerInfo()
    {
        while (shouldPoll)
        {
            await FetchPlayerInfo();
            await UniTask.Delay(pollIntervalMs);
        }
    }

    async UniTask FetchPlayerInfo()
    {
        var (err, info) = await algod.Client.GetAccountInformation(playerAccount.Address);
        if (err.IsError)
        {
            Debug.LogError(err.Message);
            shouldPoll = false;
            return;
        }

        UpdateAlgoAmount(info.Amount / 1_000_000f);
        var gameTokenAmount = info.Assets?.FirstOrDefault(a => a.AssetId == gameToken.Index).Amount ?? 0L;
        UpdateGameTokenAmount((int)gameTokenAmount);
    }

    void UpdateAlgoAmount(float val)
    {
        OnUpdateAlgoAmount.Invoke($"Algo:\n{val}");
    }

    void UpdateGameTokenAmount(int val)
    {
        OnUpdateGameTokenAmount.Invoke($"Shooty Coins:\n{val}");
    }
}
