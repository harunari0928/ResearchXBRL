using ResearchXBRL.Domain.FinancialReports;
using System;
using System.Threading.Tasks;
using Npgsql;
using PostgreSQLCopyHelper;
using ResearchXBRL.Domain.FinancialReports.Units;
using System.Linq;
using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports.Contexts;

namespace ResearchXBRL.Infrastructure.FinancialReports
{
    public sealed class FinancialReportRepository : IFinancialReportRepository, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;

        public FinancialReportRepository()
        {
            var server = Environment.GetEnvironmentVariable("DB_SERVERNAME");
            var userId = Environment.GetEnvironmentVariable("DB_USERID");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var connectionString = $"Server={server};Port={port};Database={dbName};User Id={userId};Password={password};";
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
        }

        public Task<bool> IsExists(FinancialReport reports)
        {
            throw new NotImplementedException();
        }

        public async Task Write(FinancialReport reports)
        {
            using var tran = connection.BeginTransaction();
            var reportCoverHelper = new PostgreSQLCopyHelper<ReportCover>("report_covers")
                .MapVarchar("id", x => x.DocumentId)
                .MapVarchar("company_id", x => x.CompanyId)
                .MapVarchar("document_type", x => x.DocumentType)
                .MapDate("submission_date", x => x.SubmissionDate.Date);

            await reportCoverHelper.SaveAllAsync(connection, new ReportCover[] { reports.Cover });

            var normalUnitHelper = new PostgreSQLCopyHelper<NormalUnit>("units")
                .MapVarchar("report_id", _ => reports.Cover.DocumentId)
                .MapVarchar("unit_name", x => x.Name)
                .MapInteger("unit_type", _ => 0)
                .MapVarchar("measure", x => x.Measure);

            await normalUnitHelper.SaveAllAsync(connection, reports.Units.OfType<NormalUnit>());

            var dividedUnitHelper = new PostgreSQLCopyHelper<DividedUnit>("units")
                .MapVarchar("report_id", _ => reports.Cover.DocumentId)
                .MapVarchar("unit_name", x => x.Name)
                .MapInteger("unit_type", _ => 1)
                .MapVarchar("unit_numerator", x => x.UnitNumerator)
                .MapVarchar("unit_denominator", x => x.UnitDenominator);

            await dividedUnitHelper.SaveAllAsync(connection, reports.Units.OfType<DividedUnit>());

            var contextsHelper = new PostgreSQLCopyHelper<Context>("contexts")
                .MapVarchar("report_id", _ => reports.Cover.DocumentId)
                .MapVarchar("context_name", x => x.Name)
                .MapInteger("period_type", x => x.Period is InstantPeriod ? 0 : 1)
                .MapDate("period_from", x => x.Period is DurationPeriod p ? p.From.Date : null)
                .MapDate("period_to", x => x.Period is DurationPeriod p ? p.From.Date : null)
                .MapDate("instant_date", x => x.Period is InstantPeriod p ? p.InstantDate.Date : null);

            await contextsHelper.SaveAllAsync(connection, reports.Contexts);

            var reportItemHelper = new PostgreSQLCopyHelper<FinancialReportItem>("report_items")
                .MapUUID("id", _ => Guid.NewGuid())
                .MapVarchar("report_id", _ => reports.Cover.DocumentId)
                .MapVarchar("classification", x => x.Classification)
                .MapVarchar("xbrl_name", x => x.XBRLName)
                .MapNumeric("amounts", x => x.Amounts)
                .MapNumeric("numerical_accuracy", x => x.NumericalAccuracy)
                .MapNumeric("scale", x => x.Scale)
                .MapVarchar("unit_name", x => x.UnitName)
                .MapVarchar("context_name", x => x.ContextName);

            await reportItemHelper.SaveAllAsync(connection, reports);

            await tran.CommitAsync();
        }
    }
}
