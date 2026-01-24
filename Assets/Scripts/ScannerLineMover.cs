using UnityEngine;

public class ScannerLineMover : MonoBehaviour
{
    public GameObject basePlane;    // 拖入 BasePlane
    public float bpm = 120f;        // 默认 bpm
    private float startZ;
    private float endZ;
    private float scanX;
    private float scanY;
    private float speed;

    void Start()
    {
        Renderer renderer = basePlane.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        startZ = bounds.min.z;
        endZ = bounds.max.z;

        Vector3 pos = transform.position;
        scanX = pos.x;
        scanY = pos.y;

        transform.position = new Vector3(scanX, scanY, startZ);

        UpdateSpeedFromBPM();
    }

    void Update()
    {
        float z = transform.position.z + speed * Time.deltaTime;

        if (z > endZ)
        {
            z = startZ;
        }

        transform.position = new Vector3(scanX, scanY, z);
    }

    // set BMP
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        UpdateSpeedFromBPM();
        // Debug.Log("BPM set to: " + bpm + ", speed: " + speed);
    }

    // Calculate speed
    private void UpdateSpeedFromBPM()
    {
        float distance = endZ - startZ;

        float beatsPerSecond = bpm / 60f;
        float totalBeats = 4f * 4f; // 4 bars * 4 beats
        float duration = totalBeats / beatsPerSecond;

        speed = distance / duration;
    }
}