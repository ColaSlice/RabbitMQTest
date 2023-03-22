using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Publisher.Client;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlowerController : ControllerBase
    {
        private Rpc _rpc = new Rpc();
        
        // GET: api/Flower
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Flower/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Flower
        [HttpPost]
        public async Task<string> Post(string value)
        {
            var response = await _rpc.Main(value);
            Console.WriteLine(response);
            return response;
        }

        // PUT: api/Flower/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Flower/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
