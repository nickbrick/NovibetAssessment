using Microsoft.EntityFrameworkCore;

namespace IpInfoService
{
    public static class NovibetQueries
    {
        public static async Task<Country> GetOrAddCountryByBasicIpInfo(NovibetContext context, BasicIpInfo basicInfo)
        {
            var country = await context.Countries
                .SingleOrDefaultAsync(country => country.Name == basicInfo.CountryName);
            country ??= new Country(basicInfo);
            return country;
        }
    }
}
