using System.Net.Http;
using Microsoft.Extensions.Options;
using Application.Interfaces;
using System.Threading.Tasks;
using Application.User;
using System;
using Newtonsoft.Json;

namespace Infrastructure.Security
{
    public class FacebookAccessor :IFacebookAccessor
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<FacebookAppSettings> _config;
        public FacebookAccessor(IOptions<FacebookAppSettings> config)
        {
            _config=config;
            _httpClient = new HttpClient
            {
                BaseAddress= new System.Uri("https://graph.facebook.com")
            };
            _httpClient.DefaultRequestHeaders
            .Accept
            .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FacebookUserInfo> FacebookLogin(string accessToken)
        {
            //verify token valid
            var verifytoken = await _httpClient.GetAsync($"debug_token?input_token={accessToken}&access_token={_config.Value.AppId}|{_config.Value.AppSecret}");
            if (!verifytoken.IsSuccessStatusCode)
                return null;
            var result = await GetAsync<FacebookUserInfo>(accessToken,"me","fields=name,email,picture.width(100).height(100)");

            return result;
        }

        private async Task<T> GetAsync<T>(string accessToken, string endpoint, string args)
        {
            var response = await _httpClient.GetAsync($"{endpoint}?access_token={accessToken}&{args}");
             if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}