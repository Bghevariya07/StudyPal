using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class UserService
{
    private readonly IMongoCollection<UserProfile> _userProfiles;

    public UserService(IMongoClient mongoClient, string databaseName, string collectionName)
    {
        var database = mongoClient.GetDatabase(databaseName);
        _userProfiles = database.GetCollection<UserProfile>(collectionName);
    }

    public async Task<List<UserProfile>> GetUserProfilesByDetail(string detail)
    {
        return await _userProfiles.Find(profile => profile.FirstName.ToLower() == detail.ToLower() || profile.LastName.ToLower() == detail.ToLower() 
            || profile.Email.ToLower() == detail.ToLower() || profile.Username.ToLower() == detail.ToLower()).ToListAsync();
    }

    public async Task<List<UserProfile>> GetAllUserProfiles()
    {
        return await _userProfiles.Find(_ => true).ToListAsync();
    }
}
