using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


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
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set; }

        [BsonRepresentation(BsonType.String)]
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