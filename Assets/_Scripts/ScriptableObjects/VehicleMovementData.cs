using UnityEngine;

[CreateAssetMenu(fileName = "VehicleData", menuName = "Data/PlayerData/VehicleData")]
public class VehicleMovementData : ScriptableObject
{
    public string vehicleName;

    [Header("Acceleration & Braking")]
    [Range(1f, 100f)]
    public float throttleForce = 50f;
    [Range(1f, 50f)]
    public float brakePushRate = 20f;
    [Range(1f, 50f)]
    public float brakeReleaseRate = 8f;

    [Header("Turning")]
    [Range(1f, 10f)]
    public float turnStrength = 2f;
    [Range(1f, 50f)]
    public float wheelRotationSpeed = 18f;
    [Range(1f, 90f)]
    public float wheelRotationLimit = 38f;

    [Header("Weight & Balance")]
    public Vector3 centerOfMass = new Vector3(0f, 0.8f, 0f);
    [Range(10f, 100f)]
    public float fallMod = 30f;
    [Range(0f, 1000f)]
    public float stabilizerTolerance = 300f;
    [Range(0f, 1000f)]
    public float stabilizerReactionSpeed = 400f;

    [Header("Miscellaneous")]
    public float jumpForce = 1000f;
    public float boostForce = 50f;
}
