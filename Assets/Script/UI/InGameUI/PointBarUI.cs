using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointBarUI : MonoBehaviour
{
    [SerializeField] private Image pointBarFill;
    private float maxPoint;
    private float currentPoint;

    public void SetPointUI(float newPoint, float newMax)
    {
        //Debug.Log($"newPoint: {newPoint}, newMax: {newMax}");
        maxPoint = Mathf.Max(0.0001f, newMax);
        currentPoint = Mathf.Clamp(newPoint, 0f, maxPoint);
        //Debug.Log($"maxPoint: {maxPoint}, currentPoint: {currentPoint}");
        UpdateFillBar();
    }

    private void UpdateFillBar()
    {
        //Debug.Log($"currentPoint: {currentPoint}, maxPoint: {maxPoint}");
        //Debug.Log($"currentPoint / maxPoint: {currentPoint / maxPoint}");
        pointBarFill.fillAmount = currentPoint / maxPoint;
    }
}
