using System.ComponentModel.DataAnnotations;

namespace Auth_Turkeysoftware.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Trunca todos os campos de string de um objeto para o comprimento máximo especificado pelo atributo MaxLength.
        /// </summary>
        /// <param name="obj">O objeto cujas propriedades de string serão truncadas.</param>
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
