using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Protocol;
using System.Data;

namespace IpInfoService
{
    [Route("api/ipinfo")]
    [ApiController]
    public class IpInfoController : Controller
    {
        private readonly NovibetContext context;
        private readonly IMemoryCache cache;
        private readonly IConfiguration configuration;

        public IpInfoController(NovibetContext context, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this.context = context;
            cache = memoryCache;
            this.configuration = configuration;
        }

        [HttpGet("getinfo")]
        public async Task<IActionResult> GetInfo(string address)
        {
            if (String.IsNullOrEmpty(address) || context.IpInfos == null)
                return BadRequest("Address cannot be empty.");

            if (!IpInfo.TryPack(address, out byte[] packed))
                return BadRequest("Address is not in a valid format.");

            uint dec = BitConverter.ToUInt32(packed, 0);
            BasicIpInfo? response = GetCachedResponse(dec);
            if (response != null)
            {
                Console.WriteLine("Cache hit");
                return Ok(response.Value.ToJson());
            }

            var ipInfo = await GetPersistedIpInfo(packed);
            if (ipInfo != null)
            {
                Console.WriteLine("Database hit");
                response = new BasicIpInfo(ipInfo);
                CacheResponse(dec, response.Value);
                return Ok(response.Value.ToJson());
            }

            Console.WriteLine("External API fallback");
            try { response = await Ip2c.Get(dec); }
            catch { throw; }
            await PersistIpInfo(address, packed, response.Value);
            CacheResponse(dec, response.Value);
            return Ok(response.Value.ToJson());
        }
        
        [HttpGet("report")]
        public async Task<IActionResult> Report([FromBody] string[] codes)
        {
            var response = new List<CountrySummary>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("IpInfoServiceContext")))
            {
                using (SqlCommand cmd = new SqlCommand("Country_GetIpCounts", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    bool all = codes.Length == 0;
                    cmd.Parameters.Add("@twoLetterCodes", SqlDbType.VarChar).Value = all ? null : String.Join(',', codes);

                    con.Open();
                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        response.Add(new CountrySummary
                        {
                            CountryName = reader.GetString("CountryName"),
                            AddressCount = reader.GetInt32("AddressCount"),
                            AddressLastUpdated = reader.GetDateTime("AddressLastUpdated")
                        });
                    }
                    return Ok(response.ToJson());
                }
            }
        }
        
        private void CacheResponse(uint address, BasicIpInfo response)
        {
            // Can't key the cache with byte array
            cache.Set(address, response, DateTime.Now + new TimeSpan(1, 0, 0));
        }
        
        private BasicIpInfo? GetCachedResponse(uint address)
        {
            return cache.TryGetValue(address, out BasicIpInfo response) ? response : null;
        }
        
        private async Task PersistIpInfo(string address, byte[] packed, BasicIpInfo response)
        {
            var country = await NovibetQueries.GetOrAddCountryByBasicIpInfo(context, response);

            var info = new IpInfo()
            {
                Address = address,
                Country = country,
                PackedAddress = packed
            };
            await context.IpInfos.AddAsync(info);
            await context.SaveChangesAsync();
        }
        
        private async Task<IpInfo?> GetPersistedIpInfo(byte[] packedAddress)
        {
            return await context.IpInfos
            .Include(i => i.Country)
            .FirstOrDefaultAsync(info => info.PackedAddress == packedAddress);
        }
    }
}
