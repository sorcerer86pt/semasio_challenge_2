using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using semasio_challenge_2.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System;
using System.Security.Cryptography;

namespace semasio_challenge_2.Services
{
    public class CampaignService
    {
        private readonly IMongoCollection<Campaign> _campaigns;
        private readonly ConsoleLogger _consoleLogger;

        public CampaignService(IConfiguration config)
        {
            MongoClient client = new MongoClient(config.GetConnectionString("CampaignsDB"));
            var database = client.GetDatabase("CampaignDB");
            _campaigns = database.GetCollection<Campaign>("Campaigns");
            _consoleLogger = new ConsoleLogger();
        }

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

        public async Task<List<Campaign>> Import(List<Campaign> campaigns)
        {
            await _campaigns.InsertManyAsync(campaigns);
            return campaigns;
        }

        public async Task<string> ExportAsJsonString()
        {
            return await GetCampaignJsonAsync();

        }

        public async Task<Campaign> Update(string id, Campaign campaign)
        {
            await _campaigns.ReplaceOneAsync(cpgupd => cpgupd.Id == id, campaign);
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

        public async Task<string> GetBestOnlineFromParams(OnlineStrategyParameters parameters)
        {
            return await Task.Run(() => GetBestOnlineStrategy(parameters));
        }

        private string GetBestOnlineStrategy(OnlineStrategyParameters parameters)
        {
            string bestStrategy = null;
            Campaign cpg = _campaigns.Find(cpg => cpg.Id == parameters.Id).FirstOrDefault();
           
            
            foreach(Strategy strategy in cpg.Strategies)
            {
                if( strategy.StrategyType == StrategyType.Online && 
                    strategy.ExtraElements.URL == parameters.URL && 
                    strategy.StrategyBudget >= parameters.Budget)
                {
                    bestStrategy = strategy.StrategyName;
                    int remainingBudget = strategy.StrategyBudget - parameters.Budget;

                    FilterDefinition<Campaign> filter = Builders<Campaign>.Filter.And(
                        Builders<Campaign>.Filter.Eq(cpg=> cpg.Id, parameters.Id),
                        Builders<Campaign>.Filter.ElemMatch<Strategy>(cpg=> cpg.Strategies, x=>x.StrategyName == bestStrategy)

                        );

                    UpdateDefinition<Campaign> update = Builders<Campaign>.Update.Set(cpg => cpg.Strategies[-1].StrategyBudget, remainingBudget);
                    
                    _campaigns.FindOneAndUpdateAsync(filter, update);
                    
                    break;
                }
            }
            

            return bestStrategy;
        }

        private async Task AsyncDistributeBudget(string id)
        {
            await Task.Run(() => DivideBudgetEqually(id));


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
            int numStrategies = campaignRecord.Strategies.Count;

            int equalBudget = campaignBudget / numStrategies;
            _consoleLogger.LogMessage($"Got Budget: {campaignBudget}, Got Number Strategies: {numStrategies}, Divided Budget: {equalBudget}", LogMessageLevel.Information);

            var campaignFilter = Builders<Campaign>.Filter.Eq(cpg => cpg.Id, campaignID)
                 & Builders<Campaign>.Filter.ElemMatch<Strategy>(cpg => cpg.Strategies, x => x.StrategyBudget != equalBudget);


            var strategyUpdate = Builders<Campaign>.Update.Set(cpg => cpg.Strategies[-1].StrategyBudget, equalBudget);
            var campaignUpdate = Builders<Campaign>.Update.Set(cpg => cpg.CampaignBudget, 0);

            _consoleLogger.LogMessage($"strategy Update Instruction = {strategyUpdate.ToJson()}", LogMessageLevel.Information);
            _consoleLogger.LogMessage($"campaign Update Instruction = {campaignUpdate.ToJson()}", LogMessageLevel.Information);

            // update the each strategy budget to equal value
            var resultStrategy = _campaigns.FindOneAndUpdateAsync<Campaign>(campaignFilter, strategyUpdate);

            // update the campaign budget to 0, since we've distributed all the budget to the strategies
            var resultCampaign = _campaigns.FindOneAndUpdateAsync<Campaign>(campaignFilter, campaignUpdate);


        }

       

        private string ReturnCampaignAsJson()
        {
            var jsonCampaign = _campaigns.ToJson();
            return jsonCampaign;
        }


    }
}