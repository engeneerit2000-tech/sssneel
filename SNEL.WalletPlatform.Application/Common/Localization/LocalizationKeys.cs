namespace SNEL.WalletPlatform.Application.Common.Localization;

public static class LocalizationKeys
{
    // Default
    public const string InternalServerErrorDefaultMessage = "Default.InternalServerError";

    /// <summary>Default API envelope message for HTTP 201 (when not overridden).</summary>
    public const string ApiSuccessCreated = "Api.Success.Created";
    /// <summary>Default API envelope message for HTTP 200 (when not overridden).</summary>
    public const string ApiSuccessOk = "Api.Success.Ok";
    /// <summary>Default API envelope message for other 2xx (when not overridden).</summary>
    public const string ApiSuccessCompleted = "Api.Success.Completed";



    public const string UnAuthorizedDefaultMessage = "UnAuthorizedDefaultMessage";
    public const string Required = "Required";
    public const string PartnerNotFound = "Partner.NotFound";
    public const string PartnerDeleteBlockedByReferences = "Partner.Delete.BlockedByReferences";
    public const string CustomerNotFound = "Customer.NotFound";
    public const string CustomerProfileNotFound = "Customer.ProfileNotFound";
    public const string CustomerDeleteBlockedByReferences = "Customer.Delete.BlockedByReferences";
    public const string CustomerDeleteSuccess = "Customer.Delete.Success";
    public const string OtpChallengeCannontBeConsumed = "OtpChallenge.CannontBeConsumed";
    public const string OtpChallengeVerificationFailed = "OtpChallenge.VerificationFailed";
    public const string OtpChallengeCannontBeVerified = "OtpChallenge.CannontBeVerified";
    public const string OtpChallengeExpired = "OtpChallenge.Expired";
    public const string OtpChallengeNotFound = "OtpChallenge.NotFound";
    public const string UserInvalidPassword = "User.InvalidPassword";
    public const string UserPhoneNumberAlreadyExists = "User.PhoneNumberAlreadyExists";
    public const string UserMobileNoMustNotContainSpaces = "User.MobileNo.MustNotContainSpaces";
    public const string UserMobileNoInvalidLength = "User.MobileNo.InvalidLength";
    public const string UserEmailAlreadyExists = "User.EmailAlreadyExists";
    public const string UserNationalNoAlreadyExists = "User.NationalNoAlreadyExists";
    public const string AreaInvalid = "Lookups.AreaInvalid";
    public const string GenderInvalid = "Lookups.GenderInvalid";
    public const string IdentityUsernameAlreadyExists = "Identity.UsernameAlreadyExists";
    public const string IdentityEmailAlreadyExists = "Identity.EmailAlreadyExists";
    public const string UnexpectedError = "Common.UnexpectedError";
    public const string BillingInquiryInvalidRequest = "Billing.Inquiry.InvalidRequest";
    public const string BillingInquiryServiceNotFound = "Billing.Inquiry.ServiceNotFound";
    public const string BillingInquiryNotSupported = "Billing.Inquiry.NotSupported";
    public const string BillingInquiryConnectorNotConfigured = "Billing.Inquiry.ConnectorNotConfigured";
    public const string BillingInquiryNoInvoicesFound = "Billing.Inquiry.NoInvoicesFound";
    public const string BillingInquirySubscriptionNotFound = "Billing.Inquiry.SubscriptionNotFound";
    public const string BillingInquiryServiceIdNotFound = "Billing.Inquiry.ServiceIdNotFound";
    public const string BillingInquiryServiceCodeNotFound = "Billing.Inquiry.ServiceCodeNotFound";
    public const string BillingInquiryInvoiceRefRequired = "Billing.Inquiry.InvoiceRefRequired";
    public const string BillingInquiryInvoiceNotFound = "Billing.Inquiry.InvoiceNotFound";

    public const string BillingCustomActionsInvalidRequest = "Billing.CustomActions.InvalidRequest";
    public const string BillingCustomActionsServiceNotFound = "Billing.CustomActions.ServiceNotFound";
    public const string BillingCustomActionsNotSupported = "Billing.CustomActions.NotSupported";

    public const string BillingServiceCodeAlreadyExists = "Billing.Service.CodeAlreadyExists";
    public const string BillingServiceDuplicateCurrencies = "Billing.Service.DuplicateCurrencies";
    public const string BillingServiceDeleteBlockedByReferences = "Billing.Service.Delete.BlockedByReferences";
    public const string BillingServiceBankAccountRequired = "Billing.Service.BankAccount.Required";
    public const string BillingServiceBankAccountNotFound = "Billing.Service.BankAccount.NotFound";
    public const string BillingServiceBankAccountInvalidType = "Billing.Service.BankAccount.InvalidType";
    public const string BillingApplyIdentifierRequired = "Billing.Apply.IdentifierRequired";
    public const string BillingApplyPaymentNotFound = "Billing.Apply.PaymentNotFound";
    public const string BillingApplyPaymentNotCompleted = "Billing.Apply.PaymentNotCompleted";
    public const string BillingApplyNoEligibleItems = "Billing.Apply.NoEligibleItems";
    public const string BillingApplyConnectorNotConfigured = "Billing.Apply.ConnectorNotConfigured";
    public const string BillingApplyFailedForBiller = "Billing.Apply.FailedForBiller";
    public const string BillingApplyAtLeastOnePaymentIdRequired = "Billing.Apply.AtLeastOnePaymentIdRequired";
    public const string BillingApplyAtLeastOneValidPaymentIdRequired = "Billing.Apply.AtLeastOneValidPaymentIdRequired";
    public const string BillingApplySnelSettingsInvalid = "Billing.Apply.SnelSettingsInvalid";
    public const string BillingApplySnelInsertFailed = "Billing.Apply.SnelInsertFailed";
    public const string PaymentInitializeInvalidRequest = "Payment.Initialize.InvalidRequest";
    public const string PaymentInitializeMethodNotFound = "Payment.Initialize.MethodNotFound";
    public const string PaymentInitializeBillingServiceNotFound = "Payment.Initialize.BillingServiceNotFound";
    public const string PaymentInitializeMethodInactive = "Payment.Initialize.MethodInactive";
    public const string PaymentInitializeWalletAccountNotFound = "Payment.Initialize.WalletAccountNotFound";
    public const string PaymentInitializeWalletAccountNotActive = "Payment.Initialize.WalletAccountNotActive";
    public const string PaymentInitializeCurrencyNotSupported = "Payment.Initialize.CurrencyNotSupported";
    public const string PaymentInitializeAmountBelowMin = "Payment.Initialize.AmountBelowMin";
    public const string PaymentInitializeAmountAboveMax = "Payment.Initialize.AmountAboveMax";
    public const string PaymentInitializeChannelNotConfigured = "Payment.Initialize.ChannelNotConfigured";
    public const string PaymentInitializeInvalidConfig = "Payment.Initialize.InvalidConfig";
    public const string PaymentMethodNotFound = "Payment.Method.NotFound";
    public const string PaymentMethodCodeAlreadyExists = "Payment.Method.CodeAlreadyExists";
    public const string PaymentMethodIncompatible = "Payment.Method.Incompatible";
    public const string PaymentMethodCurrencyDuplicateCurrencies = "Payment.Method.Currency.DuplicateCurrencies";
    public const string PaymentProviderCodeAlreadyExists = "Payment.Provider.CodeAlreadyExists";
    public const string PaymentProviderPartnerAlreadyHasProvider = "Payment.Provider.PartnerAlreadyHasProvider";
    public const string PaymentProviderNotFound = "Payment.Provider.NotFound";
    public const string PaymentProviderCreateSuccess = "Payment.Provider.CreateSuccess";
    public const string PaymentProviderUpdateSuccess = "Payment.Provider.UpdateSuccess";
    public const string PaymentProviderDeleteSuccess = "Payment.Provider.DeleteSuccess";
    public const string CustomerSubscriptionNotFound = "CustomerSubscription.NotFound";
    public const string CustomerSubscriptionAlreadyExists = "CustomerSubscription.AlreadyExists";
    public const string CustomerSubscriptionAreaCodeRequired = "CustomerSubscription.AreaCodeRequired";
    public const string CustomerSubscriptionAreaCodeInvalid = "CustomerSubscription.AreaCodeInvalid";
    public const string CustomerSubscriptionSubscriptionNumberRequired = "CustomerSubscription.SubscriptionNumberRequired";
    public const string CustomerSubscriptionSubscriptionNumberInvalid = "CustomerSubscription.SubscriptionNumberInvalid";
    public const string CustomerSubscriptionInvalidSubscriptionNumber = "CustomerSubscription.InvalidSubscriptionNumber";
    public const string CustomerSubscriptionConfirmation = "CustomerSubscription.Confirmation";
    public const string AccountNotFoundForCustomer = "Account.NotFound";
    public const string AccountOrdersUpdateItemsRequired = "Account.OrdersUpdateItemsRequired";
    public const string PaymentAmountMustBePositive = "Payment.AmountInvalid";
    public const string BillingPayBillsInvoiceIdsRequired = "Payment.InvoicesAreRequired";
    public const string BillingPayBillsInvoiceIdentifierRequired = "Payment.InvoiceIdentifierRequired";
    public const string BillingPayBillsAmbiguousInvoiceIdentifiers = "Payment.AmbiguousInvoiceIdentifiers";
    public const string BillingPayBillsInvoicesNotFound = "Payment.BillsNotFound";
    public const string BillingPayBillsSomeInvoicesNotFound = "Payment.OneOrMoreBillsISNotFound";
    public const string PaymentAmountExceedsInvoiceOutstandingAmount = "Payment.AmountExceedsBillAmount";
    public const string PaymentAmountExceedsInvoiceAvailableAmount = "Payment.AmountExceedsBillAvailableAmount";
    public const string PaymentBillControlConcurrencyConflict = "Payment.BillControlConcurrencyConflict";
    public const string PaymentAmountMustEqualSumOfInvoicesOutstandingAmount = "Payment.BillsAmountMustBePaidAll";
    public const string BillingPayBillsInvoicesMustHaveSameCurrency = "Payment.BillsDonotHaveSameCurrency";
    public const string BillingPayBillsInvoicesMustHaveSameBillingRef = "Payment.BillsDonotHaveSameBillingReference";
    public const string AccountNotBelongToCustomerPartner = "Payment.AccountDoesNotBelongToCustomer";
    public const string AccountCurrencyMismatchWithInvoiceCurrency = "Payment.AccountInvoiceCurrencyMismtch";
    public const string AccountInsufficientBalance = "Payment.AccountInsufficientBalance";
    public const string BillsMustBeForTheSameSubscription = "Payment.BillsMustBeForTheSameSubscription";
    public const string InvoiceHasNoDueAmount = "Payment.InvoiceHasNoDueAmount";


    public const string BillerAccountDoesNotExist = "Account.BillerAccountDoesNotExist";


    public const string CloudTokenNotFound = "CloudToken.NotFound";

    public const string PaymentNotFound = "Payment.NotFound";
    public const string PaymentWasNotInitiated = "Payment.NotInitiated";
    public const string PaymentStatusNotPending = "Payment.StatusNotPending";
    public const string PaymentCancelNotAllowed = "Payment.Cancel.NotAllowed";
    public const string PaymentCancelLockConflict = "Payment.Cancel.LockConflict";
    public const string PaymentCancelInvalidFinalStatus = "Payment.Cancel.InvalidFinalStatus";
    public const string PaymentManualCompletePlatformOnly = "Payment.ManualComplete.PlatformOnly";
    public const string PaymentManualApplyPlatformOnly = "Payment.ManualApply.PlatformOnly";
    public const string PaymentManualReconcilePlatformOnly = "Payment.ManualReconcile.PlatformOnly";
    public const string PaymentIdRequired = "Payment.Id.Required";
    public const string PaymentExternalRefRequired = "Payment.ExternalRef.Required";

    public const string InvoiceNotFound = "Invoice.NotFound";
    public const string InvoiceInvalidId = "Invoice.InvalidId";

    public const string PlatformInvoiceNotFound = "PlatformInvoice.NotFound";
    public const string PlatformInvoiceInvalidId = "PlatformInvoice.InvalidId";

    public const string CreateJournal = "Journal.InvalidModel";

    public const string BillerAddInvalidRequest = "Biller.Add.InvalidRequest";
    public const string BillerAddCodeAlreadyExists = "Biller.Add.CodeAlreadyExists";
    public const string BillerAddPartnerLegalNameAlreadyExists = "Biller.Add.PartnerLegalNameAlreadyExists";
    public const string BillerAddAccountTypeNotFound = "Biller.Add.AccountTypeNotFound";
    public const string BillerAddCurrencyNotFound = "Biller.Add.CurrencyNotFound";
    public const string BillerAddSuccess = "Biller.Add.Success";
    public const string BillerNotFound = "Biller.NotFound";
    public const string BillerUpdateSuccess = "Biller.Update.Success";
    public const string BillerDeleteSuccess = "Biller.Delete.Success";

    // Customer Favorites
    public const string CustomerFavoritesFavoriteTypeNotFound = "CustomerFavorites.FavoriteTypeNotFound";
    public const string CustomerFavoritesSubscriptionNotFound = "CustomerFavorites.SubscriptionNotFound";
    public const string CustomerFavoritesSubscriptionNotBelongToCustomer = "CustomerFavorites.SubscriptionNotBelongToCustomer";
    public const string CustomerFavoritesAlreadyExists = "CustomerFavorites.AlreadyExists";

    public const string BillingInquiryBillingRefRequired = "BillingInquiry.BillingRefRequired";
    public const string AmountCheckFailed = "BillingInquiry.AmountCheckFailed";

    // Auth
    public const string AuthAccountCreatedSuccessfully = "Auth.AccountCreatedSuccessfully";
    public const string AuthPasswordUpdatedSuccessfully = "Auth.PasswordUpdatedSuccessfully";
    public const string AuthMobileUpdatedSuccessfully = "Auth.MobileUpdatedSuccessfully";
    public const string NotificationChannelInvalid = "Notification.Channel.Invalid";

    // Jobs
    public const string JobsEndMustBeOnOrAfterStart = "Jobs.EndMustBeOnOrAfterStart";
    public const string JobsNotRegisteredOrNotAvailable = "Jobs.NotRegisteredOrNotAvailable";
    public const string JobsTriggeredSuccessfully = "Jobs.TriggeredSuccessfully";

    // Payment (additional)
    public const string PaymentLockAcquireFailed = "Payment.LockAcquireFailed";
    public const string PaymentNoInvoicesToPay = "Payment.NoInvoicesToPay";
    public const string PaymentProviderPartnerNotFound = "Payment.Provider.PartnerNotFound";
    public const string PaymentHistoryOrderHasNoItems = "Payment.History.OrderHasNoItems";
    public const string PaymentHistoryUnsupportedItemType = "Payment.History.UnsupportedItemType";
    public const string PaymentBillerDailyJobLocked = "Payment.BillerDailyJobLocked";

    /// <summary>PSP init: client/Sdk-style next step (init-topup, init-bill, init-meter)</summary>
    public const string PaymentPspInitClientPrepared = "Payment.PspInit.ClientPrepared";
    public const string PaymentPspInitNeedPhoneNumber = "Payment.PspInit.NeedPhoneNumber";
    public const string PaymentPspInitPhoneNumberNotSupported = "Payment.PspInit.PhoneNumberNotSupported";
    /// <summary>PSP init: payment is processing or needs external or polling/webhook follow-up</summary>
    public const string PaymentPspInitProviderPending = "Payment.PspInit.ProviderPending";
    /// <summary>PSP init: completed in this call (journals, apply)</summary>
    public const string PaymentPspInitCompletedInSession = "Payment.PspInit.CompletedInSession";    
    public const string PaymentTopupSummaryCalculated = "Payment.Topup.SummaryCalculated";
    public const string PaymentMeterSummaryCalculated = "Payment.Meter.SummaryCalculated";
    public const string PaymentBillSummaryCalculated = "Payment.Bill.SummaryCalculated";
    public const string PaymentTopupInitNoPaymentRecord = "Payment.TopupInit.NoPaymentRecord";
    public const string PaymentTopupInitUnexpectedError = "Payment.TopupInit.UnexpectedError";
    public const string PaymentMeterInitNoPaymentRecord = "Payment.MeterInit.NoPaymentRecord";
    public const string PaymentBillInitNoPaymentRecord = "Payment.BillInit.NoPaymentRecord";
    public const string PaymentBillConfirmSuccess = "Payment.Bill.ConfirmSuccess";
    public const string PaymentCancelSuccess = "Payment.Cancel.Success";
    public const string PaymentBillCannotPaid = "Payment.BillInit.CannotPaid";
    public const string FileDownloadSuccess = "File.Download.Success";
    public const string AccountOrderReorderedSuccessfully = "Account.Order.ReorderedSuccessfully";
    public const string CustomerProfileUpdatedSuccessfully = "Customer.ProfileUpdatedSuccessfully";
    public const string PaymentConfirmNotOutgoingBillOrMeter = "Payment.Confirm.NotOutgoingBillOrMeter";
    public const string PaymentConfirmInvalidStatus = "Payment.Confirm.InvalidStatus";
    public const string PaymentConfirmationNeeded = "Payment.Confirm.Needed";
    public const string PaymentConfirmationExternal = "Payment.Confirm.External";
    public const string PaymentCompletedAndConfirmedSuccessfully = "Payment.Confirm.Completed";
    public const string PaymentConfirmedAndWillCompletedExternally = "Payment.Complete.External";

    public const string PaymentTopupRateQuerySuccess = "Payment.TopupRate.QuerySuccess";
    public const string PaymentTopupRateListSuccess = "Payment.TopupRate.ListSuccess";
    public const string PaymentTopupRateExportSuccess = "Payment.TopupRate.ExportSuccess";
    public const string PaymentCompletedTopupByCurrencyQuerySuccess = "Payment.CompletedTopup.ByCurrency.QuerySuccess";
    public const string PaymentCompletedTopupByCurrencyExportSuccess = "Payment.CompletedTopup.ByCurrency.ExportSuccess";
    public const string PaymentCompletedTopupByCurrencyRowsExportSuccess = "Payment.CompletedTopup.ByCurrency.RowsExportSuccess";
    public const string PaymentCompletedTopupByCurrencyRowsListSuccess = "Payment.CompletedTopup.ByCurrency.RowsListSuccess";
    public const string PaymentCompletedTopupByDateQuerySuccess = "Payment.CompletedTopup.ByDate.QuerySuccess";
    public const string PaymentCompletedTopupByDateExportSuccess = "Payment.CompletedTopup.ByDate.ExportSuccess";
    public const string PaymentCompletedTopupByDateRowsExportSuccess = "Payment.CompletedTopup.ByDate.RowsExportSuccess";
    public const string PaymentCompletedTopupByDateRowsListSuccess = "Payment.CompletedTopup.ByDate.RowsListSuccess";
    public const string PaymentCompletedTopupByDateInvalidDateRange = "Payment.CompletedTopup.ByDate.InvalidDateRange";
    public const string PaymentCompletedPostpaidPayBillByDateQuerySuccess = "Payment.CompletedPostpaidPayBill.ByDate.QuerySuccess";
    public const string PaymentCompletedPostpaidPayBillByDateExportSuccess = "Payment.CompletedPostpaidPayBill.ByDate.ExportSuccess";
    public const string PaymentCompletedPostpaidPayBillByDateRowsExportSuccess = "Payment.CompletedPostpaidPayBill.ByDate.RowsExportSuccess";
    public const string PaymentCompletedPostpaidPayBillByDateRowsListSuccess = "Payment.CompletedPostpaidPayBill.ByDate.RowsListSuccess";
    public const string PaymentCompletedPostpaidPayBillByDateInvalidDateRange = "Payment.CompletedPostpaidPayBill.ByDate.InvalidDateRange";
    public const string PaymentCompletedPostpaidPayBillByCenterQuerySuccess = "Payment.CompletedPostpaidPayBill.ByCenter.QuerySuccess";
    public const string PaymentCompletedPostpaidPayBillByCenterExportSuccess = "Payment.CompletedPostpaidPayBill.ByCenter.ExportSuccess";
    public const string PaymentCompletedPostpaidPayBillByCenterRowsExportSuccess = "Payment.CompletedPostpaidPayBill.ByCenter.RowsExportSuccess";
    public const string PaymentCompletedPostpaidPayBillByCenterRowsListSuccess = "Payment.CompletedPostpaidPayBill.ByCenter.RowsListSuccess";
    public const string PaymentCompletedPostpaidPayBillByCenterInvalidDateRange = "Payment.CompletedPostpaidPayBill.ByCenter.InvalidDateRange";
    public const string PaymentCompletedPrepaidPayBillByDateQuerySuccess = "Payment.CompletedPrepaidPayBill.ByDate.QuerySuccess";
    public const string PaymentCompletedPrepaidPayBillByDateExportSuccess = "Payment.CompletedPrepaidPayBill.ByDate.ExportSuccess";
    public const string PaymentCompletedPrepaidPayBillByDateRowsExportSuccess = "Payment.CompletedPrepaidPayBill.ByDate.RowsExportSuccess";
    public const string PaymentCompletedPrepaidPayBillByDateRowsListSuccess = "Payment.CompletedPrepaidPayBill.ByDate.RowsListSuccess";
    public const string PaymentCompletedPrepaidPayBillByDateInvalidDateRange = "Payment.CompletedPrepaidPayBill.ByDate.InvalidDateRange";
    public const string JournalLineStatementExportSuccess = "Account.JournalLineStatement.ExportSuccess";
    public const string PaymentValidationPspSyncNotPollable = "Payment.Validation.PspSyncNotPollable";
    public const string PaymentValidationPspSyncMissingRef = "Payment.Validation.PspSyncMissingRef";
    public const string PaymentValidationPspSyncNoMethod = "Payment.Validation.PspSyncNoMethod";
    public const string PaymentValidationPspSyncNoPsp = "Payment.Validation.PspSyncNoPsp";

    public const string CommonValidationPageSizePositive = "Common.Validation.PageSizePositive";
    public const string CommonValidationMinimumAgePositive = "Common.Validation.MinimumAgePositive";
    public const string PaymentPspConnectorNotRegisteredForPolling = "Payment.Psp.ConnectorNotRegisteredForPolling";
    public const string PaymentWebhookCannotCompleteInStatus = "Payment.Webhook.CannotCompleteInStatus";

    public const string PaymentAkibaPayInitiateMethodNotImplemented = "Payment.AkibaPay.InitiateMethodNotImplemented";
    public const string PaymentAkibaPayFromNumberRequired = "Payment.AkibaPay.FromNumberRequired";
    public const string PaymentAkibaPayFromNumberInvalid = "Payment.AkibaPay.FromNumberInvalid";
    public const string PaymentAkibaPayPaymentRefRequired = "Payment.AkibaPay.PaymentRefRequired";
    public const string PaymentAkibaPayInitiateHttpFailed = "Payment.AkibaPay.InitiateHttpFailed";
    public const string PaymentAkibaPayManagementCredentialsNotConfigured = "Payment.AkibaPay.ManagementCredentialsNotConfigured";
    public const string PaymentAkibaPayStatusLookupPaymentRefRequired = "Payment.AkibaPay.StatusLookupPaymentRefRequired";
    public const string PaymentAkibaPayManagementLoginHttpFailed = "Payment.AkibaPay.ManagementLoginHttpFailed";
    public const string PaymentAkibaPayManagementLoginParseFailed = "Payment.AkibaPay.ManagementLoginParseFailed";
    public const string PaymentAkibaPayStatusHttpFailed = "Payment.AkibaPay.StatusHttpFailed";
    public const string PaymentAkibaPayStatusParseFailed = "Payment.AkibaPay.StatusParseFailed";
    public const string PaymentAkibaPayStatusCheckUnexpectedError = "Payment.AkibaPay.StatusCheckUnexpectedError";
    public const string PaymentAkibaPayStatusNotFoundFailed = "Payment.AkibaPay.StatusNotFoundFailed";
    public const string PaymentAkibaPayConfigMissingOnMethod = "Payment.AkibaPay.ConfigMissingOnMethod";
    public const string PaymentAkibaPayConfigFieldsMissing = "Payment.AkibaPay.ConfigFieldsMissing";
    public const string PaymentAkibaPayConfigFieldEmpty = "Payment.AkibaPay.ConfigFieldEmpty";
    public const string PaymentAkibaPayConfigBaseUrlInvalid = "Payment.AkibaPay.ConfigBaseUrlInvalid";

    public const string PaymentEasypayInitiateMethodNotImplemented = "Payment.Easypay.InitiateMethodNotImplemented";
    public const string PaymentEasypayFromNumberRequired = "Payment.Easypay.FromNumberRequired";
    public const string PaymentEasypayFromNumberInvalid = "Payment.Easypay.FromNumberInvalid";
    public const string PaymentEasypayPaymentRefRequired = "Payment.Easypay.PaymentRefRequired";
    public const string PaymentEasypayInitiateHttpFailed = "Payment.Easypay.InitiateHttpFailed";
    public const string PaymentEasypayManagementCredentialsNotConfigured = "Payment.Easypay.ManagementCredentialsNotConfigured";
    public const string PaymentEasypayStatusLookupPaymentRefRequired = "Payment.Easypay.StatusLookupPaymentRefRequired";
    public const string PaymentEasypayManagementLoginHttpFailed = "Payment.Easypay.ManagementLoginHttpFailed";
    public const string PaymentEasypayManagementLoginParseFailed = "Payment.Easypay.ManagementLoginParseFailed";
    public const string PaymentEasypayStatusHttpFailed = "Payment.Easypay.StatusHttpFailed";
    public const string PaymentEasypayStatusParseFailed = "Payment.Easypay.StatusParseFailed";
    public const string PaymentEasypayStatusCheckUnexpectedError = "Payment.Easypay.StatusCheckUnexpectedError";
    public const string PaymentEasypayStatusReferenceNotFound = "Payment.Easypay.StatusReferenceNotFound";
    public const string PaymentEasypayConfigMissingOnMethod = "Payment.Easypay.ConfigMissingOnMethod";
    public const string PaymentEasypayConfigFieldsMissing = "Payment.Easypay.ConfigFieldsMissing";
    public const string PaymentEasypayConfigFieldEmpty = "Payment.Easypay.ConfigFieldEmpty";
    public const string PaymentEasypayConfigBaseUrlInvalid = "Payment.Easypay.ConfigBaseUrlInvalid";
    public const string PaymentEasypayCardCurrencyUnsupported = "Payment.Easypay.CardCurrencyUnsupported";
    public const string PaymentEasypayCardAmountInvalid = "Payment.Easypay.CardAmountInvalid";
    public const string PaymentEasypayCardEmailRequired = "Payment.Easypay.CardEmailRequired";
    public const string PaymentEasypayCardReturnUrlInvalid = "Payment.Easypay.CardReturnUrlInvalid";

    public const string PaymentAfrimonyInitiateMethodNotImplemented = "Payment.Afrimony.InitiateMethodNotImplemented";
    public const string PaymentAfrimonyFromNumberRequired = "Payment.Afrimony.FromNumberRequired";
    public const string PaymentAfrimonyFromNumberInvalid = "Payment.Afrimony.FromNumberInvalid";
    public const string PaymentAfrimonyFromNumberPrefixNotSupported = "Payment.Afrimony.FromNumberPrefixNotSupported";
    public const string PaymentAfrimonyPaymentRefRequired = "Payment.Afrimony.PaymentRefRequired";
    public const string PaymentAfrimonyInitiateHttpFailed = "Payment.Afrimony.InitiateHttpFailed";
    public const string PaymentAfrimonyOAuthCredentialsNotConfigured = "Payment.Afrimony.OAuthCredentialsNotConfigured";
    public const string PaymentAfrimonyStatusLookupPaymentRefRequired = "Payment.Afrimony.StatusLookupPaymentRefRequired";
    public const string PaymentAfrimonyOAuthHttpFailed = "Payment.Afrimony.OAuthHttpFailed";
    public const string PaymentAfrimonyOAuthServiceUnavailable = "Payment.Afrimony.OAuthServiceUnavailable";
    public const string PaymentAfrimonyOAuthParseFailed = "Payment.Afrimony.OAuthParseFailed";
    public const string PaymentAfrimonyStatusHttpFailed = "Payment.Afrimony.StatusHttpFailed";
    public const string PaymentAfrimonyStatusParseFailed = "Payment.Afrimony.StatusParseFailed";
    public const string PaymentAfrimonyStatusCheckUnexpectedError = "Payment.Afrimony.StatusCheckUnexpectedError";
    public const string PaymentAfrimonyStatusNotFoundFailed = "Payment.Afrimony.StatusNotFoundFailed";
    public const string PaymentAfrimonyConfigMissingOnMethod = "Payment.Afrimony.ConfigMissingOnMethod";
    public const string PaymentAfrimonyConfigFieldEmpty = "Payment.Afrimony.ConfigFieldEmpty";
    public const string PaymentAfrimonyConfigBaseUrlInvalid = "Payment.Afrimony.ConfigBaseUrlInvalid";
    public const string PaymentAfrimonyConfigOAuthUrlInvalid = "Payment.Afrimony.ConfigOAuthUrlInvalid";
    public const string PaymentAfrimonyCurrencyUnsupported = "Payment.Afrimony.CurrencyUnsupported";

    public const string PaymentDefaultPaymentMethodNotConfigured = "Payment.DefaultPaymentMethod.NotConfigured";
    public const string PaymentDefaultPaymentMethodInvalidConfig = "Payment.DefaultPaymentMethod.InvalidConfig";
    public const string PaymentMethodCodesBothOrNeitherRequired = "Payment.MethodCodes.BothOrNeitherRequired";
    public const string PaymentMethodCurrencyNotConfigured = "Payment.Method.CurrencyNotConfigured";
    public const string PaymentAmountOutsideMethodLimits = "Payment.Amount.OutsideMethodLimits";
    public const string PaymentPspConnectorMissing = "Payment.Psp.ConnectorMissing";
    public const string PaymentBillingServiceOrPartnerInactive = "Payment.BillingService.OrPartnerInactive";
    public const string PaymentBillingServiceCurrencyNotConfigured = "Payment.BillingService.CurrencyNotConfigured";
    public const string PaymentAmountOutsideBillingServiceLimits = "Payment.Amount.OutsideBillingServiceLimits";
    public const string PaymentBillingEligibilityNotMet = "Payment.Billing.EligibilityNotMet";
    public const string PaymentInvoiceCurrencyNotConfigured = "Payment.Invoice.CurrencyNotConfigured";
    public const string PaymentInvoiceBillingServiceMissing = "Payment.Invoice.BillingServiceMissing";
    public const string PaymentBillingServiceCurrencyInactive = "Payment.BillingServiceCurrency.Inactive";
    public const string PaymentBillerConnectorNotConfigured = "Payment.Biller.ConnectorNotConfigured";
    public const string PaymentInvoiceOngoing = "Payment.Invoice.OngoingPayment";
    public const string PaymentMeterReserveExceedsAvailable = "Payment.Meter.ReserveExceedsAvailable";
    public const string PaymentMeterOngoing = "Payment.Meter.OngoingPayment";
    public const string PaymentQuartzJobNotRegistered = "Payment.Quartz.JobNotRegistered";
    public const string PaymentCreateArtifactsControlReservationFailed = "Payment.CreateArtifacts.ControlReservationFailed";
    public const string PaymentOrderDescriptionVia = "Payment.Order.DescriptionVia";
    public const string PaymentPrintCurrencyNotOnMethod = "Payment.Print.CurrencyNotOnMethod";
    public const string BillingApplyPaymentNull = "Billing.Apply.PaymentNull";
    public const string BillerIsEligibleByDefault = "Biller.Connector.EligibleByDefault";
    public const string PaymentPspPaymentNotEligible = "Payment.Psp.PaymentNotEligible";
    public const string PaymentPspInitTransactionCreateFailed = "Payment.Psp.InitTransactionCreateFailed";
    public const string SnelBillerAreaNotSupported = "SnelBiller.AreaNotSupported";
    public const string SnelBillerReglementIdeMissing = "SnelBiller.Reglement.IdeMissing";
    public const string SnelBillerReglementIdeEmpty = "SnelBiller.Reglement.IdeEmpty";
    public const string SnelBillerReglementDuplicateItem = "SnelBiller.Reglement.DuplicateItem";
    public const string SnelBillerReglementRowCountMismatch = "SnelBiller.Reglement.RowCountMismatch";
    public const string SnelBillerBillingRefInvalid = "SnelBiller.BillingRef.Invalid";
    public const string SnelBillerBillingRefAreaEmpty = "SnelBiller.BillingRef.AreaEmpty";
    public const string SnelBillerAreaJournalLocked = "SnelBiller.AreaJournalLocked";

    public const string MeterRechargeExistenceCheckFailed = "MeterRecharge.ExistenceCheckFailed";

    // Billing Apply (additional)
    public const string BillingApplyNoConnectorForBiller = "Billing.Apply.NoConnectorForBiller";

    // Accounting
    public const string AccountingCustomerAccountNotFound = "Accounting.CustomerAccountNotFound";
    public const string AccountingPspAccountNotFound = "Accounting.PspAccountNotFound";
    public const string AccountingPspBankAccountNotFound = "Accounting.PspBankAccountNotFound";
    public const string AccountingRevenueAccountNotFound = "Accounting.RevenueAccountNotFound";
    public const string AccountingTaxPayableAccountNotFound = "Accounting.TaxPayableAccountNotFound";
    public const string AccountingTopupCostAccountNotFound = "Accounting.TopupCostAccountNotFound";
    public const string AccountingBillingServiceCurrencyNotConfigured = "Accounting.BillingServiceCurrencyNotConfigured";
    public const string AccountingBillerNotFound = "Accounting.BillerNotFound";
    public const string AccountingBillerAccountNotFound = "Accounting.BillerAccountNotFound";
    public const string AccountingBillerBankAccountNotFound = "Accounting.BillerBankAccountNotFound";
    public const string AccountingPaymentCostAccountNotFound = "Accounting.PaymentCostAccountNotFound";
    public const string AccountingUnsupportedItemType = "Accounting.UnsupportedItemType";

    #region  Journals

    public static class JournalPostingDescriptionKeys
    {
        public const string Topup = "JournalPosting.Topup";
        public const string TopupFees = "JournalPosting.TopupFees";
        public const string BillPayment = "JournalPosting.BillPayment";
        public const string TopupFeesTax = "JournalPosting.TopupFeesTax";
        public const string TopupCost = "JournalPosting.TopupCost";
        public const string TopupCostTax = "JournalPosting.TopupCostTax";
        public const string PaymentFees = "JournalPosting.PaymentFees";
        public const string PaymentFeesTax = "JournalPosting.PaymentFeesTax";
        public const string PaymentCost = "JournalPosting.PaymentCost";
        public const string PaymentCostTax = "JournalPosting.PaymentCostTax";
        public const string PaymentCommission = "JournalPosting.PaymentCommission";
        public const string PaymentCommissionTax = "JournalPosting.PaymentCommissionTax";
        public const string TopupSettlement = "JournalPosting.TopupSettlement";
        public const string PaymentSettlement = "JournalPosting.PaymentSettlement";
        public static string OpeningBalance = "JournalPosting.OpeningBalance";
        public static string PaymentVoucher = "JournalPosting.PaymentVoucher";
        public static string ReceiptVoucher = "JournalPosting.ReceiptVoucher";
        public static string CreditNote = "JournalPosting.CreditNote";
        public static string DebitNote = "JournalPosting.DebitNote";
        public static string ManualJournal = "JournalPosting.ManualJournal";

    }

    /// <summary>Localization keys for platform invoice line items (evaluate with requested culture).</summary>
    public static class PlatformInvoiceDescriptionKeys
    {
        public const string TopupFees = "PlatformInvoice.TopupFees";
        public const string TopupCost = "PlatformInvoice.TopupCost";
        public const string PaymentFees = "PlatformInvoice.PaymentFees";
        public const string PaymentCost = "PlatformInvoice.PaymentCost";
        public const string PaymentCommission = "PlatformInvoice.PaymentCommission";
    }

    #endregion

    #region  API create success messages (descriptive)

    public const string JournalPaymentVoucherCreateSuccess = "Journal.PaymentVoucher.CreateSuccess";
    public const string JournalReceiptVoucherCreateSuccess = "Journal.ReceiptVoucher.CreateSuccess";
    public const string JournalCreditNoteCreateSuccess = "Journal.CreditNote.CreateSuccess";
    public const string JournalDebitNoteCreateSuccess = "Journal.DebitNote.CreateSuccess";
    public const string JournalManualJournalCreateSuccess = "Journal.ManualJournal.CreateSuccess";
    public const string JournalOpenBalance = "Journal.OpenBalance";

    public const string CloudTokenCreateSuccess = "CloudToken.CreateSuccess";

    public const string CustomerCreateSuccess = "Customer.Create.Success";
    public const string CustomerFavoritesCreateSuccess = "CustomerFavorites.Create.Success";
    public const string CustomerSubscriptionCreateSuccess = "CustomerSubscription.Create.Success";

    public const string BillingServiceCreateSuccess = "Billing.Service.Create.Success";

    public const string UserCreateSuccess = "User.Create.Success";

    #endregion

    #region  API update success messages (descriptive)

    public const string CustomerUpdateSuccess = "Customer.Update.Success";
    public const string CustomerStatusUpdateSuccess = "Customer.Status.Update.Success";

    public const string CustomerFavoritesFavoriteSetSuccess = "CustomerFavorites.Favorite.Set.Success";
    public const string CustomerFavoritesFavoriteAlreadySetSuccess = "CustomerFavorites.Favorite.AlreadySet.Success";
    public const string CustomerFavoritesFavoriteUnsetSuccess = "CustomerFavorites.Favorite.Unset.Success";
    public const string CustomerFavoritesFavoriteAlreadyUnsetSuccess = "CustomerFavorites.Favorite.AlreadyUnset.Success";

    public const string CustomerSubscriptionUpdateSuccess = "CustomerSubscription.Update.Success";
    public const string BillingServiceUpdateSuccess = "Billing.Service.Update.Success";

    public const string UserUpdateSuccess = "User.Update.Success";

    public const string OtpChallengeGeneratedSuccess = "OtpChallenge.Generated.Success";

    #endregion

    #region  API delete success messages (descriptive)

    public const string AuthDeleteSuccess = "Auth.Delete.Success";
    public const string CloudTokenDeleteSuccess = "CloudToken.Delete.Success";
    public const string CustomerSubscriptionDeleteSuccess = "CustomerSubscription.Delete.Success";
    public const string UserDeleteSuccess = "User.Delete.Success";
    public const string UserDeleteBlockedByReferences = "User.Delete.BlockedByReferences";

    #endregion
    #region  Common

    public static class CommonKeys
    {
        public const string CurrencyNotFound = "Common.CurrencyNotFound";//Currency {0} not found.
        public const string PaymentMethodByCodeAndPSPCodeNotFound = "Common.PaymentMethodByCodeAndPSPCodeNotFound";//Payment Method {0} for PSP {1} not found.
        public const string AmountOutOfRange = "Common.AmountOutOfRange";//Amount {0} is out of range [{1},{2}].
        public const string CurrencyValidationError = "Common.CurrencyValidationError";//Error validating currency {0}.
    }

    #endregion

    public static class PaymentOrderKeys
    {
        public const string DescriptionTopup = "PaymentOrder.DescriptionTopup";
        public const string DescriptionTopupBref = "PaymentOrder.DescriptionTopupBref";
        public const string DescriptionPayBill = "PaymentOrder.DescriptionPayBill";
        public const string DescriptionPayBillBref = "PaymentOrder.DescriptionPayBillBref";
        public const string DescriptionMeterCharge = "PaymentOrder.DescriptionMeterCharge";
        public const string DescriptionMeterChargeBref = "PaymentOrder.DescriptionMeterChargeBref";
    }

    public const string CouldNotSendOTPRightNow = "CouldNotSendOTPRightNow";

    public static class PaymentKeys
    {
        public const string NotApplicable = "Payment.NotApplicable";
    }

    // Lookup translations
    public const string LookupTranslationTypeInvalid = "LookupTranslation.TypeInvalid";
    public const string LookupTranslationLookupNotFound = "LookupTranslation.LookupNotFound";
    public const string LookupTranslationLanguageRequired = "LookupTranslation.LanguageRequired";
    public const string LookupTranslationLanguageNotSupported = "LookupTranslation.LanguageNotSupported";
    public const string LookupTranslationNameRequired = "LookupTranslation.NameRequired";
    public const string LookupTranslationAdded = "LookupTranslation.Added";
    public const string LookupTranslationUpdated = "LookupTranslation.Updated";

    public static class BillerCustomKeys
    {
        public static class Actions {
            public static string Unknown = "BillerCustom.Actions.Unknown";
            public static string Error = "BillerCustom.Actions.Error";
        }
        

        public static class SNEL
        {
            public static string LastTransactionStatus = "BillerCustom.SNEL.LastTransactionStatus";
            public static string ListTransactions = "BillerCustom.SNEL.ListTransactions";
            public static string Reprint = "BillerCustom.SNEL.Reprint";
            public static string CountOfTransactionsToList = "BillerCustom.SNEL.CountOfTransactionsToList";
            public static string ReceiptNo = "BillerCustom.SNEL.ReceiptNo";
            public static string TransactionId = "BillerCustom.SNEL.TransactionId";
            public static string TransactionStatus = "BillerCustom.SNEL.TransactionStatus";
            public static string TransactionReceiptNo = "BillerCustom.SNEL.TransactionReceiptNo";
            public static string TransactionAmount = "BillerCustom.SNEL.TransactionAmount";
            public static string ReceiptNumber = "BillerCustom.SNEL.ReceiptNumber";
            public static string Tokens = "BillerCustom.SNEL.Tokens";
            public static string LastNTransactions = "BillerCustom.SNEL.LastNTransactions";
        }
    }

    // User management
    public const string UserNotFound = "User.NotFound";
    public const string UserManagementScopeOnlyOwnUsers = "User.Management.ScopeOnlyOwnUsers";
    public const string UserManagementInvalidRole = "User.Management.InvalidRole";
    public const string UserManagementRoleNotAllowed = "User.Management.RoleNotAllowed";
    public const string UserManagementCustomerCreatePlatformOnly = "User.Management.CustomerCreatePlatformOnly";
    public const string UserManagementPartnerIdRequired = "User.Management.PartnerIdRequired";
    public const string UserIdentityRefMissing = "User.IdentityRefMissing";
    public const string UserCannotDeleteSelf = "User.CannotDeleteSelf";
    public const string UserContextMissing = "User.ContextMissing";
    public const string UserActorNotFound = "User.ActorNotFound";
    public const string UserPartnerLinkRequired = "User.PartnerLinkRequired";
    public const string UserManagementForbidden = "User.Management.Forbidden";
    public const string UserManagementTargetMustBeCustomer = "User.Management.TargetMustBeCustomer";
    public const string PartnerTypeMismatchForRole = "Partner.TypeMismatchForRole";

    public const string CampaignNotFound = "Campaign.NotFound";
    public const string CampaignInvalidRequest = "Campaign.InvalidRequest";
    public const string CampaignTranslationsRequired = "Campaign.TranslationsRequired";
    public const string CampaignTranslationLanguageRequired = "Campaign.Translation.LanguageRequired";
    public const string CampaignTranslationLanguageNotSupported = "Campaign.Translation.LanguageNotSupported";
    public const string CampaignTranslationMessageRequired = "Campaign.Translation.MessageRequired";
    public const string CampaignTranslationTitleRequired = "Campaign.Translation.TitleRequired";
    public const string CampaignCreateSuccess = "Campaign.CreateSuccess";
    public const string CampaignUpdateSuccess = "Campaign.UpdateSuccess";
    public const string CampaignDeleteSuccess = "Campaign.DeleteSuccess";
    public const string CampaignUpdateNotAllowedWhenCompleted = "Campaign.Update.NotAllowedWhenCompleted";
    public const string CampaignScheduleNotAllowedWhenCompleted = "Campaign.Schedule.NotAllowedWhenCompleted";
    public const string CampaignAudienceGroupNotSupported = "Campaign.AudienceGroup.NotSupported";

    
}
