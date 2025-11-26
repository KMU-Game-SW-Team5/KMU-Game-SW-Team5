using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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
