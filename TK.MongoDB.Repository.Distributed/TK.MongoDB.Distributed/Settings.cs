namespace TK.MongoDB.Distributed
{
    /// <summary>
    /// Database and Collection configurations
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Connection String name from *.config file. Default value is set to <i>MongoDocConnection</i>
        /// </summary>
        public static string ConnectionStringSettingName { get; set; } = "MongoDocConnection";

        /// <summary>
        /// Documents expire after seconds. Default value is set to <i>15778463 seconds (6 months)</i>
        /// </summary>
        public static int ExpireAfterSeconds { get; private set; } = 15778463; //6 months

        /// <summary>
        /// Master collection's name. Default is set to '<i>master</i>'
        /// </summary>
        public static string MasterCollectionName { get; private set; } = "master";
    }
}
