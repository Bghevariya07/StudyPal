using MongoDB.Driver;
using server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChatService
{
    private readonly IMongoCollection<ChatMessage> _messages;

    public ChatService(IMongoDatabase database)
    {
        _messages = database.GetCollection<ChatMessage>("Messages");
    }

    public async Task<List<ChatMessage>> GetMessagesForUserAsync(string userId)
    {
        return await _messages.Find(message => message.SenderId == userId || message.ReceiverId == userId).ToListAsync();
    }

    public async Task<List<ChatMessage>> GetAllMessagesAsync()
    {
        return await _messages.Find(_ => true).ToListAsync();
    }

    public async Task SendMessageAsync(ChatMessage message)
    {
        await _messages.InsertOneAsync(message);
    }
}

