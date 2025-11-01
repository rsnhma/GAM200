using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotoPuzzleController : MonoBehaviour
{
    [Header("Puzzle Pieces")]
    [SerializeField] private Transform[] photoPieces; // 4 torn pieces

    [Header("UI Elements")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private TextMeshProUGUI bloodText;
    [SerializeField] private CanvasGroup puzzlePanelCanvasGroup;

    [Header("Memorabilia Settings")]
    [SerializeField] private string memorabiliaItemID = "torn_class_photo"; // Memorabilia ID to add

    [Header("Settings")]
    [SerializeField] private float panelFadeSpeed = 2f;
    [SerializeField] private float bloodTextDelay = 1f;
    [SerializeField] private float bloodTextFadeDuration = 2f;
    [SerializeField] private float panelCloseDelay = 4f;

    public static bool puzzleSolved = false;
    private bool isPuzzleActive = false;
    private string bloodMessage = "They said I took their key. But they took my future.";

    void Start()
    {
        // Initialize UI
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // Keep blood text inactive until puzzle is solved
        if (bloodText != null)
        {
            bloodText.gameObject.SetActive(false);
            bloodText.text = bloodMessage;
        }

        puzzleSolved = false;
    }

    void Update()
    {
        if (!puzzleSolved && isPuzzleActive)
        {
            CheckPuzzleCompletion();
        }
    }

    public void OpenPuzzle()
    {
        Debug.Log("OpenPuzzle called!");

        if (puzzlePanel != null)
        {
            Debug.Log("Activating puzzle panel...");
            puzzlePanel.SetActive(true);
            isPuzzleActive = true;

            // Set initial alpha to 0 if using CanvasGroup
            if (puzzlePanelCanvasGroup != null)
            {
                puzzlePanelCanvasGroup.alpha = 0f;
            }

            StartCoroutine(FadeInPanel());
        }
        else
        {
            Debug.LogError("Puzzle panel is NULL! Please assign it in the Inspector.");
        }
    }

    private void CheckPuzzleCompletion()
    {
        bool allCorrect = true;

        // Check if all pieces are correctly rotated (z rotation = 0)
        foreach (Transform piece in photoPieces)
        {
            if (piece != null)
            {
                // Normalize rotation to 0-360 range and check if it's at 0
                float zRotation = NormalizeAngle(piece.eulerAngles.z);
                if (Mathf.Abs(zRotation) > 0.1f) // Small tolerance for floating point
                {
                    allCorrect = false;
                    break;
                }
            }
        }

        if (allCorrect)
        {
            puzzleSolved = true;
            StartCoroutine(OnPuzzleComplete());
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    private IEnumerator OnPuzzleComplete()
    {
        // Disable piece interaction
        foreach (Transform piece in photoPieces)
        {
            var touchScript = piece.GetComponent<PhotoPieceRotate>();
            if (touchScript != null)
                touchScript.enabled = false;
        }

        // Wait before showing blood text
        yield return new WaitForSeconds(bloodTextDelay);

        // Show blood text with fade-in effect
        if (bloodText != null)
        {
            bloodText.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInBloodText());
        }

        // Wait before closing panel
        yield return new WaitForSeconds(panelCloseDelay);

        // Close puzzle panel
        yield return StartCoroutine(FadeOutPanel());

        // Complete the puzzle
        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        isPuzzleActive = false;

        // Add memorabilia to journal using JournalManager
        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.AddMemorabilia(memorabiliaItemID);
            Debug.Log($"Added {memorabiliaItemID} to memorabilia");
        }
    }

    private IEnumerator FadeInPanel()
    {
        if (puzzlePanelCanvasGroup != null)
        {
            puzzlePanelCanvasGroup.alpha = 0f;
            while (puzzlePanelCanvasGroup.alpha < 1f)
            {
                puzzlePanelCanvasGroup.alpha += Time.deltaTime * panelFadeSpeed;
                yield return null;
            }
        }
    }

    private IEnumerator FadeOutPanel()
    {
        if (puzzlePanelCanvasGroup != null)
        {
            while (puzzlePanelCanvasGroup.alpha > 0f)
            {
                puzzlePanelCanvasGroup.alpha -= Time.deltaTime * panelFadeSpeed;
                yield return null;
            }
        }

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
    }

    private IEnumerator FadeInBloodText()
    {
        if (bloodText != null)
        {
            Color textColor = bloodText.color;
            textColor.a = 0f;
            bloodText.color = textColor;

            float elapsed = 0f;
            while (elapsed < bloodTextFadeDuration)
            {
                elapsed += Time.deltaTime;
                textColor.a = Mathf.Lerp(0f, 1f, elapsed / bloodTextFadeDuration);
                bloodText.color = textColor;
                yield return null;
            }
        }
    }
}