using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Application.Partners.Models.Input;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Application.Partners.Interfaces;

public interface IPartnerRepository : IEntityRepository<PartnerEntity,long>
{
    Task<PartnerEntity?> GetByIdWithContactsAsync(long id);
    Task<bool> ExistsByLegalNameAsync(string legalName);
    Task<bool> ExistsByLegalNameExceptIdAsync(string legalName, long partnerId, CancellationToken cancellationToken = default);
    Task<bool> HasCustomersAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<bool> HasOtherBillersAsync(long partnerId, long? excludingBillerId = null, CancellationToken cancellationToken = default);
    Task<bool> HasOtherPaymentProvidersAsync(long partnerId, long? excludingPaymentProviderId = null, CancellationToken cancellationToken = default);
    Task<bool> HasPaymentMethodsAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<bool> HasPaymentsAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<bool> HasJournalLinesOnAccountsAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<bool> HasPartnerContactsAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<int> CountByVisibilityScopeAsync(long actorPartnerId, bool canViewAllNonCustomerPartners, RelatedPartnersQueryInputModel query);
    Task<IReadOnlyCollection<PartnerEntity>> GetPagedByVisibilityScopeAsync(long actorPartnerId, bool canViewAllNonCustomerPartners, RelatedPartnersQueryInputModel query);
    Task<int> CountAsync(PartnersQueryInputModel query);
    Task<IReadOnlyCollection<PartnerEntity>> GetPagedAsync(PartnersQueryInputModel query);
}
