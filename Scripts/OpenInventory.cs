using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenInventory : MonoBehaviour
{
    [SerializeField] GameObject inventory;
    //[SerializeField] KeyCode[] toggleInvKeys;
    [SerializeField] public StarterAssetsInputs starterAssetsInputs;
    [SerializeField] private ThirdPersonController Camera;
    //private ThirdPersonController Camera;
    //private _camera = GetComponent<ThirdPersonController>();
    void Update()
    {
        
        //check all keys that could be pressed to show inventory. I currently have Tab and i as inventory keys
        //for (int i = 0; i < toggleInvKeys.Length; i++)
        //{
            if (starterAssetsInputs.inventory)
            {
                inventory.gameObject.SetActive(!inventory.gameObject.activeSelf);

                if (inventory.gameObject.activeSelf)
                    ShowCursor();
                else
                    HideCursor();

                starterAssetsInputs.inventory = false;
            }
        //}
    }

    //funcs to hide/show cursor. Just set these up so that when in inventory, mouse will be visible and not locked.
    public void ShowCursor()
    {
        starterAssetsInputs.cursorLocked = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Camera.LockCameraPosition = true;
    }
    public void HideCursor()
    {
        starterAssetsInputs.cursorLocked = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Camera.LockCameraPosition = false;
/*        Camera = new ThirdPersonController();
        Camera.transform.GetChild(0).GetComponent<ThirdPersonController>().LockCameraPosition = false;*/
    }
}
