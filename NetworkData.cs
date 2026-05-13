using System.Text.Json;
using System.Diagnostics;

namespace SteamRelayPicker
{
    public static class NetworkData
    {
        #pragma warning disable CA2211
        public static Dictionary<string, List<string>> serverLoc = [];
        #pragma warning restore CA2211

        static readonly HttpClient client = new();

        private static readonly JsonSerializerOptions _options = new()
        { 
            PropertyNameCaseInsensitive = true 
        };

        public static async Task GetData(string url)
        {
// do i really need try catch for software? or i could implement it to write in logs i think
            try	
            {
                serverLoc.Clear();

                string json = await client.GetStringAsync(url);

                var data = JsonSerializer.Deserialize<Root>(json, _options)!;

                foreach (var loc in data.pops)
                {
                    string locName = loc.Value.desc;
                    List<string> ipList = [];
                    if (loc.Value.relays != null && loc.Value.relays.Count > 0)
                    {
                        foreach (var r in loc.Value.relays) ipList.Add(r.ipv4);
                        serverLoc.Add(locName, ipList);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public static async Task PingTest(List<string> ipList, CheckBox cb)
        {
            bool successfulIp = false;
            string city = cb.Text.Replace("Pinging...", "");

            foreach (string ip in ipList)
            {
                int result = await Task.Run(async () => {
                    return await PingServer(ip);
                });

                if (result < 999)
                {
                    
                    cb.Text = $"{city}{result}ms";

                    // Optional: Color code based on speed!
                    if (result < 100) cb.ForeColor = Color.Green;
                    else if (result < 150) cb.ForeColor = Color.Orange;
                    else cb.ForeColor = Color.Red;
                    successfulIp = true;
                    break;
                }
            }
            if (!successfulIp) {
                cb.Text = $"{city}Unreachable";
                cb.ForeColor = Color.Red;
            }
            
        }

        public static async Task<int> PingServer(string ip)
        {

            ProcessStartInfo psi = new()
            {
                FileName = "ping",
                Arguments = ip,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = new() {StartInfo = psi})
            {
                process.Start();

                // Read the output and errors
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    return 1;
                }

                return ParsePing(output);
            }
        }

        public static int ParsePing(string output)
        {
            int index = output.IndexOf("time=");
            if (index == -1) return 999;

            int start = index + 5;
            int end = output.IndexOf("ms", start);
            string msValue = output.Substring(start, end - start);

            return int.TryParse(msValue, out int result) ? result : 999;
        }
    }
}

#pragma warning disable CA1050
#pragma warning disable IDE1006
public class Root { public required Dictionary<string, Loc> pops { get; set; } }
public class Loc { public required string desc { get; set; } public List<Relay>? relays { get; set; } }
public class Relay { public required string ipv4 { get; set; } }
#pragma warning restore IDE1006
#pragma warning restore CA1050