using COFIDE.WFF.Application.Features.Plantillas.Commands.RegistrarPlantilla;
using COFIDE.WFF.Application.Features.Plantillas.Queries.ValidarExistencia;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.AprobarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarEnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EditarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.EnviarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RechazarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.Commands.RegistrarSolicitud;
using COFIDE.WFF.Application.Features.Solicitudes.DTOs;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace COFIDE.WFF.Application.Tests.Tests.Solicitudes;

public class SolicitudValidatorTest
{
    [Fact]
    public void Registrar_Sin_Tipo_Debe_Fallar()
    {
        var resultado = new RegistrarSolicitudValidator().TestValidate(ComandoRegistrar(idTipo: 0));

        resultado.ShouldHaveValidationErrorFor(x => x.IdTipoSolicitud)
            .WithErrorCode("TIPO_SOLICITUD_INVALID");
    }

    [Fact]
    public void Registrar_Completo_Debe_Ser_Valido()
    {
        var resultado = new RegistrarSolicitudValidator().TestValidate(ComandoRegistrar(idTipo: 1));

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Enviar_Sin_Plantilla_Ni_Configuracion_Debe_Fallar()
    {
        var resultado = new EnviarSolicitudValidator().TestValidate(ComandoEnviar(idPlantilla: null, configuracion: null));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.ErrorCode == "CONFIGURACION_REQUERIDA");
    }

    [Fact]
    public void Enviar_Con_Plantilla_Debe_Ser_Valido()
    {
        var resultado = new EnviarSolicitudValidator().TestValidate(ComandoEnviar(idPlantilla: 7, configuracion: null));

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Editar_Sin_Asunto_Debe_Fallar()
    {
        var comando = new EditarSolicitudCommand(8, new SolicitudRequestDto(IdTipoSolicitud: 1, IdSubTipoSolicitud: 2, IdtipoOperacion: 3));

        var resultado = new EditarSolicitudValidator().TestValidate(comando);

        resultado.ShouldHaveValidationErrorFor(x => x.Request.Asunto)
            .WithErrorCode("ASUNTO_SOLICITUD_REQUIRED");
    }

    [Fact]
    public void EditarEnviar_Completo_Debe_Ser_Valido()
    {
        var comando = new EditarEnviarSolicitudCommand(8, new SolicitudRequestDto(
            IdTipoSolicitud: 1, IdSubTipoSolicitud: 2, IdtipoOperacion: 3, Asunto: "Contrato"));

        var resultado = new EditarEnviarSolicitudValidator().TestValidate(comando);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Aprobar_Con_Id_Cero_Debe_Fallar()
    {
        var resultado = new AprobarSolicitudValidator().TestValidate(new AprobarSolicitudCommand(0, null, null));

        resultado.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorCode("SOLICITUD_INVALID");
    }

    [Fact]
    public void Rechazar_Con_Id_Cero_Debe_Fallar()
    {
        var resultado = new RechazarSolicitudValidator().TestValidate(new RechazarSolicitudCommand(0, "motivo", null, null));

        resultado.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorCode("SOLICITUD_INVALID");
    }

    [Fact]
    public void Rechazar_Completo_Debe_Ser_Valido()
    {
        var resultado = new RechazarSolicitudValidator().TestValidate(new RechazarSolicitudCommand(8, "motivo", null, null));

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Plantilla_Reutilizable_Sin_Nombre_Debe_Fallar()
    {
        var resultado = new RegistrarPlantillaValidator().TestValidate(
            new RegistrarPlantillaCommand(null, "Descripcion", false, true, null));

        resultado.ShouldHaveValidationErrorFor(x => x.NombrePlantilla)
            .WithErrorCode("PLANTILLA_NAME_REQUIRED");
    }

    [Fact]
    public void Plantilla_No_Reutilizable_Sin_Nombre_Debe_Ser_Valida()
    {
        var resultado = new RegistrarPlantillaValidator().TestValidate(
            new RegistrarPlantillaCommand(null, null, false, false, null));

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Existencia_Sin_Nombre_Debe_Fallar()
    {
        var resultado = new ValidarExistenciaValidator().TestValidate(new ValidarExistenciaQuery(null!, true));

        resultado.ShouldHaveValidationErrorFor(x => x.NombrePlantilla)
            .WithErrorCode("PLANTILLA_NAME_REQUERID");
    }

    private static RegistrarSolicitudCommand ComandoRegistrar(long idTipo) =>
        new(idTipo, 2, 3, "Contrato", null, false, false, null, null, null, null, null, null, null, null, null);

    private static EnviarSolicitudCommand ComandoEnviar(long? idPlantilla, string? configuracion) =>
        new(1, 2, 3, "Contrato", null, false, false, null, null, null, null, null, idPlantilla, configuracion, null, null);
}
