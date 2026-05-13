using System.Diagnostics;

namespace SteamRelayPicker
{
    public static class Firewall
    {
        public static async Task UpdateFirewall(FlowLayoutPanel flowPanel)
        {
            List<string> selectedIps = UserSelectionList(flowPanel);
            
            string ruleName = "SteamRelayPicker Rule";
            string deleteArgs = $"advfirewall firewall delete rule name=\"{ruleName}\"";

            if (selectedIps.Count > 0)
            {
                string remoteIps = string.Join(",", selectedIps);

                string addOutArgs = $"advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block remoteip={remoteIps}";
                string addInArgs = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=block remoteip={remoteIps}";

                await RunCommand("netsh", deleteArgs);
                await RunCommand("netsh", addOutArgs);
                await RunCommand("netsh", addInArgs);

                MessageBox.Show($"Found {selectedIps.Count} relays.");
            }
            else
            {
                await RunCommand("netsh", deleteArgs);
                MessageBox.Show($"No relays selected! Cleared existing rules.");
            }
            
            /*
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "netsh",
                    Arguments = "",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                };

                // 'await' the start of the process
                await Task.Run(() => Process.Start(psi)); 

                await UserSelectionList(flowPanel);
                
                MessageBox.Show($"Action 'asd' performed for: asda");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }*/
        }

        private static List<string> UserSelectionList(FlowLayoutPanel flowPanel)
        {
            List<string> ipToBlock = [];
            foreach (CheckBox chkbx in flowPanel.Controls.OfType<CheckBox>())
            {
                if (chkbx.Checked)
                {
                    if (chkbx.Tag is List<string> ip) ipToBlock.AddRange(ip);
                }
            }
            return ipToBlock;
        }

        private static Task RunCommand(string fileName, string arguments)
        {
            return Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Verb = "runas" // This requests Admin privileges if needed
                    };

                    using Process? process = Process.Start(psi);
                    process?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Firewall Error: {ex.Message}");
                }
            });
        }
    }
}