using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// This script is a simple setup for attaching behavior to gameobjects that are spawned using the tool (including Syncs and Backdrop prefabs)
public class StoryObject : MonoBehaviour
{


    public UnityEvent activationEvent;
    
    private void Start()
    {
        StoryTeller.RegisterStoryObject(this);
    }


    public void Activate()
    {
        activationEvent.Invoke();
    }
    
    
    private StoryTeller _storyTeller;
    private StoryTeller StoryTeller
    {
        get
        {
            if (_storyTeller == null)
                _storyTeller = FindObjectOfType<StoryTeller>();
            return _storyTeller;
        }
    } 
    
}
