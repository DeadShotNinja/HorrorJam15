using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEvent : MonoBehaviour
{
    public AK.Wwise.Event playMonsterIdleWwiseEvent;
    public AK.Wwise.Event playMonsterWalkWwiseEvent;
    public AK.Wwise.Event playMonsterRunWwiseEvent;

    /// <summary>
    /// Calls the Wwise event, if valid, to play audio
    /// </summary>
    /// <param name="caller">The game object calling the event</param>
    /// 
    
    public void PostMonsterIdle()
    {
        PostEvent(playMonsterIdleWwiseEvent);
    }
    public void PostMonsterWalk()
    {
        PostEvent(playMonsterWalkWwiseEvent);
    }
    public void PostMonsterRun()
    {
        PostEvent(playMonsterRunWwiseEvent);
    }

    private void PostEvent(AK.Wwise.Event wwiseEvent)
    {
        // Posts the wwiseEvent on the caller game object if the event is valid.
        if (wwiseEvent.IsValid())
        {
            wwiseEvent.Post(gameObject);
        }

        // Log warning for audio not yet created.
        else
        {
            Debug.LogWarning("Warning: missing audio for audio event: " + name);
        }
    }
}
