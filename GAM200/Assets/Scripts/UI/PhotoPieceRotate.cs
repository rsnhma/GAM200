using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PhotoPieceRotate : MonoBehaviour, IPointerClickHandler
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationAngle = 90f;

    [Header("Audio")]
    [SerializeField] private AudioClip rotateSound;

    private AudioSource audioSource;
    private Image image;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        image = GetComponent<Image>();

        // Make sure the image is raycast-able
        if (image != null)
        {
            image.raycastTarget = true;
        }
    }

    // For UI elements, use IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only rotate if puzzle isn't solved yet
        if (!PhotoPuzzleController.puzzleSolved)
        {
            RotatePiece();
        }
    }

    private void RotatePiece()
    {
        transform.Rotate(0f, 0f, rotationAngle);
        PlayRotateSound();
        Debug.Log($"Rotated {gameObject.name} to {transform.eulerAngles.z}");
    }

    private void PlayRotateSound()
    {
        if (audioSource != null && rotateSound != null)
        {
            audioSource.PlayOneShot(rotateSound);
        }
    }
}