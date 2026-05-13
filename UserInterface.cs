namespace SteamRelayPicker
{
    public partial class UserInterface : Form
    {
        private bool _refreshFlag = false;
        private bool _selectAllState = false;
        private CheckBox _chkBoxAll = null!;
        private FlowLayoutPanel _flowPanel = null!;
        
        public UserInterface()
        {
            SetupUI();
        }
        private async Task RefreshData()
        {
            _flowPanel.Controls.Clear();
            await NetworkData.GetData("https://api.steampowered.com/ISteamApps/GetSDRConfig/v1?appid=730");
        }

        private async Task PopulateCheckbox()
        {
            foreach (var data in NetworkData.serverLoc)
            {
                List<string> ipList = data.Value;

                CheckBox cb = new()
                {
                    Text = $"{data.Key} - Pinging...",
                    AutoSize = true,
                    Tag = ipList,
                    Margin = new Padding(2)
                };

                cb.CheckedChanged += (s,e) =>
                {
                    if (!cb.Checked && _chkBoxAll.Checked) {
                        _selectAllState = true;
                        _chkBoxAll.Checked = false;
                        _selectAllState = false;
                    }
                };

                _flowPanel.Controls.Add(cb);

                _ = NetworkData.PingTest(ipList, cb);
            }
        }

        private void ToggleAllCheckboxes(bool shouldBeChecked)
        {
            foreach (Control control in _flowPanel.Controls)
            {
                if (control is CheckBox cb)
                {
                    cb.Checked = shouldBeChecked;
                }
            }
        }

        private void SetupUI()
        {
            Text = "CS2 Relay Picker v1";
            Size = new Size(460, 600);

            _flowPanel = new()
            {
                Location = new Point(20,50),
                Size = new Size(400, 400),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblHeader = new() { Text = "Refresh first before confirming.", Location = new Point(20, 20), AutoSize = true };
            Label lblTag = new() { Text = "created by garden (y).", Location = new Point(20, 540), AutoSize = true };

            Button btnRefresh = new()
            { 
                Text = "Refresh Server List", 
                Location = new Point(20, 470), 
                Size = new Size(160, 40),
                BackColor = Color.Yellow
            };

            Button btnUpdateRules = new()
            { 
                Text = "Confirm Rules", 
                Location = new Point(200, 470), 
                Size = new Size(160, 40),
                BackColor = Color.Green 
            };

            _chkBoxAll = new() { Text = "Select All", Location = new Point(20, 450), AutoSize = true };
            _chkBoxAll.CheckedChanged += (s, e) =>
            {
                if (!_selectAllState) ToggleAllCheckboxes(_chkBoxAll.Checked);
            };

            btnRefresh.Click += async (s, e) => await RunCommand("refresh");
            btnUpdateRules.Click += async (s, e) => await RunCommand("updaterules");

            Controls.Add(lblHeader);
            Controls.Add(lblTag);
            Controls.Add(btnRefresh);
            Controls.Add(btnUpdateRules);
            Controls.Add(_chkBoxAll);
            Controls.Add(_flowPanel);
        }

        private async Task RunCommand(string action)
        {
            if (!_refreshFlag && (action != "refresh"))
            {
                MessageBox.Show("Refresh first!");
            }
            else
            {
                switch (action)
                {
                    case "refresh":
                        _refreshFlag = true;
                        await RefreshData();
                        await PopulateCheckbox();
                        break;
                    case "updaterules":
                        await Firewall.UpdateFirewall(_flowPanel);
                        break;
                }
            }
        }
    }
}