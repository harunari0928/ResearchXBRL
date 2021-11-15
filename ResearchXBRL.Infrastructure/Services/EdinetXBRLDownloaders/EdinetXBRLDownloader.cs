using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    public abstract class EdinetXBRLDownloader : IEdinetXBRLDownloader
    {
        private readonly HttpClient httpClient;
        private readonly string apiVersion;

        public EdinetXBRLDownloader(
            IHttpClientFactory httpClientFactory,
            string apiVersion = "v1")
        {
            httpClient = httpClientFactory.CreateClient(typeof(EdinetXBRLDownloader).Name);
            this.apiVersion = apiVersion;
        }

        public IAsyncEnumerable<EdinetXBRLData> Download(DateTimeOffset start, DateTimeOffset end)
        {
            var fiveYearsAgo = DateTimeOffset.Now.AddYears(-5);
            if (fiveYearsAgo > start || fiveYearsAgo > end)
            {
                throw new ArgumentException("EdinetAPIは5年以上前のデータ取得に対応していません");
            }

            var jstStartDate = start.ToOffset(TimeSpan.FromHours(9)).DateTime;
            var jstEndDate = end.ToOffset(TimeSpan.FromHours(9)).DateTime;
            var documentIds = GetFilteredDocumentIds(jstStartDate, jstEndDate);
            return GetDocumentFiles(documentIds);
        }

        private async IAsyncEnumerable<EdinetXBRLData> GetDocumentFiles(IAsyncEnumerable<string> documentIds)
        {
            await foreach (var docuemntId in documentIds)
            {
                var queryParameters = $"type=1";
                var url = $"{DocumentAPIUrl(docuemntId)}?{queryParameters}";
                using var responseMessage = await httpClient.GetAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("書類API接続処理失敗", null, responseMessage.StatusCode);
                }

                yield return new EdinetXBRLData
                {
                    DocumentId = docuemntId,
                    ZippedDataStream = await responseMessage.Content.ReadAsStreamAsync()
                };
            }
        }

        protected abstract IAsyncEnumerable<string> GetFilteredDocumentIds(DateTime start, DateTime end);

        protected async IAsyncEnumerable<DocumentInfo> GetAllDocumentInfos(DateTime start, DateTime end)
        {
            foreach (var date in EnumerateDates(start, end))
            {
                var queryParameters = $"date={date:yyyy-MM-dd}&type=2";
                using var responseMessage = await httpClient.GetAsync($"{DocumentListAPIUrl}?{queryParameters}");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("書類一覧API接続処理失敗", null, responseMessage.StatusCode);
                }

                foreach (var documentInfo in (await DocumentListAPIResponse.Create(responseMessage)).Results)
                {
                    yield return documentInfo;
                }
            }
        }

        protected sealed class DocumentListAPIResponse
        {
            public IReadOnlyList<DocumentInfo> Results { get; init; } = new List<DocumentInfo>();

            public static async Task<DocumentListAPIResponse> Create(HttpResponseMessage responseMessage)
            {
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DocumentListAPIResponse>(jsonString,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                    ?? throw new Exception(@$"書類一覧APIレスポンスJSON文字列のデシリアライズに失敗
対象JSON文字列: {jsonString}");
            }
        }

        protected sealed class DocumentInfo
        {
            public string DocID { get; init; } = "";
            public string OrdinanceCode { get; init; } = "";
            public string FormCode { get; init; } = "";
        }

        private static IEnumerable<DateTime> EnumerateDates(DateTime start, DateTime end)
        {
            return Enumerable
                    .Range(0, 1 + end.Subtract(start).Days)
                    .Select(Convert.ToDouble)
                    .Select(start.AddDays);
        }

        private string DocumentListAPIUrl
            => $"https://disclosure.edinet-fsa.go.jp/api/{apiVersion}/documents.json";

        private string DocumentAPIUrl(string documentId)
            => $"https://disclosure.edinet-fsa.go.jp/api/{apiVersion}/documents/{documentId}";
    }
}
