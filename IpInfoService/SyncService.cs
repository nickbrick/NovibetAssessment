using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IpInfoService
{
    public class SyncService : IHostedService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IMemoryCache cache;

        private NovibetContext? context;
        private List<Country>? countries = new List<Country>();
        private Timer? timer;
        private bool isSyncing;
        private const int batchSize = 100;
        private static readonly TimeSpan period = new TimeSpan(hours: 1, 0, 0);

        public SyncService(IServiceScopeFactory scopeFactory, IMemoryCache cache)
        {
            this.scopeFactory = scopeFactory;
            this.cache = cache;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(new TimerCallback(SyncAll), null, TimeSpan.Zero, period);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async void SyncAll(object? state)
        {
            if (isSyncing) { return; }

            isSyncing = true;
            using (var scope = scopeFactory.CreateScope())
            {
                context = scope.ServiceProvider.GetRequiredService<NovibetContext>();
                countries = context.Countries.ToList();
                var table = context.IpInfos
                    .Include(info => info.Country)
                    .OrderBy(info => info.Id);
                int lastBatchSize = 0;
                int totalProcessed = 0;
                do
                {
                    var batch = table.Skip(totalProcessed).Take(batchSize).ToList();
                    foreach (var record in batch)
                        await Sync(record);
                    context.SaveChanges();
                    lastBatchSize = batch.Count;
                    totalProcessed += lastBatchSize;
                }
                while (lastBatchSize == batchSize);
            }

            countries.Clear();
            isSyncing = false;
        }

        private async Task Sync(IpInfo record)
        {
            uint dec = BitConverter.ToUInt32(record.PackedAddress, 0);
            var theirs = await Ip2c.Get(dec);
            var ours = new BasicIpInfo(record);
            if (theirs != ours)
            {
                var correctCountry = countries.SingleOrDefault(country => country.Name == theirs.CountryName);
                if (correctCountry == null)
                {
                    correctCountry = new Country(theirs);
                    countries.Add(correctCountry);
                }
                record.Country = correctCountry;
                cache.Remove(dec);
            }
        }
    }
}
