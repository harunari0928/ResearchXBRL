using System;
using System.Collections.Generic;

namespace ResearchXBRL.Domain.AccountElements
{
    public sealed class AccountElement
    {
        public string AccountName { get; init; } = "";
        public string XBRLName { get; init; } = "";
        public string Type { get; init; } = "";
        public string SubstitutionGroup { get; init; } = "";
        public bool Abstract { get; init; } = false;
        public bool Nillable { get; init; } = false;
        public string Balance { get; init; } = "";
        public string PeriodType { get; init; } = "";
        public DateTime TaxonomyVersion { get; init; }
        public string Classification { get; set; }
    }
}
