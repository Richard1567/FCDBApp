using Microsoft.EntityFrameworkCore;

namespace FCDBApp.Models
{
    public class InspectionContext : DbContext
    {
        public InspectionContext(DbContextOptions<InspectionContext> options) : base(options)
        {
        }

        public DbSet<InspectionTable> InspectionTables { get; set; }
        public DbSet<InspectionDetails> InspectionDetails { get; set; }
        public DbSet<InspectionItems> InspectionItems { get; set; }
        public DbSet<InspectionCategories> InspectionCategories { get; set; }
        public DbSet<InspectionType> InspectionTypes { get; set; }
        public DbSet<JobCard> JobCards { get; set; }
        public DbSet<PartUsed> PartsUsed { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define primary keys for each entity
            modelBuilder.Entity<InspectionCategories>().HasKey(c => c.CategoryID);
            modelBuilder.Entity<InspectionItems>().HasKey(i => i.InspectionItemID);
            modelBuilder.Entity<InspectionDetails>().HasKey(d => d.InspectionDetailID);
            modelBuilder.Entity<InspectionTable>().HasKey(t => t.InspectionID);
            modelBuilder.Entity<JobCard>().HasKey(j => j.JobCardID);
            modelBuilder.Entity<PartUsed>().HasKey(p => p.PartUsedID);
            modelBuilder.Entity<InspectionTable>()
           .HasMany(i => i.Details)
           .WithOne(d => d.Inspection)
           .HasForeignKey(d => d.InspectionID);
            modelBuilder.Entity<InspectionDetails>()
           .HasKey(d => d.InspectionDetailID);
            modelBuilder.Entity<InspectionDetails>()
                .Property(d => d.InspectionDetailID)
                .ValueGeneratedOnAdd();
            // Configure relationships for InspectionDetails entity
            modelBuilder.Entity<InspectionDetails>()
                .HasOne(d => d.Inspection)
                .WithMany(t => t.Details)
                .HasForeignKey(d => d.InspectionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InspectionDetails>()
                .HasOne(d => d.Item)
                .WithMany()
                .HasForeignKey(d => d.InspectionItemID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships for PartUsed entity without navigation property
            modelBuilder.Entity<PartUsed>()
                .HasOne<JobCard>() // No navigation property
                .WithMany(j => j.PartsUsed)
                .HasForeignKey(p => p.JobCardID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JobCard entity to use a row version for concurrency control
            modelBuilder.Entity<JobCard>()
                .Property(j => j.RowVersion)
                .IsRowVersion();

            // Additional configuration for entity properties if needed
            modelBuilder.Entity<InspectionTable>()
                .Property(t => t.SubmissionTime)
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<InspectionCategories>().HasData(
                new InspectionCategories { CategoryID = 1, CategoryName = "Load Mission Disc and Log to Vehicle" },
                new InspectionCategories { CategoryID = 2, CategoryName = "General Security Conditions" },
                new InspectionCategories { CategoryID = 3, CategoryName = "Trunk Normal/Depot Mode Tests" },
                new InspectionCategories { CategoryID = 4, CategoryName = "Trunk Unit Checks" },
                new InspectionCategories { CategoryID = 5, CategoryName = "TrunkMode/Trunk Setting Test" },
                new InspectionCategories { CategoryID = 6, CategoryName = "Return to Depot Mode Test" },
                new InspectionCategories { CategoryID = 7, CategoryName = "H&S" },
                new InspectionCategories { CategoryID = 8, CategoryName = "Depot Mode Settings" }
            );

            modelBuilder.Entity<InspectionItems>().HasData(
                new InspectionItems { InspectionItemID = 1, CategoryID = 1, ItemDescription = "Open escape hatch check that the alarm sounds and engine immobilised", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 2, CategoryID = 1, ItemDescription = "Check engine won't start when immobiliser key is vertical", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 3, CategoryID = 1, ItemDescription = "Check 112/160 meters immobilised and alarms operates at correct distance", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 4, CategoryID = 1, ItemDescription = "Check all off mode escape hatches overlock", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 5, CategoryID = 1, ItemDescription = "Check pavement alarm system", InspectionTypeIndicator = "14" },
                new InspectionItems { InspectionItemID = 6, CategoryID = 1, ItemDescription = "Check bulk head door locks are secure and immobilises when open", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 7, CategoryID = 1, ItemDescription = "Check and record date/time on computer. Reset if needed", InspectionTypeIndicator = "4" },
                new InspectionItems { InspectionItemID = 8, CategoryID = 1, ItemDescription = "Check and record date and time on Finger scanner", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 9, CategoryID = 1, ItemDescription = "Check PQ bolts on outside of the door, operate alarm with 15mm+10mm of thread showing", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 10, CategoryID = 1, ItemDescription = "Check PQ internal bolts operate alarm when released", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 11, CategoryID = 1, ItemDescription = "Check Escape Hatch shoot bolt test activates alarm and remote panel LED", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 12, CategoryID = 1, ItemDescription = "Transfer Hatch locks", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 13, CategoryID = 1, ItemDescription = "Check red Hijack buttons operate - GPS Tracking", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 14, CategoryID = 1, ItemDescription = "Press green siren only button, alarm activates when button latches in and stops when button is out", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 15, CategoryID = 1, ItemDescription = "Check black vehicle track buttons, operate GPS Tracking", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 16, CategoryID = 1, ItemDescription = "Check External Door latch and cylinder", InspectionTypeIndicator = "124" },
                new InspectionItems { InspectionItemID = 17, CategoryID = 1, ItemDescription = "Check red Power light under LED Panel", InspectionTypeIndicator = "24" },
                new InspectionItems { InspectionItemID = 18, CategoryID = 1, ItemDescription = "Check green lamp - external door button", InspectionTypeIndicator = "24" },
                new InspectionItems { InspectionItemID = 19, CategoryID = 1, ItemDescription = "Check green lamp - Access Control door", InspectionTypeIndicator = "24" },

                // General Security Conditions
                new InspectionItems { InspectionItemID = 20, CategoryID = 2, ItemDescription = "Glazing", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 21, CategoryID = 2, ItemDescription = "Cab doors", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 22, CategoryID = 2, ItemDescription = "Air lock doors", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 23, CategoryID = 2, ItemDescription = "Rear doors", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 24, CategoryID = 2, ItemDescription = "Coin Pass through unit", InspectionTypeIndicator = "4" },
                new InspectionItems { InspectionItemID = 25, CategoryID = 2, ItemDescription = "Under vehicle protection incl. tail lift", InspectionTypeIndicator = "3" },
                new InspectionItems { InspectionItemID = 26, CategoryID = 2, ItemDescription = "Escape Hatch operation", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 27, CategoryID = 2, ItemDescription = "Escape Hatch security", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 28, CategoryID = 2, ItemDescription = "Alarm system", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 29, CategoryID = 2, ItemDescription = "Vehicle immobilisation", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 30, CategoryID = 2, ItemDescription = "GPS tracking check to ARC", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 31, CategoryID = 2, ItemDescription = "Check CCTV operative and recording", InspectionTypeIndicator = "14" },

                // H&S
                new InspectionItems { InspectionItemID = 32, CategoryID = 7, ItemDescription = "Fire Extinguisher", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 33, CategoryID = 7, ItemDescription = "First Aid Box", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 34, CategoryID = 7, ItemDescription = "Check general interior panel condition, drivers seat and cab floor mat condition", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 35, CategoryID = 7, ItemDescription = "Check entrance door grab handle fitted, condition and it is secure", InspectionTypeIndicator = "14" },

                // Depot Mode Settings
                new InspectionItems { InspectionItemID = 36, CategoryID = 8, ItemDescription = "Shut down", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 37, CategoryID = 8, ItemDescription = "Vault lights", InspectionTypeIndicator = "1234" },

                // Trunk Normal/Depot Mode Tests
                new InspectionItems { InspectionItemID = 38, CategoryID = 3, ItemDescription = "Roof escape hatch", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 39, CategoryID = 3, ItemDescription = "Side door and escape route (full alarm immobilisation)", InspectionTypeIndicator = "134" },
                new InspectionItems { InspectionItemID = 40, CategoryID = 3, ItemDescription = "Roof hatch spin bolts", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 41, CategoryID = 3, ItemDescription = "Door escape spin bolts", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 42, CategoryID = 3, ItemDescription = "Sounder Button (siren only)", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 43, CategoryID = 3, ItemDescription = "Vault 1 door open", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 44, CategoryID = 3, ItemDescription = "Vault 2 door open (cab module warn lamp operates)", InspectionTypeIndicator = "34" },

                // Trunk Unit Checks
                new InspectionItems { InspectionItemID = 45, CategoryID = 4, ItemDescription = "Warning lights working correctly", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 46, CategoryID = 4, ItemDescription = "Internal sounder operates", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 47, CategoryID = 4, ItemDescription = "Operation of custodian key switch", InspectionTypeIndicator = "34" },

                // TrunkMode/Trunk Setting Test
                new InspectionItems { InspectionItemID = 48, CategoryID = 5, ItemDescription = "No operation of doors/vaults door", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 49, CategoryID = 5, ItemDescription = "Hi-jack reset function", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 50, CategoryID = 5, ItemDescription = "Drivers door/airlock open", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 51, CategoryID = 5, ItemDescription = "Passenger door/airlock open", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 52, CategoryID = 5, ItemDescription = "Vault 1 door open", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 53, CategoryID = 5, ItemDescription = "Vault 2 door open", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 54, CategoryID = 5, ItemDescription = "Immobilisation (alarms activate mayday sent)", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 55, CategoryID = 5, ItemDescription = "Access limited 816", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 56, CategoryID = 5, ItemDescription = "Reset on key", InspectionTypeIndicator = "34" },

                // Return to Depot Mode Test
                new InspectionItems { InspectionItemID = 57, CategoryID = 6, ItemDescription = "Tacho distance/speed activates", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 58, CategoryID = 6, ItemDescription = "Rear door opening", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 59, CategoryID = 6, ItemDescription = "Smoke cloak operation", InspectionTypeIndicator = "3" },
                new InspectionItems { InspectionItemID = 60, CategoryID = 6, ItemDescription = "Distance travelled in TRUNK Mode", InspectionTypeIndicator = "34" },
                new InspectionItems { InspectionItemID = 61, CategoryID = 6, ItemDescription = "Check external Kaba key readers and cover condition", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 62, CategoryID = 6, ItemDescription = "Check entrance door lock operation and inner striker condition, check OEM check strap", InspectionTypeIndicator = "14" },
                new InspectionItems { InspectionItemID = 63, CategoryID = 6, ItemDescription = "Check internal escape overrides on cab doors, realign if required", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 64, CategoryID = 6, ItemDescription = "Test drop safe operation, and backplate is present and secure", InspectionTypeIndicator = "14" },
                new InspectionItems { InspectionItemID = 65, CategoryID = 6, ItemDescription = "Check safe doors operation and backplate is present and secure", InspectionTypeIndicator = "14" },
                new InspectionItems { InspectionItemID = 66, CategoryID = 6, ItemDescription = "Check rear coin area condition and divider secure", InspectionTypeIndicator = "14" },
                new InspectionItems { InspectionItemID = 67, CategoryID = 6, ItemDescription = "Check Vehicle & auxiliary battery condition, drop test", InspectionTypeIndicator = "1234" },
                new InspectionItems { InspectionItemID = 68, CategoryID = 6, ItemDescription = "Check all emergency security plates are present and secure", InspectionTypeIndicator = "14" },
                new InspectionItems { InspectionItemID = 69, CategoryID = 6, ItemDescription = "Lubricate locks and components as required", InspectionTypeIndicator = "1234" }
            );

            modelBuilder.Entity<InspectionType>().HasData(
                new InspectionType { InspectionTypeID = 1, TypeName = "OPV - every 8 Weeks or ExSSG - every 4 weeks", Frequency = "every 8 weeks or every 4 weeks" },
                new InspectionType { InspectionTypeID = 2, TypeName = "Coin vehicles - every 26 weeks", Frequency = "every 26 weeks" },
                new InspectionType { InspectionTypeID = 3, TypeName = "Trunkers - every 4 weeks", Frequency = "every 4 weeks" },
                new InspectionType { InspectionTypeID = 4, TypeName = "CIT - every 8 weeks or 816 - every 4 weeks", Frequency = "every 8 weeks or every 4 weeks" }
            );
        }
    }
}
