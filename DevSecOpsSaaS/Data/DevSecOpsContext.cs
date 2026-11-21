using System;
using System.Collections.Generic;
using DevSecOpsSaaS.Models;
using Microsoft.EntityFrameworkCore;

namespace DevSecOpsSaaS.Data;

public partial class DevSecOpsContext : DbContext
{
    public DevSecOpsContext(DbContextOptions<DevSecOpsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alerte> Alertes { get; set; }

    public virtual DbSet<Analyse> Analyses { get; set; }

    public virtual DbSet<ClePipeline> ClePipelines { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<NiveauCriticite> NiveauCriticites { get; set; }

    public virtual DbSet<NiveauGravite> NiveauGravites { get; set; }

    public virtual DbSet<Organisation> Organisations { get; set; }

    public virtual DbSet<ParametreScoring> ParametreScorings { get; set; }

    public virtual DbSet<Projet> Projets { get; set; }

    public virtual DbSet<RoleUtilisateur> RoleUtilisateurs { get; set; }

    public virtual DbSet<StatutAlerte> StatutAlertes { get; set; }

    public virtual DbSet<StatutVulnerabilite> StatutVulnerabilites { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    public virtual DbSet<Vulnerabilite> Vulnerabilites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alerte>(entity =>
        {
            entity.ToTable("Alerte");

            entity.HasIndex(e => e.VulnerabiliteId, "UQ_Alerte_VulnerabiliteId").IsUnique();

            entity.Property(e => e.DateCreation).HasDefaultValueSql("(sysutcdatetime())", "DF_Alerte_DateCreation");

            entity.HasOne(d => d.PrisEnChargePar).WithMany(p => p.Alertes)
                .HasForeignKey(d => d.PrisEnChargeParId)
                .HasConstraintName("FK_Alerte_PrisEnChargePar");

            entity.HasOne(d => d.StatutAlerte).WithMany(p => p.Alertes)
                .HasForeignKey(d => d.StatutAlerteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alerte_StatutAlerte");

            entity.HasOne(d => d.Vulnerabilite).WithOne(p => p.Alerte)
                .HasForeignKey<Alerte>(d => d.VulnerabiliteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alerte_Vulnerabilite");
        });

        modelBuilder.Entity<Analyse>(entity =>
        {
            entity.ToTable("Analyse");

            entity.Property(e => e.Branche).HasMaxLength(150);
            entity.Property(e => e.CommitSha).HasMaxLength(100);
            entity.Property(e => e.DateAnalyse).HasDefaultValueSql("(sysutcdatetime())", "DF_Analyse_DateAnalyse");
            entity.Property(e => e.NumeroBuild).HasMaxLength(100);
            entity.Property(e => e.Outil).HasMaxLength(100);
            entity.Property(e => e.ScoreMax).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.ClePipeline).WithMany(p => p.Analyses)
                .HasForeignKey(d => d.ClePipelineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Analyse_ClePipeline");

            entity.HasOne(d => d.Projet).WithMany(p => p.Analyses)
                .HasForeignKey(d => d.ProjetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Analyse_Projet");
        });

        modelBuilder.Entity<ClePipeline>(entity =>
        {
            entity.ToTable("ClePipeline");

            entity.HasIndex(e => e.HashCle, "UQ_ClePipeline_HashCle").IsUnique();

            entity.Property(e => e.DateCreation).HasDefaultValueSql("(sysutcdatetime())", "DF_ClePipeline_DateCreation");
            entity.Property(e => e.HashCle).HasMaxLength(256);
            entity.Property(e => e.Nom).HasMaxLength(120);
            entity.Property(e => e.Prefixe).HasMaxLength(32);

            entity.HasOne(d => d.Projet).WithMany(p => p.ClePipelines)
                .HasForeignKey(d => d.ProjetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClePipeline_Projet");
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.ToTable("Invitation");

            entity.HasIndex(e => e.Jeton, "UQ_Invitation_Jeton").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Jeton).HasMaxLength(200);

            entity.HasOne(d => d.InvitePar).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.InviteParId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_InvitePar");

            entity.HasOne(d => d.Organisation).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.OrganisationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_Organisation");

            entity.HasOne(d => d.Role).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_RoleUtilisateur");
        });

        modelBuilder.Entity<NiveauCriticite>(entity =>
        {
            entity.HasKey(e => e.CriticiteId);

            entity.ToTable("NiveauCriticite");

            entity.HasIndex(e => e.Code, "UQ_NiveauCriticite_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Libelle).HasMaxLength(100);
        });

        modelBuilder.Entity<NiveauGravite>(entity =>
        {
            entity.HasKey(e => e.GraviteId);

            entity.ToTable("NiveauGravite");

            entity.HasIndex(e => e.Code, "UQ_NiveauGravite_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Libelle).HasMaxLength(100);
        });

        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.ToTable("Organisation");

            entity.HasIndex(e => e.Nom, "UQ_Organisation_Nom").IsUnique();

            entity.Property(e => e.DateCreation).HasDefaultValueSql("(sysutcdatetime())", "DF_Organisation_DateCreation");
            entity.Property(e => e.Nom).HasMaxLength(150);
        });

        modelBuilder.Entity<ParametreScoring>(entity =>
        {
            entity.ToTable("ParametreScoring");

            entity.HasIndex(e => e.OrganisationId, "UQ_ParametreScoring_OrganisationId").IsUnique();

            entity.Property(e => e.FacteurCritique)
                .HasDefaultValue(2.50m, "DF_ParametreScoring_FacteurCritique")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FacteurFaible)
                .HasDefaultValue(1.00m, "DF_ParametreScoring_FacteurFaible")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FacteurHaute)
                .HasDefaultValue(2.00m, "DF_ParametreScoring_FacteurHaute")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FacteurInterne)
                .HasDefaultValue(1.00m, "DF_ParametreScoring_FacteurInterne")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FacteurInternet)
                .HasDefaultValue(1.50m, "DF_ParametreScoring_FacteurInternet")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FacteurMoyenne)
                .HasDefaultValue(1.50m, "DF_ParametreScoring_FacteurMoyenne")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SeuilAlerte)
                .HasDefaultValue(8.00m, "DF_ParametreScoring_SeuilAlerte")
                .HasColumnType("decimal(6, 2)");
            entity.Property(e => e.TelegramChatId).HasMaxLength(100);

            entity.HasOne(d => d.Organisation).WithOne(p => p.ParametreScoring)
                .HasForeignKey<ParametreScoring>(d => d.OrganisationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParametreScoring_Organisation");
        });

        modelBuilder.Entity<Projet>(entity =>
        {
            entity.ToTable("Projet");

            entity.Property(e => e.DateCreation).HasDefaultValueSql("(sysutcdatetime())", "DF_Projet_DateCreation");
            entity.Property(e => e.DepotUrl).HasMaxLength(500);
            entity.Property(e => e.Nom).HasMaxLength(150);

            entity.HasOne(d => d.Criticite).WithMany(p => p.Projets)
                .HasForeignKey(d => d.CriticiteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projet_NiveauCriticite");

            entity.HasOne(d => d.Organisation).WithMany(p => p.Projets)
                .HasForeignKey(d => d.OrganisationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projet_Organisation");

            entity.HasMany(d => d.Utilisateurs).WithMany(p => p.Projets)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjetUtilisateur",
                    r => r.HasOne<Utilisateur>().WithMany()
                        .HasForeignKey("UtilisateurId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProjetUtilisateur_Utilisateur"),
                    l => l.HasOne<Projet>().WithMany()
                        .HasForeignKey("ProjetId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProjetUtilisateur_Projet"),
                    j =>
                    {
                        j.HasKey("ProjetId", "UtilisateurId");
                        j.ToTable("ProjetUtilisateur");
                    });
        });

        modelBuilder.Entity<RoleUtilisateur>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.ToTable("RoleUtilisateur");

            entity.HasIndex(e => e.Code, "UQ_RoleUtilisateur_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Libelle).HasMaxLength(100);
        });

        modelBuilder.Entity<StatutAlerte>(entity =>
        {
            entity.ToTable("StatutAlerte");

            entity.HasIndex(e => e.Code, "UQ_StatutAlerte_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Libelle).HasMaxLength(100);
        });

        modelBuilder.Entity<StatutVulnerabilite>(entity =>
        {
            entity.ToTable("StatutVulnerabilite");

            entity.HasIndex(e => e.Code, "UQ_StatutVulnerabilite_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Libelle).HasMaxLength(100);
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.ToTable("Utilisateur");

            entity.HasIndex(e => e.Email, "UQ_Utilisateur_Email").IsUnique();

            entity.Property(e => e.DateCreation).HasDefaultValueSql("(sysutcdatetime())", "DF_Utilisateur_DateCreation");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EstActif).HasDefaultValue(true, "DF_Utilisateur_EstActif");
            entity.Property(e => e.MotDePasseHash).HasMaxLength(500);
            entity.Property(e => e.Nom).HasMaxLength(100);
            entity.Property(e => e.Prenom).HasMaxLength(100);
            entity.Property(e => e.Telephone).HasMaxLength(30);

            entity.HasOne(d => d.Organisation).WithMany(p => p.Utilisateurs)
                .HasForeignKey(d => d.OrganisationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Utilisateur_Organisation");

            entity.HasOne(d => d.Role).WithMany(p => p.Utilisateurs)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Utilisateur_RoleUtilisateur");
        });

        modelBuilder.Entity<Vulnerabilite>(entity =>
        {
            entity.ToTable("Vulnerabilite");

            entity.HasIndex(e => new { e.ProjetId, e.CveId, e.Package }, "UQ_Vulnerabilite_Projet_Cve_Package").IsUnique();

            entity.Property(e => e.CveId).HasMaxLength(100);
            entity.Property(e => e.DatePremiereDetection).HasDefaultValueSql("(sysutcdatetime())", "DF_Vulnerabilite_DatePremiereDetection");
            entity.Property(e => e.Fichier).HasMaxLength(500);
            entity.Property(e => e.Package).HasMaxLength(250);
            entity.Property(e => e.ScoreBrut).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.ScoreReel).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Titre).HasMaxLength(250);

            entity.HasOne(d => d.AnalyseDetectee).WithMany(p => p.Vulnerabilites)
                .HasForeignKey(d => d.AnalyseDetecteeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vulnerabilite_AnalyseDetectee");

            entity.HasOne(d => d.Gravite).WithMany(p => p.Vulnerabilites)
                .HasForeignKey(d => d.GraviteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vulnerabilite_NiveauGravite");

            entity.HasOne(d => d.Projet).WithMany(p => p.Vulnerabilites)
                .HasForeignKey(d => d.ProjetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vulnerabilite_Projet");

            entity.HasOne(d => d.StatutVulnerabilite).WithMany(p => p.Vulnerabilites)
                .HasForeignKey(d => d.StatutVulnerabiliteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vulnerabilite_StatutVulnerabilite");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
