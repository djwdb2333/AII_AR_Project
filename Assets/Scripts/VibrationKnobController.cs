using UnityEngine;
using TMPro;

public class VibrationKnobController : MonoBehaviour
{
    public Material knobMaterial;
    public string fillAmountParam = "_FillAmount";
    public float minFill = 0f;
    public float maxFill = 0.75f;
    public float sensitivity = 0.005f;

    private bool isDragging = false;
    private Vector2 lastMousePos;
    private float currentFill = 0.37f;

    public TextMeshProUGUI vibrationText;

    public static VibrationKnobController Instance;  // 单例，供外部调用

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        knobMaterial.SetFloat(fillAmountParam, currentFill);
        UpdateVibrationText();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    lastMousePos = Input.mousePosition;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastMousePos;

            float deltaValue = (delta.x + delta.y) * sensitivity;
            currentFill = Mathf.Clamp(currentFill + deltaValue, minFill, maxFill);

            knobMaterial.SetFloat(fillAmountParam, currentFill);
            UpdateVibrationText();

            lastMousePos = currentMousePos;
        }
    }

    void UpdateVibrationText()
    {
        float strength = Mathf.InverseLerp(minFill, maxFill, currentFill);
        vibrationText.text = Mathf.RoundToInt(strength * 100f).ToString();

        // 映射到 0 - 255
        int strengthByte = Mathf.RoundToInt(Mathf.Lerp(100f, 255f, strength));

        // 发送 WebSocket 消息
        if (ESP32Client.Instance != null)
        {
            ESP32Client.Instance.SendVibrationStrength(strengthByte);
        }
    }

    public float GetVibrationStrength()
    {
        return Mathf.InverseLerp(minFill, maxFill, currentFill);
    }
}