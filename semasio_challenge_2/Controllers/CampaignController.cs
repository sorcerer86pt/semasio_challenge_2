using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using semasio_challenge_2.Models;
using semasio_challenge_2.Services;

namespace semasio_challenge_2.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {

        private readonly CampaignService _campaignService;
        const string folderName = "files";
        readonly string folderPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        public CampaignController(CampaignService campaignService)
        {
            _campaignService = campaignService;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
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
        [HttpPost("/Import", Name = "Import")]
        public async Task<IActionResult> Import([FromBody] IFormFile campaignFile)
        {
            string resultFilePath = Path.Combine(folderPath, campaignFile.FileName);
            using (var fileContentStream = new MemoryStream())
            {
                await campaignFile.CopyToAsync(fileContentStream);
                await System.IO.File.WriteAllBytesAsync(resultFilePath, fileContentStream.ToArray());
            }
            var campaigns = await _campaignService.Import(resultFilePath);
            return CreatedAtRoute("Get", campaigns);
        }

        // GET: api/Campaign/DistributeEqually/{ID}
        [HttpGet("/DistributeEqually/{id}", Name = "DistributeEqually")]
        public async Task DistributeCampaignBudger(string id)
        {
            await _campaignService.DistributeBudget(id);
        }

        //GET: api/Campaign/ExportJson
        [HttpGet("/ExportJson", Name = "ExportJson")]
        public async Task ExportJson()
        {
            var tst = await _campaignService.ExportAsJsonString();
            Response.ContentType = "text/plain";
            await Response.SendFileAsync(tst);
            System.IO.File.Delete(tst);
        }

        [HttpPost("/ObtainBestOnlineStrategy", Name = "ObtainBestOnlineStrategy")]
        public async Task<ActionResult<string>> ObtainBestOnlineStrategy([FromBody] OnlineStrategyParameters parameters)
        {
            string result = await _campaignService.GetBestOnlineFromParams(parameters);
            return Ok(result);
        } 

    }
}
