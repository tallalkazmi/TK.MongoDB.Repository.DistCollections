# TK.MongoDB.Repository.Distributed
Repository pattern implementation with ***distributed collections*** of MongoDB in .NET Framework

## Intro

This solution provides a simple and easy way to create distributed collections based on specified properties in a model. You may also use nested levels of collections by keeping a backlink in the source collection.

A master collection, created by default, acts as an reference index to all the collections stored. Master collection maintains the *creation date* and *update date* of each record, you may also *name* a collection if needed.

## Usage

#### Configure Settings

1. Default `ConnectionStringSettingName` is set to "*MongoDocConnection*", but this can be configured by calling the following static method:

   ```c#
   Settings.Configure("MongoDocConnection");
   ```

2. Default `ExpiryAfterSeconds` index is set to "*15778463*" *(approx: 6 months)*, but this can be configured by calling the following static method:

   ```c#
   Settings.Configure(expireAfterSeconds: 2592000);
   ```

3. Default `MasterCollectionName` is set to "*master*", but this can be configured by calling the following static method:

   ```c#
   Settings.Configure(masterCollectionName: "master");
   ```

4. Example:

   ```c#
   public class MessageUnitTest
   {
       Repository<Activity> ActivityRepository;
       public MessageUnitTest()
       {
           Settings.Configure("MongoDocConnection");
           ActivityRepository = new Repository<Activity>();
       }
   
   	//.... other methods and properties
   }
   ```

#### Creating Models

The solution has a predefined attribute `Distribute` with an optional value of `Level`, adding the attribute triggers the collection distribution based on the property.

Providing `Level` makes logical nesting of collections by keeping backlinks, for example in the model below the first entry with *Caterer*, *Client* and *Order* defined will be added to source collection and subsequent entries added to a new collection created. In this way the source collection will always have an entry to corelate with the nested collection.

Creating a document model implementing $BaseEntity$ to use in repository.

```c#
public class Message : BaseEntity<ObjectId>
{
    [BsonRequired]
    public string Text { get; set; }


    [Distribute]
    public long Caterer { get; set; }


    [Distribute]
    public long Client { get; set; }


    [Distribute(Level = 1)]
    public long? Order { get; set; }
}
```

#### Repository methods

<u>*Collection Ids*</u> can be fetched using ***Master Repository*** and needs to be defined for each method in repository, hence, each call may run on a different collection altogether. 

1. Find *asynchronous* (using Linq Expression.)

   ```c#
   Activity result = await ActivityRepository.FindAsync(CollectionId, x => x.Id == new ObjectId("5e36997898d2c15a400f8968"));
   ```
   
2. Get *asynchronous* (using Linq Expression.)

   Has paged records in a `Tuple<IEnumerable<T>, long>` of records and total count.
   
   ```c#
   var result = await ActivityRepository.GetAsync(1, 10, x => x.Name.Contains("abc") && x.Deleted == false);
   ```
   
3. Insert *asynchronous* 

   ```c#
   Activity activity = new Activity()
   {
       Text = "xyz",
       Client = 2,
       Caterer = 2
   };
   
   Activity result = await ActivityRepository.InsertAsync(activity);
   ```

7. Update *asynchronous* 

   ```c#
   Activity activity = new Activity()
   {
   	Id = new ObjectId("5e36998998d2c1540ca23894"),
   	Text = "Changed"
   };
   
   bool result = await ActivityRepository.UpdateAsync(CollectionId, activity);
   ```

8. Delete *asynchronous* (by Id)

   ```c#
   bool result = await ActivityRepository.DeleteAsync(CollectionId, new ObjectId("5e36998998d2c1540ca23894"));
   ```

9. Count *asynchronous* 

   ```c#
   long result = await ActivityRepository.CountAsync(CollectionId);
   ```

10. Exists *asynchronous* (using Linq Expression)

   ```c#
   bool result = await ActivityRepository.ExistsAsync(CollectionId, x => x.Name == "abc");
   ```

#### Master Repository methods

1. Get *asynchronous*

   ```c#
   var result = await MasterRepository.GetAsync();
   ```

2. Get *asynchronous* (using dictionary of keys and values)

   ```c#
   Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
   {
       { "Client", 1 }
   };
   
   var result = await MasterRepository.GetAsync(keyValuePairs);
   ```

3. Update *asynchronous* 

   ```c#
   bool result = await MasterRepository.UpdateAsync("53df73c45d7e493b86746066a693534c", "Untitled-3");
   ```


## Tests

Refer to **TK.MongoDB.Test** project for <u>Unit Tests</u> for:

- Repository
- Master Repository