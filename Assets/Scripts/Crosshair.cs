using UnityEngine;

public class Crosshair : MonoBehaviour
{
    // The texture for our crosshair.
    public Texture2D crosshairTexture;

    // ThisRect will contain the position and size of the crosshair.
    private Rect crosshairRect;

    void Start()
    {
        // Check if a texture has been assigned.
        if (crosshairTexture != null)
        {
            // Calculate the position of the crosshair.
            float crosshairSize = Screen.width * 0.05f; // 5% of the screen width.
            crosshairRect = new Rect(
                (Screen.width - crosshairSize) / 2,
                (Screen.height - crosshairSize) / 2,
                crosshairSize,
                crosshairSize
            );
        }
        else
        {
            Debug.LogWarning("Crosshair texture not set in the inspector.");
        }
    }

    void OnGUI()
    {
        // If the texture is assigned, draw it.
        if (crosshairTexture != null)
        {
            GUI.DrawTexture(crosshairRect, crosshairTexture);
        }
    }
}
