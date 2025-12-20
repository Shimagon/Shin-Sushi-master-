using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightRecorder : MonoBehaviour
{
    [Header("Capture Settings")]
    [Tooltip("録画するカメラ（空欄ならMainCamera）")]
    public Camera recordingCamera;

    [Tooltip("1クリップの長さ（秒）")]
    public float clipDuration = 3.0f;

    [Tooltip("フレームレート（高いほど滑らかだが重い）")]
    [Range(1, 30)]
    public int recordingFps = 10;

    [Tooltip("保存する最大クリップ数")]
    public int maxClips = 5;

    [Tooltip("縮小率")]
    [Range(0.1f, 1f)]
    public float resolutionScale = 0.5f;

    private bool isRecordingBurst = false;
    private List<List<Texture2D>> capturedClips = new List<List<Texture2D>>();

    [Tooltip("録画間隔（秒）")]
    public float interval = 20f;
    private float timer = 0f;

    void Start()
    {
        if (recordingCamera == null)
            recordingCamera = Camera.main;

        if (ScoreManager.Instance != null)
        {
            // Reset storage
            ScoreManager.Instance.replayFrames.Clear();
        }

        // Start first recording immediately? or wait?
        // Let's record at 0, 20, 40...
        StartCoroutine(RecordBurst());
    }

    void OnDestroy()
    {
        // No event to unsubscribe
    }

    public void StopRecording()
    {
        StopAllCoroutines();
        // Updateでのタイマーも止めるためにフラグ管理が必要だが、
        // 簡易的にコンポーネントを無効化する
        this.enabled = false;
        isRecordingBurst = false;
    }

    void Update()
    {
        if (isRecordingBurst) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            StartCoroutine(RecordBurst());
        }
    }

    /* Deleted OnScoreChange */

    IEnumerator RecordBurst()
    {
        isRecordingBurst = true;
        List<Texture2D> currentClip = new List<Texture2D>();
        
        float interval = 1.0f / recordingFps;
        float elapsed = 0f;

        while (elapsed < clipDuration)
        {
            yield return new WaitForEndOfFrame();
            
            Texture2D frame = CaptureFrame();
            if (frame != null)
            {
                currentClip.Add(frame);
            }

            // 次のフレームまで待機（FPS制御）
            float frameStart = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - frameStart < interval)
            {
                yield return null;
            }
            elapsed += (Time.realtimeSinceStartup - frameStart);
        }

        // クリップ保存
        capturedClips.Add(currentClip);

        // 最大数を超えたら古いものを削除
        if (capturedClips.Count > maxClips)
        {
            // 古いテクスチャのメモリ解放が必要ならここで行う
            // UnityのGCに任せるか、Destroyするか。Texture2Dは明示的Destroy推奨。
            foreach (var tex in capturedClips[0]) Destroy(tex);
            capturedClips.RemoveAt(0);
        }

        // ScoreManagerへ送信（全クリップを結合して渡す）
        FlattenAndSendToManager();

        isRecordingBurst = false;
    }

    void FlattenAndSendToManager()
    {
        if (ScoreManager.Instance == null) return;

        ScoreManager.Instance.replayFrames.Clear();
        
        foreach (var clip in capturedClips)
        {
            ScoreManager.Instance.replayFrames.AddRange(clip);
            // クリップの間に「黒」などを挟むか、そのまま繋げるか
            // ここではそのまま繋げます
        }
    }

    Texture2D CaptureFrame()
    {
        if (recordingCamera == null) return null;

        int width = Screen.width;
        int height = Screen.height;
        int scaledWidth = Mathf.FloorToInt(width * resolutionScale);
        int scaledHeight = Mathf.FloorToInt(height * resolutionScale);

        RenderTexture rt = new RenderTexture(scaledWidth, scaledHeight, 24);
        RenderTexture currentRT = recordingCamera.targetTexture;

        recordingCamera.targetTexture = rt;
        recordingCamera.Render();
        recordingCamera.targetTexture = currentRT;

        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(scaledWidth, scaledHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, scaledWidth, scaledHeight), 0, 0);
        screenShot.Apply();
        RenderTexture.active = null;

        Destroy(rt);
        return screenShot;
    }
}
