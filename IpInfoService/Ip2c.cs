using RestSharp;

namespace IpInfoService
{
    public static class Ip2c
    {
        private const string baseUrl = "https://ip2c.org/";
        private const string decQuerystring = "?dec={0}";
        public static async Task<BasicIpInfo> Get(uint address)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(String.Format(decQuerystring, address));
            var result = await client.GetAsync(request);
            try
            {
                var parts = result.Content.Split(';');

                string code = parts[0];
                if (code == "0")
                    throw new Exception("Wrong ip2c input.");

                if (code != "1" && code != "2")
                    throw new Exception("Unexpected ip2c output.");

                var response = new BasicIpInfo
                {
                    TwoLetterCode = parts[1],
                    ThreeLetterCode = parts[2],
                    CountryName = parts[3]
                };
                return response;
            }
            catch { throw; }
            finally { client.Dispose(); }
        }
    }
}
