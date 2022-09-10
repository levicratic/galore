using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* -- FEATURES --
 * Mouse cursor is hidden during gameplay
 * Pause when ESC is pressed. Unpause when pressed again.
 *  - pausing/unpausing must show and hide the mouse cursor, respectively
 * Rapidly slow time to half-speed when space bar is pressed
 * Gradually speed time back up after 1.5ish seconds of slow (timeout condition)
 * If space bar is released, then rapidly bring it back to 1 (user stopped condition)
 * Have a slight cooldown before you can slow again -- not implemented, does it need it?
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

    private float timeScale // every time the time scale is changed
    {
        get { return Time.timeScale; }
        set {
            Time.timeScale = value > .999f ? 1 : value;
            Time.fixedDeltaTime = 0.02f * timeScale;
            lightDim.intensity = timeScale * timeScale;
            audioDim.pitch = timeScale;
            _inverse = 1; // apply change to all variables, then reset the inverse curve
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
        pause.started += TogglePause; // toggle

        slow.started += _ => slowing = true; //InvokeRepeating("Slow", 0, .02f);
        slow.canceled += _ => slowing = false;

        Cursor.visible = false;
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
            timeScale = slowMult * timeScale / (slowMult - .5f + timeScale);
            timeout = false;
        }
        else if (timeScale < .999f)
        {
            if (slowing || timeout)
            {
                timeScale = slowMult * 3 * timeScale / (slowMult * 3 - 1 + timeScale); // slowly return to normal
                slowing = false; timeout = true;
            }
            else
            {
                //timeScale = slowMult / 2 * timeScale / (slowMult / 2 - 1 + timeScale); // rapidly return to 1
                inverseScale = slowMult * inverseScale / (slowMult - .5f + inverseScale);
            }
        }

        // update variables to match scale
    }

    private void TogglePause(InputAction.CallbackContext obj)
    {
        if (!paused) // pause
        {
            timeScale = 0;
            spritePause.SetActive(true);
            audioSource.Pause();
            paused = true;
            Cursor.visible = true;
        }
        else // unpause
        {
            timeScale = 1;
            spritePause.SetActive(false);
            audioSource.UnPause();
            paused = false;
            Cursor.visible = false;
        }
    }

}
