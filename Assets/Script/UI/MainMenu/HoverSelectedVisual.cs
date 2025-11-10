using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class HoverSelectedVisual : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] GameObject selectedImage; // 선택(탭/토글처럼 유지)
    [SerializeField] GameObject hoverImage;    // 마우스 올렸을 때
    [SerializeField] private bool ignoreSelectedImage = false;
    [SerializeField] private TabSelectionGroup group;  // 그룹 연결

    Button btn;
    private bool isSelected;

    void Awake()
    {
        btn = GetComponent<Button>();
        group?.Register(this);

        if (hoverImage) hoverImage.SetActive(false);
        if (selectedImage) selectedImage.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e) { if (!isSelected && hoverImage) hoverImage.SetActive(true); }
    public void OnPointerExit(PointerEventData e) { if (hoverImage) hoverImage.SetActive(false); }

    public void OnPointerClick(PointerEventData e)
    {
        if (group != null && !ignoreSelectedImage)
        {
            group.OnButtonSelected(this);
        }
        else
        {
            // 그룹이 없는 경우
            SetSelected(true);
        }
    }

    // 외부에서 탭 선택 시 호출용
    public void SetSelected(bool on)
    {
        if (selectedImage && !ignoreSelectedImage) selectedImage.SetActive(on);
        if (hoverImage) hoverImage.SetActive(false);
    }
}
