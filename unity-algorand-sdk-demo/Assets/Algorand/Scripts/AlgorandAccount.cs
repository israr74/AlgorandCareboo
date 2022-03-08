using AlgoSdk;
using AlgoSdk.Crypto;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAccount", menuName = "Algorand/Create Account")]
public class AlgorandAccount : ScriptableObject
{
    [SerializeField]
    string passPhrase;
    
    [SerializeField]
    Algod algod;

    PrivateKey privateKey => Mnemonic.FromString(passPhrase).ToPrivateKey();

    public Address Address => privateKey.ToPublicKey();

    public Ed25519.KeyPair GetKeyPair()
    {
        return privateKey.ToKeyPair();
    }

    [ContextMenu(nameof(Randomize))]
    public void Randomize()
    {
        var randomPrivateKey = AlgoSdk.Crypto.Random.Bytes<PrivateKey>();
        passPhrase = randomPrivateKey.ToMnemonic().ToString();
    }
    
    [ContextMenu(nameof(LogAccountInfo))]
    public void LogAccountInfo()
    {
        LogAccountInfoAsync().Forget();
    }
    
    async UniTaskVoid LogAccountInfoAsync()
    {
        Debug.Log($"My account address: {Address}");
        Debug.Log($"My account private key: {privateKey}");
        var (err, accountInfo) = await algod.Client.GetAccountInformation(Address);
        if (err.IsError) Debug.LogError(err.Message);
        Debug.Log($"My account amount: {accountInfo.Amount / 1_000_000f} algo");
        Debug.Log($"My account has {accountInfo.CreatedAssets?.Length ?? 0} created assets.");
    }
}
