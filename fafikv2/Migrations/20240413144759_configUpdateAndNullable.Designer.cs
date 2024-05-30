﻿// <auto-generated />
using System;
using Fafikv2.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Fafikv2.Migrations
{
    [DbContext(typeof(DiscordBotDbContext))]
    [Migration("20240413144759_configUpdateAndNullable")]
    partial class configUpdateAndNullable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Fafikv2.Data.Models.Server", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ConfigId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ServerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ServerId")
                        .IsUnique()
                        .HasFilter("[ServerId] IS NOT NULL");

                    b.ToTable("ServerConfigs");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerUsers", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ServerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserServerStatsId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "ServerId");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserServerStatsId");

                    b.ToTable("ServerUsers");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BotInteractionGlobal")
                        .HasColumnType("int");

                    b.Property<float>("GlobalKarma")
                        .HasColumnType("real");

                    b.Property<int>("MessagesCountGlobal")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserLevel")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.UserServerStats", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BotInteractionServer")
                        .HasColumnType("int");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MessagesCountServer")
                        .HasColumnType("int");

                    b.Property<float>("ServerKarma")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("ServerUsersStats");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerConfig", b =>
                {
                    b.HasOne("Fafikv2.Data.Models.Server", "Server")
                        .WithOne("Config")
                        .HasForeignKey("Fafikv2.Data.Models.ServerConfig", "ServerId");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerUsers", b =>
                {
                    b.HasOne("Fafikv2.Data.Models.Server", "Server")
                        .WithMany("Users")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fafikv2.Data.Models.User", "User")
                        .WithMany("Servers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fafikv2.Data.Models.UserServerStats", "UserServerStats")
                        .WithMany()
                        .HasForeignKey("UserServerStatsId");

                    b.Navigation("Server");

                    b.Navigation("User");

                    b.Navigation("UserServerStats");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.Server", b =>
                {
                    b.Navigation("Config");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.User", b =>
                {
                    b.Navigation("Servers");
                });
#pragma warning restore 612, 618
        }
    }
}
