// UniOpener.cs
// Controls the animation for doors and windows.
// Project: VR school building
// Author: Matthias Hofherr
// Created: 2026

using System.Collections;
using UnityEngine;

public class UniOpener : MonoBehaviour
{
    // Prevents open and tilt animations from running at the same time.
    private enum ActiveMode
    {
        None,
        Open,
        Tilt
    }

    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform objectToAnimate;
    [SerializeField] private Transform objectToAnimate2;
    [SerializeField] private Transform handleToAnimate;

    [Header("State/Test")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isTilted = false;
    [SerializeField] private bool simulateTriggerOccupied = false;

    [Header("Auto Close")]
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseSpeed = 25f;
    [SerializeField] private float autoCloseDelay = 0.5f;
    [SerializeField] private bool reopenOnTriggerBlocked = true;

    [Header("Rotation Animation")]
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animSpeed = 90f;
    [SerializeField] private Vector3 localRotationAxis = Vector3.up;

    [Header("Tilting Animation")]
    [SerializeField] private bool enableTilting = false;
    [SerializeField] private float tiltClosedAngle = 0f;
    [SerializeField] private float tiltOpenAngle = 15f;
    [SerializeField] private float tiltAnimSpeed = 90f;
    [SerializeField] private Vector3 localTiltRotationAxis = Vector3.right;

    [Header("Tilt Handle Settings")]
    [SerializeField] private float tiltHandleAngle = -90f;
    [SerializeField] private Vector3 tiltHandleRotationAxis = Vector3.forward;

    [Header("Sliding Animation")]
    [SerializeField] private bool enableSliding = false;
    [SerializeField] private float slideDistance = 1f;
    [SerializeField] private float slideSpeed = 1f;
    [SerializeField] private Vector3 localSlideAxis = Vector3.right;

    [Header("Handle Animation")]
    [SerializeField] private bool enableHandleAnimation = true;
    [SerializeField] private bool keepHandlePosistion = false;
    [SerializeField] private float handleDownAngle = 25f;
    [SerializeField] private float handleAnimationTime = 0.35f;
    [SerializeField] private Vector3 handleRotationAxis = Vector3.forward;

    [Header("Audiofiles")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private bool loopOpenSound = false;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private bool loopCloseSound = false;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Close Sound Timing")]
    [SerializeField] private float closeSoundDelay = 0.1f;

    private Coroutine mainRoutine;
    private Coroutine handleRoutine;
    private Coroutine autoCloseRoutine;

    private bool lastIsOpen;
    private bool lastIsTilted;

    private int triggerObjectsInside = 0;
    private bool autoCloseInProgress = false;

    private ActiveMode activeMode = ActiveMode.None;

    private Vector3 closedPosition;
    private Vector3 closedPosition2;
    private Quaternion handleStartRotation;

    private void Awake()
    {
        // Use this transform as a fallback when no target object is assigned.
        if (objectToAnimate == null)
        {
            objectToAnimate = transform;
        }

        // Keep the initial state valid when tilting is disabled.
        if (!enableTilting)
        {
            isTilted = false;
        }

        // Open and tilted are mutually exclusive states.
        if (isOpen && isTilted)
        {
            isTilted = false;
        }

        if (isOpen)
        {
            activeMode = ActiveMode.Open;
        }
        else if (isTilted)
        {
            activeMode = ActiveMode.Tilt;
        }
        else
        {
            activeMode = ActiveMode.None;
        }

        // Store closed positions for sliding animations and reset checks.
        closedPosition = objectToAnimate.localPosition;

        if (objectToAnimate2 != null)
        {
            closedPosition2 = objectToAnimate2.localPosition;
        }

        if (handleToAnimate != null)
        {
            handleStartRotation = handleToAnimate.localRotation;
        }

        SetupAudio();

        lastIsOpen = isOpen;
        lastIsTilted = isTilted;

        // Apply the configured start state without playing an animation.
        ApplyInstantState();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!enableTilting && isTilted)
        {
            isTilted = false;
            lastIsTilted = false;
            return;
        }

        // Route Inspector state changes through the regular animation logic.
        if (isOpen != lastIsOpen)
        {
            bool requestedOpenState = isOpen;
            isOpen = lastIsOpen;

            SetOpen(requestedOpenState);
            return;
        }

        if (isTilted != lastIsTilted)
        {
            bool requestedTiltState = isTilted;
            isTilted = lastIsTilted;

            SetTilted(requestedTiltState);
            return;
        }

        ReopenIfBlockedWhileNotClosed();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoClose)
        {
            return;
        }

        triggerObjectsInside++;

        ReopenIfBlockedWhileNotClosed();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!autoClose)
        {
            return;
        }

        triggerObjectsInside = Mathf.Max(0, triggerObjectsInside - 1);
    }

    private void ReopenIfBlockedWhileNotClosed()
    {
        // Optional safety behavior: reopen only while the door is not fully closed.
        if (!autoClose || !reopenOnTriggerBlocked || !IsAutoCloseBlocked())
        {
            return;
        }

        if (activeMode == ActiveMode.Tilt || isOpen)
        {
            return;
        }

        if (IsAtClosedState())
        {
            return;
        }

        autoCloseInProgress = false;
        SetOpen(true);
    }

    public void Toggle()
    {
        ToggleOpen();
    }

    public void ToggleOpen()
    {
        if (activeMode == ActiveMode.Tilt)
        {
            return;
        }

        SetOpen(!isOpen);
    }

    public void ToggleTilt()
    {
        if (!enableTilting || activeMode == ActiveMode.Open)
        {
            return;
        }

        SetTilted(!isTilted);
    }

    public void Open()
    {
        SetOpen(true);
    }

    public void Close()
    {
        SetOpen(false);
    }

    public void Tilt()
    {
        SetTilted(true);
    }

    public void CloseTilt()
    {
        SetTilted(false);
    }

    public void SetOpen(bool open)
    {
        if (activeMode == ActiveMode.Tilt)
        {
            return;
        }

        if (open && isTilted)
        {
            return;
        }

        if (isOpen == open)
        {
            return;
        }

        activeMode = ActiveMode.Open;

        isOpen = open;

        if (isOpen)
        {
            isTilted = false;
        }

        lastIsOpen = isOpen;
        lastIsTilted = isTilted;

        AnimateOpenState();
    }

    public void SetTilted(bool tilted)
    {
        if (!enableTilting)
        {
            return;
        }

        if (activeMode == ActiveMode.Open)
        {
            return;
        }

        if (tilted && isOpen)
        {
            return;
        }

        if (isTilted == tilted)
        {
            return;
        }

        activeMode = ActiveMode.Tilt;

        isTilted = tilted;

        if (isTilted)
        {
            isOpen = false;
        }

        lastIsOpen = isOpen;
        lastIsTilted = isTilted;

        AnimateTiltState();
    }

    private void AnimateOpenState()
    {
        if (objectToAnimate == null)
        {
            return;
        }

        StopRunningRoutines();

        mainRoutine = StartCoroutine(AnimateOpenSequence(isOpen));
    }

    private void AnimateTiltState()
    {
        if (objectToAnimate == null)
        {
            Debug.LogWarning("No object to animate assigned.", this);
            return;
        }

        StopRunningRoutines();

        mainRoutine = StartCoroutine(AnimateTiltSequence(isTilted));
    }

    private void StopRunningRoutines()
    {
        // Prevent overlapping animations, delayed auto-close checks, and looped audio.
        if (mainRoutine != null)
        {
            StopCoroutine(mainRoutine);
            mainRoutine = null;
        }

        if (handleRoutine != null)
        {
            StopCoroutine(handleRoutine);
            handleRoutine = null;
        }

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }

        StopLoopSound();
    }

    private void ApplyInstantState()
    {
        if (objectToAnimate == null)
        {
            return;
        }

        if (enableTilting && isTilted)
        {
            objectToAnimate.localRotation = GetTargetTiltRotation(true);
        }
        else if (enableSliding)
        {
            objectToAnimate.localPosition = GetTargetSlidePosition(isOpen);

            if (objectToAnimate2 != null)
            {
                objectToAnimate2.localPosition = GetTargetSlidePosition2(isOpen);
            }
        }
        else
        {
            objectToAnimate.localRotation = GetTargetRotation(isOpen);
        }

        if (handleToAnimate != null)
        {
            if (keepHandlePosistion && isOpen)
            {
                handleToAnimate.localRotation = GetHandleDownRotation(false);
            }
            else if (enableTilting && keepHandlePosistion && isTilted)
            {
                handleToAnimate.localRotation = GetHandleDownRotation(true);
            }
            else
            {
                handleToAnimate.localRotation = handleStartRotation;
            }
        }
    }

    private IEnumerator AnimateOpenSequence(bool opening)
    {
        if (opening)
        {
            PlaySound(openSound, loopOpenSound);

            if (enableHandleAnimation && handleToAnimate != null)
            {
                if (keepHandlePosistion)
                {
                    yield return AnimateHandleDown(false);
                }
                else
                {
                    handleRoutine = StartCoroutine(AnimateHandleDownAndUp(false));

                    float delayBeforeOpening = Mathf.Max(handleAnimationTime * 0.5f, 0.01f);
                    yield return new WaitForSeconds(delayBeforeOpening);
                }
            }

            if (enableSliding)
            {
                yield return SlideTo(true);
            }
            else
            {
                yield return RotateTo(true);
            }

            // Stop opening audio exactly when the target open state is reached.
            StopOpenSound();

            mainRoutine = null;
            StartAutoCloseCheck();
            yield break;
        }
        else
        {
            if (enableSliding)
            {
                yield return SlideTo(false);
            }
            else
            {
                yield return RotateTo(false);
            }

            if (enableHandleAnimation && handleToAnimate != null && keepHandlePosistion)
            {
                yield return AnimateHandleUp();
            }

            StopLoopSound();
            activeMode = ActiveMode.None;
            autoCloseInProgress = false;
        }

        mainRoutine = null;
    }

    private IEnumerator AnimateTiltSequence(bool tilting)
    {
        if (tilting)
        {
            PlaySound(openSound, loopOpenSound);

            if (enableHandleAnimation && handleToAnimate != null)
            {
                if (keepHandlePosistion)
                {
                    yield return AnimateHandleDown(true);
                }
                else
                {
                    handleRoutine = StartCoroutine(AnimateHandleDownAndUp(true));

                    float delayBeforeOpening = Mathf.Max(handleAnimationTime * 0.5f, 0.01f);
                    yield return new WaitForSeconds(delayBeforeOpening);
                }
            }

            yield return TiltDoorTo(true);

            StopOpenSound();
        }
        else
        {
            yield return TiltDoorTo(false);

            if (enableHandleAnimation && handleToAnimate != null && keepHandlePosistion)
            {
                yield return AnimateHandleUp();
            }

            StopLoopSound();
            activeMode = ActiveMode.None;
        }

        mainRoutine = null;
    }

    private IEnumerator RotateTo(bool opening)
    {
        Quaternion targetRotation = GetTargetRotation(opening);

        bool closeSoundPlayed = false;

        float rotationSpeed = animSpeed;

        // Use the dedicated auto-close speed only for automatic closing.
        if (!opening && autoClose && autoCloseInProgress)
        {
            rotationSpeed = Mathf.Max(autoCloseSpeed, 0.01f);
        }

        while (Quaternion.Angle(objectToAnimate.localRotation, targetRotation) > 0.1f)
        {
            if (!opening && !closeSoundPlayed)
            {
                float remainingAngle = Quaternion.Angle(objectToAnimate.localRotation, targetRotation);
                float remainingTime = remainingAngle / Mathf.Max(rotationSpeed, 0.01f);

                if (remainingTime <= closeSoundDelay)
                {
                    PlaySound(closeSound, loopCloseSound);
                    closeSoundPlayed = true;
                }
            }

            objectToAnimate.localRotation = Quaternion.RotateTowards(
                objectToAnimate.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        objectToAnimate.localRotation = targetRotation;

        if (!opening && !closeSoundPlayed)
        {
            PlaySound(closeSound, loopCloseSound);
        }
    }

    private IEnumerator TiltDoorTo(bool opening)
    {
        Quaternion targetRotation = GetTargetTiltRotation(opening);

        bool closeSoundPlayed = false;

        while (Quaternion.Angle(objectToAnimate.localRotation, targetRotation) > 0.1f)
        {
            if (!opening && !closeSoundPlayed)
            {
                float remainingAngle = Quaternion.Angle(objectToAnimate.localRotation, targetRotation);
                float remainingTime = remainingAngle / Mathf.Max(tiltAnimSpeed, 0.01f);

                if (remainingTime <= closeSoundDelay)
                {
                    PlaySound(closeSound, loopCloseSound);
                    closeSoundPlayed = true;
                }
            }

            objectToAnimate.localRotation = Quaternion.RotateTowards(
                objectToAnimate.localRotation,
                targetRotation,
                tiltAnimSpeed * Time.deltaTime
            );

            yield return null;
        }

        objectToAnimate.localRotation = targetRotation;

        if (!opening && !closeSoundPlayed)
        {
            PlaySound(closeSound, loopCloseSound);
        }
    }

    private IEnumerator SlideTo(bool opening)
    {
        Vector3 targetPosition = GetTargetSlidePosition(opening);
        Vector3 targetPosition2 = GetTargetSlidePosition2(opening);

        bool closeSoundPlayed = false;

        float currentSlideSpeed = slideSpeed;

        // For sliding doors/windows, auto-close speed uses the same unit as slideSpeed.
        if (!opening && autoClose && autoCloseInProgress)
        {
            currentSlideSpeed = Mathf.Max(autoCloseSpeed, 0.01f);
        }

        while (
            Vector3.Distance(objectToAnimate.localPosition, targetPosition) > 0.001f ||
            (objectToAnimate2 != null && Vector3.Distance(objectToAnimate2.localPosition, targetPosition2) > 0.001f)
        )
        {
            if (!opening && !closeSoundPlayed)
            {
                float remainingDistance = Vector3.Distance(objectToAnimate.localPosition, targetPosition);
                float remainingTime = remainingDistance / Mathf.Max(currentSlideSpeed, 0.01f);

                if (remainingTime <= closeSoundDelay)
                {
                    PlaySound(closeSound, loopCloseSound);
                    closeSoundPlayed = true;
                }
            }

            objectToAnimate.localPosition = Vector3.MoveTowards(
                objectToAnimate.localPosition,
                targetPosition,
                currentSlideSpeed * Time.deltaTime
            );

            if (objectToAnimate2 != null)
            {
                objectToAnimate2.localPosition = Vector3.MoveTowards(
                    objectToAnimate2.localPosition,
                    targetPosition2,
                    currentSlideSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        objectToAnimate.localPosition = targetPosition;

        if (objectToAnimate2 != null)
        {
            objectToAnimate2.localPosition = targetPosition2;
        }

        if (!opening && !closeSoundPlayed)
        {
            PlaySound(closeSound, loopCloseSound);
        }
    }

    private void StartAutoCloseCheck()
    {
        if (!autoClose || !isOpen)
        {
            return;
        }

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
        }

        autoCloseRoutine = StartCoroutine(AutoCloseWhenPossible());
    }

    private IEnumerator AutoCloseWhenPossible()
    {
        float delay = Mathf.Max(autoCloseDelay, 0f);
        float elapsed = 0f;

        while (autoClose && isOpen)
        {
            // If the trigger is blocked, the initial delay is considered complete.
            // Once the trigger is free again, the door may close immediately.
            if (IsAutoCloseBlocked())
            {
                elapsed = delay;
                yield return null;
                continue;
            }

            if (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                yield return null;
                continue;
            }

            autoCloseRoutine = null;
            autoCloseInProgress = true;
            SetOpen(false);
            yield break;
        }

        autoCloseRoutine = null;
    }

    private bool IsAutoCloseBlocked()
    {
        return simulateTriggerOccupied || triggerObjectsInside > 0;
    }

    private bool IsAtClosedState()
    {
        if (objectToAnimate == null)
        {
            return true;
        }

        if (enableSliding)
        {
            if (Vector3.Distance(objectToAnimate.localPosition, closedPosition) > 0.001f)
            {
                return false;
            }

            if (objectToAnimate2 != null && Vector3.Distance(objectToAnimate2.localPosition, closedPosition2) > 0.001f)
            {
                return false;
            }

            return true;
        }

        return Quaternion.Angle(objectToAnimate.localRotation, GetTargetRotation(false)) <= 0.1f;
    }

    private Quaternion GetTargetRotation(bool open)
    {
        float angle = open ? openAngle : closedAngle;

        return Quaternion.AngleAxis(
            angle,
            localRotationAxis.normalized
        );
    }

    private Quaternion GetTargetTiltRotation(bool tilted)
    {
        float angle = tilted ? tiltOpenAngle : tiltClosedAngle;

        return Quaternion.AngleAxis(
            angle,
            localTiltRotationAxis.normalized
        );
    }

    private Vector3 GetTargetSlidePosition(bool open)
    {
        if (!open)
        {
            return closedPosition;
        }

        return closedPosition + localSlideAxis.normalized * slideDistance;
    }

    private Vector3 GetTargetSlidePosition2(bool open)
    {
        if (!open)
        {
            return closedPosition2;
        }

        return closedPosition2 - localSlideAxis.normalized * slideDistance;
    }

    private Quaternion GetHandleDownRotation(bool useTiltHandleSettings)
    {
        float targetHandleAngle = useTiltHandleSettings ? tiltHandleAngle : handleDownAngle;
        Vector3 targetHandleAxis = useTiltHandleSettings ? tiltHandleRotationAxis : handleRotationAxis;

        return handleStartRotation * Quaternion.AngleAxis(
            targetHandleAngle,
            targetHandleAxis.normalized
        );
    }

    private IEnumerator AnimateHandleDownAndUp(bool useTiltHandleSettings)
    {
        yield return AnimateHandleTo(GetHandleDownRotation(useTiltHandleSettings), handleAnimationTime * 0.5f);
        yield return AnimateHandleTo(handleStartRotation, handleAnimationTime * 0.5f);
    }

    private IEnumerator AnimateHandleDown(bool useTiltHandleSettings)
    {
        yield return AnimateHandleTo(GetHandleDownRotation(useTiltHandleSettings), handleAnimationTime);
    }

    private IEnumerator AnimateHandleUp()
    {
        yield return AnimateHandleTo(handleStartRotation, handleAnimationTime);
    }

    private IEnumerator AnimateHandleTo(Quaternion targetRotation, float duration)
    {
        if (handleToAnimate == null)
        {
            yield break;
        }

        Quaternion startRotation = handleToAnimate.localRotation;
        float time = Mathf.Max(duration, 0.01f);
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / time);

            handleToAnimate.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        handleToAnimate.localRotation = targetRotation;
    }

    private void SetupAudio()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    private void PlaySound(AudioClip clip, bool loop)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        if (loop)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            audioSource.loop = false;
            audioSource.volume = volume;
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void StopOpenSound()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

    private void StopLoopSound()
    {
        if (audioSource == null)
        {
            return;
        }

        if (audioSource.loop && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }
}