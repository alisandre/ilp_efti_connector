namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Categoria del mezzo/equipaggiamento usato nel trasporto (MILOS EcmrEquipmentCategory).
/// </summary>
public enum EcmrEquipmentCategory
{
    /// <summary>Container (CN)</summary>
    CONTAINER,
    /// <summary>Semirimorchio (SM)</summary>
    SEMITRAILER,
    /// <summary>Rimorchio (TE)</summary>
    TRAILER,
    /// <summary>Cassa mobile (SW)</summary>
    SWAP_BODY,
    /// <summary>Cisterna (TN)</summary>
    TANK,
    /// <summary>Pianale (FL)</summary>
    FLAT_RACK,
    /// <summary>Furgone (VN)</summary>
    VAN,
    /// <summary>Frigorifero (RF)</summary>
    REEFER,
    /// <summary>Open Top (OT)</summary>
    OPEN_TOP,
    /// <summary>Bulk (BK)</summary>
    BULK,
    /// <summary>Silos (SL)</summary>
    SILO,
    /// <summary>Veicolo completo (VC)</summary>
    VEHICLE_CARRIER,
    /// <summary>Altro</summary>
    OTHER
}
