using UnityEngine;

public class MenuInitializer : MonoBehaviour
{
    private void Start()
    {
        // Start menu BGM when menu loads
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMenuBGM();
        }
    }
}