using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultyUI : MonoBehaviour
{
    [Header("텍스트 컴포넌트 (인스펙터에 연결)")]
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("각 난이도 활성 색상 (인스펙터에서 설정)")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color hardColor = Color.red;

    private void OnEnable()
    {
        SettingsService.OnGameDifficultyChanged += ApplyDifficulty;
        // 초기 상태 적용
        ApplyDifficulty(SettingsService.GameDifficulty);
    }

    private void OnDisable()
    {
        SettingsService.OnGameDifficultyChanged -= ApplyDifficulty;
    }

    private void Start()
    {
        difficultyText = GetComponent<TextMeshProUGUI>();
        SettingsService.OnGameDifficultyChanged += ApplyDifficulty;
        // 초기 상태 적용
        ApplyDifficulty(SettingsService.GameDifficulty);
    }

    // 인덱스에 따라 표시 및 색상 적용
    // 0 = EASY, 1 = NORMAL, 2 = HARD
    private void ApplyDifficulty(int difficulty)
    {
        // 활성 색상 적용
        switch (difficulty)
        {
            case 0:
                difficultyText.text = "EASY";
                difficultyText.color = easyColor;
                break;
            case 1:
                difficultyText.text = "NORMAL";
                difficultyText.color = normalColor;
                break;
            case 2:
                difficultyText.text = "HARD";
                difficultyText.color = hardColor;
                break;
            default:
                Debug.LogWarning($"DifficultyUI: 알 수 없는 difficulty 값 {difficulty}");
                break;
        }
    }
}
