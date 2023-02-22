using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Normal.Realtime;

public class ScoreboardScript : RealtimeComponent
{
    private ScoreboardModel _model;
    public RealtimeAvatarManager _avatarManager;
    private Text _scoreboardText;

    private void Awake()
    {
        _scoreboardText = GetComponent<Text>();
    }

    {
        private ScoreboardModel model;
        {
            set
            {
                if (_model != null)
                {
                    
                    _model.ScoreboardTextDidChange -= ScoreBoardDidChange;
                }
                _model = value;

                if (_model != null)
                {
                    UpdateScoreboardText();

                    _model.ScoreboardTextDidChange += ScoreBoardDidChange;
                }
            }
        }
    }
    private void ScoreBoardDidChange(ScoreboardModel model, string value)
    {
        UpdateScoreboardText();
    }

    private void UpdateScoreboardText()
    {
        _scoreboardText.text = _model._scoreboardText;
    }
};
