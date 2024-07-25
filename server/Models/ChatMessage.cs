﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

public class ChatMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required]
    [BsonElement("senderId")]
    public string SenderId { get; set; }

    [Required]
    [BsonElement("receiverId")]
    public string ReceiverId { get; set; }

    [BsonElement("message")]
    public string Message { get; set; }

    [Required]
    [BsonElement("time")]
    public DateTime Time { get; set; }
}

