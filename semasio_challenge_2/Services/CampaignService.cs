using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using semasio_challenge_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace semasio_challenge_2.Services
{
	public class CampaignService
    {
        private readonly IMongoCollection<Campaign> _campaigns;
        private readonly IMongoCollection<Strategy> _strategies;

        public CampaignService(IConfiguration config)
        {
            MongoClient client = new MongoClient(config.GetConnectionString("CampaignsDB"));
            var database = client.GetDatabase("CampaignDB");
			_campaigns = database.GetCollection<Campaign>("Campaigns");
            _strategies = database.GetCollection<Strategy>("Strategies");
        }

        /**
         * Returns all campaigns
         */
        public async Task<List<Campaign>> Get() => await _campaigns.Find(cpg => true).ToListAsync();

        public async Task<Campaign> Get(string campaignId) => await _campaigns.Find(cpg => cpg.Id == campaignId).FirstOrDefaultAsync();

        public async Task<Campaign> Create(Campaign campaign)
        {
            await _campaigns.InsertOneAsync(campaign);
            return campaign;
        }

        public async Task <List<Campaign>> Import(List<Campaign> campaigns)
        {
            await _campaigns.InsertManyAsync(campaigns);
            return campaigns;
        }

        public async Task<JsonDocument> Export()
        {
            
        }

        public async Task<Campaign> Update(string id, Campaign campaign)
        {
            await _campaigns.ReplaceOneAsync(cpgupd => cpgupd.Id == id, campaign);
            return campaign;
        }

        public async Task Remove(string id)
        {
            await _campaigns.DeleteOneAsync(cpgdel => cpgdel.Id == id);
        }

        public async Task DistributeBudget(string id)
        {
            await AsyncDistributeBudget(id);
        }


        private async Task AsyncDistributeBudget(string id)
        {
           await  Task.Run(() => DivideBudgetEqually(id));


        }

        private void DivideBudgetEqually(string campaignID)
        {
            Campaign campaignRecord = _campaigns.Find(cpg => cpg.Id == campaignID).FirstOrDefault();
            int campaignBudget = campaignRecord.CampaignBudget;
            int numStrategies = campaignRecord.StrategyList.Length;
            Strategy[] strategiesInCampaign = campaignRecord.StrategyList;

            int equalBudget = campaignBudget / numStrategies;
            var strategies = campaignRecord.StrategyList;
            // since the value is the same, we can set it up here and reuse it
            var update = Builders<Strategy>.Update.Set("StrategyBudget", equalBudget);

            // goes for each strategy in the campaign strategy list and updates its budget
            foreach ( Strategy strategy in strategiesInCampaign)
            {
                string strategyId = strategy.Id;
                _strategies.UpdateOneAsync(strategyUpd => strategyUpd.Id == strategy.Id, update);

            }

            // update the campaign budget to 0, since we've distributed all the budget to the strategies
            var campaignUpdate = Builders<Campaign>.Update.Set("CampaignBudget", 0);
            _campaigns.UpdateOneAsync(cpg => cpg.Id == campaignID, campaignUpdate);
            

        }

    }
}