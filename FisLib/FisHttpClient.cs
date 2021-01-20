using HandlebarsDotNet;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FisLib
{
    public class FisHttpClient
    {
        HttpClient _client;
        public FisHttpClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> ChexSystems(string authToken)
        {
            var resp = await _client.PostAsync("chexsystems?wsdl", GetContent(authToken));
            return resp;
        }

        private StringContent GetContent(string authToken)
        {
            var model = new QualifileWebRequest 
            {
                FirstName= "",
                LastName = "",
                Address = "",
                City= "",
                State= QualifileWebRequest.StatesAndTerritories.MO ,
                ZipCode = "",
                Country =  QualifileWebRequest.Countries.US,
                Ssn = "",
                DriverLicenseNumber = "",
                DriverLicenseState= "MO",
                PhoneNumber= "",
                BirthDate = new DateTime(1946,3,5),
                UserDefinedRecord= ""
            };
            var soapString = File.ReadAllText($"{AppContext.BaseDirectory}/{MyStrings.CallCheckString}");
            var template = Handlebars.Compile(soapString);
            var country = model?.Country.ToDescriptionString().ToUpper();

            var data = new
            {
                AuthToken = authToken,
                AcquirerId = "",
                ChexsyStemsVersion = "",
                CustomerId = "",
                IncludeChexSystems = "false",
                IncludeIdentityManager = "false",
                IncludeOfac = "true",
                IncludeQualifile = "true",
                InquiryId = "",
                LocationId = "",
                QualifileVersion = "",
                Staging = "true",
                StrategyTypeId = "",
                UserDefinedTransaction = "",
                BirthDate = model?.BirthDate.ConvertToMMDDYYYY(),
                Citynm = model?.City,
                CompositePhoneNumber = model?.PhoneNumber,
                Country = country,
                FirstName = model?.FirstName.ToUpper(),
                GovernmentNumber = model?.Ssn.RemoveDashesFromSSN(),
                IdentificationState = model?.DriverLicenseState,
                IdentificationStateNumber = model?.DriverLicenseNumber,
                LastName = model?.LastName.ToUpper(),
                MiddleName = model?.MiddleInitial,
                PostalPlusFour = model?.ZipCode.RemoveDashesFromSSN(),
                State = model?.State,
                StreetAddress = model?.Address.ToUpper(),
                DepositAmt = model?.DepositAmount,
                PrevAddressMonthsNbr = model?.MonthsAtPreviousAddress,
                PrevAddressYearsNbr = model?.YearsAtPreviousAddress,
                PrevCityNm = model?.PreviousCity,
                PrevCountryCd = model?.PreviousCountry,
                PrevPostalPlusFourCd = model?.PreviousZipCode,
                PrevStateCd = model?.PreviousState,
                PrevStreetAddress = model?.PreviousAddress,
                UserDefinedRecord = model?.UserDefinedRecord
            };
            var result = template(data);
            var content = new StringContent(result, Encoding.UTF8, "application/xml");
            return content;
        }
    }

    public static class MyStrings
    {
        public static string CallCheckString = "GetCallChexSystemsServiceV001.txt";
            
        public static string ConvertToMMDDYYYY(this DateTime date)
        {
            if (date == null || date == default(DateTime))
            {
                date = DateTime.Now;
            }
            return date.ToString("MMddyyyy");
        }
        public static string RemoveDashesFromSSN(this string ssn)
        {
            return ssn.Replace("-", string.Empty).Replace(" ", string.Empty);
        }

        public static string ToDescriptionString(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            try
            {
                DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

                if (attributes != null &&
                    attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            catch (Exception ex)
            {
            return string.Empty;
            }
        }
    }
} 
