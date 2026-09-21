using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Engine & Handling")]
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float reverseAcceleration = 20f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float brakeDrag = 4f;
    [SerializeField] private float normalDrag = 1f;

    [Header("Engine Sound")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private float minPitch = 0.6f;
    [SerializeField] private float maxPitch = 2.0f;
    [SerializeField] private float pitchChangeSpeed = 5f;

    [Header("Visual Wheels")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float wheelRadius = 0.2f;

    private Rigidbody rb;
    private float moveInput;
    private float steerInput;
    private bool isBraking;
    private float currentRollAngle;
    private bool controlsEnabled = false;

    public float CurrentSpeed => Vector3.Dot(rb.linearVelocity, transform.forward);
    public float SpeedKmh => Mathf.Abs(CurrentSpeed) * 3.6f;

    public void SetControlsEnabled(bool state)
    {
        controlsEnabled = state;
        if (!state)
        {
            moveInput = 0f;
            steerInput = 0f;
            isBraking = false;
        }
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = position;
        rb.rotation = rotation;
        transform.position = position;
        transform.rotation = rotation;
        Physics.SyncTransforms();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.3f, 0f);

        if (engineAudioSource == null)
            engineAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (controlsEnabled)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                moveInput = 0f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput -= 1f;

                steerInput = 0f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) steerInput += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) steerInput -= 1f;

                isBraking = keyboard.spaceKey.isPressed;
            }
        }

        UpdateWheelVisuals();
        UpdateEngineSound();
    }

    private void FixedUpdate()
    {
        ApplyBrakingAndDrag();
        ApplyDriveForce();
        ApplySteering();
        ApplySidewaysGrip();
    }

    private void ApplyDriveForce()
    {
        if (isBraking || Mathf.Abs(moveInput) < 0.01f)
            return;

        float forwardSpeed = CurrentSpeed;

        if (moveInput > 0f && forwardSpeed < maxSpeed)
        {
            rb.AddForce(transform.forward * (moveInput * acceleration), ForceMode.Acceleration);
        }
        else if (moveInput < 0f && forwardSpeed > -maxSpeed * 0.4f)
        {
            rb.AddForce(transform.forward * (moveInput * reverseAcceleration), ForceMode.Acceleration);
        }
    }

    private void ApplySteering()
    {
        float speedRatio = Mathf.InverseLerp(0.2f, maxSpeed, Mathf.Abs(CurrentSpeed));
        float direction = CurrentSpeed >= 0f ? 1f : -1f;

        float turn = steerInput * turnSpeed * speedRatio * direction * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void ApplyBrakingAndDrag()
    {
        if (isBraking)
        {
            rb.linearDamping = brakeDrag;
        }
        else if (Mathf.Abs(moveInput) < 0.01f)
        {
            rb.linearDamping = normalDrag;
        }
        else
        {
            rb.linearDamping = 0.05f;
        }
    }

    private void ApplySidewaysGrip()
    {
        Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVelocity * 10f, ForceMode.Acceleration);
    }

    private void UpdateWheelVisuals()
    {
        float distance = CurrentSpeed * Time.deltaTime;
        float deltaRoll = (distance / (2f * Mathf.PI * wheelRadius)) * 360f;
        currentRollAngle = (currentRollAngle + deltaRoll) % 360f;

        float steerAngle = steerInput * maxSteerAngle;

        ApplyWheelTransform(frontLeftWheel, currentRollAngle, steerAngle);
        ApplyWheelTransform(frontRightWheel, currentRollAngle, steerAngle);
        ApplyWheelTransform(rearLeftWheel, currentRollAngle, 0f);
        ApplyWheelTransform(rearRightWheel, currentRollAngle, 0f);
    }

    private void ApplyWheelTransform(Transform wheel, float roll, float steer)
    {
        if (wheel == null) return;
        wheel.localRotation = Quaternion.Euler(0f, steer, 0f) * Quaternion.Euler(roll, 0f, 0f);
    }

    private void UpdateEngineSound()
    {
        if (engineAudioSource == null) return;

        float speedRatio = Mathf.Clamp01(Mathf.Abs(CurrentSpeed) / maxSpeed);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, pitchChangeSpeed * Time.deltaTime);
    }
}