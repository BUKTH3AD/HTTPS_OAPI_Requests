namespace HTTPS_OAPI_Requests

{
    internal class Program
    {
        static string token = default;
        static List<HttpClient> clients = new List<HttpClient>();
        static async Task StartSession()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://EWKSC:13299/api/v1.0/Session.StartSession");
            //request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Authorization", "KSCBasic user=\"dGVzdF9hZG1pbg==\", pass=\"S2FzcGVyc2t5MQ==\"");
            var content = new StringContent("", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            token = await response.Content.ReadAsStringAsync();
            string[] input = token.Split('"');
            token = input[3];
        }

        static async Task GetTask(HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://EWKSC:13299/api/v1.0/Tasks.GetTaskData");
            request.Headers.Add("X-KSC-Session", token);
            var content = new StringContent("{\r\n    \"strTask\":\"104\"\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
        static async Task GetHostInfo(HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://EWKSC:13299/api/v1.0/HostGroup.GetHostInfo");
            request.Headers.Add("X-KSC-Session", token);
            var content = new StringContent("{\r\n    \"strHostName\":\"5593fcfd-e950-4b54-9732-2c160a35b8d7\",\r\n    \"pFields2Return\":[\"KLHST_WKS_DN\",\"KLHST_WKS_GROUPID\",\"KLHST_WKS_LAST_VISIBLE\",\"KLHST_WKS_STATUS\"]\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        static void CreateHTTPClients()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

            for (int i = 0; i < 300; i++)
            {

                clients.Add(new HttpClient(handler));
            }

        }

        static async Task Main(string[] args)
        {
            await StartSession();

            CreateHTTPClients();

            var b = clients.Select(client =>
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await GetTask(client);
                        await GetHostInfo(client);
                        await Task.Delay(TimeSpan.FromMilliseconds(10));
                    }

                })
            );

            await Task.WhenAll(b);
        }
    }
}
