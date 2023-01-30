namespace IpInfoService
{
    public partial class IpInfo
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Address { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] PackedAddress { get; set; } = null!;
        public virtual Country Country { get; set; } = null!;

        public static bool TryPack(string address, out byte[] packed)
        {
            try
            {
                byte[] bytes = new byte[4];
                var parts = address.Split('.');
                for (int i = 0; i < 4; i++)
                    bytes[i] = Byte.Parse(parts[i]);
                packed = bytes;
                return true;
            }
            catch { packed = null; return false; }
        }
    }
}
