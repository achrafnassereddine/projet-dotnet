using System;
using System.Collections.Generic;

namespace DevSecOpsSaaS.Models;

public partial class Alerte
{
    public int AlerteId { get; set; }

    public int StatutAlerteId { get; set; }

    public DateTime DateCreation { get; set; }

    public DateTime? DatePriseEnCharge { get; set; }

    public DateTime? DateCloture { get; set; }

    public string? Commentaire { get; set; }

    public bool NotificationEnvoyee { get; set; }

    public int VulnerabiliteId { get; set; }

    public int? PrisEnChargeParId { get; set; }

    public virtual Utilisateur? PrisEnChargePar { get; set; }

    public virtual StatutAlerte StatutAlerte { get; set; } = null!;

    public virtual Vulnerabilite Vulnerabilite { get; set; } = null!;
}
