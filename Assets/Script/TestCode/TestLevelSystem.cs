using UnityEngine;

public class TestLevelSystem : MonoBehaviour
{
    [SerializeField] private PlayerLevelSystem levelSystem;
    [SerializeField] private InGameUIManager inGameUIManager;

    private void Update()
    {
        if (InputBlocker.IsInputBlocked)
            return;

        if (Input.GetKeyDown(KeyCode.U)) levelSystem.AddExp(100);
    }
}
