using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace semasio_challenge_2.Models
{
    public class ExtraElements
    {
        [JsonProperty("URL")]
        [BsonIgnoreIfNull]
        public string URL { get; set; }

        [BsonIgnoreIfNull]
        [JsonProperty("ChannelSlots")]
        public List<ChannelSlot> ChannelSlots { get; set; }

        [BsonIgnoreIfNull]
        [JsonProperty("Latitude")]
        public double Latitude { get; set; }

        [BsonIgnoreIfNull]
        [JsonProperty("Longitude")]
        public double Longitude { get; set; }
    }
}