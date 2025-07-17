using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Shared.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Trunca automaticamente todas as propriedades do tipo string de um objeto para o tamanho máximo
        /// definido pelos atributos <see cref="MaxLengthAttribute"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Este método de extensão verifica todas as propriedades string do objeto e trunca os valores
        /// que excedem o comprimento máximo especificado pelo <see cref="MaxLengthAttribute"/>.
        /// </para>
        /// <para>
        /// A operação é realizada in-place, modificando o objeto original. Propriedades sem
        /// <see cref="MaxLengthAttribute"/> permanecem inalteradas.
        /// </para>
        /// <example>
        /// <code>
        /// var usuario = new Usuario { Nome = "Nome muito longo que será truncado" };
        /// usuario.TruncateAllFields(); // Trunca se existir [MaxLength] na propriedade
        /// [MaxLenght(10)] //Nome: "Nome muito" 
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="obj">O objeto alvo cujas propriedades string serão truncadas. Não pode ser nulo.</param>
        /// <exception cref="ArgumentNullException">Lançada quando o objeto de entrada é nulo.</exception>
        public static void TruncateAllFields(this object obj)
        {
            var properties = obj.GetType().GetProperties();
            MaxLengthAttribute? maxLengthAttribute;

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (property.PropertyType == typeof(string))
                {
                    maxLengthAttribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(property, typeof(MaxLengthAttribute));
                    if (maxLengthAttribute != null)
                    {
                        if (property.GetValue(obj) is string value)
                        {
                            if (value.Length > maxLengthAttribute.Length)
                            {
                                property.SetValue(obj, value.Substring(0, maxLengthAttribute.Length));
                            }
                        }
                    }
                }
            }
        }
    }
}
