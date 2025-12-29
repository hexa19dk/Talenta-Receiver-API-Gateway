using Grpc.Core;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using TalentaReceiver.Models;
using TalentaReceiver.Protos;

namespace TalentaReceiver.Utils
{
    public interface IHelperService
    {
        Task<Protos.ResponseMessage> PostToSapApi(object request, string url, string transType);
        Task<string> GetEmployeePersonnel6(string nip, string url);
        Task<string> GetTalentaData(string url);
        Task<string> GetCostCenterCodes(string code);
        Task<int?> GetPageNumberFromUrl(string url);
        bool CheckNipWhitelist(string nipRequest);
    }

    public class HelperService : IHelperService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IHelperService> _log;
        public HelperService(HttpClient httpClient, ILogger<IHelperService> log)
        {
            _httpClient = httpClient;
            _log = log;
        }

        public async Task<Protos.ResponseMessage> PostToSapApi(object request, string url, string transType)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(request);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var respCode = Convert.ToInt32(response.StatusCode);
                    var respMessage = await response.Content.ReadAsStringAsync();

                    var jsonConv = JObject.Parse(respMessage);
                    string mainMessage = jsonConv["message"]?.ToString()!;
                    string sapMessage = jsonConv["return"]?["message"]?.ToString()!;

                    if (mainMessage == "ERROR")
                    {
                        _log.LogError(respCode, $"Error post {transType} data to SAP, data: {payload}, message: {respMessage}");
                        return new Protos.ResponseMessage
                        {
                            Code = 400,
                            Message = respMessage
                        };
                    }

                    _log.LogInformation(respCode, $"Successfully post {transType} data to SAP, data: {payload}, message: {respMessage}");
                    return new Protos.ResponseMessage
                    {
                        Code = 200,
                        Message = respMessage
                    };
                }
                else
                {
                    var respCode = Convert.ToInt32(response.StatusCode);
                    var respMessage = await response.Content.ReadAsStringAsync();
                    _log.LogError(respCode, $"Error post {transType} data to SAP, data: {payload}, message: {respMessage}");

                    return new Protos.ResponseMessage
                    {
                        Code = respCode,
                        Message = $"Post Failed with status {Convert.ToInt32(response.StatusCode)}, error message: {respMessage}"
                    };
                }
            }
            catch (Exception ex)
            {
                var payload = JsonConvert.SerializeObject(request, Formatting.Indented);
                _log.LogError(StatusCode.Internal.ToString(), $"Error post {transType} data to SAP, data: {payload}, message: {ex.Message}");

                return new ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
                throw;
            }
        }

        public async Task<string> GetTalentaData(string url)
        {
            try
            {
                using var client = new HttpClient();

                client.Timeout = TimeSpan.FromSeconds(30);

                string contentResult = File.ReadAllText("appsettings.json");
                JObject jsonObject = JObject.Parse(contentResult);
                string username = (string)jsonObject["TalentaSecretKey"]?["hmac_username"]!;
                string secret = (string)jsonObject["TalentaSecretKey"]?["hmac_secret"]!;
                var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret!));

                var uri = new Uri(url);
                string pathAndQuery = uri.PathAndQuery;
                var requestLine = $"GET {pathAndQuery} HTTP/1.1";
                var dateHeader = DateTime.UtcNow.ToString("R");

                var payload = $"date: {DateTime.UtcNow.ToString("R")}\n{requestLine}";
                var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
                var request = new HttpRequestMessage(HttpMethod.Get, $"{url}");
                request.Headers.Add("Authorization", $"hmac username=\"{username}\", algorithm=\"hmac-sha256\", headers=\"date request-line\", signature=\"{signature}\"");
                request.Headers.Add("Date", dateHeader);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                return result!;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetEmployeePersonnel6(string nip, string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                return content;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<string> GetCostCenterCodes(string code)
        {
            var result = CostCenterCodes.GetCode(code);
            return Task.FromResult(result);
        }

        public static class CostCenterCodes
        {
            public static readonly Dictionary<string, string> NameToCodeMap = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Direct", "D1" },
                { "In-direct", "D2" },
                { "Direksi", "D3" }
            };

            public static string GetCode(string name)
            {
                var normalized = Normalize(name);
                return NameToCodeMap
                    .FirstOrDefault(x => Normalize(x.Key) == normalized).Value ?? "UNKNOWN";
            }

            private static string Normalize(string input)
            {
                return input?
                    .Trim()
                    .Replace("–", "-")
                    .Replace("—", "-")
                    .Replace("‐", "-")
                    .Replace(" ", "")
                    .ToLowerInvariant() ?? string.Empty;
            }
        }

        public async Task<int?> GetPageNumberFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);
                string pageValue = query["page"]!;
                return int.TryParse(pageValue, out int page) ? page : null;
            }
            catch
            {
                return null;
            }
        }

        public bool CheckNipWhitelist(string nipRequest)
        {
            var lNip = new List<string>
            {
                "933",
                "15220",
                "20729",
                "21057",
                "21587",
                "22102",
                "10002646"
            };

            if (!lNip.Contains(nipRequest))
                return false;

            return true;
        }

        
    }
}
