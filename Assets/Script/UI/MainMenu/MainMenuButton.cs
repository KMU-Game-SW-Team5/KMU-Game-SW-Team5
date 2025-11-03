using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // 실제 클릭 시 실행할 로직은 여기 연결
    public UnityEvent onClick;

    // 호버될 때 보여줄 이미지(밑에 깔리는 이미지)
    [SerializeField] private GameObject hoverImage;

    private void Awake()
    {
        if (hoverImage != null)
            hoverImage.SetActive(false);
    }

    // eventData: 마우스 클릭, 터치 입력에 대한 정보 객체
    // (어디 클릭했는지, 어떤 버튼 클릭했는지, 몇 번 했는지 등)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverImage != null)
            hoverImage.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
        {
            if (hoverImage != null)
                hoverImage.SetActive(false);

            onClick.Invoke();
        }
            
    }
}
