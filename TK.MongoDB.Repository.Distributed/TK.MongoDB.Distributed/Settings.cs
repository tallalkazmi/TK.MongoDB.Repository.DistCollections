using System.Collections.Generic;

namespace TK.MongoDB.Distributed
{
    /// <summary>
    /// Database: Connection and Collection Indexes configurations
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Connection String name from *.config file. Default value is set to <i>MongoDocConnection</i>.
        /// </summary>
        public static string ConnectionStringSettingName { get; set; } = "MongoDocConnection";

        /// <summary>
        /// Documents expire after seconds. Default value is set to <i>15778463 seconds (6 months)</i>.
        /// </summary>
        public static int ExpireAfterSeconds { get; set; } = 15778463; //6 months
    }

    /// <summary>
    /// Master: Collection and Trigger configurations
    /// </summary>
    public class MasterSettings
    {
        internal static string ConnectionStringSettingName = Settings.ConnectionStringSettingName;
        internal static int ExpireAfterSeconds = Settings.ExpireAfterSeconds;
        internal static IDictionary<string, object> PropertiesBeforeInsert;
        internal static IDictionary<string, object> PropertiesAfterInsert;

        /// <summary>
        /// Name for Master collection. Default is set to '<i>master</i>'.
        /// </summary>
        public static string CollectionName { get; set; } = "master";

        /// <summary>
        /// Additional properties for Master collection [Optional].
        /// </summary>
        public static IEnumerable<string> AdditionalProperties { get; set; }

        /// <summary>
        /// Set properties and their values to auto-update after the specified trigger is fired.
        /// </summary>
        /// <param name="properties">Key value pair of properties</param>
        /// <param name="trigger">Trigger</param>
        public static void SetProperties(IDictionary<string, object> properties, Triggers trigger)
        {
            switch (trigger)
            {
                case Triggers.BeforeInsert:
                    PropertiesBeforeInsert = properties;
                    break;

                case Triggers.AfterInsert:
                    PropertiesAfterInsert = properties;
                    break;
            }
        }

        /// <summary>
        /// Triggers for auto-update
        /// </summary>
        public enum Triggers
        {
            /// <summary>
            /// On before the document is inserted.
            /// </summary>
            BeforeInsert = 1,

            /// <summary>
            /// On after the document has been inserted.
            /// </summary>
            AfterInsert = 2
        }
    }
}
