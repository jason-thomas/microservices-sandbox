using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cnp.Sdk;
using cnp_api_wrapper.Configuration;
using cnp_api_wrapper.Models.Common;
using Microsoft.Extensions.Options;

namespace cnp_api_wrapper.Mappings
{
    public abstract class EcheckMappingBase
    {
        private static readonly IReadOnlyDictionary<string, orderSourceType> OrderSourceLookup =
            typeof(orderSourceType)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(orderSourceType))
                .ToDictionary(field => field.Name, field => (orderSourceType)field.GetValue(null)!, StringComparer.OrdinalIgnoreCase);

        protected EcheckMappingBase(IOptions<CnpOptions> options)
        {
            Options = options.Value;
        }

        protected CnpOptions Options { get; }

        protected orderSourceType ResolveOrderSource(string? orderSource)
        {
            if (!string.IsNullOrWhiteSpace(orderSource) && OrderSourceLookup.TryGetValue(orderSource, out var parsed))
            {
                return parsed;
            }

            return orderSourceType.ecommerce;
        }

        protected string ResolveReportGroup(string? reportGroup) =>
            string.IsNullOrWhiteSpace(reportGroup) ? Options.ReportGroup : reportGroup;

        protected echeckAccountTypeEnum ResolveAccountType(string? accountType)
        {
            if (!string.IsNullOrWhiteSpace(accountType) &&
                Enum.TryParse(accountType, true, out echeckAccountTypeEnum parsed))
            {
                return parsed;
            }

            throw new ArgumentException("AccountType must be provided and match a valid CNP account type value.", nameof(accountType));
        }

        protected echeckType MapAccount(EcheckBankAccount account)
        {
            if (account is null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            var result = new echeckType
            {
                accNum = account.AccountNumber,
                routingNum = account.RoutingNumber,
                accType = ResolveAccountType(account.AccountType),
                checkNum = account.CheckNumber
            };

            if (!string.IsNullOrWhiteSpace(account.AccountHolderName))
            {
                result.echeckCustomerId = account.AccountHolderName;
            }

            return result;
        }

        protected contact? MapContact(EcheckContact? contact)
        {
            if (contact is null)
            {
                return null;
            }

            return new contact
            {
                name = contact.Name,
                addressLine1 = contact.AddressLine1,
                addressLine2 = contact.AddressLine2,
                addressLine3 = contact.AddressLine3,
                city = contact.City,
                state = contact.State,
                zip = contact.Zip,
                email = contact.Email,
                phone = contact.Phone
            };
        }
    }
}
