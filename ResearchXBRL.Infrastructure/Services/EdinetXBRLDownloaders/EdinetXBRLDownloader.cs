using System;
using System.IO;
using System.Net;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders
{
    public abstract class EdinetXBRLDownloader : IEdinetXBRLDownloader
    {
        private readonly HttpClient httpClient;
        private readonly string apiVersion;
        private readonly ThrottlingService throttlingService = new(500);
        private readonly ThrottlingService throttlingService2 = new(500);

        public EdinetXBRLDownloader(
            IHttpClientFactory httpClientFactory,
            string apiVersion = "v1")
        {
            httpClient = httpClientFactory.CreateClient(typeof(EdinetXBRLDownloader).Name);
            this.apiVersion = apiVersion;
        }

        public async IAsyncEnumerable<IResult<EdinetXBRLData>> Download(DateTimeOffset start, DateTimeOffset end)
        {
            var fiveYearsAgo = DateTimeOffset.Now.AddYears(-5);
            if (fiveYearsAgo > start || fiveYearsAgo > end)
            {
                yield return new Abort<EdinetXBRLData> { Message = "EdinetAPIは5年以上前のデータ取得に対応していません" };
                yield break;
            }

            var jstStartDate = start.ToOffset(TimeSpan.FromHours(9)).DateTime;
            var jstEndDate = end.ToOffset(TimeSpan.FromHours(9)).DateTime;
            await foreach (var item in HandleDocumentInfos(GetFilteredDocumentByIds(jstStartDate, jstEndDate)))
            {
                yield return item;
            }
        }

        private async IAsyncEnumerable<IResult<EdinetXBRLData>> HandleDocumentInfos(IAsyncEnumerable<IResult<DocumentInfo>> documentInfos)
        {
            await foreach (var info in documentInfos)
            {
                switch (info)
                {
                    case Failed<DocumentInfo> failed:
                        yield return new Failed<EdinetXBRLData> { Message = failed.Message };
                        break;
                    case Abort<DocumentInfo> abort:
                        yield return new Abort<EdinetXBRLData> { Message = abort.Message };
                        yield break;
                    case Succeeded<DocumentInfo> succeeded:
                        yield return new Succeeded<EdinetXBRLData>(new EdinetXBRLData
                        {
                            DocumentId = succeeded.Value.DocID,
                            DocumentType = succeeded.Value.DocTypeCode,
                            CompanyId = succeeded.Value.EdinetCode,
                            DocumentDateTime = DateTime.Parse(succeeded.Value.SubmitDateTime),
                            LazyZippedDataStream = GetLazyZippedDataStream(succeeded.Value)
                        });
                        break;
                    default:
                        throw new NotSupportedException($"{nameof(Download)}メソッドから予期しない型が検出されました。当該型に対する処理の実装をお願いします。");
                }
            }
        }

        private Lazy<Task<IResult<MemoryStream>>> GetLazyZippedDataStream(DocumentInfo info)
        {
            return new Lazy<Task<IResult<MemoryStream>>>(() => GetZippedDataStream(info), true);
        }

        private async Task<IResult<MemoryStream>> GetZippedDataStream(DocumentInfo info)
        {
            var queryParameters = $"type=1";
            var url = $"{DocumentAPIUrl(info.DocID)}?{queryParameters}";
            using var responseMessage = await httpClient.GetAsync(url);
            await Task.Delay(throttlingService2.HealingTime);
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    throttlingService2.SlowDown();
                }
                return new Failed<MemoryStream> { Message = $"書類API接続処理失敗 ステータスコード:${responseMessage.StatusCode}" };
            }

            try
            {
                throttlingService2.Reset();
                return new Succeeded<MemoryStream>(await ReadContentStream(responseMessage));
            }
            catch (Exception ex)
            {
                return new Failed<MemoryStream> { Message = ex.ToString() };
            }
        }

        private static async Task<MemoryStream> ReadContentStream(HttpResponseMessage responseMessage)
        {
            using var contentStream = await responseMessage.Content.ReadAsStreamAsync();
            var copiedStream = new MemoryStream();
            await contentStream.CopyToAsync(copiedStream);
            return copiedStream;
        }

        protected abstract IAsyncEnumerable<IResult<DocumentInfo>> GetFilteredDocumentByIds(DateTime start, DateTime end);

        protected async IAsyncEnumerable<IResult<DocumentInfo>> GetAllDocumentInfos(DateTime start, DateTime end)
        {
            foreach (var date in EnumerateDates(start, end))
            {
                var queryParameters = $"date={date:yyyy-MM-dd}&type=2";
                using var responseMessage = await httpClient.GetAsync($"{DocumentListAPIUrl}?{queryParameters}");
                await Task.Delay(throttlingService.HealingTime);
                if (!responseMessage.IsSuccessStatusCode)
                {
                    if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throttlingService.SlowDown();
                    }
                    yield return new Failed<DocumentInfo> { Message = $"書類一覧API接続処理失敗 ステータスコード:{responseMessage.StatusCode}" };
                    continue;
                }

                foreach (var documentInfo in (await DocumentListAPIResponse.Create(responseMessage)).Results)
                {
                    throttlingService.Reset();
                    yield return new Succeeded<DocumentInfo>(documentInfo);
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
            public string EdinetCode { get; init; } = "";
            public string DocTypeCode { get; init; } = "";
            public string SubmitDateTime { get; init; } = "";
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
