using System.Collections.Generic;

namespace semasio_challenge_2.Models
{
    public class TvStrategy : Strategy
    {
        public TvStrategy(Strategy strategy)
        {
            StrategyType = strategy.StrategyType;
            StrategyBudget = strategy.StrategyBudget;
            ExtraElements = strategy.ExtraElements;
            ChannelSlots = strategy.ExtraElements.ChannelSlots;




        }
        public  List<ChannelSlot> ChannelSlots { get; set; }


    }
}