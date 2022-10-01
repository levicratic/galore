using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/* -- FEATURES --
 * Mouse cursor is hidden during gameplay --> canceled
 * Pause when ESC is pressed. Unpause when pressed again.
 *  - pausing/unpausing must show and hide the mouse cursor, respectively
 * Rapidly slow time to half-speed when space bar is pressed
 * Gradually speed time back up after 1.5ish seconds of slow (timeout condition)
 * If space bar is released, then rapidly bring it back to 1 (user stopped condition)
 * Have a slight cooldown before you can slow again -- not implemented, does it need it?
 */

public class PauseTime : MonoBehaviour
{

    enum STATE { paused, slowing, timeout }

    private STATE game;

    [Header("Inputs - ESC / SPACE")]
    public InputAction PauseAction;
    public InputAction SlowAction;

    [Header("Multipliers")]
    public float slowFade = 6f;

    [Header("Time-based Objects")]
    public Light lightSource;
    public AudioSource musicPlayer;
    public Image pauseSprite;

    [Header("Pause Colors - can / can't slow")]
    public Color defaultColor = new Color(100/255, 1, 1, 0); // #75FFFF blue
    public Color deniedColor = new Color(1, 72/255, 64/255, 0); //#FF4840 red
    // denied color is not implemented yet
    // I'd have to have a new scale, I think

    private float _timeScale = 1f;
    private float timeScale // accessed every time the time scale is changed
    {
        get { return _timeScale; }
        set { _timeScale = value > .98f ? 1 : value; }
    }

    private bool paused = false;
    private bool slowing = false;
    private bool timeout = false;

    private void Start()
    {
        PauseAction.started += TogglePause;

        SlowAction.started += _ => slowing = true;
        SlowAction.canceled += _ => slowing = false;

        pauseSprite.color = defaultColor;
        //Cursor.visible = false;
    }

    private void OnEnable()
    {
        PauseAction.Enable();
        SlowAction.Enable();
    }
    private void OnDisable()
    {
        PauseAction.Disable();
        SlowAction.Disable();
    }

    
    void FixedUpdate() // SLOW DOWN feature
    {
        if (paused) return; // cannot slow down while paused

        // when space is pressed, start slowing down
        if (slowing && !timeout && Time.timeScale > .5001f)
        {
            timeScale = slowFade * timeScale / (slowFade - .5f + timeScale);
            UpdateTime();
        }
        else if (Time.timeScale < .98f) //
        {
            if (slowing || timeout) // slowly return to normal
            {
                timeScale = slowFade * 3 * timeScale / (slowFade * 3 - 1 + timeScale); // slowly return to normal
                slowing = false; timeout = true;
                UpdateTime();
            }
            else // unpausing will enter this block to speed things back to normal
            {
                timeScale = slowFade * timeScale / (slowFade - 1 + timeScale); // rapidly return to 1
                UpdateTime();
            }
        }
        else timeout = false; // because timeScale == 1 at this point
        //*/
    }

    private void UpdateTime()
    {
        Time.fixedDeltaTime = 0.02f * (Time.timeScale = timeScale);
        lightSource.intensity = Time.timeScale * Time.timeScale;
        musicPlayer.pitch = Time.timeScale;
        pauseSprite.color = defaultColor + new Color(0, 0, 0, (1 - timeScale) * (1 - timeScale));
    }

    private void TogglePause(InputAction.CallbackContext obj)
    {
        if (!paused) // pause
        {
            timeScale = 0;
            UpdateTime();
            musicPlayer.pitch = -0.2f;
            paused = true;
            //Cursor.visible = true;
        }
        else // unpause, resume with slight reduction in speed
        {
            timeScale = 0.96f;
            UpdateTime();
            timeout = true; // prevent slow down until scale returns to 1
            paused = false;
            //Cursor.visible = false;
        }
    }

}
