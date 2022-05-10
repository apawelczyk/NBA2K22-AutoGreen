namespace AutoGreen
{
    public class MicroStopwatch : System.Diagnostics.Stopwatch
    {
        readonly double _microSecPerTick = 1000D / System.Diagnostics.Stopwatch.Frequency;

        public MicroStopwatch()
        {

        }

        public long ElapsedMicroseconds
        {
            get
            {
                return (long)(ElapsedTicks * _microSecPerTick);
            }
        }
    }
}
