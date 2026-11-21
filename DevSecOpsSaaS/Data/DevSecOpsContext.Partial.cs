using System.Linq;
using DevSecOpsSaaS.Models;
using Microsoft.EntityFrameworkCore;

namespace DevSecOpsSaaS.Data;

public partial class DevSecOpsContext
{
    public int? OrganisationIdCourante { get; set; }

    public int? UtilisateurIdCourant { get; set; }

    public bool FiltrerParAffectationDeveloppeur { get; set; }

    public void AppliquerFiltresMultiTenant(ModelBuilder modelBuilder)
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
                                      || e.Utilisateurs.Any(u => u.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<ClePipeline>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Projet.Utilisateurs.Any(u => u.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<Analyse>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Projet.Utilisateurs.Any(u => u.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<Vulnerabilite>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Projet.Utilisateurs.Any(u => u.UtilisateurId == UtilisateurIdCourant.Value))));

        modelBuilder.Entity<Alerte>()
            .HasQueryFilter(e => !OrganisationIdCourante.HasValue
                              || (e.Vulnerabilite.Projet.OrganisationId == OrganisationIdCourante.Value
                                  && (!FiltrerParAffectationDeveloppeur
                                      || !UtilisateurIdCourant.HasValue
                                      || e.Vulnerabilite.Projet.Utilisateurs.Any(u => u.UtilisateurId == UtilisateurIdCourant.Value))));
    }
}