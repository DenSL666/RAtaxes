using EveSdeModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticDataStorage.Models
{
    public abstract class BaseModel
    {
        protected static string Pack<T>(IEnumerable<T> collection) where T : class, new()
        {
            if (collection == null || !collection.Any())
                return string.Empty;
            return typeof(T) switch
            {
                Type t when t == typeof(BaseProduct) => string.Join("$", collection.OfType<BaseProduct>().Select(x => $"{x.TypeID};{x.Quantity}{x.Probability}")),
                Type t when t == typeof(BaseMaterial) => string.Join("$", collection.OfType<BaseMaterial>().Select(x => $"{x.TypeID};{x.Quantity}")),
                _ => throw new NotSupportedException($"Тип {typeof(T).Name} не поддерживается")
            };
        }

        protected static IEnumerable<T> UnPack<T>(string str) where T : class, new()
        {
            if (string.IsNullOrEmpty(str))
                return Enumerable.Empty<T>();
            var arr = str.Split('$').Select(x => x.Split(';')).ToArray();
            return typeof(T) switch
            {
                Type t when t == typeof(BaseProduct) => arr.Where(x => x.Length >= 2).Select(x => new BaseProduct() 
                { TypeID = x[0], Quantity = x[1], Probability = (x.Length == 3) ? x[2] : string.Empty }).Cast<T>(),
                Type t when t == typeof(BaseMaterial) => arr.Where(x => x.Length == 2).Select(x => new BaseMaterial() { TypeID = x[0], Quantity = x[1] }).Cast<T>(),
                _ => throw new NotSupportedException($"Тип {typeof(T).Name} не поддерживается")
            };
        }
    }
}
