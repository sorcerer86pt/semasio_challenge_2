using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace semasio_challenge_2.Models
{
    public class Campaign
    {
        [BsonConstructor]
        public Campaign()
        {
        }

        [BsonConstructor]
        public Campaign(string name, List<Strategy> strategies, int campaignBudget)
        {
            Name = name;
            CampaignBudget = campaignBudget;
            Strategies = new List<Strategy>();
            foreach(Strategy strategy in strategies)
            {
                switch (strategy.StrategyType)
                {
                    case StrategyType.Online:
                        OnlineStrategy onlineStrategy = new OnlineStrategy(strategy);
                        Strategies.Add(onlineStrategy);
                        break;

                    case StrategyType.Outdoor:
                        OutdoorStrategy outdoorStrategy = new OutdoorStrategy(strategy);
                        Strategies.Add(outdoorStrategy);
                        break;

                    case StrategyType.TV:
                        TvStrategy tvStrategy = new TvStrategy(strategy);
                        Strategies.Add(tvStrategy);
                        break;

                    default:
                        Strategies.Add(strategy);
                        break;

                }
                    
            }
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("strategies")]
        public List<Strategy> Strategies { get; set; }

        [JsonProperty("campaignBudget")]
        public int CampaignBudget { get; set; }



    }
}