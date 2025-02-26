using Auth_Turkeysoftware.Exceptions;

namespace Auth_Turkeysoftware.Enums
{
    public static class EnumExtension
    {
        private static string ERRO_ENUM_NAO_ENCONTRADO = "Não foi possível converter valor para um Enum";
        public static T? GetEnumByName<T>(string enumName)
        {
            try {
                foreach (T value in Enum.GetValues(typeof(T)))
                {
                    if (value.ToString().Equals(enumName))
                    {
                        return value;
                    }
                }
                return (T)Enum.ToObject(typeof(T), 0);
            } catch (ArgumentNullException) {
                throw new InvalidEnumValueException(ERRO_ENUM_NAO_ENCONTRADO);
            } catch (ArgumentException) {
                throw new InvalidEnumValueException(ERRO_ENUM_NAO_ENCONTRADO);
            }
        }

        public static T? GetEnumByInt<T>(int enumInt)
        {
            try {
                return (T)Enum.ToObject(typeof(T), enumInt);
            } catch (ArgumentNullException) {
                throw new InvalidEnumValueException(ERRO_ENUM_NAO_ENCONTRADO);
            } catch (ArgumentException) {
                throw new InvalidEnumValueException(ERRO_ENUM_NAO_ENCONTRADO);
            }
        }
    }
}
