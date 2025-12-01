using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform entryParent; // ScrollView의 Content
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private int maxEntries = 10;   // 10등까지

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void OnEnable()
    {
        Time.timeScale = 0f;
        InputBlocker.Block();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void OnDisable()
    {
        Time.timeScale = 1f;
        InputBlocker.Unblock();
    }

    public void OnClickReturnToMainMenu()
    {
        if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/MainUI.unity"))
            SceneManager.LoadScene("Assets/Scenes/MainUI.unity");
        else
            Debug.LogWarning("Assets/Scenes/MainUI.unity가 Build Settings에 등록되어 있지 않습니다");
    }

    public void RefreshLeaderboard(List<LeaderboardEntry> entries)
    {
        // 기존 행들 제거
        foreach (var entry in spawnedEntries)
        {
            Destroy(entry.gameObject);
        }
        spawnedEntries.Clear();

        int count = Mathf.Min(maxEntries, entries.Count);

        for (int i = 0; i < count; i++)
        {
            var data = entries[i];
            var entry = Instantiate(entryPrefab, entryParent);

            if (data.Id == LeaderboardService.Instance.LastSubmittedEntry.Id)
            {
                entry.GetComponent<EntryUI>().SetData(data, i+1, true);
            }
            else
            {
                entry.GetComponent<EntryUI>().SetData(data, i+1, false);
            }
            
            spawnedEntries.Add(entry);
        }
    }
}
