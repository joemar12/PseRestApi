﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PseRestApi.Infrastructure.Persistence;

#nullable disable

namespace PseRestApi.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PseRestApi.Domain.Entities.HistoricalTradingData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double?>("AvgPrice")
                        .HasColumnType("float");

                    b.Property<double?>("ChangeClose")
                        .HasColumnType("float");

                    b.Property<double?>("ChangeClosePercChangeClose")
                        .HasColumnType("float");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Currency")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("CurrentPe")
                        .HasColumnType("float");

                    b.Property<double?>("FiftyTwoWeekHigh")
                        .HasColumnType("float");

                    b.Property<double?>("FiftyTwoWeekLow")
                        .HasColumnType("float");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<double?>("LastTradePrice")
                        .HasColumnType("float");

                    b.Property<DateTime?>("LastTradedDate")
                        .HasColumnType("datetime2");

                    b.Property<double?>("PercChangeClose")
                        .HasColumnType("float");

                    b.Property<int>("SecurityId")
                        .HasColumnType("int");

                    b.Property<double?>("SqHigh")
                        .HasColumnType("float");

                    b.Property<double?>("SqLow")
                        .HasColumnType("float");

                    b.Property<double?>("SqOpen")
                        .HasColumnType("float");

                    b.Property<double?>("SqPrevious")
                        .HasColumnType("float");

                    b.Property<string>("Symbol")
                        .HasColumnType("nvarchar(450)");

                    b.Property<double?>("TotalValue")
                        .HasColumnType("float");

                    b.Property<double?>("TotalVolume")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("SecurityId");

                    b.HasIndex("Symbol");

                    b.ToTable("HistoricalTradingData", (string)null);
                });

            modelBuilder.Entity("PseRestApi.Domain.Entities.SecurityInfo", b =>
                {
                    b.Property<int>("SecurityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SecurityId"));

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("SecurityName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("SecurityStatus")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("nvarchar(2)");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.HasKey("SecurityId");

                    b.HasIndex("Symbol")
                        .IsUnique();

                    b.ToTable("SecurityInfo", (string)null);
                });

            modelBuilder.Entity("PseRestApi.Domain.Entities.HistoricalTradingData", b =>
                {
                    b.HasOne("PseRestApi.Domain.Entities.SecurityInfo", "SecurityInfo")
                        .WithMany("HistoricalTradingData")
                        .HasForeignKey("SecurityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SecurityInfo");
                });

            modelBuilder.Entity("PseRestApi.Domain.Entities.SecurityInfo", b =>
                {
                    b.Navigation("HistoricalTradingData");
                });
#pragma warning restore 612, 618
        }
    }
}
