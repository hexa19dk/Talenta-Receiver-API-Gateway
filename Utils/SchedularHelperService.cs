using Google.Apis.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using TalentaReceiver.Models;
using static TalentaReceiver.Utils.SchedularHelperService;

namespace TalentaReceiver.Utils
{
    public interface ISchedularHelperService
    {
        //Task<string> GetTalentaDataWithTimeout(string url, CancellationToken cancellationToken = default);
        Task<ApiResponse> PostToSapApiWithRetry(OvertimeSAP overtimeSAP, string apiUrl, string operationType, int maxRetries = 2, CancellationToken cancellationToken = default);
    }

    public class SchedularHelperService : ISchedularHelperService
    {
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ISchedularHelperService> _logger;
        public SchedularHelperService(System.Net.Http.IHttpClientFactory httpClientFactory, ILogger<ISchedularHelperService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        //public async Task<string> GetTalentaDataWithTimeout(string url, CancellationToken cancellationToken = default)
        //{
        //    using var httpClient = _httpClientFactory.CreateClient();

        //    // Configure timeout and retry policies
        //    httpClient.Timeout = TimeSpan.FromSeconds(30);

        //    try
        //    {
        //        var response = await httpClient.GetAsync(url, cancellationToken);

        //        // Handle specific HTTP status codes
        //        switch (response.StatusCode)
        //        {
        //            case HttpStatusCode.UnprocessableEntity: // 422
        //                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        //                _logger.LogWarning($"API returned 422 for URL {url}. Response: {errorContent}");
        //                throw new HttpRequestException($"Unprocessable Entity (422): {errorContent}", null, response.StatusCode);

        //            case HttpStatusCode.TooManyRequests: // 429
        //                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
        //                _logger.LogWarning($"Rate limit hit for URL {url}. Retry after: {retryAfter}");
        //                throw new HttpRequestException($"Rate limit exceeded. Retry after {retryAfter}", null, response.StatusCode);

        //            case HttpStatusCode.InternalServerError: // 500
        //            case HttpStatusCode.BadGateway: // 502
        //            case HttpStatusCode.ServiceUnavailable: // 503
        //            case HttpStatusCode.GatewayTimeout: // 504
        //                var serverErrorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        //                _logger.LogWarning($"Server error {response.StatusCode} for URL {url}. Response: {serverErrorContent}");
        //                throw new HttpRequestException($"Server error ({response.StatusCode}): {serverErrorContent}", null, response.StatusCode);
        //        }

        //        response.EnsureSuccessStatusCode();

        //        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        //        if (string.IsNullOrWhiteSpace(content))
        //        {
        //            throw new InvalidOperationException("Empty response received from API");
        //        }

        //        return content;
        //    }
        //    catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        //    {
        //        throw new OperationCanceledException("Request was cancelled", ex, cancellationToken);
        //    }
        //    catch (TaskCanceledException ex)
        //    {
        //        throw new OperationCanceledException("Request timed out", ex);
        //    }
        //}

        public async Task<ApiResponse> PostToSapApiWithRetry(OvertimeSAP overtimeSAP, string apiUrl, string operationType, int maxRetries = 2, CancellationToken cancellationToken = default)
        {
            var retryCount = 0;

            while (retryCount <= maxRetries)
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    var jsonContent = JsonConvert.SerializeObject(overtimeSAP);
                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(apiUrl, httpContent, cancellationToken);

                    // Handle different response codes
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        return new ApiResponse
                        {
                            IsSuccess = true,
                            Message = $"Success: {operationType} processed",
                            Data = responseContent
                        };
                    }

                    // Handle specific error codes that shouldn't be retried
                    if (response.StatusCode == HttpStatusCode.BadRequest ||
                        response.StatusCode == HttpStatusCode.UnprocessableEntity)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        return new ApiResponse
                        {
                            IsSuccess = false,
                            Message = $"Client error ({response.StatusCode}): {errorContent}"
                        };
                    }

                    // For server errors, throw exception to trigger retry
                    var serverErrorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Server error ({response.StatusCode}): {serverErrorContent}", null, response.StatusCode);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "Operation was cancelled"
                    };
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    retryCount++;
                    var delay = 1000 * retryCount; // Exponential backoff: 1s, 2s, 3s

                    _logger.LogWarning($"SAP API call failed (attempt {retryCount}/{maxRetries + 1}): {ex.Message}. Retrying in {delay}ms...");
                    await Task.Delay(delay, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"SAP API call failed after {maxRetries + 1} attempts: {ex.Message}");
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"Failed after {maxRetries + 1} attempts: {ex.Message}"
                    };
                }
            }

            return new ApiResponse
            {
                IsSuccess = false,
                Message = $"Max retries ({maxRetries + 1}) exceeded"
            };
        }

        // Supporting classes
        public class ApiResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? Data { get; set; }
        }
    }
}
