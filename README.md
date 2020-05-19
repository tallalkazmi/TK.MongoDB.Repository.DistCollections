# TK.MongoDB.Repository.Distributed
Repository pattern implementation with ***distributed collections*** of MongoDB in .NET Framework

## Intro

This solution provides a simple and easy way to create distributed collections based on specified properties in a model. You may also use nested levels of collections while keeping a backlink in the source collection.

A master collection, created by default, acts as an reference index to all the collections stored. Master collection maintains the *creation date* and *update date* of each record, you may also *name* a collection if needed.

## Usage

### Configuration

- #### Settings

1. Default `ConnectionStringSettingName` is set to "*MongoDocConnection*", but this can be configured by setting the property:

   ```c#
   Settings.ConnectionStringSettingName = "MongoDocConnection";
   ```

2. Default `ExpiryAfterSeconds` index is set to "*15778463*" *(approx: 6 months)*, but this can be configured by setting property:

   ```c#
   Settings.ExpireAfterSeconds = 2592000;
   ```


- #### Master Settings


1. Default `CollectionName` is set to "*master*", but this can be configured by calling the following static method:

   ```c#
   MasterSettings.CollectionName = "master";
   ```

2. Additional properties may be added to the master collection by using the following property:

   ```c#
   MasterSettings.AdditionalProperties = new string[] { "CreatedBy", "UpdatedBy" };
   ```

3. Additional properties may be set on `BeforeInsert` & `AfterInsert` triggers by calling the `MasterSettings.SetProperties()` method anywhere in the application, for example:

   ```c#
   MasterSettings.SetProperties(new Dictionary<string, object>() { { "CreatedBy", Guid.Parse("FC09E7EE-5E78-E811-80C7-000C29DADC00") } }, MasterSettings.Triggers.BeforeInsert);
   MasterSettings.SetProperties(new Dictionary<string, object>() { { "UpdatedBy", Guid.Parse("6B9F4B43-5F78-E811-80C7-000C29DADC00") } }, MasterSettings.Triggers.AfterInsert);
               
   Message message = new Message()
   {
       Text = $"Test message # {DateTime.UtcNow.ToShortTimeString()}",
       Client = 3,
       Caterer = 4
   };
   Message result = await MessageRepository.InsertAsync(message);
   ```

- #### Example:

  ```c#
  public class MessageUnitTest
  {
     	public MessageUnitTest()
      {
  		Settings.ConnectionStringSettingName = "MongoDocConnection";
  		MasterSettings.AdditionalProperties =
              new string[] { "CreatedBy", "UpdatedBy" };
      }
  
  	//.... other methods and properties
  }
  ```

### Creating Models

The solution has a predefined attribute `Distribute` with an optional value of `Level`, adding the attribute triggers the collection distribution based on the property.

Providing `Level` makes logical nesting of collections by keeping backlinks, for example in the model below the first entry with *Caterer*, *Client* and *Order* defined will be added to source collection and subsequent entries added to a new collection created. In this way the source collection will always have an entry to corelate with the nested collection.

Creating a document model inheriting `BaseEntity`​ to use in repository.

```c#
public class Message : BaseEntity
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

### Repository methods

<u>*Collection Ids*</u> can be fetched using ***Master Repository*** and needs to be defined for each method in repository, hence, each call may run on a different collection altogether. 

1. Find *asynchronous* (using LINQ Expression)

   ```c#
   Message result = await MessageRepository.FindAsync(CollectionId, x => x.Id == new ObjectId("5e36997898d2c15a400f8968"));
   ```
   
2. Get *asynchronous* (using LINQ Expression)

   - Has paged records in a `Tuple<IEnumerable<T>, long>` of records and total count.
   
   
   ```c#
   var result = await MessageRepository.GetAsync(CollectionId, 1, 10, x => x.Text.Contains("abc") && x.Deleted == false);
   ```
   
3. Get *asynchronous* (using Filter Definition)

   - Has paged records in a `Tuple<IEnumerable<T>, long>` of records and total count.
   
   
   ```c#
   MessageSearchParameters searchParameters = new MessageSearchParameters()
   {
       Text = "Change",
       Caterer = null,
       Client = null,
       Order = null
   };
      
   var builder = Builders<Message>.Filter;
   var filter = builder.Empty;
   if (!string.IsNullOrWhiteSpace(searchParameters.Text))
   {
       var criteriaFilter = builder.Regex(x => x.Text, new BsonRegularExpression($".*{searchParameters.Text}.*"));
       filter &= criteriaFilter;
   }
      
   if (searchParameters.Caterer.HasValue)
   {
       var criteriaFilter = builder.Eq(x => x.Caterer, searchParameters.Caterer.Value);
       filter &= criteriaFilter;
   }
      
   if (searchParameters.Client.HasValue)
   {
       var criteriaFilter = builder.Eq(x => x.Client, searchParameters.Client.Value);
       filter &= criteriaFilter;
   }
      
   if (searchParameters.Order.HasValue)
   {
       var criteriaFilter = builder.Eq(x => x.Order, searchParameters.Order.Value);
       filter &= criteriaFilter;
   }
      
   var result = await MessageRepository.GetAsync(CollectionId, 1, 10, filter);
   ```
   
4. Insert *asynchronous* 

   ```c#
   Message message = new Message()
   {
         Text = "xyz",
         Client = 2,
         Caterer = 2
   };
   
   Message result = await MessageRepository.InsertAsync(message);
   ```

5. Update *asynchronous* 

   ```c#
   Message message = new Message()
   {
         Id = new ObjectId("5e36998998d2c1540ca23894"),
         Text = "Changed"
   };
   
   bool result = await MessageRepository.UpdateAsync(CollectionId, message);
   ```

6. Delete *asynchronous* (by Id)

   ```c#
   bool result = await MessageRepository.DeleteAsync(CollectionId, new ObjectId("5e36998998d2c1540ca23894"));
   ```

7. Count *asynchronous* 

   ```c#
   long result = await MessageRepository.CountAsync(CollectionId);
   ```

8. Exists *asynchronous* (using LINQ Expression)

   ```c#
   bool result = await MessageRepository.ExistsAsync(CollectionId, x => x.Text == "abc");
   ```

### Master Repository methods

1. Find *asynchronous*

   ```c#
   var builder = Builders<BsonDocument>.Filter;
   var collectionFilter = builder.Eq("CollectionId", "c7a7935f1ebd440e9b85003c1b81b3c3");
   var result = await MasterRepository.FindAsync(collectionFilter);
   var record = BsonSerializer.Deserialize<MasterGetViewModel>(result.ToJson());
   ```

2. Get *asynchronous*

   - Has paged records in a `Tuple<IEnumerable<BsonDocument>, long>` of records and total count.

   ```c#
   var result = await MasterRepository.GetAsync(1, 20);
   var records = BsonSerializer.Deserialize<IEnumerable<MasterGetViewModel>>(result.Item1.ToJson());
   ```

3. Get *asynchronous* (using dictionary of keys and values)

   ```c#
   Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
   {
       { "Client", 1 }
   };
   
   var result = await MasterRepository.GetAsync(keyValuePairs);
   var records = BsonSerializer.Deserialize<IEnumerable<MasterGetViewModel>>(result.Item1.ToJson());
   ```

4. Get *asynchronous*  (using Filter Definition)

   ```c#
   var builder = Builders<BsonDocument>.Filter;
   var clientFilter = builder.Eq("Client", 1);
   var catererFilter = builder.Eq("Caterer", 1);
   var filter = clientFilter & catererFilter;
   
   var result = await MasterRepository.GetAsync(1, 20, filter);
   var records = BsonSerializer.Deserialize<IEnumerable<MasterGetViewModel>>(result.Item1.ToJson());
   ```

5. Update *asynchronous* 

   ```c#
   bool result = await MasterRepository.UpdateAsync("53df73c45d7e493b86746066a693534c", "Untitled-3");
   ```


## Tests

Refer to **TK.MongoDB.Test** project for <u>Unit Tests</u> for:

- Repository
- Master Repository
