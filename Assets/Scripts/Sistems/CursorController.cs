using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] private bool lockCursorOnStart = true;

    private void Start()
    {
        if (lockCursorOnStart)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
