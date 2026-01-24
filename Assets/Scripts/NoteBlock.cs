using UnityEngine;

public class NoteBlock : MonoBehaviour
{
    public enum NoteType
    {
        Whole,      // 4拍
        Half,       // 2拍
        Quarter,    // 1拍
        Eighth,     // 0.5拍
        Sixteenth   // 0.25拍
    }

    public NoteType noteType = NoteType.Quarter;  // 默认是四分音符
    public float triggerDistance = 0.05f;
    public bool hasTriggered = false;
    public ScannerLineMover scanner;
    public GameObject basePlane;

    private float planeMinZ;
    private float planeMaxZ;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float scaleLerpTime = 0.2f;
    private float scaleTimer = 0f;
    private bool isScaling = false;

    public float earlyTriggerOffset = 0.01f;  // 可以调，比如 0.01 ~ 0.05

    public ParticleSystem noteParticles; 
    void Start()
    {
        originalScale = transform.localScale;

        if (basePlane != null)
        {
            Renderer renderer = basePlane.GetComponent<Renderer>();
            Bounds bounds = renderer.bounds;
            planeMinZ = bounds.min.z;
            planeMaxZ = bounds.max.z;
        }
    }

    void Update()
    {
        if (scanner == null || basePlane == null) return;

        float scannerZ = scanner.transform.position.z + earlyTriggerOffset;
        float noteZ = transform.position.z;

        if (noteZ < planeMinZ || noteZ > planeMaxZ) return;

        float distance = Mathf.Abs(noteZ - scannerZ);

        if (!hasTriggered && distance < triggerDistance)
        {
            hasTriggered = true;
            TriggerNote();
        }

        if (distance > triggerDistance * 2)
        {
            hasTriggered = false;
        }

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

    void TriggerNote()
    {
        targetScale = originalScale * 1.3f;  // 放大30%
        scaleTimer = 0f;
        isScaling = true;
        float bpm = scanner.bpm;
        float beatDuration = 60f / bpm; // 单拍（四分音符）的秒数
        float multiplier = GetNoteMultiplier(noteType);
        float noteDuration = beatDuration * multiplier;
        int durationMs = Mathf.RoundToInt(noteDuration * 1000);

        Debug.Log(noteType + " triggered! Duration (ms): " + durationMs);

        ESP32Client.Instance?.SendVibrationDuration(durationMs);

        if (noteParticles != null)
        {
            // 停止粒子系统
            noteParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // ✅ 再设置 duration / startLifetime
            var main = noteParticles.main;
            main.duration = noteDuration;
            main.startLifetime = noteDuration*2;

            // ✅ 最后再播放
            noteParticles.Play();
        }

        // + arduino
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