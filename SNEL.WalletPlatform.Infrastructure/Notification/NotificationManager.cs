using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Notification.Configuration;
using  SNEL.WalletPlatform.Application.Notification.Interfaces;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Models.Output;
using SNEL.WalletPlatform.Domain.Customers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Infrastructure.Notification;

public class NotificationManager : INotificationManager
{
    private readonly NotificationConfigurations _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationManager> _logger;

    public NotificationManager(IHttpClientFactory httpClientFactory, IOptions<NotificationConfigurations> notificationConfigurations, ILogger<NotificationManager> logger)
    {
        _settings = notificationConfigurations.Value;
        _httpClient = httpClientFactory.CreateClient(NotificationConstants.NotificationServiceClient);
        _logger = logger;
    }

    private void AddLanguageHeader(HttpRequestMessage request)
    {
        request.Headers.Add("Accept-Language", Thread.CurrentThread.CurrentCulture.Name);
    }

    public async Task<Result<bool>> SendNotifictionAsync(SendNotificationInternalAPIInputModel model)
    {
        string url = _settings.BaseURL + $"/notification-api/notifications";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddLanguageHeader(request);

        var serializedBody = JsonSerializer.Serialize(model);
        request.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");

        _logger.LogInformation($"Notification Service API call - Send Notification. Body: {serializedBody}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase, serializedBody);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        if (result == null || result.Value == null)
        {
            LogNotificationServiceCommunicationError(url, result?.Message, serializedBody);
        }

        _logger.LogInformation($"Notification Service API call response: {result}");
        return result;
    }

    public async Task<Result<PagedResult<NotificationMessageInternalAPIOutputModel>>> GetNotificationMessagesAsync(GetNotificationsMessagesInternalAPIInputModel model)
    {
        string url = _settings.BaseURL + $"/notification-api/notifications";

        // add paramters
        url += $"?index={model.Index}&size={model.Size}&isAscending={model.IsAscending}&sortBy={model.SortBy}";
        url += $"&language={model.Language}&isRead={model.IsRead}&ReceiverId={model.ReceiverId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddLanguageHeader(request);

        _logger.LogInformation($"Notification Service API call - Get Notification Messages - Notification Service Request URI: {url}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase);
        }

        var result = await response.Content.ReadFromJsonAsync<QueryResult<NotificationMessageInternalAPIOutputModel>>();

        if (result == null || result.Data == null)
        {
            LogNotificationServiceCommunicationError(url, result.Title);
        }

        _logger.LogInformation($"Notification Service API call - Get Notification Messages response: {result}");
        return Result<PagedResult<NotificationMessageInternalAPIOutputModel>>
            .Success(new PagedResult<NotificationMessageInternalAPIOutputModel>(result.Data.Rows, result.Data.Count));
    }

    public async Task<Result<bool>> ReadNotificationAsync(Guid notificaitonId, Guid userId)
    {
        string url = _settings.BaseURL + $"/notification-api/notifications/{notificaitonId}/mark-read?userId={userId}";

        var request = new HttpRequestMessage(HttpMethod.Put, url);
        AddLanguageHeader(request);

        _logger.LogInformation($"Notification Service API call - Read Notification - Notification Service Request URI: {url}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();

        if (result == null || result.Value == null)
        {
            LogNotificationServiceCommunicationError(url, result?.Message);
        }

        _logger.LogInformation($"Notification Service API call - Read Notification response {result}");
        return result;
    }

    public async Task<Result<int>> GetNewNotificationsCountAsync(Guid userId, NotificationChannel channel)
    {
        string url = _settings.BaseURL + $"/notification-api/notification-receivers/{userId}/new-notificaitons-count?Channel={channel}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddLanguageHeader(request);

        _logger.LogInformation($"Notification Service API call - get Notifications count - Notification Service Request URI: {url}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<int>>();

        if (result == null || result.Value == null)
        {
            LogNotificationServiceCommunicationError(url, result?.Message);
        }
        _logger.LogInformation($"Notification Service API call - get Notifications count response: {result}");
        return result;
    }

    public async Task<Result<bool>> ResetNewNotificationsCountAsync(Guid userId, NotificationChannel channel)
    {
        string url = _settings.BaseURL + $"/notification-api/notification-receivers/{userId}/new-notificaitons-reset?channel={channel}";

        var request = new HttpRequestMessage(HttpMethod.Put, url);
        AddLanguageHeader(request);

        _logger.LogInformation($"Notification Service API call - reset new Notifications count - Notification Service Request URI: {url}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();

        if (result == null || result.Value == null)
        {
            LogNotificationServiceCommunicationError(url, result?.Message);
        }

        _logger.LogInformation($"Notification Service API call - reset new Notifications count response {result}");
        return result;
    }

    public async Task<Result<bool>> CreateCloudMessagingTokenAsync(Guid receiverId, string tokenId, int? platformId, string? deviceName)
    {
        string url = _settings.BaseURL + $"/notification-api/receivers/{receiverId}/cm-tokens";

        var body = new
        {
            tokenId,
            platformId = platformId ?? 0,
            deviceName
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddLanguageHeader(request);
        var serializedBody = JsonSerializer.Serialize(body);
        request.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");

        _logger.LogInformation("Notification Service API call - Create CM Token. URI: {Url}, Body: {Body}", url, serializedBody);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase, serializedBody);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        if (result == null || result.Value == null)
        {
            LogNotificationServiceCommunicationError(url, result?.Message, serializedBody);
            return Result<bool>.Failure(result?.Message ?? "Notification service returned empty response.", AppHttpStatusCodes.Failed);
        }

        _logger.LogInformation("Notification Service API call - Create CM Token response: {Result}", result);
        return result;
    }

    public async Task<Result<bool>> DeleteCloudMessagingTokenAsync(Guid receiverId, string tokenId)
    {
        string url = _settings.BaseURL + $"/notification-api/receivers/{receiverId}/cm-tokens";

        var body = new { tokenId };
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        AddLanguageHeader(request);
        var serializedBody = JsonSerializer.Serialize(body);
        request.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");

        _logger.LogInformation("Notification Service API call - Delete CM Token. URI: {Url}, Body: {Body}", url, serializedBody);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            LogNotificationServiceCommunicationError(url, response.ReasonPhrase, serializedBody);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        if (result == null || result.Value == null)
        {
            LogNotificationServiceCommunicationError(url, result?.Message, serializedBody);
            return Result<bool>.Failure(result?.Message ?? "Notification service returned empty response.", AppHttpStatusCodes.Failed);
        }

        _logger.LogInformation("Notification Service API call - Delete CM Token response: {Result}", result);
        return result;
    }

    private void LogNotificationServiceCommunicationError(string url, string? message = null, string? body = null)
    {
        _logger.LogError($"Notification service communication failed. URL: {url}, Body: {body}, Returned Message: {message}");
    }    
}