using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace semasio_challenge_2.Models
{
    public class Campaign
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set; }

        public string Name {get; set;}

        public Strategy[] Strategies {get; set;}

        public int CampaignBudget {get; set;}

        
    }
}