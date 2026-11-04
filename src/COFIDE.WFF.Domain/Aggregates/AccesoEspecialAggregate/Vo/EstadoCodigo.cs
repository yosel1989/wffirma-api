using COFIDE.WFF.Domain.Common.ValueObjects;
using COFIDE.WFF.Domain.Exceptions;

namespace COFIDE.WFF.Domain.Aggregates.AccesoEspecialAggregate.Vo
{
    public sealed class EstadoCodigo : ValueObject
    {
        public static readonly EstadoCodigo Activo = new('A', "Activo");
        public static readonly EstadoCodigo Inactivo = new('I', "Inactivo");
        public static readonly EstadoCodigo Eliminado = new('E', "Eliminado");

        private static readonly Dictionary<char, EstadoCodigo> _estados =
            new()
            {
            { 'A', Activo },
            { 'I', Inactivo },
            { 'E', Eliminado }
            };

        public char Codigo { get; }
        public string Nombre { get; }

        private EstadoCodigo(char codigo, string nombre)
        {
            Codigo = codigo;
            Nombre = nombre;
        }

        public static EstadoCodigo FromCodigo(char codigo)
        {
            if (!_estados.TryGetValue(codigo, out var estado))
                throw new DomainException(new Common.Errors.DomainError("", $"Estado inválido: {codigo}"));

            return estado;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Codigo;
        }

        public override string ToString() => Nombre;
    }
}
