using Newtonsoft.Json;

namespace semasio_challenge_2.Models
{
    public class ChannelSlot
    {

        [JsonProperty("channelName")]
        public string ChannelName { get; set; }

        [JsonProperty("fromHour")]
        public string FromHour { get; set; }

        [JsonProperty("toHour")]
        public string ToHour { get; set; }
    }
}