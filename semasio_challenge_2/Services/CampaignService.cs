using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using semasio_challenge_2.Models;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

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

        #region Public Interface

        /**
         * Returns all campaigns
         */
        public async Task<List<Campaign>> Get() => await _campaigns.Find(Builders<Campaign>.Filter.Empty).ToListAsync();

        /**
         * Return one campaign
         */
        public async Task<Campaign> Get(string campaignId)
        {
            return await _campaigns.Find(cpg => cpg.Id == campaignId).FirstOrDefaultAsync();
        }

        /**
         * Insert a Campaign Record
         */
        public async Task<Campaign> Create(Campaign campaign)
        {
            await _campaigns.InsertOneAsync(campaign);
            return campaign;
        }

        /**
         * Given a filePath attempts to read the file and enter as many records as possible
         */
        public async Task<List<Campaign>> Import(string filePath)
        {
            List<Campaign> insertedCampaigns = new List<Campaign>();

            using (var streamReader = new StreamReader(filePath))
            {
                string line;
                while ((line = await streamReader.ReadLineAsync()) != null)
                {
                    using (var jsonReader = new JsonReader(line))
                    {
                        var context = BsonDeserializationContext.CreateRoot(jsonReader);
                        var document = _campaigns.DocumentSerializer.Deserialize(context);
                        await _campaigns.InsertOneAsync(document);
                        insertedCampaigns.Add(document);
                    }
                }
            }
            return insertedCampaigns;
            
        }

        /**
         * Exports the campaigns as Json
         */
        public async Task<string> ExportAsJsonString()
        {
            return await GenerateExportJsonFile();

        }

        /**
         * Update (Replace) the campaign
         */
        public async Task<Campaign> Update(string id, Campaign campaign)
        {
            await _campaigns.ReplaceOneAsync(cpgupd => cpgupd.Id == id, campaign);
            return campaign;
        }

        /**
         * Delete a Campaign
         */
        public async Task<DeleteResult> Remove(string id)
        {
            var filter = Builders<Campaign>.Filter.Eq(cpg => cpg.Id, id);
            return await _campaigns.DeleteOneAsync(filter);
        }

        /**
         * Distributes the Campaign Budget equally along all defined strategies for that campaign
         */
        public async Task DistributeBudget(string id)
        {
            await AsyncDistributeBudget(id);
        }

        /**
         * Returns the best online strategy according to the given params and update it's strategy budget
         */
        public async Task<string> GetBestOnlineFromParams(OnlineStrategyParameters parameters)
        {
            return await Task.Run(() => GetBestOnlineStrategy(parameters));
        }

        #endregion

        #region Private methods
        private string GetBestOnlineStrategy(OnlineStrategyParameters parameters)
        {
            string bestStrategy = null;
            Campaign cpg = _campaigns.Find(cpg => cpg.Id == parameters.Id).FirstOrDefault();


            foreach (Strategy strategy in cpg.Strategies)
            {
                if (strategy.StrategyType == StrategyType.Online &&
                    strategy.ExtraElements.URL == parameters.URL &&
                    strategy.StrategyBudget >= parameters.Budget)
                {
                    bestStrategy = strategy.StrategyName;
                    int remainingBudget = strategy.StrategyBudget - parameters.Budget;

                    FilterDefinition<Campaign> filter = Builders<Campaign>.Filter.And(
                        Builders<Campaign>.Filter.Eq(cpg => cpg.Id, parameters.Id),
                        Builders<Campaign>.Filter.ElemMatch<Strategy>(cpg => cpg.Strategies, x => x.StrategyName == bestStrategy)

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

        private async Task<string> GenerateExportJsonFile()
        {

            string outputFileName = Path.GetTempFileName();
            using (var streamWriter = new StreamWriter(outputFileName))
            {
                await _campaigns.Find(new BsonDocument())
                    .ForEachAsync(async (document) =>
                    {
                        using (var stringWriter = new StringWriter())
                        using (var jsonWriter = new JsonWriter(stringWriter))
                        {
                            var context = BsonSerializationContext.CreateRoot(jsonWriter);
                            _campaigns.DocumentSerializer.Serialize(context, document);
                            var line = stringWriter.ToString();
                            await streamWriter.WriteLineAsync(line);
                        }
                    });
            }
            return outputFileName;


        }


        /**
         * Divide the budget of a campaign equally between all defined strategies for that campaign
         * so if a campaign has 3000 of budget and 3 strategies, 
         * each strategy would get 1000, and the campaign budget is set to 0
         */
        private async void DivideBudgetEqually(string campaignID)
        {
            Campaign campaignRecord = _campaigns.Find(cpg => cpg.Id == campaignID).FirstOrDefault();
            int campaignBudget = campaignRecord.CampaignBudget;
            int numStrategies = campaignRecord.Strategies.Count;
            if (campaignBudget == 0)
            {
                return;
            }
            int equalBudget = campaignBudget / numStrategies;
            _consoleLogger.LogMessage($"Got Budget: {campaignBudget}, Got Number Strategies: {numStrategies}, Divided Budget: {equalBudget}", LogMessageLevel.Information);

            var campaignFilter = Builders<Campaign>.Filter.Eq(cpg => cpg.Id, campaignID);
            // mongo c# driver has yet to have a fluent syntax for Array Filters.
            var updateDefinition = Builders<Campaign>.Update.Set("Strategies.$[stra].StrategyBudget", equalBudget);

            var arrayFilterStratergy = new List<ArrayFilterDefinition>();
            arrayFilterStratergy.Add(
                 new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("stra.StrategyBudget", new BsonDocument("$gte", 0)))
            );

            // update the each strategy budget to equal value
            var resultStrategy = await _campaigns.UpdateOneAsync(
                campaignFilter,
                updateDefinition,
                new UpdateOptions { ArrayFilters = arrayFilterStratergy });

            if (resultStrategy.MatchedCount > 0)
            {
                _consoleLogger.LogMessage($"Matched {resultStrategy.ModifiedCount} records");
            }

            var campaignUpdate = Builders<Campaign>.Update.Set(cpg => cpg.CampaignBudget, 0);
            // update the campaign budget to 0, since we've distributed all the budget to the strategies
            var resultCampaign = await _campaigns.FindOneAndUpdateAsync<Campaign>(campaignFilter, campaignUpdate);


        }


        #endregion



    }
}