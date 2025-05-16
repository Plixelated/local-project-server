using System;
using System.Collections.Generic;
using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using project_model;

namespace project_model;

public class ModelContext : IdentityDbContext<ProjectUser>
{
    public ModelContext() { }
    //DI
    public ModelContext(DbContextOptions<ModelContext> options) : base(options) { }
    //Reference to Entry Table
    public DbSet<Entry> Entries { get; set; }
    //Reference to Value Table
    public DbSet<Values> SubmittedValues { get; set; }
    //Reference to User Origin Table
    public DbSet<UserOrigin> UserOrigin { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //Exit if already configured
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        //Load ENV
        DotNetEnv.Env.Load();
        var _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings_DefaultConnection");
        //Verify ENV files are not empty
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new Exception("Connection String Not Found in .env file");
        }

        //User connection string for PostgreSql Connection
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Configure Entry Entity
        modelBuilder.Entity<Entry>(entity =>
        {
             entity.Property(e => e.ID)
            .ValueGeneratedOnAdd(); //Auto Increment

        });

        //Configure Value Entity
        modelBuilder.Entity<Values>(entity =>
        {
            entity.HasOne(e => e.Entry) //Parent Entity
            .WithMany(p => p.SubmittedValues) //One to Many
            .HasForeignKey(v => v.EntryOrigin) //Explicitly states FK
            .HasPrincipalKey(e => e.Origin) //Use origin for FK even though its not the PK of entry
            .OnDelete(DeleteBehavior.Cascade) //Cascade on Delete
            .HasConstraintName("FK_Entry_Submission");

            entity.Property(e => e.SubmissionID)
            .ValueGeneratedOnAdd(); //Auto Increment

            //Non-Negative Constraints
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_RateStars_Values_Only", "r_s >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_FrequencyPlanet_Values_Only", "f_p >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_NearEarth_Values_Only", "n_e >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_FractionLife_Values_Only", "f_l >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_FractionIntelligence_Values_Only", "f_i >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_FractionCommunication_Values_Only", "f_c >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NonNegative_Length_Values_Only", "l >= 0"));

        });

        //Configure User Origin Entity
        modelBuilder.Entity<UserOrigin>(entity =>
        {
            entity.HasKey(uo => new { uo.UserId, uo.EntryOrigin });

            entity.HasOne(uo => uo.User) //One
            .WithOne(u => u.UserOrigin) //To One
            .HasForeignKey<UserOrigin>(uo => uo.UserId) //Explicitly define FK
            .OnDelete(DeleteBehavior.Cascade); //Cascades Delete so if user is deleted so is the link

            entity.HasOne(uo => uo.Entry) //One
            .WithMany(e => e.UserOrigin) //To Many
            .HasForeignKey(uo => uo.EntryOrigin) //Explicitly define FK
            .HasPrincipalKey(e => e.Origin) //Use origin for FK even though its not the PK of entry
            .OnDelete(DeleteBehavior.Cascade); //Cascades Delete so if entry is deleted so is the Link
        });
    }
}