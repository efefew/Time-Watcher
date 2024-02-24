using System;

namespace Test
{
    internal class TimeInterval
    {
        public DateTime start; 
        public DateTime end;
        public bool emergencyExit = false;
        public TimeInterval(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }
        public TimeInterval(DateTime start) 
        {
            this.start = start;
            emergencyExit = true;
        }
        public TimeSpan Interval() 
        {
            if(end == null || emergencyExit)
                return TimeSpan.Zero;
            return end.Subtract(start);
        }
    }
}
