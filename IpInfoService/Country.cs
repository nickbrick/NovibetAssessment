using System.Text.Json.Serialization;

namespace IpInfoService
{
    public partial class Country
    {
        public Country()
        {
            IpInfos = new HashSet<IpInfo>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string TwoLetterCode { get; set; } = null!;
        public string ThreeLetterCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        public virtual ICollection<IpInfo> IpInfos { get; set; }

        public Country(BasicIpInfo basicInfo)
        {
            Name = basicInfo.CountryName;
            TwoLetterCode = basicInfo.TwoLetterCode;
            ThreeLetterCode = basicInfo.ThreeLetterCode;
        }
    }
}
