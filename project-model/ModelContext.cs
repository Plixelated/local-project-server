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
    public ModelContext(DbContextOptions<ModelContext> options) : base(options) { }

    public DbSet<Entry> Entries { get; set; }
    public DbSet<Values> SubmittedValues { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=project_user;Database=submissions");
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        //Load ENV
        DotNetEnv.Env.Load();
        var _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings_DefaultConnection");
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new Exception("Connection String Not Found in .env file");
        }

/*        IConfigurationBuilder builder = new ConfigurationBuilder().AddEnvironmentVariables();
        IConfigurationRoot configuration = builder.Build();*/

        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entry>(entity =>
        {
             entity.Property(e => e.ID)
            .ValueGeneratedOnAdd(); //Auto Increment

        });

        modelBuilder.Entity<Values>(entity =>
        {
            entity.HasOne(e => e.Entry) //Parent Entity
            .WithMany(p => p.SubmittedValues) //One to Many
            .HasForeignKey(v => v.EntryOrigin) //Explicitly states FK
            .HasPrincipalKey(e => e.Origin) //Use origin for FK even though its not the PK of entry
            .OnDelete(DeleteBehavior.Cascade) //Maybe not cascade? Decide Later
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

        //Need to build entity later
        modelBuilder.Entity<UserOrigin>(entity =>
        {
            entity.HasKey(uo => new { uo.UserId, uo.EntryOrigin });
            entity.HasOne(uo => uo.User)
            .WithOne(u => u.UserOrigin)
            .HasForeignKey<UserOrigin>(uo => uo.UserId);


        });
    }
}