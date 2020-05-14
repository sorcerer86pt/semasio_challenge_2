using Newtonsoft.Json;

namespace semasio_challenge_2.Models
{
    public class OutdoorStrategy : Strategy
    {
        public OutdoorStrategy( Strategy strategy)
        {
            StrategyName = strategy.StrategyName;
            StrategyType = strategy.StrategyType;
            StrategyBudget = strategy.StrategyBudget;
            ExtraElements = strategy.ExtraElements;
            Latitude = strategy.ExtraElements.Latitude;
            Longitude = strategy.ExtraElements.Longitude;
        }

        [JsonProperty("Latitude")]
        public double Latitude { get; set; }

        [JsonProperty("Longitude")]
        public double Longitude { get; set; }
    }
}