using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VidPlayer : MonoBehaviour
{
    [SerializeField] string Intro;
    private VideoPlayer videoPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;
        PlayVideo();
    }

    public void PlayVideo()
    {
        if (videoPlayer){
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, Intro);
            Debug.Log(videoPath);
            videoPlayer.url = videoPath;
            videoPlayer.Play();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("Game");
    }
}
