﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterSpawner : MonoBehaviour {

    #region Public Variables
    public GameObject letter;
    public LetterGroupHandler letterGroupHandler;
    public AudioHandler audioHandler;
    #endregion

    #region Private Variables
    // Wideness of the letter spawn area
    private float wideness = 2f;
    private GameObject targetObject;
    private bool isTargetDead = false;
    #endregion

    void Awake() {
        for (int i = 0; i< letter.transform.childCount ;i++) {
            if (letter.transform.GetChild(i).GetComponent<BoxCollider>() == null){
                BoxCollider bc = letter.transform.GetChild(i).gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(1.0f, 1.0f, 1.0f);
            }

            if (letter.transform.GetChild(i).GetComponent<BoxCollider>() == null){
                Rigidbody rb = letter.transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
                rb.mass = 1000;
                rb.drag = 0.5f;
            }

            letter.transform.GetChild(i).transform.localScale = new Vector3(0.4f,0.4f,0.4f);

            letter.transform.GetChild(i).gameObject.layer = 8;
        }

    }

    void Start () {
        StartCoroutine(spawner());
    }

    void Update()
    {
        if (targetObject != null && targetObject.GetComponent<KillTimer>().killable == false)
        {
            isTargetDead = true;
        }
    }

    private IEnumerator spawner(){
        // Take active Group List from the letter handler
        List<LetterGroup> activeGroups = letterGroupHandler.getActiveGroupList();
        if (activeGroups.Count == 0) {
            Debug.Log("There aren't any active groups");
            yield break;
        }
        // TODO Loop will have a time limit 
        while (true){
            Debug.Log("SpawnerLoop started.");
            //Selected group will spawn
            int selectedGroupIndex = UnityEngine.Random.Range(0, activeGroups.Count);

            // Gets a randomized version of the spawn group
            int[] randomizedArray = activeGroups[selectedGroupIndex].getRandomizedArray();
            // Selects the target letter randomly
            int targetIndex = UnityEngine.Random.Range(0, randomizedArray.Length);
            // Used for Round counting
            int roundCount = 0;
            // Resets variable
            isTargetDead = false;

            // Loop until the target object is dead
            while (!isTargetDead) { 
                // If the user can't hit the target object twice send it alone for the next rounds
                if (roundCount < 2){
                    // Spawning function
                    StartCoroutine(groupSpawner(randomizedArray, randomizedArray[targetIndex]));
                }
                else {
                    // Create an array that just has the target object 
                    int[] soloArray = {randomizedArray[targetIndex] };
                    StartCoroutine(groupSpawner(soloArray, randomizedArray[targetIndex]));
                }
                roundCount++;
                yield return new WaitForSeconds(10f);
            }
        }
    }

    // Takes the randomized version of the selected group and then spawns it with random timing and velocities
    IEnumerator groupSpawner(int[] randomizedArray, int target)
    {
        // Vocalize the targetted letter
        sayWhatToCut(target);
        yield return new WaitForSeconds(1.0f);

        GameObject throwable;
        // Throws all of the array
        for (int i = 0; i < randomizedArray.Length; i++)
        {
            // Instantiaye letter
            throwable = Instantiate(letter.transform.GetChild(randomizedArray[i]).gameObject);
            // Add related components for the letter
            addLetterScripts(ref throwable, randomizedArray[i]);
            // Add Event Triggers
            addLetterEvenTriggers(ref throwable);
            // Selected target is being marked as killable and with green color, will add points in case of death
            if (randomizedArray[i] == target)
            {
                targetObject = throwable;
                throwable.GetComponent<KillTimer>().killable = true;
                throwable.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                if (throwable.transform.childCount > 0)
                    throwable.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            }
            // Non-selected targets are being marked as not killable will decrease points in case of death
            else
            {
                throwable.GetComponent<KillTimer>().killable = false;
            }

            Rigidbody rb = throwable.GetComponent<Rigidbody>();
            // Create a random spawn point between -wideness and wideness values
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-wideness, wideness), 0.15f, 3f); ;
            throwable.transform.position = pos;
            // Randomize rotation
            throwable.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(170.0f, 190.0f), UnityEngine.Random.Range(-30.0f,30.0f));

            // This section creates random throwing variables. Prevents the objects being thrown away from the map
            if (pos.x < 0f)
                rb.velocity = new Vector3(UnityEngine.Random.Range(-pos.x - wideness, wideness), 2f, 0f);
            else
                rb.velocity = new Vector3(UnityEngine.Random.Range(-wideness, -pos.x + wideness), 2f, 0f);
            // Waits 1.5 seconds between two spawns
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.5f));
        }
    }

    // Add letter scripts
    private void addLetterScripts(ref GameObject temp, int index)
    {
        GameObject throwable = temp;
        // Script that destroys the object after leaving map
        throwable.AddComponent<TerminateIfExists>();
        // Script that counts gaze time and starts detonation 
        throwable.AddComponent<KillTimer>();
        // Script that involves Detonation settings 
        throwable.AddComponent<DetonationController>();
        // Kill timer will be disabled at the start because timer is at its Start function
        if (throwable.GetComponent<KillTimer>() != null)
            throwable.GetComponent<KillTimer>().enabled = false;

        // Gives the scriptless letter prefab to for detonation particles
        if (throwable.GetComponent<DetonationController>() != null){
            throwable.GetComponent<DetonationController>().pref = letter.transform.GetChild(index).gameObject;
        }

        temp = throwable;
    }
    
    //Adds event triggers
    private void addLetterEvenTriggers(ref GameObject temp) {
        GameObject throwable = temp;
        // Bounds an event trigger to the object
        EventTrigger trig = throwable.gameObject.AddComponent<EventTrigger>();
        // Creates an event entry
        EventTrigger.Entry entry_1 = new EventTrigger.Entry();
        // Sets the first event type
        entry_1.eventID = EventTriggerType.PointerEnter;
        if (throwable.GetComponent<KillTimer>() != null)
            entry_1.callback.AddListener((eventData) => { throwable.GetComponent<KillTimer>().enabled = true; });
        // Saves the first listener
        trig.triggers.Add(entry_1);

        // Sets the second event type
        EventTrigger.Entry entry_2 = new EventTrigger.Entry();
        entry_2.eventID = EventTriggerType.PointerExit;
        if (throwable.GetComponent<KillTimer>() != null)
            entry_2.callback.AddListener((eventData) => {
                throwable.GetComponent<KillTimer>().enabled = false;
                throwable.GetComponent<KillTimer>().resetTimer();
            });
        // Saves the second listener
        trig.triggers.Add(entry_2);

        temp = throwable;
    }

    //TODO vocalize what to cut then wait for 1 second
    private void sayWhatToCut(int indexToVocalize){
        if (audioHandler.GetComponent<AudioHandler>() == null) {
            Debug.Log("Audio Handler script is null!");
            return;
        }
         audioHandler.GetComponent<AudioHandler>().play(indexToVocalize); 
    }
}
