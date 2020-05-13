namespace TK.MongoDB
{
    public class Settings
    {
        protected static string ConnectionStringSettingName { get; private set; } = "MongoDocConnection";
        protected static int ExpireAfterSeconds { get; private set; } = 15778463; //6 months
        protected static string MasterCollectionName { get; private set; } = "master";

        /// <summary>
        /// Database & Collection configurations
        /// </summary>
        /// <param name="connectionStringSettingName">Connection String name from *.config file. Default value is set to <i>MongoDocConnection</i></param>
        /// <param name="expireAfterSeconds">Documents expire after seconds. Default value is set to <i>15778463 seconds (6 months)</i></param>
        /// <param name="masterCollectionName">Master collection's name. Default is set to '<i>master</i>'</param>
        public static void Configure(string connectionStringSettingName = null, int? expireAfterSeconds = null, string masterCollectionName = null)
        {
            if (!string.IsNullOrWhiteSpace(connectionStringSettingName)) ConnectionStringSettingName = connectionStringSettingName;
            if (expireAfterSeconds.HasValue) ExpireAfterSeconds = expireAfterSeconds.Value;
            if (!string.IsNullOrWhiteSpace(masterCollectionName)) MasterCollectionName = masterCollectionName;
        }
    }
}
