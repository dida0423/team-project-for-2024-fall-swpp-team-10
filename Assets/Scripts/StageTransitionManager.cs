using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class StageTransitionManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Canvas transitionCanvas;
    [SerializeField] TextMeshProUGUI stageCleared;
    [SerializeField] TextMeshProUGUI countdownText;

    [Header("Character References")]
    private GameObject activeCharacter;

    [Header("Transition Settings")]
    [SerializeField] float cameraTransitionDuration = 2.0f;
    [SerializeField] float countdownDuration = 5.0f;
    [SerializeField] Animator transitionAnimator;  // Reference to the Animator component


    private bool isTransitioning = false;
    private Camera mainCamera;
    private Transform finalCameraAngle;

    void Awake()
    {
        mainCamera = Camera.main;

        // Hide UI elements initially
        transitionCanvas.gameObject.SetActive(false);
    }

    public void SetCurrentCharacter(GameObject _character)
    {
        activeCharacter = _character;
    }

    private bool GetFinalCameraAngle()
    {
        if (activeCharacter != null)
        {
            finalCameraAngle = activeCharacter.transform.Find("FinalCameraAngle");
            return true;
        }
        return false;
    }

    public IEnumerator StartStageTransition()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // Make sure time is not scaled
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        if (!GetFinalCameraAngle())
        {
            yield break;
        }

        // Store starting camera transform
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        float startTime = Time.time;

        // Move camera to final position
        while (Time.time - startTime < cameraTransitionDuration)
        {
            float t = (Time.time - startTime) / cameraTransitionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, finalCameraAngle.position, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, finalCameraAngle.rotation, smoothT);

            yield return null;
        }

        // Ensure final position
        mainCamera.transform.position = finalCameraAngle.position;
        mainCamera.transform.rotation = finalCameraAngle.rotation;

        // Now stop time for the rest of the transition
        Time.timeScale = 0f;

        // Smoothly show "Stage Cleared" text
        transitionCanvas.gameObject.SetActive(true);
        stageCleared.gameObject.SetActive(true);
        stageCleared.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            stageCleared.alpha = elapsed;
            yield return null;
        }

        // Start countdown
        countdownText.gameObject.SetActive(true);
        for (int i = (int)countdownDuration; i > 0; i--)
        {
            countdownText.text = $"Moving to Boss Stage in {i}...";
            yield return new WaitForSecondsRealtime(1f);
        }

        // Play animation - Uncompleted
        if (transitionAnimator != null)
        {
            // Play the animation
            transitionAnimator.Play("MainStageEnd");

            // Get animation length
            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName("MainStageEnd"))
            {
                yield return null;
                stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            }
        }

        // Reset time scale before scene change
        Time.timeScale = originalTimeScale;

        // Load boss scene
        GameManager.inst.LoadBossStage();
    }
}