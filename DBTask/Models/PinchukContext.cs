using Microsoft.EntityFrameworkCore;

namespace DBTask.Models
{
    public class PinchukContext : DbContext
    {
        public virtual DbSet<Altnames> Altnames { get; set; }
        public virtual DbSet<Doma> Doma { get; set; }
        public virtual DbSet<Kladr> Kladr { get; set; }
        public virtual DbSet<Socrbase> Socrbase { get; set; }
        public virtual DbSet<Street> Street { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        // Unable to generate entity type for table 'dbo.FLAT'. Please see the warning messages.
        // Unable to generate entity type for table 'dbo.SOCRBASE_copy'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Server=DESKTOP-E5JOGLU\SQLEXPRESS;Database=Pinchuk;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Altnames>(entity =>
            {
                entity.HasKey(e => new {e.Oldcode, e.Newcode, e.Level});

                entity.ToTable("ALTNAMES");

                entity.Property(e => e.Oldcode)
                    .HasColumnName("OLDCODE")
                    .HasMaxLength(19)
                    .IsUnicode(false);

                entity.Property(e => e.Newcode)
                    .HasColumnName("NEWCODE")
                    .HasMaxLength(19)
                    .IsUnicode(false);

                entity.Property(e => e.Level)
                    .HasColumnName("LEVEL")
                    .HasMaxLength(1)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Doma>(entity =>
            {
                entity.HasKey(e => new {e.Name, e.Korp, e.Socr, e.Code});

                entity.ToTable("DOMA");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Korp)
                    .HasColumnName("KORP")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Socr)
                    .HasColumnName("SOCR")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasColumnName("CODE")
                    .HasMaxLength(19)
                    .IsUnicode(false);

                entity.Property(e => e.Gninmb)
                    .HasColumnName("GNINMB")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Index)
                    .HasColumnName("INDEX")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Ocatd)
                    .HasColumnName("OCATD")
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.Uno)
                    .HasColumnName("UNO")
                    .HasMaxLength(4)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Kladr>(entity =>
            {
                entity.HasKey(e => new {e.Name, e.Code});

                entity.ToTable("KLADR");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasColumnName("CODE")
                    .HasMaxLength(13)
                    .IsUnicode(false);

                entity.Property(e => e.Gninmb)
                    .HasColumnName("GNINMB")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Ocatd)
                    .HasColumnName("OCATD")
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.PostIndex)
                    .HasColumnName("POST_INDEX")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Socr)
                    .HasColumnName("SOCR")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Uno)
                    .HasColumnName("UNO")
                    .HasMaxLength(4)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Socrbase>(entity =>
            {
                entity.HasKey(e => new {e.Level, e.Scname});

                entity.ToTable("SOCRBASE");

                entity.Property(e => e.Level)
                    .HasColumnName("LEVEL")
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Scname)
                    .HasColumnName("SCNAME")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.KodTSt)
                    .HasColumnName("KOD_T_ST")
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.Socrname)
                    .HasColumnName("SOCRNAME")
                    .HasMaxLength(29)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Street>(entity =>
            {
                entity.HasKey(e => new {e.Name, e.Socr, e.Code});

                entity.ToTable("STREET");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Socr)
                    .HasColumnName("SOCR")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasColumnName("CODE")
                    .HasMaxLength(17)
                    .IsUnicode(false);

                entity.Property(e => e.Gninmb)
                    .HasColumnName("GNINMB")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Index)
                    .HasColumnName("INDEX")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Ocatd)
                    .HasColumnName("OCATD")
                    .HasMaxLength(11)
                    .IsUnicode(false);

                entity.Property(e => e.Uno)
                    .HasColumnName("UNO")
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Socrname)
                    .HasColumnName("SOCRNAME")
                    .HasMaxLength(29)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("USERS");

                //entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}