using AlgoSdk;
using UnityEngine;
using UnityEngine.Events;

public class OwnedWeaponItem : MonoBehaviour
{
    public AssetParams asset;
    public UnityEvent<string> OnWeaponTextUpdate = new UnityEvent<string>();

    public void UpdateWeaponAmount(int amount)
    {
        var text = $"{asset.Name} x{amount}";
        OnWeaponTextUpdate.Invoke(text);
    }
}
