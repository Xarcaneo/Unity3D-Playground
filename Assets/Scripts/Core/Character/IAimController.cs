using UnityEngine;

/// <summary>
/// Interface defining the required properties and methods for an aim controller.
/// </summary>
public interface IAimController
{
    /// <summary>
    /// Gets the current pitch angle of the aim (vertical rotation).
    /// </summary>
    float pitch { get; }

    /// <summary>
    /// Gets the local rotation for yaw (horizontal rotation).
    /// </summary>
    Quaternion yawLocalRotation { get; }

    /// <summary>
    /// Gets the local rotation for pitch (vertical rotation).
    /// </summary>
    Quaternion pitchLocalRotation { get; }

    /// <summary>
    /// Gets the forward direction of the aim.
    /// </summary>
    Vector3 heading { get; }

    /// <summary>
    /// Gets the aim direction that the character is currently facing.
    /// </summary>
    Vector3 aimHeading { get; }

    /// <summary>
    /// Gets the forward vector of the pitch transform.
    /// </summary>
    Vector3 forward { get; }

    /// <summary>
    /// Gets the up vector of the yaw transform.
    /// </summary>
    Vector3 yawUp { get; }

    /// <summary>
    /// Gets or sets the multiplier applied to the turn rate. Used for slowing turn speed when zooming, etc.
    /// </summary>
    float turnRateMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the steering rate for aiming. Controls how quickly the character aligns to the aim direction.
    /// </summary>
    float steeringRate { get; set; }

    /// <summary>
    /// Gets the difference in yaw between the character's direction and the aim direction.
    /// </summary>
    float aimYawDiff { get; }

    /// <summary>
    /// Adds a specified amount to the yaw (horizontal rotation).
    /// </summary>
    /// <param name="rotation">The amount of yaw to add.</param>
    void AddYaw(float rotation);

    /// <summary>
    /// Resets the yaw rotation to its local identity.
    /// </summary>
    void ResetYawLocal();

    /// <summary>
    /// Resets the yaw rotation to its local identity with a specified offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the yaw rotation.</param>
    void ResetYawLocal(float offset);

    /// <summary>
    /// Adds a specified amount to the pitch (vertical rotation).
    /// </summary>
    /// <param name="rotation">The amount of pitch to add.</param>
    void AddPitch(float rotation);

    /// <summary>
    /// Resets the pitch rotation to its local identity.
    /// </summary>
    void ResetPitchLocal();

    /// <summary>
    /// Adds rotation input for both yaw and pitch.
    /// </summary>
    /// <param name="y">Yaw amount to add.</param>
    /// <param name="p">Pitch amount to add.</param>
    void AddRotation(float y, float p);

    /// <summary>
    /// Adds rotation input relative to a specified transform.
    /// </summary>
    /// <param name="input">The rotation input as a Vector2.</param>
    /// <param name="relativeTo">The transform relative to which the rotation is applied.</param>
    void AddRotationInput(Vector2 input, Transform relativeTo);

    /// <summary>
    /// Sets constraints on yaw (horizontal) rotation.
    /// </summary>
    /// <param name="center">The center of the yaw constraint.</param>
    /// <param name="range">The range of the yaw constraint.</param>
    void SetYawConstraints(Vector3 center, float range);

    /// <summary>
    /// Sets constraints on pitch (vertical) rotation.
    /// </summary>
    /// <param name="min">The minimum pitch constraint.</param>
    /// <param name="max">The maximum pitch constraint.</param>
    void SetPitchConstraints(float min, float max);

    /// <summary>
    /// Sets constraints on heading direction.
    /// </summary>
    /// <param name="center">The center of the heading constraint.</param>
    /// <param name="range">The range of the heading constraint.</param>
    void SetHeadingConstraints(Vector3 center, float range);

    /// <summary>
    /// Resets yaw (horizontal) constraints.
    /// </summary>
    void ResetYawConstraints();

    /// <summary>
    /// Resets pitch (vertical) constraints.
    /// </summary>
    void ResetPitchConstraints();

    /// <summary>
    /// Resets heading direction constraints.
    /// </summary>
    void ResetHeadingConstraints();

    /// <summary>
    /// Gets the transform component associated with the aim controller.
    /// </summary>
    Transform transform { get; }
}
