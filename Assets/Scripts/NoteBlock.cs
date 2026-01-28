using UnityEngine;

public class NoteBlock : MonoBehaviour
{
    public enum NoteType
    {
        Whole,
        Half,
        Quarter,
        Eighth,
        Sixteenth
    }

    public NoteType noteType = NoteType.Quarter;
    public float triggerDistance = 0.05f;
    public ScannerLineMover scanner;
    public GameObject basePlane;

    private bool hasTriggered = false;
    private bool insideScannerPlane = false;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float scaleLerpTime = 0.2f;
    private float scaleTimer = 0f;
    private bool isScaling = false;

    public float earlyTriggerOffset = 0.01f;

    public ParticleSystem noteParticles;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (scanner == null || basePlane == null) return;

        // 门禁：不在 ScannerPlane 内直接退出
        if (!insideScannerPlane) return;

        float scannerZ = scanner.transform.position.z + earlyTriggerOffset;
        float noteZ = transform.position.z;

        float distance = Mathf.Abs(noteZ - scannerZ);

        if (!hasTriggered && distance < triggerDistance)
        {
            hasTriggered = true;
            TriggerNote();
        }

        // 防止抖动重复触发
        if (distance > triggerDistance * 2f)
        {
            hasTriggered = false;
        }

        // 缩放动画
        if (isScaling)
        {
            scaleTimer += Time.deltaTime;
            float t = scaleTimer / scaleLerpTime;

            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);

            if (t >= 1f)
            {
                transform.localScale = originalScale;
                isScaling = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == basePlane)
        {
            insideScannerPlane = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == basePlane)
        {
            insideScannerPlane = false;
        }
    }

    void TriggerNote()
    {
        targetScale = originalScale * 1.3f;
        scaleTimer = 0f;
        isScaling = true;

        float bpm = scanner.bpm;
        float beatDuration = 60f / bpm;
        float multiplier = GetNoteMultiplier(noteType);
        float noteDuration = beatDuration * multiplier;

        int durationMs = Mathf.RoundToInt(noteDuration * 1000f);

        Debug.Log(noteType + " triggered, duration(ms): " + durationMs);

        ESP32Client.Instance?.SendVibrationDuration(durationMs);

        if (noteParticles != null)
        {
            noteParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = noteParticles.main;
            main.duration = noteDuration;
            main.startLifetime = noteDuration * 2f;

            noteParticles.Play();
        }
    }

    float GetNoteMultiplier(NoteType type)
    {
        switch (type)
        {
            case NoteType.Whole: return 4f;
            case NoteType.Half: return 2f;
            case NoteType.Quarter: return 1f;
            case NoteType.Eighth: return 0.5f;
            case NoteType.Sixteenth: return 0.25f;
            default: return 1f;
        }
    }
}