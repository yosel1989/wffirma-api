using COFIDE.WFF.Domain.Common.Errors;
namespace COFIDE.WFF.Domain.Aggregates.PlantillaAggregate.Errors;

public static class PlantillaErrors
{
    public static readonly DomainError IdPlantillaInvalido =
       new("DOM_PLT_001", "Id Plantilla inválido.");

    public static readonly DomainError IdPlantillaEtapaInvalido =
       new("DOM_PLT_002", "Id Plantilla Etapa inválido.");

    public static readonly DomainError OrdenEtapaInvalido =
       new("DOM_PLT_003", "Orden Etapa inválido.");

    public static readonly DomainError NombrePlantillaRequired = new("DOM_PLT_004", "Nombre de plantilla es obligatorio.");

    public static readonly DomainError SecuenciaInicioEtapasInvalida =
       new("DOM_PLT_005", "Las etapas deben iniciar en 1.");

    public static readonly DomainError SecuenciaEtapasInvalida = new("DOM_PLT_006", "No debe existir saltos en el orden de etapa.");
    public static readonly DomainError UsuarioAsignadoInvalido = new("DOM_PLT_007", "Usuario asignado inválido.");
    public static readonly DomainError CantidadMaximaAprobadores = new("DOM_PLT_008", "La cantidad máxima de aprobadores es {0}.");
    public static readonly DomainError ValidarNuevaEtapa = new("DOM_PLT_009", "No puedes agregar una nueva etapa porque la anterior es inválida");
    public static readonly DomainError ExistenciaPrimeraEtapa = new("DOM_PLT_010", "La plantilla debe tener al menos una etapa");
    public static readonly DomainError OrdenEtapaInvalidoEspecifico = new("DOM_PLT_011", "La etapa {0} no es válida");
    public static readonly DomainError LongitudNombrePlantilla = new("DOM_PLT_012", "Nombre de plantilla no debe exceder de los {0} caracteres.");
    public static readonly DomainError LongitudDescripcionPlantilla = new("DOM_PLT_013", "Descripción de plantilla no debe exceder de los {0} caracteres.");
    public static readonly DomainError TipoEtapaInvalido = new("DOM_PLT_014", "Tipo de etapa inválido.");
    public static readonly DomainError LimiteFirmantesSecuencial = new("DOM_PLT_015", "La etapa secuencial solo permite un firmante.");
    public static readonly DomainError LimiteFirmantesParalelo = new("DOM_PLT_016", "La etapa paralela requiere al menos dos firmantes.");
    public static readonly DomainError TipoAprobadorInvalido = new("DOM_PLT_017", "Tipo de aprobador inválido.");
    public static readonly DomainError EliminarPropioFirmante = new("DOM_PLT_019", "No puedes eliminarte a ti mismo del flujo.");
    public static readonly DomainError EliminarEtapaAprobada = new("DOM_PLT_020", "No puedes eliminar una etapa ya aprobada.");
    public static readonly DomainError CambiarOrdenEtapaAprobada = new("DOM_PLT_021", "No puedes cambiar el orden de una etapa ya aprobada.");
    public static readonly DomainError CambiarTipoEtapaEtapaAprobada = new("DOM_PLT_022", "No puedes cambiar el tipo de una etapa ya aprobada.");
    public static readonly DomainError ModificarFirmanteEtapaAprobada = new("DOM_PLT_023", "No puedes modificar firmantes de una etapa ya aprobada.");
    public static readonly DomainError ValidezNuevaEtapaFirmante = new("DOM_PLT_024", "La etapa debe tener al menos un firmante activo.");
    public static readonly DomainError ValidarPlantillaPublicaReutilizable = new("DOM_PLT_025", "La plantilla pública debe ser reutilizable.");

    public static readonly DomainError DescripcionPlantillaRequired = new("DOM_PLT_026", "Descripción de plantilla es obligatorio.");
    public static readonly DomainError PropietarioPlantilla = new("DOM_PLT_027", "Sólo el propietario de la plantilla puede actualizarlo.");   
    public static readonly DomainError ModificarPlantillaPublica = new("DOM_PLT_028", "Una plantilla pública no puede ser actualizada.");
    public static readonly DomainError PermisoCoordinadorPlantillaPrivada = new("DOM_SOL_034", "No tiene permisos para editar esta plantilla privada.");
    public static readonly DomainError PermisoCoordinadorPlantillaArea = new("DOM_SOL_034", "No tiene permisos para editar esta plantilla o pertenece a otra área.");
    public static readonly DomainError EsPlantillaPrivada = new("DOM_PLT_025", "La plantilla seleccionada es privada.");

}