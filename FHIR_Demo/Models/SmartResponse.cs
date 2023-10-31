using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace FHIR_Demo.Models
{
    public class SmartResponse
    {
        [JsonPropertyName("need_patient_banner")]
        public bool NeedPatientBanner { get; set; }

        [JsonPropertyName("smart_style_url")]
        public string SmartStyleUrl { get; set; }

        [JsonPropertyName("patient")]
        public string PatientId { get; set; }

        [JsonPropertyName("token_type")]
        public string token_type { get; set; }

        [JsonPropertyName("scope")]
        public string Scopes { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}