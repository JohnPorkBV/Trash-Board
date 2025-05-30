﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TrashBoard.Data;

#nullable disable

namespace TrashBoard.Migrations
{
    [DbContext(typeof(TrashboardDbContext))]
    [Migration("20250526203738_AddTrashDetections")]
    partial class AddTrashDetections
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Trash_Board.Models.TrashDetection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<float>("ConfidenceScore")
                        .HasColumnType("real");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("DetectedObject")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Hour")
                        .HasColumnType("int");

                    b.Property<float>("Humidity")
                        .HasColumnType("real");

                    b.Property<float>("Precipitation")
                        .HasColumnType("real");

                    b.Property<float>("Temp")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<float>("Windforce")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("TrashDetections");
                });
#pragma warning restore 612, 618
        }
    }
}
