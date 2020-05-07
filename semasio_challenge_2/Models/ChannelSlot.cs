using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace semasio_challenge_2.Models
{
    public class ChannelSlot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set; }
        public string ChannelName {get; set;} 

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }
    }
}