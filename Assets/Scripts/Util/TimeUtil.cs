using System.Collections;
using UnityEngine;

namespace AsteroidGameUtils
{
    // Todo Extend further for other utils like fixed update, etc
    public static class TimeUtil
    {
        static readonly float fixedDeltaTimeDefault = Time.fixedDeltaTime;

        public static IEnumerator WaitForSeconds(float seconds)
        {
            float timer = 0.0f;

            while(timer < seconds)
            {
                yield return null;
                timer += Time.deltaTime;
            }
        }

        public static void SetTimeScale(float scale, bool adjustFixedDeltaTime)
        {
            Time.timeScale = scale;

            if (adjustFixedDeltaTime)
                Time.fixedDeltaTime = fixedDeltaTimeDefault * Time.timeScale;
        }
    }
}