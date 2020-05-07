using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace semasio_challenge_2.Models
{
    public class Campaign
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set; }

        public string Name {get; set;}

        public Strategy[] StrategyList {get; set;}

        public int CampaignBudget {get; set;}

        
    }
}