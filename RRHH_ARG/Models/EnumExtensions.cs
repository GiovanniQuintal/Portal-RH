using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace RRHH_ARG.Models
{
    public static class EnumExtensions
    {
        // Método que usa reflexión para obtener el valor del atributo Display(Name)
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? enumValue.ToString();
        }
    }
}

