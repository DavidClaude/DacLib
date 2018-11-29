using System;

namespace DacLib.Generic
{
    public enum LogLevel {
        /// <summary>
        /// Custom description without troubles
        /// </summary>
        Info = 0,
        /// <summary>
        /// Notices of unexpected but not fatal results
        /// </summary>
        Warning = 1,
        /// <summary>
        /// Dangerous errors that might lead the program to crashes
        /// </summary>
        Error = 2
    }
}

