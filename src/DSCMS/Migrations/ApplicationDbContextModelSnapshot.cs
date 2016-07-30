using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DSCMS.Data;

namespace DSCMS.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("DSCMS.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("DSCMS.Models.Content", b =>
                {
                    b.Property<int>("ContentId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<int>("ContentTypeId");

                    b.Property<int>("CreatedBy");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("LastUpdatedBy");

                    b.Property<DateTime>("LastUpdatedDate");

                    b.Property<int>("TemplateId");

                    b.Property<string>("Title");

                    b.Property<string>("UrlToDisplay")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 100);

                    b.HasKey("ContentId");

                    b.HasIndex("ContentTypeId");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("LastUpdatedBy");

                    b.HasIndex("TemplateId");

                    b.ToTable("Contents");
                });

            modelBuilder.Entity("DSCMS.Models.ContentItem", b =>
                {
                    b.Property<int>("ContentItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ContentId");

                    b.Property<int>("ContentTypeItemId");

                    b.Property<string>("Value");

                    b.HasKey("ContentItemId");

                    b.HasIndex("ContentId");

                    b.HasIndex("ContentTypeItemId");

                    b.ToTable("ContentItems");
                });

            modelBuilder.Entity("DSCMS.Models.ContentType", b =>
                {
                    b.Property<int>("ContentTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("DefaultTemplateForContent");

                    b.Property<string>("Description");

                    b.Property<int>("ItemsPerPage");

                    b.Property<string>("Name");

                    b.Property<int>("TemplateId");

                    b.Property<string>("Title");

                    b.HasKey("ContentTypeId");

                    b.HasIndex("DefaultTemplateForContent");

                    b.HasIndex("TemplateId");

                    b.ToTable("ContentTypes");
                });

            modelBuilder.Entity("DSCMS.Models.ContentTypeItem", b =>
                {
                    b.Property<int>("ContentTypeItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ContentTypeId");

                    b.Property<string>("Name");

                    b.HasKey("ContentTypeItemId");

                    b.HasIndex("ContentTypeId");

                    b.ToTable("ContentTypeItems");
                });

            modelBuilder.Entity("DSCMS.Models.Layout", b =>
                {
                    b.Property<int>("LayoutId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileContents");

                    b.Property<string>("FileLocation");

                    b.Property<string>("Name");

                    b.HasKey("LayoutId");

                    b.ToTable("Layouts");
                });

            modelBuilder.Entity("DSCMS.Models.Template", b =>
                {
                    b.Property<int>("TemplateId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileContents");

                    b.Property<string>("FileLocation");

                    b.Property<int>("IsForContentType");

                    b.Property<int>("LayoutId");

                    b.Property<string>("Name");

                    b.HasKey("TemplateId");

                    b.HasIndex("LayoutId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("DSCMS.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DisplayName");

                    b.Property<string>("Email");

                    b.Property<string>("Password");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("DSCMS.Models.Content", b =>
                {
                    b.HasOne("DSCMS.Models.ContentType", "ContentType")
                        .WithMany("Contents")
                        .HasForeignKey("ContentTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DSCMS.Models.User", "CreatedByUser")
                        .WithMany("CreatedContent")
                        .HasForeignKey("CreatedBy")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DSCMS.Models.User", "LastUpdatedByUser")
                        .WithMany("UpdatedContent")
                        .HasForeignKey("LastUpdatedBy")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DSCMS.Models.Template", "Template")
                        .WithMany("Contents")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DSCMS.Models.ContentItem", b =>
                {
                    b.HasOne("DSCMS.Models.Content", "Content")
                        .WithMany("ContentItems")
                        .HasForeignKey("ContentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DSCMS.Models.ContentTypeItem", "ContentTypeItem")
                        .WithMany("ContentItems")
                        .HasForeignKey("ContentTypeItemId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DSCMS.Models.ContentType", b =>
                {
                    b.HasOne("DSCMS.Models.Template", "DefaultContentTemplate")
                        .WithMany("HasAsDefaultContentTemplate")
                        .HasForeignKey("DefaultTemplateForContent");

                    b.HasOne("DSCMS.Models.Template", "Template")
                        .WithMany("ContentTypes")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DSCMS.Models.ContentTypeItem", b =>
                {
                    b.HasOne("DSCMS.Models.ContentType", "ContentType")
                        .WithMany("ContentTypeItems")
                        .HasForeignKey("ContentTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DSCMS.Models.Template", b =>
                {
                    b.HasOne("DSCMS.Models.Layout", "Layout")
                        .WithMany("Templates")
                        .HasForeignKey("LayoutId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("DSCMS.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("DSCMS.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DSCMS.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
