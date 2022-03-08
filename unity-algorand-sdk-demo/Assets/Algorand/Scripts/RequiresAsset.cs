using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

public class RequiresAsset : MonoBehaviour
{
    public Algod algod;
    public AlgorandAccount player;
    public AlgorandAssetParams requiredAsset;

    public void Awake()
    {
        CheckRequiredAsset().Forget();
    }

    async UniTaskVoid CheckRequiredAsset()
    {
        var (infoErr, info) = await algod.Client.GetAccountInformation(player.Address);
        if (infoErr.IsError) Debug.LogError(infoErr.Message);

        var assetAmount = info.Assets?.FirstOrDefault(a => a.AssetId == requiredAsset.Index).Amount ?? 0;
        gameObject.SetActive(assetAmount > 0);
    }
}
