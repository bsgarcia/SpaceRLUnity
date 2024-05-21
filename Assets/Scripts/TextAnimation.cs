using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimation : MonoBehaviour
{
    public Text rewardText;
    public float initialScore = 100f;
    public float decreaseRate = .1f; // Rate at which the score decreases per second

    private float currentScore;
    
    private bool isDone = true;

    void Start()
    {
        // reward text is text parent object
        rewardText = GetComponent<Text>();
        // output: the type or namespace name 'Text' could not be found
        // correct this
        // rewardText = GetComponent<TextMeshProUGUI>();
        // currentScore = initialScore;
        // UpdateScoreText();
        // StartCoroutine(DecreaseScoreOverTime());
    }

    public bool IsDone()
    {
        return isDone;
    }

    public void DecreaseScore(float factor, float reward, float decreaseRate)
    {
        StartCoroutine(DecreaseScoreOverTime(factor, reward, decreaseRate));
    }

    IEnumerator DecreaseScoreOverTime(float factor, float reward, float decreaseRate)
    {   
        isDone = false;
        currentScore = reward;
        initialScore = reward;
        yield return new WaitForSeconds(.5f); // Wait for 1 second before starting the decrease
        UpdateScoreText();
        while (currentScore > initialScore*factor)
        {
            currentScore -= decreaseRate; // * Time.deltaTime;
            UpdateScoreText();
            yield return new WaitForSeconds(.005f); // Wait for 1 second before updating the score again
        }
        isDone = true;
    }

    void UpdateScoreText()
    {
        // Calculate color interpolation based on score percentage
        float min = initialScore * 0.5f;
        float max = initialScore;
        float colorPercentage = Mathf.Clamp01((currentScore - min) / (max - min));
        Color textColor = Color.Lerp(Color.white, Color.red, 1f - colorPercentage);

        // Update text color
        rewardText.color = textColor;

        // Update text value (you can format it according to your needs)
        rewardText.text = "" + Mathf.Round(currentScore).ToString();

        // Visual feedback: Fading
        StartCoroutine(FadeText());
    }

    IEnumerator FadeText()
    {
        // Fade out the text
        rewardText.CrossFadeAlpha(0f, 1f, false);

        // Wait for the fade duration
        yield return new WaitForSeconds(.09f);

        // Fade in the text
        rewardText.CrossFadeAlpha(1f, 1f, false);
    }
}
