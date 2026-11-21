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
