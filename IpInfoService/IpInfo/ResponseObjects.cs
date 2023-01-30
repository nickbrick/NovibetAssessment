namespace IpInfoService
{
    public struct BasicIpInfo : IEquatable<BasicIpInfo>
    {
        public string CountryName;
        public string TwoLetterCode;
        public string ThreeLetterCode;

        public BasicIpInfo(IpInfo ipInfo)
        {
            CountryName = ipInfo.Country.Name;
            TwoLetterCode = ipInfo.Country.TwoLetterCode;
            ThreeLetterCode = ipInfo.Country.ThreeLetterCode;
        }

        public override bool Equals(object? obj)
        {
            return obj is BasicIpInfo response && Equals(response);
        }

        public bool Equals(BasicIpInfo other)
        {
            return CountryName == other.CountryName &&
                   TwoLetterCode == other.TwoLetterCode &&
                   ThreeLetterCode == other.ThreeLetterCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CountryName, TwoLetterCode, ThreeLetterCode);
        }

        public static bool operator ==(BasicIpInfo left, BasicIpInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BasicIpInfo left, BasicIpInfo right)
        {
            return !(left == right);
        }
    }

    public struct CountrySummary
    {
        public string CountryName;
        public int AddressCount;
        public DateTime AddressLastUpdated;
    }
}
