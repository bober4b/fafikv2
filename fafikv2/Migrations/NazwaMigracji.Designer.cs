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
    [Migration("20241015103730_NazwaMigracji")]
    partial class NazwaMigracji
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Fafikv2.Data.Models.BannedWords", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BannedWord")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("ServerConfigId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Time")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ServerConfigId");

                    b.ToTable("BannedWords");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.Server", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ConfigId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("AutoModeratorEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("AutoplayEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("BansEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("KicksEnabled")
                        .HasColumnType("bit");

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
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ServerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserServerStatsId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.ToTable("ServerUsers");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.Song", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Artist")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Genres")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("LinkUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Songs");
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
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<int>("UserLevel")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.UserPlayedSong", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SongId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("SongId");

                    b.HasIndex("UserId");

                    b.ToTable("UserPlayedSongs");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.UserServerStats", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BotInteractionServer")
                        .HasColumnType("int");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<DateTime?>("LastPenaltyDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("MessagesCountServer")
                        .HasColumnType("int");

                    b.Property<int>("Penalties")
                        .HasColumnType("int");

                    b.Property<float>("ServerKarma")
                        .HasColumnType("real");

                    b.Property<Guid?>("ServerUserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ServerUserId")
                        .IsUnique()
                        .HasFilter("[ServerUserId] IS NOT NULL");

                    b.ToTable("ServerUsersStats");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.BannedWords", b =>
                {
                    b.HasOne("Fafikv2.Data.Models.ServerConfig", "ServerConfig")
                        .WithMany("BannedWords")
                        .HasForeignKey("ServerConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ServerConfig");
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

                    b.Navigation("Server");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.UserPlayedSong", b =>
                {
                    b.HasOne("Fafikv2.Data.Models.Song", "Song")
                        .WithMany()
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Fafikv2.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Song");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.UserServerStats", b =>
                {
                    b.HasOne("Fafikv2.Data.Models.ServerUsers", "ServerUsers")
                        .WithOne("UserServerStats")
                        .HasForeignKey("Fafikv2.Data.Models.UserServerStats", "ServerUserId");

                    b.Navigation("ServerUsers");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.Server", b =>
                {
                    b.Navigation("Config");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerConfig", b =>
                {
                    b.Navigation("BannedWords");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.ServerUsers", b =>
                {
                    b.Navigation("UserServerStats");
                });

            modelBuilder.Entity("Fafikv2.Data.Models.User", b =>
                {
                    b.Navigation("Servers");
                });
#pragma warning restore 612, 618
        }
    }
}
