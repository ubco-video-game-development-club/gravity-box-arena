using UnityEngine;
using UnityEngine.Events;

public class ScoreSystem : MonoBehaviour
{
    public const string HIGH_SCORE_PREF = "player.highscore";

    [System.Serializable] public class OnScoreChangedEvent : UnityEvent<int> { }

    public int Score { get { return score; } }
    private int score;
    private int highScore;
    private bool achievedHighScore = false;

    [SerializeField] private OnScoreChangedEvent onScoreChanged = new OnScoreChangedEvent();
    [SerializeField] private UnityEvent onNewHighscore = new UnityEvent();

    void Awake()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_PREF);
    }

    public void AddScore(int amount) 
    {
        //This assumes we don't ever want to take away player score.
        //If we do, just change this accordingly.
        if(amount < 0) 
        {
            Debug.LogError("Cannot add negative score.");
            return;
        }

        score += amount;

        if(score > highScore)
        {
            if (!achievedHighScore) onNewHighscore.Invoke();
            
            achievedHighScore = true;
            PlayerPrefs.SetInt(HIGH_SCORE_PREF, score);
            highScore = score;
        }

        onScoreChanged.Invoke(score);
    }

    public void AddScoreChangedListener(UnityAction<int> call) 
    {
        onScoreChanged.AddListener(call);
    }

    public void RemoveScoreChangedListener(UnityAction<int> call) 
    {
        onScoreChanged.RemoveListener(call);
    }
}
