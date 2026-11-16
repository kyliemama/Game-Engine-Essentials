using UnityEngine;
using UnityEngine.UI;

// Bottom-left weapon frame. Shows the bare-hand sprite or the weapon sprite.
// 0 = bare hands, 1 = weapon (the same values the teammates use).
public class WeaponFrameUI : MonoBehaviour
{
    [SerializeField] Image weaponImage;     // The Image inside the frame.
    [SerializeField] Sprite bareHandSprite; // Shown for 0.
    [SerializeField] Sprite weaponSprite;   // Shown for 1.

    [Header("Testing")]
    [SerializeField] bool testWithKeys = true; // Press 1 for bare hands, 2 for the weapon.

    int currentWeapon = -1;

    void Start()
    {
        SetWeapon(0);
    }

    void Update()
    {
        if (!testWithKeys) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeapon(1);
    }

    // Call from the weapon switching code. 0 = bare hands, 1 = weapon.
    public void SetWeapon(int weapon)
    {
        if (weaponImage == null || weapon == currentWeapon) return;

        currentWeapon = weapon;
        weaponImage.sprite = weapon == 1 ? weaponSprite : bareHandSprite;
        weaponImage.enabled = weaponImage.sprite != null;
    }
}
