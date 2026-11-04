using AutoMapper;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Mapper;

public sealed class AccountMapper : Profile
{
    public AccountMapper()
    {
        CreateMap<AccountEntity, AccountOutputModel>();
        CreateMap<AccountTypeEntity, AccountTypeOutputModel>();
    }
}
