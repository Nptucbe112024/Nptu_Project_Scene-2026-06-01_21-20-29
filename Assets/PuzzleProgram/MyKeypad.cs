using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NavKeypad
{
    public class MyKeypad : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;

        public UnityEvent OnAccessGranted => onAccessGranted;
        public UnityEvent OnAccessDenied => onAccessDenied;

        [Header("Door")]
        [SerializeField] private Transform door;
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private float doorOpenSpeed = 2f;

        [Header("Keypad Display")]
        [SerializeField] private string accessGrantedText = "Granted";
        [SerializeField] private string accessDeniedText = "Denied";

        [Tooltip("錯誤訊息顯示秒數。")]
        [SerializeField] private float displayResultTime = 1.5f;

        [Tooltip("成功訊息顯示秒數。")]
        [SerializeField] private float correctMessageTime = 5f;

        [Header("Display Text Colors")]
        [SerializeField]
        private Color screenNormalColor =
            new Color(0.98f, 0.50f, 0.03f, 1f);

        [SerializeField]
        private Color screenDeniedColor =
            new Color(1f, 0f, 0f, 1f);

        [SerializeField]
        private Color screenGrantedColor =
            new Color(0f, 0.62f, 0.07f, 1f);

        [Header("Sound Effects")]
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;

        [Header("Keypad References")]
        [SerializeField] private TMP_Text keypadDisplayText;
        [SerializeField] private AudioSource audioSource;

        [Header("Hint UI")]
        [SerializeField] private GameObject keypadHintPanel;
        [SerializeField] private TMP_Text keypadHintText;

        [Header("Hint Distance")]
        [SerializeField] private Transform player;
        [SerializeField] private float hintShowDistance = 1.5f;

        private string currentInput = "";

        private bool passwordReady;
        private bool accessWasGranted;
        private bool forceShowResultMessage;
        private bool hintPermanentlyHidden;

        private Vector3 openDoorPosition;

        private const string NormalHint =
            "Enter the numbers matching the color blocks\n" +
            "Press Enter to submit";

        private void Awake()
        {
            passwordReady = false;
            accessWasGranted = false;
            forceShowResultMessage = false;
            hintPermanentlyHidden = false;

            ClearInput();
            HideHintCompletely();
            SetScreenColor(screenNormalColor);

            if (door != null)
            {
                openDoorPosition = door.position + openOffset;
            }
        }

        private void Update()
        {
            UpdateHintVisibility();
            OpenDoorMovement();
        }

        public void SetPasswordReady()
        {
            passwordReady = true;
        }

        public void AddInput(string input)
        {
            if (audioSource != null && buttonClickedSfx != null)
            {
                audioSource.PlayOneShot(buttonClickedSfx);
            }

            if (!passwordReady || accessWasGranted)
            {
                return;
            }

            switch (input.ToLower())
            {
                case "clear":
                    ClearInput();
                    break;

                case "enter":
                    // 密碼判斷交給 ColorPasswordDoor。
                    break;

                default:
                    currentInput += input;

                    if (keypadDisplayText != null)
                    {
                        keypadDisplayText.text = currentInput;
                    }

                    break;
            }
        }

        public void ShowErrorMessage()
        {
            if (!passwordReady || accessWasGranted)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(ShowErrorRoutine());
        }

        public void ShowCorrectMessage()
        {
            if (!passwordReady || accessWasGranted)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(ShowCorrectRoutine());
        }

        private IEnumerator ShowCorrectRoutine()
        {
            accessWasGranted = true;
            forceShowResultMessage = true;
            hintPermanentlyHidden = false;

            ShowHintMessage("CORRECT\nDoor unlocked");

            if (keypadDisplayText != null)
            {
                keypadDisplayText.text = accessGrantedText;
            }

            SetScreenColor(screenGrantedColor);

            onAccessGranted?.Invoke();

            if (audioSource != null && accessGrantedSfx != null)
            {
                audioSource.PlayOneShot(accessGrantedSfx);
            }

            yield return new WaitForSeconds(correctMessageTime);

            forceShowResultMessage = false;
            hintPermanentlyHidden = true;

            HideHintCompletely();
        }

        private IEnumerator ShowErrorRoutine()
        {
            forceShowResultMessage = true;

            ShowHintMessage("ERROR\nWrong password");

            if (keypadDisplayText != null)
            {
                keypadDisplayText.text = accessDeniedText;
            }

            SetScreenColor(screenDeniedColor);

            onAccessDenied?.Invoke();

            if (audioSource != null && accessDeniedSfx != null)
            {
                audioSource.PlayOneShot(accessDeniedSfx);
            }

            yield return new WaitForSeconds(displayResultTime);

            forceShowResultMessage = false;

            ClearInput();
            SetScreenColor(screenNormalColor);
        }

        private void ShowHintMessage(string message)
        {
            if (keypadHintPanel != null)
            {
                keypadHintPanel.SetActive(true);
            }

            if (keypadHintText != null)
            {
                keypadHintText.gameObject.SetActive(true);
                keypadHintText.text = message;
            }
        }

        private void UpdateHintVisibility()
        {
            if (keypadHintPanel == null || keypadHintText == null)
            {
                return;
            }

            if (hintPermanentlyHidden)
            {
                HideHintCompletely();
                return;
            }

            if (forceShowResultMessage)
            {
                return;
            }

            if (accessWasGranted)
            {
                HideHintCompletely();
                return;
            }

            bool shouldShowHint = false;

            if (passwordReady && player != null)
            {
                float distance = Vector3.Distance(
                    player.position,
                    transform.position
                );

                shouldShowHint = distance <= hintShowDistance;
            }

            keypadHintPanel.SetActive(shouldShowHint);
            keypadHintText.gameObject.SetActive(shouldShowHint);

            if (shouldShowHint)
            {
                keypadHintText.text = NormalHint;
            }
        }

        private void OpenDoorMovement()
        {
            if (!accessWasGranted || door == null)
            {
                return;
            }

            door.position = Vector3.Lerp(
                door.position,
                openDoorPosition,
                doorOpenSpeed * Time.deltaTime
            );
        }

        private void HideHintCompletely()
        {
            if (keypadHintPanel != null)
            {
                keypadHintPanel.SetActive(false);
            }

            if (keypadHintText != null)
            {
                keypadHintText.gameObject.SetActive(false);
            }
        }

        private void ClearInput()
        {
            currentInput = "";

            if (keypadDisplayText != null)
            {
                keypadDisplayText.text = "";
            }
        }

        private void SetScreenColor(Color color)
        {
            if (keypadDisplayText != null)
            {
                keypadDisplayText.color = color;
            }
        }
    }
}