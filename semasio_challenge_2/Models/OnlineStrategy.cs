using Newtonsoft.Json;

namespace semasio_challenge_2.Models
{
    public class OnlineStrategy : Strategy
    {
        public OnlineStrategy(Strategy strategy)
        {
            StrategyName = strategy.StrategyName;
            StrategyBudget = strategy.StrategyBudget;
            StrategyType = strategy.StrategyType;
            ExtraElements = strategy.ExtraElements;
            URL = strategy.ExtraElements.URL;
        }

        [JsonProperty("URL")]
        public string URL { get; set; }

    }
}