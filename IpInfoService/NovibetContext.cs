using Microsoft.EntityFrameworkCore;

namespace IpInfoService
{
    public partial class NovibetContext : DbContext
    {
        public NovibetContext()
        {
        }

        public NovibetContext(DbContextOptions<NovibetContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Country> Countries { get; set; } = null!;
        public virtual DbSet<IpInfo> IpInfos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ThreeLetterCode)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TwoLetterCode)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<IpInfo>(entity =>
            {
                entity.ToTable("IpInfo");

                entity.HasIndex(e => e.PackedAddress, "IX_IpInfo")
                    .IsUnique();

                entity.Property(e => e.Address)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.PackedAddress)
                    .HasMaxLength(4)
                    .IsFixedLength();

                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.IpInfos)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IpInfo_Country");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
