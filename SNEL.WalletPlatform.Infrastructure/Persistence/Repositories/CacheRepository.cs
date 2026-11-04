using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Interfaces.Repositories;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories
{
    public class CacheRepository(ApplicationDbContext context) : ICachingRepository
    {
        public async Task LoadCache()
        {
            var currencies = await context.Currencies.Include(c => c.Translations).AsNoTracking().ToListAsync();
            var countries = await context.Countries.Include(c => c.Translations).AsNoTracking().ToListAsync();
            var regions = await context.Regions.Include(r => r.Translations).AsNoTracking().ToListAsync();
            var cities = await context.Cities.Include(c => c.Translations).AsNoTracking().ToListAsync();
            var areas = await context.Areas.Include(a => a.Translations).AsNoTracking().ToListAsync();
            var genders = await context.Genders.Include(g => g.Translations).AsNoTracking().ToListAsync();
            var channels = await context.Channels.Include(c => c.Translations).AsNoTracking().ToListAsync();
            var partnerTypes = await context.PartnerTypes.Include(pt => pt.Translations).AsNoTracking().ToListAsync();
            var billingCategories = await context.BillingCategories.Include(bc => bc.Translations).AsNoTracking().ToListAsync();
            var billingServiceTypes = await context.BillingServiceTypes.Include(bst => bst.Translations).AsNoTracking().ToListAsync();
            var contactTypes = await context.ContactTypes.Include(ct => ct.Translations).AsNoTracking().ToListAsync();
            var paymentCategories = await context.PaymentCategories.Include(pc => pc.Translations).AsNoTracking().ToListAsync();
            var centers = await context.Centers.Include(c => c.Translations).AsNoTracking().ToListAsync();
            var completionModes = await context.CompletionModes.Include(cm => cm.Translations).AsNoTracking().ToListAsync();
            var confirmationModes = await context.ConfirmationModes.Include(cm => cm.Translations).AsNoTracking().ToListAsync();
            var confirmationTypes = await context.ConfirmationTypes.Include(ct => ct.Translations).AsNoTracking().ToListAsync();
            var paymentInitModes = await context.PaymentInitModes.Include(pim => pim.Translations).AsNoTracking().ToListAsync();
            var paymentMethodPurposes = await context.PaymentMethodPurposes.Include(pmp => pmp.Translations).AsNoTracking().ToListAsync();
            var journalLineTypes = await context.JournalLineTypes.Include(jlt => jlt.Translations).AsNoTracking().ToListAsync();
            var accountTypes = await context.AccountTypes.Include(at => at.Translations).AsNoTracking().ToListAsync();
            var paymentTypes = await context.PaymentTypes.Include(pt => pt.Translations).AsNoTracking().ToListAsync();
            CachingFactory.SetValue(currencies);
            CachingFactory.SetValue(countries);
            CachingFactory.SetValue(regions);
            CachingFactory.SetValue(cities);
            CachingFactory.SetValue(areas);
            CachingFactory.SetValue(genders);
            CachingFactory.SetValue(channels);
            CachingFactory.SetValue(partnerTypes);
            CachingFactory.SetValue(billingCategories);
            CachingFactory.SetValue(billingServiceTypes);
            CachingFactory.SetValue(contactTypes);
            CachingFactory.SetValue(paymentCategories);
            CachingFactory.SetValue(centers);
            CachingFactory.SetValue(completionModes);
            CachingFactory.SetValue(confirmationModes);
            CachingFactory.SetValue(confirmationTypes);
            CachingFactory.SetValue(paymentInitModes);
            CachingFactory.SetValue(paymentMethodPurposes);
            CachingFactory.SetValue(journalLineTypes);
            CachingFactory.SetValue(accountTypes);
            CachingFactory.SetValue(paymentTypes);            
        }

        public async Task UpdateCache<T>() where T : LookupEntity<int>
        {
            var data = await context.Set<T>().Include("Transactions").AsNoTracking().ToListAsync();
            CachingFactory.SetValue(data);
        }
    }
}
