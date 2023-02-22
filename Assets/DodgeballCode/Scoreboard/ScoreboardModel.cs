using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime.Serialization;

[RealtimeModel]
public class ScoreboardModel
{
    [RealtimeProperty(1, true, true)]
    private string_scoreboardText;
};
