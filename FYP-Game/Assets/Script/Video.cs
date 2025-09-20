using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class PlayVideoOnSceneStart : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;      // 拖 VideoPlayer 组件
    public RawImage videoUI;             // 拖 RawImage UI
    public KeyCode skipKey = KeyCode.Space;

    void Start()
    {
        // 一开始隐藏 RawImage，避免白屏
        if (videoUI != null)
            videoUI.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            // 设置 VideoPlayer 渲染模式为 APIOnly
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.targetTexture = null;

            // 准备视频
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    // 视频准备好时回调
    void OnVideoPrepared(VideoPlayer vp)
    {
        if (videoUI != null)
        {
            videoUI.texture = vp.texture;    // 把视频帧赋给 RawImage
            videoUI.gameObject.SetActive(true); // 显示 UI
        }

        vp.Play();
        vp.loopPointReached += OnVideoEnd;   // 视频播放完回调
    }

    void Update()
    {
        // 按跳过键停止播放
        if (Input.GetKeyDown(skipKey))
        {
            StopVideo();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StopVideo();
    }

    void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        if (videoUI != null)
            videoUI.gameObject.SetActive(false);
    }
}
