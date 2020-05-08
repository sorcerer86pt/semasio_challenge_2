using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using semasio_challenge_2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace semasio_challenge_2.Services
{
	public class CampaignService
    {
        private readonly IMongoCollection<Campaign> _campaigns;

        public CampaignService(IConfiguration config)
        {
            MongoClient client = new MongoClient(config.GetConnectionString("CampaignsDB"));
            var database = client.GetDatabase("CampaignDB");
			_campaigns = database.GetCollection<Campaign>("Campaigns");        }

        /**
         * Returns all campaigns
         */
        public async Task<List<Campaign>> Get() => await _campaigns.Find(cpg => true).ToListAsync();

        public async Task<Campaign> Get(string campaignId)
        {
            return await _campaigns.Find(cpg => cpg.Id == Guid.Parse(campaignId)).FirstOrDefaultAsync();
        }

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

        public async Task<string> ExportAsJsonString()
        {
            return  await GetCampaignJsonAsync();

        }

        public async Task<Campaign> Update(string id, Campaign campaign)
        {
            await _campaigns.ReplaceOneAsync(cpgupd => cpgupd.Id == Guid.Parse(id), campaign);
            return campaign;
        }

        public async Task Remove(string id)
        {
            await _campaigns.DeleteOneAsync(cpgdel => cpgdel.Id == Guid.Parse(id));
        }

        public async Task DistributeBudget(string id)
        {
            await AsyncDistributeBudget(id);
        }


        private async Task AsyncDistributeBudget(string id)
        {
           await  Task.Run(() => DivideBudgetEqually(id));


        }

        private async Task<string> GetCampaignJsonAsync()
        {
            return await Task.Run(() => ReturnCampaignAsJson());
        }

        /**
         * Divide the budget of a campaign equally between all defined strategies for that campaign
         * so if a campaign has 3000 of budget and 3 strategies, 
         * each strategy would get 1000, and the campaign budget is set to 0
         */
        private void DivideBudgetEqually(string campaignID)
        {
            Campaign campaignRecord = _campaigns.Find(cpg => cpg.Id == Guid.Parse(campaignID)).FirstOrDefault();
            int campaignBudget = campaignRecord.CampaignBudget;
            int numStrategies = campaignRecord.StrategyList.Length;

            int equalBudget = campaignBudget / numStrategies;

            // let mongodb do the array work :-)
            var update = Builders<Campaign>.Update.Set(cpg=> cpg.StrategyList[-1].StrategyBudget, equalBudget);
            _campaigns.UpdateManyAsync(cpg => cpg.Id == Guid.Parse(campaignID), update);

            // update the campaign budget to 0, since we've distributed all the budget to the strategies
            var campaignUpdate = Builders<Campaign>.Update.Set(cpg=> cpg.CampaignBudget, 0);
            _campaigns.UpdateOneAsync(cpg => cpg.Id == Guid.Parse(campaignID), campaignUpdate);
            

        }

        private string ReturnCampaignAsJson()
        {
            var jsonCampaign = _campaigns.ToJson();
            return jsonCampaign;
        }

       
    }
}