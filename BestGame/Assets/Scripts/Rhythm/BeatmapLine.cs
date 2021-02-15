using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Command n sets next tick n beats ahead; command -1 stops the rhythm.

[RequireComponent(typeof(AudioSource))]
public class BeatmapLine : MonoBehaviour
{
    public delegate void TickAction();

    public event TickAction Tick;
    public event TickAction End;

    public const float NEGLIGIBLE_VOLUME = 0.01f;

    private string line;
    private List<float> commands;
    
    private string lineName;
    private AudioSource source;

    private float initialVolume;
    
    private bool isInRhythm;

    private float bpm;

    private float lastTick;
    private float nextTick;
    private int currentCommandIndex;

    [SerializeField] private AudioClip clip;
    [SerializeField] private TextAsset commandsFile;

    public string Line
    {
        get => line;
        set => line = value;
    }
    
    public string LineName
    {
        get => lineName;
        set => lineName = value;
    }

    public float Bpm
    {
        get => bpm;
        set => bpm = value;
    }

    public bool IsPlaying
    {
        get => source.volume < NEGLIGIBLE_VOLUME;
    }
    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip = clip;
        initialVolume = source.volume;
        source.Stop();
        source.playOnAwake = false;
        
        line = CommandsReader.Read(commandsFile);
        commands = LineStringToCommandList(Line);
    }

    private void OnEnable()
    {
        Tick += DebugTick;
    }

    private void FixedUpdate()
    {
        if(isInRhythm)
            CheckTick();
    }

    private static List<float> LineStringToCommandList(string s)
    {
        string[] commandListButStrings = s.Split(' ');
        List<float> toReturn = commandListButStrings.Select(float.Parse).ToList();
        return toReturn;
    }

    private void CheckTick()
    {
        if (source.time >= nextTick)
        {
            currentCommandIndex++;
            if (currentCommandIndex >= commands.Count || commands[currentCommandIndex] <= -1)
            {
                StopRhythm();
                End?.Invoke();
            }
            else
                SetNextTickTime();
            Tick?.Invoke();
        }
    }

    private void DebugTick()
    {
//        Debug.Log($"Tick! (at {source.time})");
    }

    private void SetNextTickTime()
    {
        lastTick = nextTick;
        nextTick += MusicUtility.BeatsToSeconds(commands[currentCommandIndex], bpm);
    }
    
    public void StartRhythm()
    {
        currentCommandIndex = 0;
        isInRhythm = true;
        lastTick = 0;
        SetNextTickTime();
        source.time = 0;
        source.Play();
    }

    public void PauseRhythm()
    {
        isInRhythm = false;
        source.Pause();
    }

    public void ResumeRhythm()
    {
        isInRhythm = true;
        source.Play();
    }

    public void StopRhythm()
    {
        isInRhythm = false;
        source.Stop();
        currentCommandIndex = 0;
    }
    
    public void DeactivateLine()
    {
        source.volume = 0;
    }

    public void ActivateLine()
    {
        source.volume = initialVolume;
    }
}
