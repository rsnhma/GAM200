using UnityEngine;
using System;

public class NoiseSystem : MonoBehaviour
{
    public static event Action<Vector2, float> OnNoiseEmitted;

    public static void EmitNoise(Vector2 position, float radius)
    {
        //Debug.Log($"Noise emitted at {position} with radius {radius}");
        OnNoiseEmitted?.Invoke(position, radius);
    }

    public static class NoiseTypes
    {
        public const float SprintRadius = 10f;
        public const float LockerRadius = 12f;
        public const float PuzzleFailRadius = 15f;
    }
}
