using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace semasio_challenge_2.Models
{


    public enum StrategyType
    {
        Online,
        TV,
        Outdoor
    }

    [BsonDiscriminator("StrategyType", RootClass = true, Required = true)]
    [BsonKnownTypes(
        typeof(OnlineStrategy), 
        typeof(TvStrategy), 
        typeof(OutdoorStrategy))]
    public class Strategy
    {

        [JsonConverter(typeof(StringEnumConverter))]  // JSON.Net
        [BsonRepresentation(BsonType.String)]  // Mongo
        public  StrategyType StrategyType { get; set; }
        
        [JsonProperty("strategyBudget")]
        public  int StrategyBudget { get; set; }

        [JsonProperty("extraElements")]
        public ExtraElements ExtraElements { get; set; }
    }
}