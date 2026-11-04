using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Interfaces.Repositories
{
    public interface ICachingRepository
    {
        Task LoadCache();

        Task UpdateCache<T>() where T : LookupEntity<int>;
    }
}
