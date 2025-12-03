using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI_Camera : MonoBehaviour
{
    [Header("마우스 기반 좌우 회전 설정")]
    [SerializeField, Tooltip("마우스 좌우 위치에 따라 카메라가 회전하는 최대 각도(도). 왼쪽은 -값, 오른쪽은 +값입니다.")]
    private float horizontalAngleRange = 8f;

    [SerializeField, Tooltip("마우스 입력에 대한 좌우 반응 속도. 값이 클수록 카메라가 더 빠르게 목표 각도로 이동합니다.")]
    private float horizontalSensitivity = 6f;

    [Header("마우스 기반 상하 회전 설정")]
    [SerializeField, Tooltip("마우스 세로 위치에 따라 카메라 X축(상하) 회전의 최대 각도(도). 위는 -값, 아래는 +값입니다.")]
    private float verticalAngleRange = 5f;

    [SerializeField, Tooltip("마우스 입력에 대한 상하 반응 속도. 값이 클수록 카메라가 더 빠르게 목표 각도로 이동합니다.")]
    private float verticalSensitivity = 6f;

    // 초기 회전값을 보존해서 마우스 입력은 초기 회전값을 기준으로 오프셋만 적용하게 함
    private float initialYaw;
    private float initialPitch;

    private void Start()
    {
        var e = transform.localEulerAngles;
        initialYaw = e.y;
        initialPitch = e.x;
    }

    private void Update()
    {
        // 화면 크기 안전 검사
        if (Screen.width <= 0 || Screen.height <= 0) return;

        // 마우스 X 위치를 -1..1 범위로 정규화 (중앙=0, 왼쪽=-1, 오른쪽=+1)
        float mx = Input.mousePosition.x;
        float normalizedX = (mx / Screen.width - 0.5f) * 2f;

        // 마우스 Y 위치를 -1..1 범위로 정규화 (중앙=0, 위=-1, 아래=+1)
        float my = Input.mousePosition.y;
        float normalizedY = (my / Screen.height - 0.5f) * 2f;
        // 위쪽이 -1로 나오도록 반전(툴팁에서 '위는 -값'으로 표현했기 때문)
        normalizedY = -normalizedY;

        // 목표 각도 계산 (초기값 + 오프셋)
        float targetYaw = initialYaw + normalizedX * horizontalAngleRange;
        float targetPitch = initialPitch + normalizedY * verticalAngleRange;

        // 부드럽게 보간
        float currentYaw = transform.localEulerAngles.y;
        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * Mathf.Max(0.0001f, horizontalSensitivity));

        float currentPitch = transform.localEulerAngles.x;
        float newPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime * Mathf.Max(0.0001f, verticalSensitivity));

        Vector3 e = transform.localEulerAngles;
        e.y = newYaw;
        e.x = newPitch;
        transform.localEulerAngles = e;
    }

    private void OnValidate()
    {
        // 인스펙터에서 잘못된 값 들어오는 것을 방지
        if (horizontalAngleRange < 0f) horizontalAngleRange = 0f;
        if (horizontalSensitivity < 0f) horizontalSensitivity = 0f;
        if (verticalAngleRange < 0f) verticalAngleRange = 0f;
        if (verticalSensitivity < 0f) verticalSensitivity = 0f;
    }
}
