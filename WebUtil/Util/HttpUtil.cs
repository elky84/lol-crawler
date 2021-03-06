﻿using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebUtil.Util
{
    public static class HttpClientUtil
    {
        public static void SetDefaultHeader(this System.Net.Http.HttpClient client, HttpRequestMessage request, string userId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<T> RequestJson<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            object body)
        {
            var response = await httpClientFactory.RequestJson(httpMethod, url, JsonConvert.SerializeObject(body));
            return await response.ResponseDeserialize<T>();
        }

        public static async Task<HttpResponseMessage> RequestJson(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            object body)
        {
            using var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            return await client.SendAsync(request);
        }

        public static async Task<T> Request<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod, string url)
        {
            using var client = httpClientFactory.CreateClient();
            return await client.Request<T>(httpMethod, url);
        }


        public static async Task<T> Request<T>(this HttpClient httpClient,
            HttpMethod httpMethod, string url)
        {
            var request = new HttpRequestMessage(httpMethod, url);
            var httpResponse = await httpClient.SendAsync(request);
            return await httpResponse.ResponseDeserialize<T>();
        }

        private static async Task<T> ResponseDeserialize<T>(this HttpResponseMessage httpResponse)
        {
            return JsonConvert.DeserializeObject<T>(await httpResponse.Content.ReadAsStringAsync());
        }
    }
}