using System;

namespace FisLib
{
    public partial class QualifileWebRequest 
    {
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public StatesAndTerritories State { get; set; }
        public string ZipCode { get; set; }
        public Countries Country { get; set; }
        public string Ssn { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string DriverLicenseState { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string DepositAmount { get; set; }
        public string MonthsAtPreviousAddress { get; set; }
        public string YearsAtPreviousAddress { get; set; }
        public string PreviousCity { get; set; }
        public string PreviousCountry { get; set; }
        public string PreviousZipCode { get; set; }
        public StatesAndTerritories PreviousState { get; set; }
        public string PreviousAddress { get; set; }
        public string UserDefinedRecord { get; set; }

    }
} 
