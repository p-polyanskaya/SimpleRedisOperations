using Newtonsoft.Json;
using StackExchange.Redis;

/*
Set up the connection bypassing the server name where Redis is hosted.
Get connected with the databases of the Redis server
After establishing the Redis connection, you can perform any data related operations on the Redis database instance just like setting up Redis cache key-value and getting the values.
*/
var conn = ConnectionMultiplexer.Connect("localhost");

//getting database instances of the redis  
var database = conn.GetDatabase();

Console.WriteLine("             stringValue");

#region stringValue

var stringSetResult = database.StringSet("redisKey", "redisvalue");
Console.WriteLine("stringSetResult " + stringSetResult);

var stringGetResult = database.StringGet("redisKey");
Console.WriteLine(stringGetResult);

var stringAppendResult = database.StringAppend("redisKey", " addedValue");
Console.WriteLine("stringAppendResult " + stringAppendResult);

stringGetResult = database.StringGet("redisKey");
Console.WriteLine("New string after append: " + stringGetResult);
Console.WriteLine("Is value int " + stringGetResult.IsInteger);

var length = database.StringLength("redisKey");
Console.WriteLine("length of new value " + length);

var stringSetAndGetResponse = database.StringSetAndGet("redisKey", "newval");
Console.WriteLine("stringSetAndGetResponse " + stringSetAndGetResponse);

stringGetResult = database.StringGet("redisKey");
Console.WriteLine("Overwritten string " + stringGetResult);

#endregion

Console.WriteLine();
Console.WriteLine("             intValue");

#region intValue

var intSetResult = database.StringSet("redisIntKey", 30);
Console.WriteLine("intSetResult " + intSetResult);

var intGetResult = database.StringGet("redisIntKey");
Console.WriteLine("intGetResult " + intGetResult);

var decRes = database.StringDecrement("redisIntKey");
Console.WriteLine("decRes " + decRes);

var decGetResult = database.StringGet("redisIntKey");
Console.WriteLine("int get res after dec: " + decGetResult);

var incrRes = database.StringIncrement("redisIntKey");
Console.WriteLine("incrRes " + incrRes);

var incrGetResult = database.StringGet("redisIntKey");
Console.WriteLine("int get res after incr: " + incrGetResult);

Console.WriteLine("is value int: " + incrGetResult);

var intLength = database.StringLength("redisIntKey");
Console.WriteLine("intLength " + intLength);

var deleteRes = database.StringGetDelete("redisIntKey");
Console.WriteLine("deleteRes has value? " + deleteRes.HasValue);
Console.WriteLine("deleteRes " + deleteRes);

var getDeletedVal = database.StringGet("redisIntKey");

Console.WriteLine("get deleted val has value? " + getDeletedVal.HasValue);
Console.WriteLine("get deleted val is null? " + getDeletedVal.IsNull);
Console.WriteLine("get deleted val is int? " + getDeletedVal.IsInteger);

#endregion

Console.WriteLine();
Console.WriteLine("             hashValue");

#region hash

var bob = new Customer
{
    Age = 100,
    Email = "foo@bar.com",
    FirstName = "Bob",
    LastName = "Smith"
};

var tom = new Customer
{
    Age = 70,
    Email = "foo2@bar.com",
    FirstName = "Tom",
    LastName = "Smitttth"
};

var customers = new List<Customer> { bob, tom };
const string rootKey = "people";

var customersToAdd = customers
    .Select(customer => new HashEntry(customer.FirstName.ToString(), JsonConvert.SerializeObject(customer)))
    .ToArray();
database.HashSet(rootKey, customersToAdd);
database.KeyExpire(rootKey, DateTime.UtcNow.AddMinutes(2));

var getCustomersResult = database.HashValues(rootKey);
Console.WriteLine(string.Join(", ",
    getCustomersResult.Select(redisValue => JsonConvert.DeserializeObject<Customer>(redisValue!))
        .Select(customer => $"{customer!.FirstName} {customer.Email}")));


var deleteResponse = database.HashDelete(
    rootKey,
    new[] { new RedisValue("Bob") }
);
Console.WriteLine("deleteResponse of bob: " + deleteResponse);

getCustomersResult = database.HashValues(rootKey);
Console.WriteLine(string.Join(", ",
    getCustomersResult.Select(redisValue => JsonConvert.DeserializeObject<Customer>(redisValue!))
        .Select(customer => $"{customer!.FirstName} {customer.Email}")));

//var re=database.KeyDelete(new RedisKey("peoplessssss"));
//Console.WriteLine(re);

var exists = database.KeyExists(new RedisKey(rootKey));
Console.WriteLine("root key exists? " + exists);
var ttl = database.KeyTimeToLive(new RedisKey(rootKey));
Console.WriteLine("ttl " + ttl);

var deletedNotExistedKey = database.KeyDelete(new[] { new RedisKey(rootKey), new RedisKey("peoplesssssrs") });
Console.WriteLine("deletedNotExistedKey " + deletedNotExistedKey);

exists = database.KeyExists(new RedisKey(rootKey));
Console.WriteLine("root key exists? " + exists);

var keys = conn.GetServers().Single().Keys();
Console.WriteLine("all keys: " + string.Join(", ", keys));


/*
 127.0.0.1:6379> hgetall people
   1) "Tom"
   2) "{\"FirstName\":\"Tom\",\"LastName\":\"Smitttth\",\"Email\":\"foo2@bar.com\",\"Age\":70}"
   3) "Bob"
   4) "{\"FirstName\":\"Bob\",\"LastName\":\"Smith\",\"Email\":\"foo@bar.com\",\"Age\":100}"
 */

class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

#endregion