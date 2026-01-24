using UnityEngine;
using TMPro;

public class KnobController : MonoBehaviour
{
    public Material knobMaterial;
    public string fillAmountParam = "_FillAmount";
    public float minFill = 0f;
    public float maxFill = 0.75f;
    public float sensitivity = 0.005f;

    private bool isDragging = false;
    private Vector2 lastMousePos;
    private float currentFill = 0.37f;

    public TextMeshProUGUI bpmText;
    public ScannerLineMover scannerLine;

    void Start()
    {
        currentFill = 0.375f;
        knobMaterial.SetFloat(fillAmountParam, currentFill);

        float bpm = Mathf.Lerp(60f, 180f, Mathf.InverseLerp(minFill, maxFill, currentFill));
        bpmText.text = Mathf.RoundToInt(bpm).ToString();
        scannerLine.SetBPM(bpm);
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

            float bpm = Mathf.Lerp(60f, 180f, Mathf.InverseLerp(minFill, maxFill, currentFill));

            scannerLine.SetBPM(bpm);
            bpmText.text = Mathf.RoundToInt(bpm).ToString();

            lastMousePos = currentMousePos;
        }
    }
}