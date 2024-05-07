﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestRepo.Data;

#nullable disable

namespace TestRepo.Migrations
{
    [DbContext(typeof(Dbcontext))]
    [Migration("20240416101302_removeigncall")]
    partial class removeigncall
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TestRepo.DTO.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Accounts", (string)null);
                });

            modelBuilder.Entity("TestRepo.DTO.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Volumes")
                        .HasColumnType("int");

                    b.Property<string>("coverImage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("TestRepo.DTO.BookProgress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<int>("volumesRead")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("BookId");

                    b.ToTable("BooksProgress");
                });

            modelBuilder.Entity("TestRepo.DTO.VolProgress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<int>("pagesRead")
                        .HasColumnType("int");

                    b.Property<int>("volId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("BookId");

                    b.HasIndex("volId");

                    b.ToTable("volProgress");
                });

            modelBuilder.Entity("TestRepo.DTO.Volume", b =>
                {
                    b.Property<int>("VolumeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VolumeId"));

                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<int>("VolNumber")
                        .HasColumnType("int");

                    b.Property<int>("totalPages")
                        .HasColumnType("int");

                    b.HasKey("VolumeId");

                    b.HasIndex("BookId");

                    b.ToTable("Vols");
                });

            modelBuilder.Entity("TestRepo.DTO.BookProgress", b =>
                {
                    b.HasOne("TestRepo.DTO.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TestRepo.DTO.Book", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Book");
                });

            modelBuilder.Entity("TestRepo.DTO.VolProgress", b =>
                {
                    b.HasOne("TestRepo.DTO.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TestRepo.DTO.Book", "book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TestRepo.DTO.Volume", "Volume")
                        .WithMany()
                        .HasForeignKey("volId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Volume");

                    b.Navigation("book");
                });

            modelBuilder.Entity("TestRepo.DTO.Volume", b =>
                {
                    b.HasOne("TestRepo.DTO.Book", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");
                });
#pragma warning restore 612, 618
        }
    }
}
