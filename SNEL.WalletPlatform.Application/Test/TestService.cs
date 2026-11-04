using System.Globalization;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.FileExport.Services;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoices.Mapper;
using SNEL.WalletPlatform.Application.Invoices.Models.Output;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Application.Notification;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Application.Test.Models;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Domain.Webhooks;

namespace SNEL.WalletPlatform.Application.Test;

public class TestService(IUnitOfWork unitOfWork,
    IMapper mapper,
    NotificationService notificationService,
    ITemplateRednerer templateRenderer,
    IHtmlToPdfGenerator htmlToPdfConverter,
    ExcelFileExportService excelFileExportService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<TestService> logger,
    IAccountingService accountingService
    )
{


    public async Task<Result<bool>> SendNotification(Guid userId, bool? isTransactional, DateTime? sendingAt)
    {
        var customer = await unitOfWork.UserRepository.GetByUserIdsWithNotificationsDependenciesAsync([userId]);
        // Send Notification
        var notificationModel = new NotificationDetailsModel()
        {
            IsTransactional = isTransactional ?? true,
            NotificationCode = NotificationCodeEnum.OTP,
            NotificationType = NotificationTypeEnum.OTP,
            Params = [new ParameterDataModel() { Language = Common.Enums.LanguageEnum.FR, Params = ["p1"] }],
            SenderId = null,
            ReferenceType = ReferenceTypeEnum.PROFILE,
            ReferenceId = userId.ToString(),
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = notificationService.GetTargettedChannels(new List<NotificationChannel>() { NotificationChannel.Push }),
            Receivers = customer,
            SendingAt = sendingAt,
        };
        await notificationService.SendNotificationsAsync(new List<NotificationDetailsModel>() { notificationModel });



        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SendNavigableNotificationAsync(SendNavigableNotificationTestInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ReferenceId))
        {
            return Result<bool>.Failure("ReferenceId is required.", AppHttpStatusCodes.BadRequest);
        }

        var users = await unitOfWork.UserRepository.GetByUserIdsWithNotificationsDependenciesAsync([model.UserId]);
        if (users.Count == 0)
        {
            return Result<bool>.Failure($"User not found or has no notification dependencies: {model.UserId}", AppHttpStatusCodes.NotFound);
        }

        var requestedChannels = new List<NotificationChannel> { NotificationChannel.Push };
        if (model.IncludeSms)
            requestedChannels.Add(NotificationChannel.SMS);

        var channels = notificationService.GetTargettedChannels(requestedChannels, model.IsTransactional);
        if (channels.Count == 0)
        {
            return Result<bool>.Failure("No enabled notification channels for the request.", AppHttpStatusCodes.BadRequest);
        }

        var parameters = model.Parameters ?? [];

        var notificationModel = new NotificationDetailsModel
        {
            IsTransactional = model.IsTransactional,
            NotificationCode = model.NotificationCode,
            NotificationType = model.NotificationType,
            Params =
            [
                new ParameterDataModel
                {
                    Language = Common.Enums.LanguageEnum.FR,
                    Params = parameters
                },
                new ParameterDataModel
                {
                    Language = Common.Enums.LanguageEnum.EN,
                    Params = parameters
                }
            ],
            SenderId = null,
            ReferenceType = model.ReferenceType,
            ReferenceId = model.ReferenceId,
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = channels,
            Receivers = users,
            SaveInAppNotification = true,
            PushMessageMode = PushMessageMode.Notification,
        };

        var sent = await notificationService.SendNotificationsAsync([notificationModel]);
        if (!sent)
        {
            return Result<bool>.Failure("Notification service returned failure.", AppHttpStatusCodes.Failed);
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SendDataPushNotificationAsync(Guid userId, SendDataPushTestInputModel model)
    {
        var users = await unitOfWork.UserRepository.GetByUserIdsWithNotificationsDependenciesAsync([userId]);
        if (users.Count == 0)
        {
            return Result<bool>.Failure($"User not found or has no notification dependencies: {userId}", AppHttpStatusCodes.NotFound);
        }

        var pushChannels = notificationService.GetTargettedChannels([NotificationChannel.Push]);
        if (pushChannels.Count == 0)
        {
            return Result<bool>.Failure("Push channel is disabled in notification settings.", AppHttpStatusCodes.BadRequest);
        }

        var pushData = ResolvePushData(model);
        if (pushData.Count == 0)
        {
            return Result<bool>.Failure(
                "PushData is required. Provide pushData or top-up shortcut fields (eventType, paymentId, walletAccountId, amount, currency, balance).",
                AppHttpStatusCodes.BadRequest);
        }

        var notificationModel = new NotificationDetailsModel
        {
            IsTransactional = model.IsTransactional,
            NotificationCode = model.NotificationCode,
            NotificationType = model.NotificationType,
            SenderId = null,
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = pushChannels,
            Receivers = users,
            SaveInAppNotification = false,
            PushMessageMode = PushMessageMode.Data,
            PushData = pushData,
        };

        var sent = await notificationService.SendNotificationsAsync([notificationModel]);
        if (!sent)
        {
            return Result<bool>.Failure("Notification service returned failure.", AppHttpStatusCodes.Failed);
        }

        return Result<bool>.Success(true);
    }

    private static Dictionary<string, string> ResolvePushData(SendDataPushTestInputModel model)
    {
        return DataPushPayloadBuilder.Merge(model.PushData,
            ("eventType", model.EventType),
            ("paymentId", model.PaymentId),
            ("walletAccountId", model.WalletAccountId),
            ("amount", model.Amount),
            ("currency", model.Currency),
            ("balance", model.Balance));
    }

    public async Task<Result<PagedResult<AccountTypeLookupOutputModel>>> GetAccountTypesLocalized(string LanguageCode)
    {
        var accountTypes = await unitOfWork.AccountTypeTranslatableRepository.GetActiveAsync(LanguageCode);


        var items = mapper.Map<AccountTypeLookupOutputModel[]>(accountTypes);

        return Result<PagedResult<AccountTypeLookupOutputModel>>.Success(
            new PagedResult<AccountTypeLookupOutputModel>(items, 10));
    }

    public async Task<Result<byte[]>> TestHtmlToPDF(Guid invoiceId, CancellationToken cancellationToken)
    {
        var modelResult = await BuildInvoicePrintModelAsync(invoiceId, cancellationToken);

        if (!modelResult.IsSuccess)
            return Result<byte[]>.Failure(modelResult.Message, modelResult.StatusCode);
        var html = await templateRenderer.RenderAsync("Invoice/invoice", modelResult.Value, cancellationToken);
        var pdfBytes = await htmlToPdfConverter.GenerateFromHtmlAsync(html);

        return Result<byte[]>.Success(pdfBytes);
    }

    public async Task<Result<string>> TestHtmlPreview(Guid invoiceId, CancellationToken cancellationToken)
    {
        var modelResult = await BuildInvoicePrintModelAsync(invoiceId, cancellationToken);

        if (!modelResult.IsSuccess)
            return Result<string>.Failure(modelResult.Message, modelResult.StatusCode);

        var html = await templateRenderer.RenderAsync("Invoice/invoice", modelResult.Value, cancellationToken);

        return Result<string>.Success(html);
    }

    private async Task<Result<InvoicePrintModel>> BuildInvoicePrintModelAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithBillingServiceAsync(invoiceId);

        if (invoice is null)
        {
            return Result<InvoicePrintModel>.Failure(
                localizer[LocalizationKeys.InvoiceNotFound, invoiceId],
                AppHttpStatusCodes.NotFound);
        }

        var customerOpenInvoices =
            await unitOfWork.InvoiceRepository.GetCustomerOpenInvoicesAsync(invoice.BillingRef);

        var info = InvoiceInfoModel.Parse(invoice.InvoiceInfo);
        var languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var input = new InvoicePrintMapInput(invoice, info, customerOpenInvoices, languageCode);
        var model = mapper.Map<InvoicePrintModel>(input);

        return Result<InvoicePrintModel>.Success(model);
    }


    public async Task<Result<FileContentResult>> ExportExcelFileAsync()
    {

        var lookups = await unitOfWork.LookupRepository.GetAllAsync(localizer);
        var counties = lookups.Countries;

        var data = mapper.Map<List<object>>(counties);

        var fileResult = await excelFileExportService.ExportExcelFileAsync("TestExcel", data, typeof(LookupItemOutputModel));

        return fileResult;
    }

    public async Task<bool> TestTransaction()
    {
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var aki = AkibaPayWebhookEventEntity.Create("refref1", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

            await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
            // Simulate an error
            await unitOfWork.CompleteAsync();
        });
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var aki = AkibaPayWebhookEventEntity.Create("refref6", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

            await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var aki = AkibaPayWebhookEventEntity.Create("refref7", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

                await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
                // Simulate an error
                await unitOfWork.CompleteAsync();
            });

            // Simulate an error
            await unitOfWork.CompleteAsync();

        });

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var aki = AkibaPayWebhookEventEntity.Create("refref2", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

            await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var aki = AkibaPayWebhookEventEntity.Create("refref3", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

                await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
                // Simulate an error
                await unitOfWork.CompleteAsync();
            });

            // Simulate an error
            await unitOfWork.CompleteAsync();
            throw new Exception("Simulated outer transaction error");
        });
        
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var aki = AkibaPayWebhookEventEntity.Create("refref4", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

            await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var aki = AkibaPayWebhookEventEntity.Create("refref5", "Test Payload", "Test Signature", null, null, 0, "dd", null, null, null, null, null);

                await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(aki, ct);
                // Simulate an error
                await unitOfWork.CompleteAsync();
                throw new Exception("Simulated outer transaction error");
            });

            // Simulate an error
            await unitOfWork.CompleteAsync();
            
        });
        

        return true;




    }
}


public class TestExcelModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Age { get; set; }
}
