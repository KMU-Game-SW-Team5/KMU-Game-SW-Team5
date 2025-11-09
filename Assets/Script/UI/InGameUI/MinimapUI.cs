using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] private Image minimapContent;

    public void SetMinimap(Sprite newMinimap)
    {
        minimapContent.sprite = newMinimap;
    }
}
