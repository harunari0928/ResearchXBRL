using CsvHelper;
using ResearchXBRL.Domain.AccountItems;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.AccountItems
{
    public sealed class AccountItemsCSVWriter : IAccountItemWriter
    {
        private readonly CsvWriter csvWriter;

        public AccountItemsCSVWriter(TextWriter writer)
        {
            csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);
        }

        public async Task Write(IEnumerable<AccountItem> elements)
        {
            csvWriter.WriteHeader<AccountItem>();
            await csvWriter.NextRecordAsync();
            await csvWriter.WriteRecordsAsync(elements);
        }

        public void Dispose()
        {
            csvWriter.Dispose();
        }
    }
}
