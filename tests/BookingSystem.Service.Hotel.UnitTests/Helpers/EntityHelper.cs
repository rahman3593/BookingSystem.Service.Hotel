using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Service.Hotel.UnitTests.Helpers
{
    public static class EntityHelper
    {
        public static void SetId<T>(T entity, int id) where T : class
        {
            // Get the Id property - it might be in the current type or base type
            var type = entity.GetType();
            var idProperty = type.GetProperty("Id");

            if (idProperty == null)
            {
                // Try to find it in the base type
                var baseType = type.BaseType;
                if (baseType != null)
                {
                    idProperty = baseType.GetProperty("Id");
                }
            }

            if (idProperty == null)
            {
                throw new InvalidOperationException($"Id property not found on type {type.Name}");
            }

            idProperty.SetValue(entity, id);
        }
    }
}
