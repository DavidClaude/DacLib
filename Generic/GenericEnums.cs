using System;

namespace DacLib.Generic
{
    public enum LogLevel
    {
        /// <summary>
        /// Custom description without troubles
        /// </summary>
        Info = 1,
        /// <summary>
        /// Notices of unexpected but not fatal results
        /// </summary>
        Warning,
        /// <summary>
        /// Dangerous errors that might lead the program to crashes
        /// </summary>
        Error
    }

    public enum TimeUnit
    {
        None = 0,
        Nanosecond,
        Microsecond,
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public enum LogWriteMode
    {
        None = 0,
        Append,
        Override
    }
}