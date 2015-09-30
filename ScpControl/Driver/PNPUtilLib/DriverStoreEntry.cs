using System.Collections.Generic;

namespace ScpControl.Driver.PNPUtilLib
{
    /// <summary>
    ///     Data fields retrieved from Driver store for each driver
    /// </summary>
    public struct DriverStoreEntry
    {
        /// <summary>
        ///     Name of the INF in driver store
        /// </summary>
        public string DriverPublishedName { get; set; }

        /// <summary>
        ///     Driver package provider
        /// </summary>
        public string DriverPkgProvider { get; set; }

        /// <summary>
        ///     Driver class (ex., "System Devices")
        /// </summary>
        public string DriverClass { get; set; }

        /// <summary>
        ///     Sys file date
        /// </summary>
        public string DriverDate { get; set; }

        /// <summary>
        ///     Sys file version
        /// </summary>
        public string DriverVersion { get; set; }

        /// <summary>
        ///     Signer name. Empty if not WHQLd.
        /// </summary>
        public string DriverSignerName { get; set; }

        /// <summary>
        ///     Field count
        /// </summary>
        private const int FieldCount = 6;

        public int GetFieldCount()
        {
            return FieldCount;
        }

        public string[] GetFieldNames()
        {
            return new[]
            {
                "INF",
                "Package Provider",
                "Driver Class",
                "Driver Date",
                "Driver Version",
                "Driver Signer"
            };
        }

        public string[] GetFieldValues()
        {
            var fieldValues = new List<string>
            {
                DriverPublishedName,
                DriverPkgProvider,
                DriverClass,
                DriverDate,
                DriverVersion,
                DriverSignerName
            };

            return fieldValues.ToArray();
        }
    };
}