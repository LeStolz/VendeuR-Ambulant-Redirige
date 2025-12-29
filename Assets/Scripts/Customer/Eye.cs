using UnityEngine;

public enum EyeEmotion
{
    Angry,
    Happy,
    Normal
}

public class Eye : MonoBehaviour
{
    [SerializeField] GameObject angryEyeVisual;
    [SerializeField] GameObject happyEyeVisual;
    [SerializeField] GameObject normalEyeVisual;

    public void SetEmotion(EyeEmotion emotion)
    {
        angryEyeVisual.SetActive(emotion == EyeEmotion.Angry);
        happyEyeVisual.SetActive(emotion == EyeEmotion.Happy);
        normalEyeVisual.SetActive(emotion == EyeEmotion.Normal);
    }
}
