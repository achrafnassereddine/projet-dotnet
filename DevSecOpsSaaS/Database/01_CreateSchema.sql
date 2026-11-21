USE [master];
GO
IF DB_ID(N'DevSecOpsDB') IS NOT NULL
BEGIN
    ALTER DATABASE [DevSecOpsDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DevSecOpsDB];
END
GO
CREATE DATABASE [DevSecOpsDB];
GO
USE [DevSecOpsDB];
GO

CREATE TABLE [dbo].[RoleUtilisateur] (
    [RoleId]  INT           IDENTITY(1,1) NOT NULL,
    [Code]    NVARCHAR(50)  NOT NULL,
    [Libelle] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_RoleUtilisateur] PRIMARY KEY CLUSTERED ([RoleId] ASC),
    CONSTRAINT [UQ_RoleUtilisateur_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);

CREATE TABLE [dbo].[NiveauCriticite] (
    [CriticiteId] INT           IDENTITY(1,1) NOT NULL,
    [Code]        NVARCHAR(50)  NOT NULL,
    [Libelle]     NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_NiveauCriticite] PRIMARY KEY CLUSTERED ([CriticiteId] ASC),
    CONSTRAINT [UQ_NiveauCriticite_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);

CREATE TABLE [dbo].[NiveauGravite] (
    [GraviteId] INT           IDENTITY(1,1) NOT NULL,
    [Code]      NVARCHAR(50)  NOT NULL,
    [Libelle]   NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_NiveauGravite] PRIMARY KEY CLUSTERED ([GraviteId] ASC),
    CONSTRAINT [UQ_NiveauGravite_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);

CREATE TABLE [dbo].[StatutVulnerabilite] (
    [StatutVulnerabiliteId] INT           IDENTITY(1,1) NOT NULL,
    [Code]                  NVARCHAR(50)  NOT NULL,
    [Libelle]               NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_StatutVulnerabilite] PRIMARY KEY CLUSTERED ([StatutVulnerabiliteId] ASC),
    CONSTRAINT [UQ_StatutVulnerabilite_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);

CREATE TABLE [dbo].[StatutAlerte] (
    [StatutAlerteId] INT           IDENTITY(1,1) NOT NULL,
    [Code]            NVARCHAR(50)  NOT NULL,
    [Libelle]         NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_StatutAlerte] PRIMARY KEY CLUSTERED ([StatutAlerteId] ASC),
    CONSTRAINT [UQ_StatutAlerte_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);

CREATE TABLE [dbo].[Organisation] (
    [OrganisationId] INT           IDENTITY(1,1) NOT NULL,
    [Nom]            NVARCHAR(150) NOT NULL,
    [DateCreation]   DATETIME2(7)  NOT NULL CONSTRAINT [DF_Organisation_DateCreation] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Organisation] PRIMARY KEY CLUSTERED ([OrganisationId] ASC),
    CONSTRAINT [UQ_Organisation_Nom] UNIQUE NONCLUSTERED ([Nom] ASC)
);

CREATE TABLE [dbo].[ParametreScoring] (
    [ParametreScoringId] INT           IDENTITY(1,1) NOT NULL,
    [OrganisationId]     INT           NOT NULL,
    [FacteurFaible]      DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_FacteurFaible] DEFAULT (1.00),
    [FacteurMoyenne]     DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_FacteurMoyenne] DEFAULT (1.50),
    [FacteurHaute]       DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_FacteurHaute] DEFAULT (2.00),
    [FacteurCritique]    DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_FacteurCritique] DEFAULT (2.50),
    [FacteurInternet]    DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_FacteurInternet] DEFAULT (1.50),
    [FacteurInterne]     DECIMAL(5,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_FacteurInterne] DEFAULT (1.00),
    [SeuilAlerte]        DECIMAL(6,2)  NOT NULL CONSTRAINT [DF_ParametreScoring_SeuilAlerte] DEFAULT (8.00),
    [TelegramChatId]     NVARCHAR(100) NULL,
    CONSTRAINT [PK_ParametreScoring] PRIMARY KEY CLUSTERED ([ParametreScoringId] ASC),
    CONSTRAINT [UQ_ParametreScoring_OrganisationId] UNIQUE NONCLUSTERED ([OrganisationId] ASC),
    CONSTRAINT [FK_ParametreScoring_Organisation] FOREIGN KEY ([OrganisationId])
        REFERENCES [dbo].[Organisation] ([OrganisationId])
);

CREATE TABLE [dbo].[Utilisateur] (
    [UtilisateurId]  INT           IDENTITY(1,1) NOT NULL,
    [Email]          NVARCHAR(256) NOT NULL,
    [MotDePasseHash] NVARCHAR(500) NOT NULL,
    [Nom]            NVARCHAR(100) NOT NULL,
    [Prenom]         NVARCHAR(100) NOT NULL,
    [Telephone]      NVARCHAR(30)  NULL,
    [RoleId]         INT           NOT NULL,
    [EstActif]       BIT           NOT NULL CONSTRAINT [DF_Utilisateur_EstActif] DEFAULT (1),
    [DateCreation]   DATETIME2(7)  NOT NULL CONSTRAINT [DF_Utilisateur_DateCreation] DEFAULT (SYSUTCDATETIME()),
    [OrganisationId] INT           NOT NULL,
    CONSTRAINT [PK_Utilisateur] PRIMARY KEY CLUSTERED ([UtilisateurId] ASC),
    CONSTRAINT [UQ_Utilisateur_Email] UNIQUE NONCLUSTERED ([Email] ASC),
    CONSTRAINT [FK_Utilisateur_RoleUtilisateur] FOREIGN KEY ([RoleId])
        REFERENCES [dbo].[RoleUtilisateur] ([RoleId]),
    CONSTRAINT [FK_Utilisateur_Organisation] FOREIGN KEY ([OrganisationId])
        REFERENCES [dbo].[Organisation] ([OrganisationId])
);

CREATE TABLE [dbo].[Invitation] (
    [InvitationId]   INT           IDENTITY(1,1) NOT NULL,
    [Email]          NVARCHAR(256) NOT NULL,
    [RoleId]         INT           NOT NULL,
    [Jeton]          NVARCHAR(200) NOT NULL,
    [DateExpiration] DATETIME2(7)  NOT NULL,
    [EstUtilisee]    BIT           NOT NULL CONSTRAINT [DF_Invitation_EstUtilisee] DEFAULT (0),
    [OrganisationId] INT           NOT NULL,
    [InviteParId]    INT           NOT NULL,
    CONSTRAINT [PK_Invitation] PRIMARY KEY CLUSTERED ([InvitationId] ASC),
    CONSTRAINT [UQ_Invitation_Jeton] UNIQUE NONCLUSTERED ([Jeton] ASC),
    CONSTRAINT [FK_Invitation_RoleUtilisateur] FOREIGN KEY ([RoleId])
        REFERENCES [dbo].[RoleUtilisateur] ([RoleId]),
    CONSTRAINT [FK_Invitation_Organisation] FOREIGN KEY ([OrganisationId])
        REFERENCES [dbo].[Organisation] ([OrganisationId]),
    CONSTRAINT [FK_Invitation_InvitePar] FOREIGN KEY ([InviteParId])
        REFERENCES [dbo].[Utilisateur] ([UtilisateurId])
);

CREATE TABLE [dbo].[Projet] (
    [ProjetId]       INT           IDENTITY(1,1) NOT NULL,
    [Nom]            NVARCHAR(150) NOT NULL,
    [DepotUrl]       NVARCHAR(500) NOT NULL,
    [CriticiteId]    INT           NOT NULL,
    [ExposeInternet] BIT           NOT NULL CONSTRAINT [DF_Projet_ExposeInternet] DEFAULT (0),
    [DateCreation]   DATETIME2(7)  NOT NULL CONSTRAINT [DF_Projet_DateCreation] DEFAULT (SYSUTCDATETIME()),
    [OrganisationId] INT           NOT NULL,
    CONSTRAINT [PK_Projet] PRIMARY KEY CLUSTERED ([ProjetId] ASC),
    CONSTRAINT [FK_Projet_NiveauCriticite] FOREIGN KEY ([CriticiteId])
        REFERENCES [dbo].[NiveauCriticite] ([CriticiteId]),
    CONSTRAINT [FK_Projet_Organisation] FOREIGN KEY ([OrganisationId])
        REFERENCES [dbo].[Organisation] ([OrganisationId])
);

CREATE TABLE [dbo].[ProjetUtilisateur] (
    [ProjetId]      INT NOT NULL,
    [UtilisateurId] INT NOT NULL,
    CONSTRAINT [PK_ProjetUtilisateur] PRIMARY KEY CLUSTERED ([ProjetId] ASC, [UtilisateurId] ASC),
    CONSTRAINT [FK_ProjetUtilisateur_Projet] FOREIGN KEY ([ProjetId])
        REFERENCES [dbo].[Projet] ([ProjetId]),
    CONSTRAINT [FK_ProjetUtilisateur_Utilisateur] FOREIGN KEY ([UtilisateurId])
        REFERENCES [dbo].[Utilisateur] ([UtilisateurId])
);

CREATE TABLE [dbo].[ClePipeline] (
    [ClePipelineId]     INT           IDENTITY(1,1) NOT NULL,
    [Nom]               NVARCHAR(120) NOT NULL,
    [HashCle]           NVARCHAR(256) NOT NULL,
    [Prefixe]           NVARCHAR(32)  NOT NULL,
    [EstRevoquee]       BIT           NOT NULL CONSTRAINT [DF_ClePipeline_EstRevoquee] DEFAULT (0),
    [DateCreation]      DATETIME2(7)  NOT NULL CONSTRAINT [DF_ClePipeline_DateCreation] DEFAULT (SYSUTCDATETIME()),
    [DerniereUtilisation] DATETIME2(7)  NULL,
    [ProjetId]          INT           NOT NULL,
    CONSTRAINT [PK_ClePipeline] PRIMARY KEY CLUSTERED ([ClePipelineId] ASC),
    CONSTRAINT [UQ_ClePipeline_HashCle] UNIQUE NONCLUSTERED ([HashCle] ASC),
    CONSTRAINT [FK_ClePipeline_Projet] FOREIGN KEY ([ProjetId])
        REFERENCES [dbo].[Projet] ([ProjetId])
);

CREATE TABLE [dbo].[Analyse] (
    [AnalyseId]        INT           IDENTITY(1,1) NOT NULL,
    [DateAnalyse]      DATETIME2(7)  NOT NULL CONSTRAINT [DF_Analyse_DateAnalyse] DEFAULT (SYSUTCDATETIME()),
    [Outil]            NVARCHAR(100) NOT NULL,
    [CommitSha]        NVARCHAR(100) NOT NULL,
    [Branche]          NVARCHAR(150) NOT NULL,
    [NumeroBuild]      NVARCHAR(100) NOT NULL,
    [NbVulnerabilites] INT           NOT NULL CONSTRAINT [DF_Analyse_NbVulnerabilites] DEFAULT (0),
    [ScoreMax]         DECIMAL(6,2)  NOT NULL CONSTRAINT [DF_Analyse_ScoreMax] DEFAULT (0.00),
    [ProjetId]         INT           NOT NULL,
    [ClePipelineId]    INT           NOT NULL,
    CONSTRAINT [PK_Analyse] PRIMARY KEY CLUSTERED ([AnalyseId] ASC),
    CONSTRAINT [FK_Analyse_Projet] FOREIGN KEY ([ProjetId])
        REFERENCES [dbo].[Projet] ([ProjetId]),
    CONSTRAINT [FK_Analyse_ClePipeline] FOREIGN KEY ([ClePipelineId])
        REFERENCES [dbo].[ClePipeline] ([ClePipelineId])
);

CREATE TABLE [dbo].[Vulnerabilite] (
    [VulnerabiliteId]       INT            IDENTITY(1,1) NOT NULL,
    [CveId]                 NVARCHAR(100)  NOT NULL,
    [Titre]                 NVARCHAR(250)  NOT NULL,
    [Description]           NVARCHAR(MAX)  NOT NULL,
    [Fichier]               NVARCHAR(500)  NULL,
    [Ligne]                 INT            NULL,
    [Package]               NVARCHAR(250)  NOT NULL,
    [Recommandation]        NVARCHAR(MAX)  NULL,
    [ScoreBrut]             DECIMAL(4,2)   NOT NULL,
    [ScoreReel]             DECIMAL(6,2)   NOT NULL,
    [GraviteId]             INT            NOT NULL,
    [StatutVulnerabiliteId] INT            NOT NULL,
    [DatePremiereDetection] DATETIME2(7)   NOT NULL CONSTRAINT [DF_Vulnerabilite_DatePremiereDetection] DEFAULT (SYSUTCDATETIME()),
    [DateCorrection]        DATETIME2(7)   NULL,
    [ProjetId]              INT            NOT NULL,
    [AnalyseDetecteeId]     INT            NOT NULL,
    CONSTRAINT [PK_Vulnerabilite] PRIMARY KEY CLUSTERED ([VulnerabiliteId] ASC),
    CONSTRAINT [UQ_Vulnerabilite_Projet_Cve_Package] UNIQUE NONCLUSTERED ([ProjetId] ASC, [CveId] ASC, [Package] ASC),
    CONSTRAINT [FK_Vulnerabilite_NiveauGravite] FOREIGN KEY ([GraviteId])
        REFERENCES [dbo].[NiveauGravite] ([GraviteId]),
    CONSTRAINT [FK_Vulnerabilite_StatutVulnerabilite] FOREIGN KEY ([StatutVulnerabiliteId])
        REFERENCES [dbo].[StatutVulnerabilite] ([StatutVulnerabiliteId]),
    CONSTRAINT [FK_Vulnerabilite_Projet] FOREIGN KEY ([ProjetId])
        REFERENCES [dbo].[Projet] ([ProjetId]),
    CONSTRAINT [FK_Vulnerabilite_AnalyseDetectee] FOREIGN KEY ([AnalyseDetecteeId])
        REFERENCES [dbo].[Analyse] ([AnalyseId])
);

CREATE TABLE [dbo].[Alerte] (
    [AlerteId]            INT           IDENTITY(1,1) NOT NULL,
    [StatutAlerteId]      INT           NOT NULL,
    [DateCreation]        DATETIME2(7)  NOT NULL CONSTRAINT [DF_Alerte_DateCreation] DEFAULT (SYSUTCDATETIME()),
    [DatePriseEnCharge]   DATETIME2(7)  NULL,
    [DateCloture]         DATETIME2(7)  NULL,
    [Commentaire]         NVARCHAR(MAX) NULL,
    [NotificationEnvoyee] BIT           NOT NULL CONSTRAINT [DF_Alerte_NotificationEnvoyee] DEFAULT (0),
    [VulnerabiliteId]     INT           NOT NULL,
    [PrisEnChargeParId]   INT           NULL,
    CONSTRAINT [PK_Alerte] PRIMARY KEY CLUSTERED ([AlerteId] ASC),
    CONSTRAINT [UQ_Alerte_VulnerabiliteId] UNIQUE NONCLUSTERED ([VulnerabiliteId] ASC),
    CONSTRAINT [FK_Alerte_StatutAlerte] FOREIGN KEY ([StatutAlerteId])
        REFERENCES [dbo].[StatutAlerte] ([StatutAlerteId]),
    CONSTRAINT [FK_Alerte_Vulnerabilite] FOREIGN KEY ([VulnerabiliteId])
        REFERENCES [dbo].[Vulnerabilite] ([VulnerabiliteId]),
    CONSTRAINT [FK_Alerte_PrisEnChargePar] FOREIGN KEY ([PrisEnChargeParId])
        REFERENCES [dbo].[Utilisateur] ([UtilisateurId])
);
GO
