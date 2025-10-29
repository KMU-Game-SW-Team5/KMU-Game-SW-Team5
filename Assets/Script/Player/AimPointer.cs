using UnityEngine;

public class AimPointer : MonoBehaviour
{
    [Tooltip("조준점의 색상")]
    public Color pointerColor = Color.white;

    [Tooltip("조준점의 크기 (픽셀 단위)")]
    public int pointerSize = 5; 

    private Texture2D pointerTexture;

    void Awake()
    {
        pointerTexture = CreateCircleTexture(pointerSize);
    }

    void OnGUI()
    {
        float x = Screen.width / 2f - pointerSize / 2f;
        float y = Screen.height / 2f - pointerSize / 2f;

        GUI.color = pointerColor;
        GUI.DrawTexture(new Rect(x, y, pointerSize, pointerSize), pointerTexture);
    }

    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2((float)size / 2, (float)size / 2);
        float radius = (float)size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);


                if (distance <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return texture;
    }
}