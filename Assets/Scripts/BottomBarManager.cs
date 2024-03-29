using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomBarManager : MonoBehaviour
{
    public Button frontButton;
    public Button leftButton;
    public Button rightButton;
    public Transform objectToRotate;

    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private float rotationSpeed = 150.0f;

    private TextMeshProUGUI leftButtonText;
    private TextMeshProUGUI rightButtonText;
    private TextMeshProUGUI frontButtonText;

    private Color32 whiteColor = new Color32(255, 255, 255, 255);
    private Color32 greyColor = new Color32(100, 100, 100, 255);

    private Color cbBlue;
    private Color cbWhite;

    private void Awake()
    {
        initialRotation = objectToRotate.rotation;
        targetRotation = initialRotation;
        cbBlue = frontButton.GetComponent<Image>().color;
        cbWhite = leftButton.GetComponent<Image>().color;

        leftButtonText = leftButton.GetComponentInChildren<TextMeshProUGUI>();
        rightButtonText = rightButton.GetComponentInChildren<TextMeshProUGUI>();
        frontButtonText = frontButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        frontButton.onClick.AddListener(() => SetRotation(initialRotation, frontButton));
        leftButton.onClick.AddListener(() => RotateObject(-90, leftButton));
        rightButton.onClick.AddListener(() => RotateObject(90, rightButton));

        frontButton.Select();
    }

    private void Update()
    {
        if (isRotating)
        {
            objectToRotate.rotation = Quaternion.RotateTowards(objectToRotate.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(objectToRotate.rotation, targetRotation) < 0.01f)
            {
                objectToRotate.rotation = targetRotation; // Snap to the target rotation if close enough
                isRotating = false; // Stop rotating
            }
        }
    }

    private void RotateObject(float angle, Button button)
    {
        SetRotation(Quaternion.Euler(90, 0, objectToRotate.eulerAngles.z + angle), button);
    }

    private void SetRotation(Quaternion newRotation, Button button)
    {
        targetRotation = newRotation;
        isRotating = true;
        ChangeTextFromButton(button);
    }

    private void ChangeTextFromButton(Button button)
    {
        bool isLeft = button.gameObject.name.Contains("Left");
        bool isRight = button.gameObject.name.Contains("Right");
        bool isFront = button.gameObject.name.Contains("Front");

        leftButtonText.text = isLeft ? "< Left" : "<";
        rightButtonText.text = isRight ? "Right >" : ">";
        frontButtonText.text = isFront ? "· Front ·" : "·";

        leftButtonText.color = isLeft ? whiteColor : greyColor;
        rightButtonText.color = isRight ? whiteColor : greyColor;
        frontButtonText.color = isFront ? whiteColor : greyColor;
    }
}


