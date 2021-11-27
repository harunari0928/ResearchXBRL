using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using System.Threading.Tasks;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using Npgsql;
using PostgreSQLCopyHelper;

namespace ImportCorporationInfo
{
    sealed class Program
    {
        static async Task Main(string[] _)
        {
            var storage = new LocalStorage(".");
            var fileStream = storage.Get("EdinetcodeDlInfo.csv");
            using var streamReader = new StreamReader(fileStream);
            using var reader = new CsvReader(streamReader, CultureInfo.CurrentCulture);
            await CleanTable();
            await foreach (var chunkedMasters in GetCompanyMaster(reader).Chunk(5000))
            {
                await InsertCompanyMaster(chunkedMasters);
            }
        }

        private static async IAsyncEnumerable<CompanyMasterDTO> GetCompanyMaster(CsvReader reader)
        {
            await foreach (IDictionary<string, object> record in reader.GetRecordsAsync<dynamic>())
            {
                yield return new CompanyMasterDTO
                {
                    Code = record["ＥＤＩＮＥＴコード"]?.ToString() ?? "unknown",
                    SubmissionType = record["提出者種別"]?.ToString() ?? "unknown",
                    IsListed = record["上場区分"].ToString() == "上場",
                    IsLinking = record["連結の有無"].ToString() == "有",
                    CapitalAmount = decimal.TryParse(record["資本金"]?.ToString(), out var amount)
                    ? amount : null,
                    SettlementDate = record["決算日"]?.ToString() ?? "unknown",
                    SubmitterName = record["提出者名"]?.ToString() ?? "unknown",
                    SubmitterNameEnglish = record["提出者名（英字）"]?.ToString() ?? "unknown",
                    SubmitterNameYomigana = record["提出者名（ヨミ）"]?.ToString() ?? "unknown",
                    CompanyLocation = record["所在地"]?.ToString() ?? "unknown",
                    TypeOfIndustry = record["提出者業種"]?.ToString() ?? "unknown",
                    SecuritiesCode = record["証券コード"]?.ToString() ?? "unknown",
                    CorporateNumber = record["提出者法人番号"]?.ToString() ?? "unknown"
                };
            }
        }
        private static async Task InsertCompanyMaster(IEnumerable<CompanyMasterDTO> companyMaster)
        {
            using var connection = GetConnection();
            using var tran = connection.BeginTransaction();
            await BulkInsert(companyMaster, connection);
            await tran.CommitAsync();
        }
        private static async Task BulkInsert(IEnumerable<CompanyMasterDTO> companyMaster, NpgsqlConnection connection)
        {
            var reportCoverHelper = new PostgreSQLCopyHelper<CompanyMasterDTO>("company_master")
                            .MapVarchar("code", x => x.Code)
                            .MapVarchar("submission_type", x => x.SubmissionType)
                            .MapBoolean("is_listed", x => x.IsListed)
                            .MapBoolean("is_linking", x => x.IsLinking)
                            .MapNumeric("capital_amount", x => x.CapitalAmount)
                            .MapVarchar("settlement_date", x => x.SettlementDate)
                            .MapVarchar("submitter_name", x => x.SubmitterName)
                            .MapVarchar("submitter_name_english", x => x.SubmitterNameEnglish)
                            .MapVarchar("submitter_name_yomigana", x => x.SubmitterNameYomigana)
                            .MapVarchar("company_location", x => x.CompanyLocation)
                            .MapVarchar("type_of_industry", x => x.TypeOfIndustry)
                            .MapVarchar("securities_code", x => x.SecuritiesCode)
                            .MapVarchar("corporate_number", x => x.CorporateNumber);
            await reportCoverHelper.SaveAllAsync(connection, companyMaster);
        }
        private static NpgsqlConnection GetConnection()
        {
            var server = Environment.GetEnvironmentVariable("DB_SERVERNAME");
            var userId = Environment.GetEnvironmentVariable("DB_USERID");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var connectionString = $"Server={server};Port={port};Database={dbName};User Id={userId};Password={password};";
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection;
        }
        private static async Task CleanTable()
        {
            var command = GetConnection().CreateCommand();
            command.CommandText = "DELETE FROM company_master";
            await command.ExecuteNonQueryAsync();
        }
        private class CompanyMasterDTO
        {
            public string Code { get; init; } = "";
            public string SubmissionType { get; init; } = "";
            public bool IsListed { get; init; }
            public bool IsLinking { get; init; }
            public decimal? CapitalAmount { get; init; }
            public string SettlementDate { get; init; } = "";
            public string SubmitterName { get; init; } = "";
            public string SubmitterNameEnglish { get; init; } = "";
            public string SubmitterNameYomigana { get; init; } = "";
            public string CompanyLocation { get; init; } = "";
            public string TypeOfIndustry { get; init; } = "";
            public string SecuritiesCode { get; init; } = "";
            public string CorporateNumber { get; init; } = "";
        }
    }
}
