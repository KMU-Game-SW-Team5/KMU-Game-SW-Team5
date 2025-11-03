using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OptionsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // 실제 클릭 시 실행할 로직은 여기 연결
    public UnityEvent onClick;

    [SerializeField] private bool ignoreSelectedImage = false;

    // 마우스 올리면 보여질 이미지
    [SerializeField] private GameObject hoverImage;

    // 선택되면 밑에 깔릴 이미지
    [SerializeField] private GameObject selectedImage;

    [SerializeField] private OptionsButtonGroup group;

    private void Awake()
    {
        if (group != null)
        {
            group.Register(this);
        }

        if (selectedImage != null)
            selectedImage.SetActive(false);

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
        if (group != null && !ignoreSelectedImage)
        {
            group.OnButtonSelected(this);
        }
        else
        {
            // 그룹이 없는 경우
            SetSelectedImage(true);
        }

        if (hoverImage != null)
            hoverImage.SetActive(false);

        if (onClick != null)
            onClick.Invoke();
    }

    public void SetSelectedImage(bool on)
    {
        if (selectedImage != null && !ignoreSelectedImage)
            selectedImage.SetActive(on);
    }
}
