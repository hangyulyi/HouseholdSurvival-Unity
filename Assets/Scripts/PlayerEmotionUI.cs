using UnityEngine;
using UnityEngine.UI;

public class PlayerEmotionUI : MonoBehaviour
{
    public Image emotionImage;

    public Sprite happySprite;
    public Sprite neutralSprite;
    public Sprite tiredSprite;
    public Sprite sickSprite;
    public Sprite excitedSprite;

    public void SetEmotion(PlayerEmotion emotion)
    {
        switch (emotion)
        {
            case PlayerEmotion.Happy:
                emotionImage.sprite = happySprite;
                break;

            case PlayerEmotion.Neutral:
                emotionImage.sprite = neutralSprite;
                break;

            case PlayerEmotion.Tired:
                emotionImage.sprite = tiredSprite;
                break;

            case PlayerEmotion.Sick:
                emotionImage.sprite = sickSprite;
                break;

            case PlayerEmotion.Excited:
                emotionImage.sprite = excitedSprite;
                break;
        }
    }

    private void Start()
    {
        SetEmotion(PlayerEmotion.Happy);
    }

}
