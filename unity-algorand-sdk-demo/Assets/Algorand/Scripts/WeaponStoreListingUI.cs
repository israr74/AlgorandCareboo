using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WeaponStoreListingUI : MonoBehaviour
{
    public Button BuyButton;

    public WeaponStoreUI Store;

    public int WeaponStoreIndex;

    int weaponAmount;
    public int WeaponAmount
    {
        get => weaponAmount;
        set
        {
            weaponAmount = value;
            var text = $"{Store.weapons[WeaponStoreIndex].assetParams.name} x{value}";
            OnWeaponTextUpdate.Invoke(text);
        }
    }

    public UnityEvent<string> OnWeaponTextUpdate = new UnityEvent<string>();

    public void Awake()
    {
        Store.OnWeaponAmountUpdate.AddListener(OnWeaponAmountUpdate);
        Store.OnBought.AddListener(OnWeaponBought);
        Store.OnFinishBuy.AddListener(OnFinishBuy);
    }

    public void OnDestroy()
    {
        Store.OnWeaponAmountUpdate.RemoveListener(OnWeaponAmountUpdate);
        Store.OnBought.RemoveListener(OnWeaponBought);
        Store.OnFinishBuy.RemoveListener(OnFinishBuy);
    }

    public void OnClick()
    {
        BuyButton.enabled = false;
        Store.BuyWeapon(WeaponStoreIndex);
    }

    public void OnWeaponBought(int i)
    {
        if (WeaponStoreIndex == i)
            WeaponAmount--;
    }

    public void OnFinishBuy(int i)
    {
        BuyButton.enabled = true;
    }

    public void OnWeaponAmountUpdate(int[] amounts)
    {
        var amount = amounts[WeaponStoreIndex];
        WeaponAmount = amount;
    }
}
