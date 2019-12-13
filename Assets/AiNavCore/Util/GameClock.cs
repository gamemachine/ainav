namespace AiNav
{
    public struct GameClock
    {
        public const float MainTickRate = 30f;
        public float TimeBetweenTicks;
        public float LastTick;

        public GameClock(float interval, bool asTicksPerSecond = true)
        {
            if (asTicksPerSecond)
            {
                TimeBetweenTicks = 1000f / interval / 1000f;
            }
            else
            {
                TimeBetweenTicks = interval;
            }

            LastTick = 0f;
        }

        public static bool NetworkTick(float time, float interval, ref float lastTick)
        {
            float timeBetweenTicks = 1000f / interval / 1000f;
            var since = time - lastTick;
            if (since >= timeBetweenTicks)
            {
                lastTick = time;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Tick(float time, float timeBetweenTicks, ref float lastTick)
        {
            var since = time - lastTick;
            if (since >= timeBetweenTicks)
            {
                lastTick = time;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Tick(float time)
        {
            var since = time - LastTick;
            if (since >= TimeBetweenTicks)
            {
                LastTick = time;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
