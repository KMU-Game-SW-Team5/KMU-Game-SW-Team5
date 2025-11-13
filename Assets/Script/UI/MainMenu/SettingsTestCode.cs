using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsTestCode : MonoBehaviour
{
    private void Awake()
    {
        SettingsService.OnGameDifficultyChanged += AfterDifficultyChanged;
        SettingsService.OnMasterVolumeChanged += AfterMasterVolumeChanged;
        SettingsService.OnTooltipsChanged += AfterTooltipsChanged;

        SettingsService.OnInvertMouseChanged += AfterInvertMouseChanged;
        SettingsService.OnMouseSensitivityChanged += AfterMouseSensitivityChanged;
        SettingsService.OnCameraSensitivityChanged += AfterCameraSensitivityChanged;

        SettingsService.OnFullScreenChanged += AfterFullScreenChanged;
        SettingsService.OnVSyncChanged += AfterVSyncChanged;
        SettingsService.OnBrightnessChanged += AfterBrightnessChanged;
        SettingsService.OnResolutionChanged += AfterResolutionChanged;
    }

    private void OnDestroy()
    {
        // 구독 해제 (중복 호출 방지)
        SettingsService.OnGameDifficultyChanged -= AfterDifficultyChanged;
        SettingsService.OnMasterVolumeChanged -= AfterMasterVolumeChanged;
        SettingsService.OnTooltipsChanged -= AfterTooltipsChanged;

        SettingsService.OnInvertMouseChanged -= AfterInvertMouseChanged;
        SettingsService.OnMouseSensitivityChanged -= AfterMouseSensitivityChanged;
        SettingsService.OnCameraSensitivityChanged -= AfterCameraSensitivityChanged;

        SettingsService.OnFullScreenChanged -= AfterFullScreenChanged;
        SettingsService.OnVSyncChanged -= AfterVSyncChanged;
        SettingsService.OnBrightnessChanged -= AfterBrightnessChanged;
        SettingsService.OnResolutionChanged -= AfterResolutionChanged;
    }


    private void AfterDifficultyChanged(int idx)
    {
        switch (idx)
        {
            case 0:
                Debug.Log("GameDifficulty: Easy");
                break;
            case 1:
                Debug.Log("GameDifficulty: Normal");
                break;
            case 2:
                Debug.Log("GameDifficulty: Hard");
                break;
            default: Debug.Log("GameDifficulty: Unknown"); 
                break;
        }
    }

    private void AfterMasterVolumeChanged(float v) { Debug.Log($"MasterVolume: {v}"); }
    private void AfterTooltipsChanged(bool on) { Debug.Log($"Tooltips: {on}"); }
    private void AfterInvertMouseChanged(bool on) { Debug.Log($"InvertMouse: {on}"); }
    private void AfterMouseSensitivityChanged(float v) { Debug.Log($"MouseSensitivity: {v}"); }
    private void AfterCameraSensitivityChanged(float v) { Debug.Log($"CameraSensitivity: {v}"); }
    private void AfterFullScreenChanged(bool on) { Debug.Log($"FullScreen: {on}"); }
    private void AfterVSyncChanged(bool on) { Debug.Log($"VSync: {on}"); }
    private void AfterBrightnessChanged(float v) { Debug.Log($"Brightness: {v}"); }

    private void AfterResolutionChanged(int w, int h, bool on)
    {
        Debug.Log($"Resolution: width-{w}, height-{h}, fullscreen-{on}");
    }
}
