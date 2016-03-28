using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// The source type for our information
    /// </summary>
    public enum enumSourceType
    {
        /// <summary>A telemetered value</summary>
        Telemetered,

        /// <summary>A calculated value</summary>
        Calculated,

        /// <summary>An entered value</summary>
        Entered,

        /// <summary>An estimated value</summary>
        Estimated,

        /// <summary>Unknown type</summary>
        Unknown
    }

    /// <summary>Our collection of state types</summary>
    public enum enumStateType
    {
        /// <summary>The element is opened</summary>
        Open,
        /// <summary>The element is closed</summary>
        Closed
    }

    /// <summary>The device type of our telemetry</summary>
    public enum enumDeviceType
    {
        /// <summary>OAG</summary>
        OAG,
        /// <summary>SPS</summary>
        SPS
    }

    /// <summary>The types of measurements we know</summary>
    public enum enumMeasurementType
    {
        /// <summary>A status type</summary>
        STAT,
        /// <summary>A MGP-1 (?) type</summary>
        MGP1,
        /// <summary>An analog type</summary>
        ANLG
    }

    

    /// <summary>Our quality types</summary>
    public enum enumQualityType
    {
        /// <summary>The data are not valid</summary>
        NotValid,

        /// <summary>The data are suspect</summary>
        Suspect,

        /// <summary>The data are valid</summary>
        Valid,

        /// <summary>The data are held</summary>
        Held,

        /// <summary>The data are unknown</summary>
        Unknown,

        /// <summary>The data are disabled</summary>
        Diabled,

        /// <summary>The data are deleted</summary>
        Deleted
    }
}