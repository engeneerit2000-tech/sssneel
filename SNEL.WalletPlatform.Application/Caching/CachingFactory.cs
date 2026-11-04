using SNEL.WalletPlatform.Domain.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Caching
{
    public static class CachingFactory
    {
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();
        public static Dictionary<string, object> Cache => _cache;

        public static void SetValue<T>(List<T> value)
        {
            _cache[(typeof(T).Name)] = value;
        }

        public static List<T> GetValue<T>(Func<T, bool> predicate = null)
        {
            
            if (_cache.TryGetValue(typeof(T).Name, out var value) && value is List<T> list)
            {
                if(predicate is not null)
                {
                    return list.Where(predicate).ToList();
                }
                return list;
            }
            return new List<T>();
        }
    }
}
