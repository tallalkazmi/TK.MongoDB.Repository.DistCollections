namespace TK.MongoDB.Distributed.Models
{
    /// <summary>
    /// Insert result
    /// </summary>
    /// <typeparam name="T">Type of BaseEntity</typeparam>
    public class InsertResult<T> where T : BaseEntity
    {
        internal InsertResult(string collectionId, T result)
        {
            CollectionId = collectionId;
            Result = result;
        }

        internal InsertResult(string collectionId, string parentCollectionId, T result)
        {
            CollectionId = collectionId;
            ParentCollectionId = parentCollectionId;
            Result = result;
        }

        /// <summary>
        /// Inserted Collection Id
        /// </summary>
        public string CollectionId { get; private set; }

        /// <summary>
        /// Parent Collection Id
        /// </summary>
        public string ParentCollectionId { get; private set; }

        /// <summary>
        /// Insert operation success
        /// </summary>
        public bool Success { get { return Result != null; } }
        
        /// <summary>
        /// Inserted values
        /// </summary>
        public T Result { get; private set; }
    }

    /// <summary>
    /// Update result
    /// </summary>
    /// <typeparam name="T">Type of BaseEntity</typeparam>
    public class UpdateResult<T> where T : BaseEntity
    {
        internal UpdateResult(bool success, T result)
        {
            Success = success;
            Result = result;
        }

        /// <summary>
        /// Update operation success
        /// </summary>
        public bool Success { get; private set; }
        
        /// <summary>
        /// Updated object
        /// </summary>
        public T Result { get; private set; }
    }
}
