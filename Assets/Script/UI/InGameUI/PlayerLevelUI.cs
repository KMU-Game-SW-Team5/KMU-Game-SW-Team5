using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLVUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textLevel;

    public void SetLV(int newLevel) { textLevel.text = $"Level. {newLevel}"; }
}
