using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* -- FEATURES --
 * Pause when ESC is pressed. Unpause when pressed again.
 * Rapidly slow time to half-speed when space bar is pressed
 * Gradually speed time back up after 1.5-2 seconds of slow
 * If space bar is released, then super rapidly bring it back to 1
 * Have a slightly cooldown before you can slow again
 */

public class PauseTime : MonoBehaviour
{

    [Header("Inputs")]
    public InputAction pause;
    public InputAction slow;

    [Header("Pauseable Objects")]
    public GameObject spritePause;
    public AudioSource audioSource;

    [Header("Multipliers")]
    public float slowMult = 6f;

    private float timeScale
    {
        get { return Time.timeScale; }
        set {
            Time.timeScale = value > .999f ? 1 : value;
            Time.fixedDeltaTime = 0.02f * timeScale;

            lightDim.intensity = timeScale * timeScale;
            audioDim.pitch = timeScale;
            _inverse = 1;
        }
    }

    private float _inverse = 1f;
    private float inverseScale
    {
        get { return _inverse; }
        set {
            _inverse = value;
            Time.timeScale = 1.5f - _inverse;
            Time.fixedDeltaTime = 0.02f * timeScale;
            lightDim.intensity = timeScale * timeScale;
            audioDim.pitch = timeScale;
        }
    }

    [Header("Fade with Time")]
    public Light lightDim;
    public AudioSource audioDim;

    private bool paused = false;
    private bool slowing = false;
    private bool timeout = false;

    private void Start()
    {
        pause.started += Pause; // toggle

        slow.started += _ => slowing = true; //InvokeRepeating("Slow", 0, .02f);
        slow.canceled += _ => slowing = false;
    }

    private void OnEnable()
    {
        pause.Enable();
        slow.Enable();
    }
    private void OnDisable()
    {
        pause.Disable();
        slow.Disable();
    }

    
    void FixedUpdate() // SLOW DOWN feature
    {
        if (paused) return;

        if (slowing && timeScale > .5001f) // slow in, timeout at .5001
        {
            timeout = false;
            timeScale = slowMult * timeScale / (slowMult - .5f + timeScale); // fade to .5 fast
        }
        else if (timeScale < .999f)
        {
            if (slowing || timeout)
            {
                slowing = false; timeout = true;
                timeScale = slowMult * 3 * timeScale / (slowMult * 3 - 1 + timeScale); // fade to 1 slower
            }
            else
            {
                //timeScale = slowMult / 2 * timeScale / (slowMult / 2 - 1 + timeScale); // fade to 1 faster
                inverseScale = slowMult * inverseScale / (slowMult - .5f + inverseScale); // fade to 1 inverse
            }
        }

        // update variables to match scale
    }

    private void Pause(InputAction.CallbackContext obj)
    {
        if (!paused)
        {
            timeScale = 0;
            spritePause.SetActive(true);
            audioSource.Pause();
            paused = true;
        }
        else
        {
            timeScale = 1;
            spritePause.SetActive(false);
            audioSource.UnPause();
            paused = false;
        }
    }

}
