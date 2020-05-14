using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using semasio_challenge_2.Models;
using semasio_challenge_2.Services;

namespace semasio_challenge_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {

        private readonly CampaignService _campaignService;

        public CampaignController(CampaignService campaignService)
        {
            _campaignService = campaignService;
        }


        // GET: api/Campaign
        [HttpGet]
        public async Task<ActionResult<List<Campaign>>> Get()
        {
            return await _campaignService.Get();
        }

        // GET: api/Campaign/{ID}
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<Campaign>> Get(string id)
        {
            return await _campaignService.Get(id);
        }

        // POST: api/Campaign
        [HttpPost]
        public async Task<ActionResult<Campaign>> Create([FromBody] Campaign new_capaign)
        {
            await _campaignService.Create(new_capaign);
            return CreatedAtRoute("Get", new { id = new_capaign.Id.ToString() }, new_capaign);

        }


        // PUT: api/Campaign/{ID}
        [HttpPut("{id}")]
        public async Task<ActionResult<Campaign>> Put(string id, [FromBody] Campaign campaign)
        {
            var cpg = await _campaignService.Get(id);
            if (cpg == null)
            {
                return NotFound();
            }
            campaign.Id = cpg.Id;

            await _campaignService.Update(id, campaign);
            return CreatedAtRoute("Get", new { id = campaign.Id.ToString() }, campaign);
        }

        // DELETE: api/Campaign/{ID}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Campaign>> Delete(string id)
        {
            var s = await _campaignService.Remove(id);
            if (s == null)
            {
                return NotFound();
            }

            return NoContent();

        }

        // POST: api/Campaign/Import
        //
        [HttpPost("/Import")]
        public async Task<ActionResult<List<Campaign>>> Import([FromBody] List<Campaign> campaigns)
        {
            await _campaignService.Import(campaigns);
            return CreatedAtRoute("Get", campaigns);
        }

        // GET: api/Campaign/DistributeEqually/{ID}
        [HttpGet("/DistributeEqually/{id}")]
        public async Task DistributeCampaignBudger(string id)
        {
            await _campaignService.DistributeBudget(id);
        }

        //GET: api/Campaign/ExportJson
        [HttpGet("/ExportJson")]
        public async Task<ActionResult<string>> ExportJson()
        {
            var tst = await _campaignService.ExportAsJsonString();
            return Ok(tst);
        }

        [HttpPost("/ObtainBestOnlineStrategy")]
        public async Task<ActionResult<string>> ObtainBestOnlineStrategy([FromBody] OnlineStrategyParameters parameters)
        {
            string result = await _campaignService.GetBestOnlineFromParams(parameters);
            return Ok(result);
        } 

    }
}
