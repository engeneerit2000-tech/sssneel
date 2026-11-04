using SNEL.WalletPlatform.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Domain.Users;

public sealed class UserEntity : TrackedEntity<Guid>
{
    private UserEntity() { }

    private UserEntity(
        string username,
        string firstName,
        string? secondName,
        string? thirdName,
        string lastName,
        int genderId,
        string mobileNo,
        string email,
        string nationalNo,
        string language,
        NotificationChannel[] notificationChannels,
        string? iamRef)
    {
        Username = username;
        FirstName = firstName;
        SecondName = secondName;
        ThirdName = thirdName;
        LastName = lastName;
        GenderId = genderId;
        MobileNo = mobileNo;
        Email = email;
        NationalNo = nationalNo;
        IamRef = iamRef;
        Language = language;
        NotificationPreferences = notificationChannels
            .Distinct()
            .Select(channel => UserNotificationPreferenceEntity.Create(channel))
            .ToList();
        Status = UserStatus.Active;
    }

    public string? IamRef { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? SecondName { get; private set; }
    public string? ThirdName { get; private set; }
    public string LastName { get; private set; } = string.Empty;
    public int GenderId { get; private set; }
    public GenderEntity? Gender { get; set; } = null!;
    
    public string MobileNo { get; private set; }
    public string Email { get; private set; }
    public string NationalNo { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;

    public string Language { get; private set; } = "en";
    public ICollection<UserNotificationPreferenceEntity> NotificationPreferences { get; private set; }
        = new List<UserNotificationPreferenceEntity>();

    public long? PartnerId { get; set; }
    public PartnerEntity? Partner { get; set; }

    public static UserEntity Create(
        string username,
        string firstName,
        string? secondName,
        string? thirdName,
        string lastName,
        int genderId,
        string? mobileNo,
        string email,
        string nationalNo,
        string language,
        IEnumerable<NotificationChannel> notificationChannels,
        string? iamRef = null)
    {
        var normalizedChannels = notificationChannels
            .Distinct()
            .ToArray();

        if (normalizedChannels.Length == 0)
        {
            throw new InvalidOperationException("At least one notification channel is required.");
        }

        return new UserEntity(
            username.Trim(),
            firstName.Trim(),
            string.IsNullOrWhiteSpace(secondName) ? null : secondName.Trim(),
            string.IsNullOrWhiteSpace(thirdName) ? null : thirdName.Trim(),
            lastName.Trim(),
            genderId,
            string.IsNullOrWhiteSpace(mobileNo) ? null : mobileNo.Trim(),
            email.Trim(),
            nationalNo.Trim(),
            language: string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant(),
            notificationChannels: normalizedChannels,
            string.IsNullOrWhiteSpace(iamRef) ? null : iamRef.Trim());
    }

    public void UpdateProfile(
        string username,
        string firstName,
        string? secondName,
        string? thirdName,
        string lastName,
        int genderId,
        string? mobileNo,
        string? email,
        string? nationalNo,
        string language)
    {
        Username = username.Trim();
        FirstName = firstName.Trim();
        SecondName = string.IsNullOrWhiteSpace(secondName) ? null : secondName.Trim();
        ThirdName = string.IsNullOrWhiteSpace(thirdName) ? null : thirdName.Trim();
        LastName = lastName.Trim();
        GenderId = genderId;
        MobileNo = string.IsNullOrWhiteSpace(mobileNo) ? null : mobileNo.Trim();
        Email = email is null ? Email : email.Trim();
        NationalNo = nationalNo is null ? NationalNo : nationalNo.Trim();
        Language = string.IsNullOrWhiteSpace(language) ? Language : language.Trim().ToLowerInvariant();
    }

    public void UpdateMobileNo(string username, string mobileNo)
    {
        Username = username.Trim();
        MobileNo = mobileNo.Trim();
    }

    public void SetIamRef(string iamRef)
    {
        IamRef = string.IsNullOrWhiteSpace(iamRef) ? null : iamRef.Trim();
    }

    public void LinkPartner(PartnerEntity partner)
    {
        ArgumentNullException.ThrowIfNull(partner);
        Partner = partner;
        PartnerId = partner.Id;
    }

    public void UnlinkPartner()
    {
        Partner = null;
        PartnerId = null;
    }

    public void ChangeStatus(UserStatus status)
    {
        Status = status;
    }

    public void SetNotificationPreferences(IEnumerable<NotificationChannel> channels)
    {
        var channelSet = channels.Distinct().ToHashSet();
        if (channelSet.Count == 0)
        {
            throw new InvalidOperationException("At least one notification channel is required.");
        }

        foreach (var preference in NotificationPreferences)
        {
            preference.SetActive(channelSet.Contains(preference.NotificationChannel));
        }

        var existingChannels = NotificationPreferences
            .Select(x => x.NotificationChannel)
            .ToHashSet();

        foreach (var channel in channelSet.Where(x => !existingChannels.Contains(x)))
        {
            NotificationPreferences.Add(UserNotificationPreferenceEntity.Create(channel));
        }
    }
}
