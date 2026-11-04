using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IAccountingService
{
    Task<Result<PagedResult<AccountOutputModel>>> GetCustomerAccountsAsync(AccountsQueryInputModel model);
    Task<Result<PagedResult<AccountOutputModel>>> GetAccountsAsync(AccountsQueryInputModel model);
    Task<Result<bool>> UpdateCustomerAccountsOrderAsync(IReadOnlyList<UpdateAccountOrderItemInputModel> items);
    // Task<Result<bool>> CreateJournalsForTopupAsync(CreateJournalForTopupInputModel model);

    // JournalEntity CreateJournalsForBillPaymentAsync(PaymentEntity payment, AccountEntity customerAccount, AccountEntity pspAccount, decimal amount);
    Task<Result<(List<JournalEntity> journals, List<PlatformInvoiceEntity> platformInvoices)>> CreateJournalsForPOI(
        long customerPartnerId,
        long pspPartnerId, 
        long? billerPartnerId,
        PaymentOrderItemEntity poi, 
        List<AccountEntity> accounts,
        bool autoSettleForPSP
    );
    Task<Result<bool>> CreateJournalForPaymentVoucher(CreateJournalForJornalTypeInputModel model);
    Task<Result<bool>> CreateJournalForReciptVoucher(CreateJournalForJornalTypeInputModel model);
    Task<Result<bool>> CreateJournalForCreditNote(CreateJournalForJornalTypeInputModel model);
    Task<Result<bool>> CreateJournalForDebitNote(CreateJournalForJornalTypeInputModel model);
    Task<Result<bool>> CreateJournalForManualJournal(CreateJournalForManualInputModel model);
    Task<Result<IReadOnlyList<AccountOutputModel>>> GetAllByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(GetAccountsInputModel model);
    Task<Result<IReadOnlyList<AccountOutputModel>>> GetAllBankAccountsForPartnerAsync(GetBankAccountsInputModel model);
}
