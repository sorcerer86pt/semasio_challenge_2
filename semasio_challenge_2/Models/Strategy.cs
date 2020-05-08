using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace semasio_challenge_2.Models
{

    
    public enum StrategyType
    {
        Online,
        TV,
        Outdoor
    }

    [BsonDiscriminator(Required = true, RootClass = true)]
    [BsonKnownTypes(typeof(OnlineStrategy), typeof(TvStrategy), typeof(OutdoorStrategy))]
    public class Strategy
    {
        [BsonId(IdGenerator = typeof(GuidGenerator))]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]  // JSON.Net
        [BsonRepresentation(BsonType.String)]  // Mongo
        public StrategyType StrategyType {get; set;}

        public int StrategyBudget {get; set;}



    }

    public class OnlineStrategy: Strategy
    {
        public string URL {get; set;}

    }

    public class TvStrategy: Strategy
    {
        public ChannelSlot[] ChannelSlots { get; set; }
    }

    public class OutdoorStrategy: Strategy
    {
        public double Latitude {get; set;}
        public double Longitude {get; set;}
    }
}