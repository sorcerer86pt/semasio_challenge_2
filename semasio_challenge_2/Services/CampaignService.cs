using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using semasio_challenge_2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
        public async Task<List<Campaign>> Get() => await _campaigns.Find(Builders<Campaign>.Filter.Empty).ToListAsync();

        public async Task<Campaign> Get(string campaignId)
        {
            return await _campaigns.Find(cpg => cpg.Id == campaignId).FirstOrDefaultAsync();
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
            await _campaigns.ReplaceOneAsync(cpgupd => cpgupd.Id ==id , campaign);
            return campaign;
        }

        public async Task<DeleteResult> Remove(string id)
        {
            var filter = Builders<Campaign>.Filter.Eq(cpg => cpg.Id, id);
            return await _campaigns.DeleteOneAsync(filter);
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
            Campaign campaignRecord = _campaigns.Find(cpg => cpg.Id == campaignID).FirstOrDefault();
            int campaignBudget = campaignRecord.CampaignBudget;
            int numStrategies = campaignRecord.Strategies.Length;

            int equalBudget = campaignBudget / numStrategies;

            var campaignFilter = Builders<Campaign>.Filter.Eq(cpg => cpg.Id ,campaignID);


            var campaignUpdate = Builders<Campaign>.Update.Set(cpg=> cpg.CampaignBudget, 0);
            var strategyUpdate = Builders<Campaign>.Update.Set(cpg => cpg.Strategies[-1].StrategyBudget, equalBudget);

            // update the each strategy budget to equal value
            _campaigns.UpdateMany(campaignFilter, strategyUpdate);

            // update the campaign budget to 0, since we've distributed all the budget to the strategies
            _campaigns.UpdateOne(campaignFilter, campaignUpdate);
            

        }

        private string ReturnCampaignAsJson()
        {
            var jsonCampaign = _campaigns.ToJson();
            return jsonCampaign;
        }

       
    }
}