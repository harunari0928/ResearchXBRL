using CsvHelper;
using ResearchXBRL.Domain.AccountElements;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.AccountElements
{
    public sealed class AccountElementCSVWriter : IAccountElementWriter
    {
        private readonly CsvWriter csvWriter;

        public AccountElementCSVWriter(TextWriter writer)
        {
            csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);
        }

        public async Task Write(IEnumerable<AccountElement> elements)
        {
            csvWriter.WriteHeader<AccountElement>();
            await csvWriter.NextRecordAsync();
            await csvWriter.WriteRecordsAsync(elements);
        }

        public void Dispose()
        {
            csvWriter.Dispose();
        }
    }
}
