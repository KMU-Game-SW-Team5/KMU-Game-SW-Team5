using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class EnterNicknameUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _nickname;

    public void OnClickSubmitWrapper()
    {
        _ = OnClickSubmit();
    }

    private async Task OnClickSubmit()
    {
        InGameUIManager.Instance.HideEndingUI();
        InGameUIManager.Instance.HideEnterNicknamePanel();

        GameResult result = GameManager.Instance.GetGameResult(true);
        string nickname = _nickname.text;

        bool isSubmit = await LeaderboardService.Instance.SubmitResultAsync(result, nickname);

        if (isSubmit)
        {
            Debug.Log("전송에 성공");
            _ = InGameUIManager.Instance.ShowLeaderboardUI();
        }
        else
        {
            // 실패
            Debug.Log("전송에 실패");
        }
    }
}
