using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit.AI
{
    public static class RckAIFacialAnim
    {
        public static float smoothingSpeed = 15f;     // Adjust the smoothing speed
        public static float minMouthOpen = 10.0f;     // The mouth blendshape value for a closed mouth (0% open).

        public static float GetMouthMovementValue(float startTime, float clipFrequency, float[] audioData)
        {
            // Settings to auto-facial animation
            float maxMouthOpen = 100.0f;    // The mouth blendshape value for a fully open mouth (100% open).
            float sensitivity = 0.5f;       // Adjust the sensitivity.

            // Calculate the loudness for the current time
            float currentTime = Time.time - startTime;
            float clipTime = currentTime * clipFrequency;

            // Ensure clipTime is within the valid range
            clipTime = Mathf.Clamp(clipTime, 0, audioData.Length - 1);

            // Calculate loudness at the current time
            int startIndex = Mathf.FloorToInt(clipTime);
            int endIndex = Mathf.CeilToInt(clipTime);
            float interpolationFactor = clipTime - startIndex;
            float loudnessAtStart = Mathf.Abs(audioData[startIndex]);
            float loudnessAtEnd = Mathf.Abs(audioData[endIndex]);
            float loudnessOfTheClipRightNow = Mathf.Lerp(loudnessAtStart, loudnessAtEnd, interpolationFactor);

            // Map loudness to the 0-100 range and apply sensitivity
            float mappedMouthOpenValue = Mathf.Pow(Mathf.InverseLerp(0.0f, 1f, loudnessOfTheClipRightNow), sensitivity) * (maxMouthOpen - minMouthOpen) + minMouthOpen;

            return mappedMouthOpenValue;
        }
    }
}