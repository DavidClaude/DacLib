﻿using System;

namespace DacLib.Generic
{
    public enum DeltaMode
    {
        Const,
        Percentage
    }

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

    public enum NodeType
    {
        Unit = 0,
        Diagram
    }

    public enum NodePort
    {
        None = 0,
        Entrance,
        Exitus,
        Both
    }
}