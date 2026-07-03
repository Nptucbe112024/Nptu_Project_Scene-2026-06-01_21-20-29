using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NavKeypad;

public class ColorPasswordDoor : MonoBehaviour
{
    public enum LockColor
    {
        Red,
        Green,
        Blue
    }

    [Header("Prism Puzzle")]
    public PrismPuzzleManager puzzleManager;

    [Header("Color Slots")]
    public Renderer[] colorSlots;
    public Material redMaterial;
    public Material greenMaterial;
    public Material blueMaterial;

    [Header("Password")]
    [Range(1, 8)]
    public int passwordLength = 4;

    [Header("Door")]
    public Transform door;
    public Vector3 openOffset = new Vector3(0f, 3f, 0f);
    public float openSpeed = 2f;

    [Header("MyKeypad Connection")]
    [Tooltip("拖 KeypadStandard 上面的 MyKeypad 元件。")]
    public MyKeypad myKeypad;

    [Header("Optional UI")]
    [Tooltip("可留空。若有設定，會顯示目前輸入的數字。")]
    public TMP_Text inputText;

    [Tooltip("可留空。提示改由 MyKeypad 的 KeypadHintText 顯示。")]
    public TMP_Text messageText;

    private readonly List<LockColor> colorSequence =
        new List<LockColor>();

    private string correctPassword = "";
    private string playerInput = "";

    private bool passwordGenerated;
    private bool isDoorOpen;

    private Vector3 closedDoorPosition;
    private Vector3 openDoorPosition;

    private void Start()
    {
        if (door != null)
        {
            closedDoorPosition = door.position;
            openDoorPosition = closedDoorPosition + openOffset;
        }

        ClearScreen();
    }

    private void Update()
    {
        if (!passwordGenerated &&
            puzzleManager != null &&
            puzzleManager.isSolved)
        {
            GenerateColorPassword();
        }

        HandleKeyboardInput();
        OpenDoorMovement();
    }

    private void HandleKeyboardInput()
    {
        if (!passwordGenerated || isDoorOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) ||
            Input.GetKeyDown(KeyCode.Keypad0))
        {
            AddDigit("0");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            AddDigit("1");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            AddDigit("2");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            AddDigit("3");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Keypad4))
        {
            AddDigit("4");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Keypad5))
        {
            AddDigit("5");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Keypad6))
        {
            AddDigit("6");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7) ||
            Input.GetKeyDown(KeyCode.Keypad7))
        {
            AddDigit("7");
        }

        if (Input.GetKeyDown(KeyCode.Alpha8) ||
            Input.GetKeyDown(KeyCode.Keypad8))
        {
            AddDigit("8");
        }

        if (Input.GetKeyDown(KeyCode.Alpha9) ||
            Input.GetKeyDown(KeyCode.Keypad9))
        {
            AddDigit("9");
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ClearInput();
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitPassword();
        }
    }

    private void GenerateColorPassword()
    {
        if (puzzleManager == null ||
            puzzleManager.revealedCode.Length < 3)
        {
            return;
        }

        colorSequence.Clear();
        correctPassword = "";

        for (int i = 0; i < passwordLength; i++)
        {
            LockColor randomColor =
                (LockColor)Random.Range(0, 3);

            colorSequence.Add(randomColor);
            correctPassword += GetDigitFromColor(randomColor);
        }

        ShowColorSequence();

        passwordGenerated = true;

        if (myKeypad != null)
        {
            myKeypad.SetPasswordReady();
        }

        Debug.Log("Lock color password: " + correctPassword);
    }

    private string GetDigitFromColor(LockColor color)
    {
        switch (color)
        {
            case LockColor.Red:
                return puzzleManager.revealedCode[0].ToString();

            case LockColor.Green:
                return puzzleManager.revealedCode[1].ToString();

            case LockColor.Blue:
                return puzzleManager.revealedCode[2].ToString();

            default:
                return "";
        }
    }

    private void ShowColorSequence()
    {
        for (int i = 0; i < colorSlots.Length; i++)
        {
            if (colorSlots[i] == null)
            {
                continue;
            }

            if (i >= colorSequence.Count)
            {
                colorSlots[i].gameObject.SetActive(false);
                continue;
            }

            colorSlots[i].gameObject.SetActive(true);

            switch (colorSequence[i])
            {
                case LockColor.Red:
                    colorSlots[i].material = redMaterial;
                    break;

                case LockColor.Green:
                    colorSlots[i].material = greenMaterial;
                    break;

                case LockColor.Blue:
                    colorSlots[i].material = blueMaterial;
                    break;
            }
        }
    }

    public void AddDigit(string digit)
    {
        if (!passwordGenerated || isDoorOpen)
        {
            return;
        }

        if (playerInput.Length >= passwordLength)
        {
            return;
        }

        playerInput += digit;
        UpdateInputText();
    }

    public void ClearInput()
    {
        if (isDoorOpen)
        {
            return;
        }

        playerInput = "";
        UpdateInputText();
    }

    public void SubmitPassword()
    {
        if (!passwordGenerated || isDoorOpen)
        {
            return;
        }

        if (playerInput.Length != passwordLength)
        {
            ShowError();
            return;
        }

        if (playerInput == correctPassword)
        {
            isDoorOpen = true;

            if (myKeypad != null)
            {
                myKeypad.ShowCorrectMessage();
            }

            Debug.Log("Correct password. Door opened.");
            return;
        }

        ShowError();
    }

    private void ShowError()
    {
        if (myKeypad != null)
        {
            myKeypad.ShowErrorMessage();
        }

        playerInput = "";
        UpdateInputText();

        Debug.Log("Wrong password.");
    }

    private void OpenDoorMovement()
    {
        if (!isDoorOpen || door == null)
        {
            return;
        }

        door.position = Vector3.Lerp(
            door.position,
            openDoorPosition,
            openSpeed * Time.deltaTime
        );
    }

    private void UpdateInputText()
    {
        if (inputText != null)
        {
            inputText.text = playerInput;
        }
    }

    private void ClearScreen()
    {
        if (inputText != null)
        {
            inputText.text = "";
        }

        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}