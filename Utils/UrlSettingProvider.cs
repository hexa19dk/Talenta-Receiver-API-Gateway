using Microsoft.Extensions.Options;

namespace TalentaReceiver.Utils
{
    public interface IUrlSettingProvider
    {
        string GetEndpoint(string path);
    }

    public class UrlSettingProvider : IUrlSettingProvider
    {
        private readonly SAPUrlSettings _settings;
        private readonly IWebHostEnvironment _env; // for dynamic switching
        private readonly ILogger<UrlSettingProvider> _logger;

        public UrlSettingProvider(IOptions<SAPUrlSettings> settings, IWebHostEnvironment env, ILogger<UrlSettingProvider> logger)
        {
            _settings = settings.Value;
            _env = env;
            _logger = logger;
        }

        public string GetEndpoint(string path)
        {
            string baseUrl;

            if (!string.IsNullOrEmpty(_settings.EnvironmentOverride))
            {
                baseUrl = _settings.EnvironmentOverride.ToLower() switch
                {
                    "production" => _settings.Production,
                    "staging" => _settings.Staging,
                    _ => _settings.Staging // fallback
                };

                _logger.LogInformation("[UrlSettingProvider] Using manual override: {env}, BaseUrl: {url}",
                    _settings.EnvironmentOverride, baseUrl);
            }
            else
            {
                baseUrl = _settings.Staging;
                _logger.LogInformation("[UrlSettingProvider] No override provided. Using default Staging BaseUrl: {url}", baseUrl);
            }

            return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }

    public class SAPUrlSettings
    {
        public string Production { get; set; } = string.Empty;
        public string Staging { get; set; } = string.Empty;
        public string? EnvironmentOverride { get; set; }
    }
}
