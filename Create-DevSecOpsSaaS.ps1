<#
.SYNOPSIS
    Genere la structure complete et la base DevSecOpsDB du projet DevSecOpsSaaS (ASP.NET Core MVC .NET 10).
.NOTES
    Fichier 100% ASCII : compatible Windows PowerShell 5.1 et PowerShell 7.
#>

[CmdletBinding()]
param(
    [string]$SqlInstance      = "localhost",
    [string]$ConnectionString = "",
    [switch]$SkipDatabase,
    [switch]$SkipScaffold
)

$ErrorActionPreference = "Stop"

$RootName     = "DevSecOpsSaaS"
$SolutionFile = "DevSecOpsSaaS.sln"
$WorkDir      = (Get-Location).Path
$ProjectPath  = Join-Path $WorkDir $RootName
$ProjectFile  = Join-Path $ProjectPath "$RootName.csproj"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "Server=$SqlInstance;Database=DevSecOpsDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

function Write-Step([string]$msg) { Write-Host ""; Write-Host "=== $msg ===" -ForegroundColor Cyan }
function Write-Ok([string]$msg)   { Write-Host "  [OK]   $msg" -ForegroundColor Green }
function Write-Skip([string]$msg) { Write-Host "  [SKIP] $msg" -ForegroundColor Yellow }
function Write-Warn([string]$msg) { Write-Host "  [WARN] $msg" -ForegroundColor Yellow }

# ============================================================
# [0/8] Prerequis
# ============================================================
Write-Step "[0/8] Verification des prerequis"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "SDK .NET introuvable. Installez .NET 10 SDK depuis https://dot.net"
}
$sdkVersion = (dotnet --version).Trim()
Write-Ok "SDK .NET : $sdkVersion"

$sqlcmdAvailable = [bool](Get-Command sqlcmd -ErrorAction SilentlyContinue)
if (-not $SkipDatabase -and -not $sqlcmdAvailable) {
    Write-Warn "sqlcmd introuvable. Utilisez -SkipDatabase ou installez mssql-tools."
}

$efAvailable = [bool](Get-Command dotnet-ef -ErrorAction SilentlyContinue)
if (-not $efAvailable) {
    Write-Host "  Installation de dotnet-ef (outil global)..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef --version 10.* | Out-Null
    $env:PATH = "$env:PATH;$env:USERPROFILE\.dotnet\tools"
}
Write-Ok "dotnet-ef disponible"

# ============================================================
# [1/8] Solution + projet MVC
# ============================================================
Write-Step "[1/8] Solution et projet MVC"

if (-not (Test-Path (Join-Path $WorkDir $SolutionFile))) {
    dotnet new sln -n "DevSecOpsSaaS" --format sln | Out-Null
    Write-Ok "Solution $SolutionFile creee"
} else {
    Write-Skip "Solution $SolutionFile deja presente"
}

if (-not (Test-Path $ProjectFile)) {
    dotnet new mvc -n $RootName --framework net10.0 | Out-Null
    dotnet sln $SolutionFile add $ProjectFile | Out-Null
    Write-Ok "Projet MVC $RootName cree et ajoute a la solution"
} else {
    dotnet sln $SolutionFile add $ProjectFile 2>$null | Out-Null
    Write-Skip "Projet $RootName deja present"
}

# ============================================================
# [2/8] Packages NuGet compatibles .NET 10 (10.*)
# ============================================================
Write-Step "[2/8] Packages NuGet (.NET 10)"

Push-Location $ProjectPath
try {
    dotnet remove package Microsoft.EntityFrameworkCore.SqlServer 2>$null | Out-Null
    dotnet remove package Microsoft.EntityFrameworkCore.Design 2>$null | Out-Null
    dotnet remove package Microsoft.EntityFrameworkCore.Tools 2>$null | Out-Null
    dotnet remove package Microsoft.AspNetCore.Authentication.JwtBearer 2>$null | Out-Null

    $packages = @(
        @{ Name = "Microsoft.EntityFrameworkCore.SqlServer"; Version = "10.*" },
        @{ Name = "Microsoft.EntityFrameworkCore.Design";    Version = "10.*" },
        @{ Name = "Microsoft.EntityFrameworkCore.Tools";     Version = "10.*" },
        @{ Name = "Microsoft.AspNetCore.Authentication.JwtBearer"; Version = "10.*" },
        @{ Name = "BCrypt.Net-Next";                       Version = "4.*" }
    )

    foreach ($pkg in $packages) {
        dotnet add package $pkg.Name --version $pkg.Version --no-restore | Out-Null
        Write-Ok "Package configure : $($pkg.Name) ($($pkg.Version))"
    }

    dotnet restore | Out-Null
    Write-Ok "Restore termine sans erreur NU1202"
} finally {
    Pop-Location
}

# ============================================================
# [3/8] Arborescence
# ============================================================
Write-Step "[3/8] Arborescence"

$folders = @(
    "Areas/Admin/Controllers",
    "Areas/Admin/Views/Dashboard",
    "Areas/Admin/Views/Utilisateurs",
    "Areas/Admin/Views/Projets",
    "Areas/Admin/Views/ClesApi",
    "Areas/Admin/Views/Parametres",
    "Areas/Dev/Controllers",
    "Areas/Dev/Views/Dashboard",
    "Areas/Dev/Views/Projets",
    "Areas/Dev/Views/Vulnerabilites",
    "Areas/Soc/Controllers",
    "Areas/Soc/Views/Dashboard",
    "Areas/Soc/Views/Alertes",
    "Controllers",
    "Data",
    "Database",
    "DTOs",
    "Models",
    "Models/Enums",
    "Security",
    "Services",
    "ViewModels/Admin",
    "ViewModels/Dev",
    "ViewModels/Soc",
    "ViewModels/Compte",
    "ViewModels/Profil",
    "Views/Compte",
    "Views/Profil",
    "wwwroot/css",
    "wwwroot/js"
)

foreach ($f in $folders) {
    $p = Join-Path $ProjectPath $f
    if (-not (Test-Path $p)) {
        New-Item -ItemType Directory -Path $p -Force | Out-Null
        Write-Ok "Dossier : $f"
    }
}

# ============================================================
# [4/8] Scripts SQL (5 tables de reference + 10 tables metier)
# ============================================================
Write-Step "[4/8] Scripts SQL"

$sqlSchema = @'
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
'@

$sqlSeed = @'
USE [DevSecOpsDB];
GO

INSERT INTO [dbo].[RoleUtilisateur] ([Code], [Libelle]) VALUES
    (N'Admin',       N'Administrateur Organisation'),
    (N'Developpeur', N'Developpeur'),
    (N'AnalysteSOC', N'Analyste SOC');

INSERT INTO [dbo].[NiveauCriticite] ([Code], [Libelle]) VALUES
    (N'Faible',   N'Criticite Faible'),
    (N'Moyenne',  N'Criticite Moyenne'),
    (N'Haute',    N'Criticite Haute'),
    (N'Critique', N'Criticite Critique');

INSERT INTO [dbo].[NiveauGravite] ([Code], [Libelle]) VALUES
    (N'Faible',   N'Gravite Faible'),
    (N'Moyenne',  N'Gravite Moyenne'),
    (N'Haute',    N'Gravite Haute'),
    (N'Critique', N'Gravite Critique');

INSERT INTO [dbo].[StatutVulnerabilite] ([Code], [Libelle]) VALUES
    (N'Ouverte',           N'Ouverte'),
    (N'EnCoursCorrection', N'En cours de correction'),
    (N'Corrigee',          N'Corrigee');

INSERT INTO [dbo].[StatutAlerte] ([Code], [Libelle]) VALUES
    (N'Nouvelle',    N'Nouvelle'),
    (N'EnCours',     N'En cours'),
    (N'Resolue',     N'Resolue'),
    (N'FauxPositif', N'Faux positif');

INSERT INTO [dbo].[Organisation] ([Nom]) VALUES
    (N'Atlas CyberOps'),
    (N'Maghreb CloudSec');

DECLARE @Org1 INT = (SELECT [OrganisationId] FROM [dbo].[Organisation] WHERE [Nom] = N'Atlas CyberOps');
DECLARE @Org2 INT = (SELECT [OrganisationId] FROM [dbo].[Organisation] WHERE [Nom] = N'Maghreb CloudSec');

DECLARE @RoleAdmin INT = (SELECT [RoleId] FROM [dbo].[RoleUtilisateur] WHERE [Code] = N'Admin');
DECLARE @RoleDev   INT = (SELECT [RoleId] FROM [dbo].[RoleUtilisateur] WHERE [Code] = N'Developpeur');
DECLARE @RoleSoc   INT = (SELECT [RoleId] FROM [dbo].[RoleUtilisateur] WHERE [Code] = N'AnalysteSOC');

DECLARE @CritCritique INT = (SELECT [CriticiteId] FROM [dbo].[NiveauCriticite] WHERE [Code] = N'Critique');
DECLARE @CritMoyenne  INT = (SELECT [CriticiteId] FROM [dbo].[NiveauCriticite] WHERE [Code] = N'Moyenne');
DECLARE @CritHaute    INT = (SELECT [CriticiteId] FROM [dbo].[NiveauCriticite] WHERE [Code] = N'Haute');

DECLARE @GravCritique INT = (SELECT [GraviteId] FROM [dbo].[NiveauGravite] WHERE [Code] = N'Critique');
DECLARE @GravHaute    INT = (SELECT [GraviteId] FROM [dbo].[NiveauGravite] WHERE [Code] = N'Haute');
DECLARE @GravMoyenne  INT = (SELECT [GraviteId] FROM [dbo].[NiveauGravite] WHERE [Code] = N'Moyenne');

DECLARE @StatVulnOuverte INT = (SELECT [StatutVulnerabiliteId] FROM [dbo].[StatutVulnerabilite] WHERE [Code] = N'Ouverte');
DECLARE @StatVulnEnCours INT = (SELECT [StatutVulnerabiliteId] FROM [dbo].[StatutVulnerabilite] WHERE [Code] = N'EnCoursCorrection');

DECLARE @StatAlerteNouvelle INT = (SELECT [StatutAlerteId] FROM [dbo].[StatutAlerte] WHERE [Code] = N'Nouvelle');
DECLARE @StatAlerteEnCours  INT = (SELECT [StatutAlerteId] FROM [dbo].[StatutAlerte] WHERE [Code] = N'EnCours');

INSERT INTO [dbo].[ParametreScoring] ([OrganisationId], [SeuilAlerte], [TelegramChatId]) VALUES
    (@Org1, 8.00, N'-100200300400'),
    (@Org2, 7.50, NULL);

INSERT INTO [dbo].[Utilisateur] ([Email], [MotDePasseHash], [Nom], [Prenom], [Telephone], [RoleId], [EstActif], [OrganisationId]) VALUES
    (N'admin@atlas-cyberops.ma',   N'$2a$11$DemoSeedHashAdminAtlas2026', N'Benali',     N'Youssef', N'+212600000001', @RoleAdmin, 1, @Org1),
    (N'dev1@atlas-cyberops.ma',    N'$2a$11$DemoSeedHashDev1Atlas2026',  N'El Amrani',  N'Salma',   N'+212600000002', @RoleDev,   1, @Org1),
    (N'soc@atlas-cyberops.ma',     N'$2a$11$DemoSeedHashSocAtlas2026',   N'Mansouri',   N'Nadia',   N'+212600000003', @RoleSoc,   1, @Org1),
    (N'admin@maghreb-cloudsec.ma', N'$2a$11$DemoSeedHashAdminCloud2026', N'Ouali',      N'Mehdi',   N'+212600000004', @RoleAdmin, 1, @Org2);

DECLARE @AdminOrg1 INT = (SELECT [UtilisateurId] FROM [dbo].[Utilisateur] WHERE [Email] = N'admin@atlas-cyberops.ma');
DECLARE @Dev1Org1   INT = (SELECT [UtilisateurId] FROM [dbo].[Utilisateur] WHERE [Email] = N'dev1@atlas-cyberops.ma');
DECLARE @SocOrg1    INT = (SELECT [UtilisateurId] FROM [dbo].[Utilisateur] WHERE [Email] = N'soc@atlas-cyberops.ma');

INSERT INTO [dbo].[Invitation] ([Email], [RoleId], [Jeton], [DateExpiration], [EstUtilisee], [OrganisationId], [InviteParId]) VALUES
    (N'nouveau.dev@atlas-cyberops.ma', @RoleDev, N'inv_tok_9f8e7d6c5b4a3210', DATEADD(DAY, 7, SYSUTCDATETIME()), 0, @Org1, @AdminOrg1);

INSERT INTO [dbo].[Projet] ([Nom], [DepotUrl], [CriticiteId], [ExposeInternet], [OrganisationId]) VALUES
    (N'Passerelle Paiement Core', N'https://github.com/atlas-cyberops/payment-gateway.git', @CritCritique, 1, @Org1),
    (N'Portail Interne RH',       N'https://github.com/atlas-cyberops/hr-internal.git',     @CritMoyenne,  0, @Org1),
    (N'API Client CloudSec',       N'https://github.com/maghreb-cloudsec/client-api.git',    @CritHaute,    1, @Org2);

DECLARE @ProjPaiement INT = (SELECT [ProjetId] FROM [dbo].[Projet] WHERE [Nom] = N'Passerelle Paiement Core');
DECLARE @ProjRH       INT = (SELECT [ProjetId] FROM [dbo].[Projet] WHERE [Nom] = N'Portail Interne RH');
DECLARE @ProjCloud    INT = (SELECT [ProjetId] FROM [dbo].[Projet] WHERE [Nom] = N'API Client CloudSec');

INSERT INTO [dbo].[ProjetUtilisateur] ([ProjetId], [UtilisateurId]) VALUES
    (@ProjPaiement, @Dev1Org1);

INSERT INTO [dbo].[ClePipeline] ([Nom], [HashCle], [Prefixe], [EstRevoquee], [ProjetId]) VALUES
    (N'GitHub Actions - Main',  N'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855', N'dso_live_pay_', 0, @ProjPaiement),
    (N'GitLab CI - Internal',   N'8f434346648f6b96df89dda901c5176b10a6d83961dd3c1ac88b59b2dc327aa4', N'dso_live_hr__', 0, @ProjRH),
    (N'GitHub Actions - Cloud', N'2c26b46b68ffc68ff99b453c1d30413413422d706483bfa0f98a5e886266e7ae', N'dso_live_cld_', 0, @ProjCloud);

DECLARE @ClePaiement INT = (SELECT [ClePipelineId] FROM [dbo].[ClePipeline] WHERE [Prefixe] = N'dso_live_pay_');
DECLARE @CleRH       INT = (SELECT [ClePipelineId] FROM [dbo].[ClePipeline] WHERE [Prefixe] = N'dso_live_hr__');
DECLARE @CleCloud    INT = (SELECT [ClePipelineId] FROM [dbo].[ClePipeline] WHERE [Prefixe] = N'dso_live_cld_');

INSERT INTO [dbo].[Analyse] ([Outil], [CommitSha], [Branche], [NumeroBuild], [NbVulnerabilites], [ScoreMax], [ProjetId], [ClePipelineId]) VALUES
    (N'Trivy',   N'a1b2c3d4e5f60718293a4b5c6d7e8f9012345678', N'main',    N'build-1042', 2, 9.80, @ProjPaiement, @ClePaiement),
    (N'Semgrep', N'9876543210fedcba0987654321fedcba09876543', N'develop', N'build-218',  1, 5.40, @ProjRH,       @CleRH),
    (N'Trivy',   N'11223344556677889900aabbccddeeff00112233', N'main',    N'build-509',  1, 8.60, @ProjCloud,    @CleCloud);

DECLARE @AnaPaiement INT = (SELECT [AnalyseId] FROM [dbo].[Analyse] WHERE [NumeroBuild] = N'build-1042');
DECLARE @AnaRH       INT = (SELECT [AnalyseId] FROM [dbo].[Analyse] WHERE [NumeroBuild] = N'build-218');
DECLARE @AnaCloud    INT = (SELECT [AnalyseId] FROM [dbo].[Analyse] WHERE [NumeroBuild] = N'build-509');

INSERT INTO [dbo].[Vulnerabilite] ([CveId], [Titre], [Description], [Fichier], [Ligne], [Package], [Recommandation], [ScoreBrut], [ScoreReel], [GraviteId], [StatutVulnerabiliteId], [ProjetId], [AnalyseDetecteeId]) VALUES
    (N'CVE-2024-3094',  N'XZ Utils Backdoor',       N'Version compromise de xz-utils.',        N'Dockerfile',              14, N'xz-utils@5.6.1',      N'Mettre a jour >= 5.6.2.', 9.80, 9.80, @GravCritique, @StatVulnOuverte, @ProjPaiement, @AnaPaiement),
    (N'CVE-2024-21626', N'Echappement conteneur runc',N'Fuite de descripteur vers l hote.',     N'Dockerfile',              3,  N'runc@1.1.11',         N'Mettre a jour >= 1.1.12.', 8.60, 8.60, @GravHaute,    @StatVulnEnCours, @ProjPaiement, @AnaPaiement),
    (N'CWE-89-01',      N'Concatenation SQL',         N'Requete SQL potentiellement non param.',N'Services/ReportService.cs',88, N'System.Data.SqlClient', N'Utiliser EF Core.',         5.40, 5.40, @GravMoyenne,  @StatVulnOuverte, @ProjRH,       @AnaRH),
    (N'CVE-2025-1001',  N'Deserialisation SDK',       N'Entree non validee dans le SDK.',       N'src/deserializer.go',     42, N'pkg/codec@1.4.0',     N'Mettre a jour >= 1.4.2.', 8.60, 8.60, @GravHaute,    @StatVulnOuverte, @ProjCloud,    @AnaCloud);

DECLARE @Vuln1 INT = (SELECT [VulnerabiliteId] FROM [dbo].[Vulnerabilite] WHERE [CveId] = N'CVE-2024-3094' AND [ProjetId] = @ProjPaiement);
DECLARE @Vuln2 INT = (SELECT [VulnerabiliteId] FROM [dbo].[Vulnerabilite] WHERE [CveId] = N'CVE-2024-21626' AND [ProjetId] = @ProjPaiement);
DECLARE @Vuln4 INT = (SELECT [VulnerabiliteId] FROM [dbo].[Vulnerabilite] WHERE [CveId] = N'CVE-2025-1001' AND [ProjetId] = @ProjCloud);

INSERT INTO [dbo].[Alerte] ([StatutAlerteId], [DatePriseEnCharge], [Commentaire], [NotificationEnvoyee], [VulnerabiliteId], [PrisEnChargeParId]) VALUES
    (@StatAlerteNouvelle, NULL,               N'Alerte critique sur passerelle exposee.', 1, @Vuln1, NULL),
    (@StatAlerteEnCours,  SYSUTCDATETIME(), N'Investigation runc en cours.',             1, @Vuln2, @SocOrg1),
    (@StatAlerteNouvelle, NULL,               N'Alerte haute criticite CloudSec.',          0, @Vuln4, NULL);
GO
'@

Set-Content -Path (Join-Path $ProjectPath "Database/01_CreateSchema.sql") -Value $sqlSchema -Encoding UTF8
Set-Content -Path (Join-Path $ProjectPath "Database/02_SeedData.sql")      -Value $sqlSeed   -Encoding UTF8
Write-Ok "Scripts SQL ecrits dans Database/"

# ============================================================
# [5/8] Enums C# + DbContext partiel
# ============================================================
Write-Step "[5/8] Enums C# et DbContext partiel"

Set-Content -Path (Join-Path $ProjectPath "Models/Enums/RoleCode.cs") -Encoding UTF8 -Value @'
namespace DevSecOpsSaaS.Models.Enums;

public enum RoleCode
{
    Admin = 1,
    Developpeur = 2,
    AnalysteSOC = 3
}
'@

Set-Content -Path (Join-Path $ProjectPath "Models/Enums/CriticiteCode.cs") -Encoding UTF8 -Value @'
namespace DevSecOpsSaaS.Models.Enums;

public enum CriticiteCode
{
    Faible = 1,
    Moyenne = 2,
    Haute = 3,
    Critique = 4
}
'@

Set-Content -Path (Join-Path $ProjectPath "Models/Enums/GraviteCode.cs") -Encoding UTF8 -Value @'
namespace DevSecOpsSaaS.Models.Enums;

public enum GraviteCode
{
    Faible = 1,
    Moyenne = 2,
    Haute = 3,
    Critique = 4
}
'@

Set-Content -Path (Join-Path $ProjectPath "Models/Enums/StatutVulnerabiliteCode.cs") -Encoding UTF8 -Value @'
namespace DevSecOpsSaaS.Models.Enums;

public enum StatutVulnerabiliteCode
{
    Ouverte = 1,
    EnCoursCorrection = 2,
    Corrigee = 3
}
'@

Set-Content -Path (Join-Path $ProjectPath "Models/Enums/StatutAlerteCode.cs") -Encoding UTF8 -Value @'
namespace DevSecOpsSaaS.Models.Enums;

public enum StatutAlerteCode
{
    Nouvelle = 1,
    EnCours = 2,
    Resolue = 3,
    FauxPositif = 4
}
'@

$partialDbContext = @'
using DevSecOpsSaaS.Models;
using Microsoft.EntityFrameworkCore;

namespace DevSecOpsSaaS.Data;

public partial class DevSecOpsContext
{
    public int? OrganisationIdCourante { get; set; }

    public int? UtilisateurIdCourant { get; set; }

    public bool FiltrerParAffectationDeveloppeur { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organisation>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || e.OrganisationId == OrganisationIdCourante.Value);

        modelBuilder.Entity<ParametreScoring>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || e.OrganisationId == OrganisationIdCourante.Value);

        modelBuilder.Entity<Utilisateur>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || e.OrganisationId == OrganisationIdCourante.Value);

        modelBuilder.Entity<Invitation>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || e.OrganisationId == OrganisationIdCourante.Value);

        modelBuilder.Entity<Projet>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.ProjetUtilisateurs.Any(pu => pu.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<ProjetUtilisateur>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.UtilisateurId == UtilisateurIdCourant.Value)));

        modelBuilder.Entity<ClePipeline>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Projet.ProjetUtilisateurs.Any(pu => pu.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<Analyse>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Projet.ProjetUtilisateurs.Any(pu => pu.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<Vulnerabilite>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Projet.ProjetUtilisateurs.Any(pu => pu.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<Alerte>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Vulnerabilite.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Vulnerabilite.Projet.ProjetUtilisateurs.Any(pu => pu.UtilisateurId == UtilisateurIdCourant.Value))));
    }
}
'@
Set-Content -Path (Join-Path $ProjectPath "Data/DevSecOpsContext.Partial.cs") -Value $partialDbContext -Encoding UTF8
Write-Ok "Enums C# et DevSecOpsContext.Partial.cs ecrits"

# ============================================================
# [6/8] .gitkeep
# ============================================================
Write-Step "[6/8] Fichiers .gitkeep"

$emptyFolders = @(
    "Areas/Admin/Views/Dashboard", "Areas/Admin/Views/Utilisateurs",
    "Areas/Admin/Views/Projets",  "Areas/Admin/Views/ClesApi",
    "Areas/Admin/Views/Parametres",
    "Areas/Dev/Views/Dashboard",  "Areas/Dev/Views/Projets",
    "Areas/Dev/Views/Vulnerabilites",
    "Areas/Soc/Views/Dashboard",  "Areas/Soc/Views/Alertes",
    "DTOs", "Security", "Services",
    "ViewModels/Admin", "ViewModels/Dev", "ViewModels/Soc",
    "ViewModels/Compte", "ViewModels/Profil",
    "Views/Compte", "Views/Profil"
)
foreach ($f in $emptyFolders) {
    $gitkeep = Join-Path $ProjectPath (Join-Path $f ".gitkeep")
    if (-not (Test-Path $gitkeep)) { New-Item -ItemType File -Path $gitkeep -Force | Out-Null }
}
Write-Ok ".gitkeep places"

# ============================================================
# [7/8] Base de donnees
# ============================================================
Write-Step "[7/8] Base de donnees"

if ($SkipDatabase) {
    Write-Skip "Creation de la base ignoree (-SkipDatabase)"
} elseif (-not $sqlcmdAvailable) {
    Write-Warn "sqlcmd indisponible. Executez manuellement :"
    Write-Host "  sqlcmd -S $SqlInstance -E -i `"$ProjectPath\Database\01_CreateSchema.sql`""
    Write-Host "  sqlcmd -S $SqlInstance -E -d DevSecOpsDB -i `"$ProjectPath\Database\02_SeedData.sql`""
} else {
    $schemaFile = Join-Path $ProjectPath "Database/01_CreateSchema.sql"
    $seedFile   = Join-Path $ProjectPath "Database/02_SeedData.sql"

    Write-Host "  Execution du schema sur $SqlInstance ..."
    & sqlcmd -b -S $SqlInstance -E -i $schemaFile
    if ($LASTEXITCODE -ne 0) {
        throw "Echec de 01_CreateSchema.sql (code $LASTEXITCODE)."
    }

    Write-Host "  Insertion du jeu de donnees ..."
    & sqlcmd -b -S $SqlInstance -E -d DevSecOpsDB -i $seedFile
    if ($LASTEXITCODE -ne 0) {
        throw "Echec de 02_SeedData.sql (code $LASTEXITCODE)."
    }

    Write-Ok "Base DevSecOpsDB creee et peuplee"
}

# ============================================================
# [8/8] Scaffolding EF Core
# ============================================================
Write-Step "[8/8] Scaffolding EF Core"

if ($SkipScaffold) {
    Write-Skip "Scaffolding ignore (-SkipScaffold)"
} else {
    Push-Location $ProjectPath
    try {
        dotnet ef dbcontext scaffold `
            "$ConnectionString" `
            Microsoft.EntityFrameworkCore.SqlServer `
            --context DevSecOpsContext `
            --context-dir Data `
            --output-dir Models `
            --no-onconfiguring `
            --force

        if ($LASTEXITCODE -ne 0) {
            throw "Echec du scaffolding EF Core (code $LASTEXITCODE)."
        }
        Write-Ok "Entites generees dans Models/ et Data/DevSecOpsContext.cs"
    } finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "[TERMINE] Chapitre 1 pret et compile." -ForegroundColor Green