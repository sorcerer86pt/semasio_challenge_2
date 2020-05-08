using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace semasio_challenge_2.Models
{
    public class Campaign
    {
        [BsonId(IdGenerator = typeof(GuidGenerator))]
        [BsonRepresentation(BsonType.String)]
        public Guid Id {get; set; }

        public string Name {get; set;}

        public Strategy[] StrategyList {get; set;}

        public int CampaignBudget {get; set;}

        
    }
}