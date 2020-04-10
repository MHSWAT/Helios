﻿//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// this is a structured log item, with the additional restriction
    /// that it is intended for human consumption, so long messages
    /// can be truncated and implementation-related messages are discouraged
    /// </summary>
    public class StatusReportItem
    {
        /// <summary>
        /// This severity code mirrors the log levels where names are the same.
        ///
        /// Note: It should be a nested class of StatusReportItem, but WPF bindings require it to be
        /// a top-level type.
        /// </summary>
        public enum SeverityCode
        {
            Info,
            Warning,
            Error,
            None // no messages should be created at this level, in it used to filter out all messages
        }

        /// <summary>
        /// optional time stamp or null
        /// </summary>
        public string TimeStamp { get; set; }

        public SeverityCode Severity { get; set; } = SeverityCode.Info;

        /// <summary>
        /// the status message, which may be more than one line long
        /// but should generally be short
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Additional information related to this status, expressed as a set of
        /// flags that may be set to communicate different facts.
        /// </summary>
        [System.Flags]
        public enum StatusFlags
        {
            // no flags
            None = 0,
            // this status may be numerous or verbose and should be filtered in small status displays
            Verbose = 1,
            // this status indicates that some checked configuration item was up to date and does not need to regenerated
            ConfigurationUpToDate = 2
        }

        /// <summary>
        /// Any flags set in this value (combined via binary 'or') indicate that
        /// the corresponding fact is true about this status report item.
        /// </summary>
        public StatusFlags Flags { get; set; }

        /// <summary>
        /// a recommendation to the user or null
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// log this result
        /// </summary>
        /// <param name="logManager"></param>
        public void Log(LogManager logManager)
        {
            switch (Severity)
            {
                case SeverityCode.None:
                    throw new System.Exception($"Severity 'None' must not be used for any status report instances; implementation error");
                case SeverityCode.Info:
                    logManager.LogInfo(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogInfo(Recommendation);
                    }
                    break;
                case SeverityCode.Warning:
                    logManager.LogWarning(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogWarning(Recommendation);
                    }
                    break;
                case SeverityCode.Error:
                    logManager.LogError(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogError(Recommendation);
                    }
                    break;
            }
        }
    }
}