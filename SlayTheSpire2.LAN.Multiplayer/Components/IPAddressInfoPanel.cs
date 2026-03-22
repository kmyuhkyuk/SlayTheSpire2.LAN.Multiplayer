using System.Net.NetworkInformation;
using System.Net.Sockets;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SlayTheSpire2.LAN.Multiplayer.Services;
using BoxContainer = Godot.BoxContainer;
using Control = Godot.Control;
using HttpClient = System.Net.Http.HttpClient;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class IPAddressInfoPanel : Control
    {
        private Control? _content;

        private Control? _menu;

        private Control? _box;

        private Control? _loading;

        private CopiedLabel? _copiedLabel;

        private Control? _ipAddress;

        private IPAddressLabel? _ipAddressTitleLabel;

        private IPAddressLabel? _ipAddressLabel;

        private Control? _ipv6Address;

        private IPAddressLabel? _ipv6AddressTitleLabel;

        private IPAddressLabel? _ipv6AddressLabel;

        private Control? _localIPAddress;

        private IPAddressLabel? _localIPAddressTitleLabel;

        private Control? _localIPAddressContainer;

        private CancellationTokenSource? _cancellationTokenSource;

        private static readonly HttpClient HttpClient = new();

        public static IPAddressInfoPanel Create()
        {
            var ipAddressInfoPanel = new IPAddressInfoPanel();

            ipAddressInfoPanel.MouseFilter = MouseFilterEnum.Ignore;

            ipAddressInfoPanel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var content = new VBoxContainer { Name = "Content" };
            ipAddressInfoPanel.AddChild(content);

            content.MouseFilter = MouseFilterEnum.Stop;

            var menu = new Control { Name = "Menu" };
            content.AddChild(menu);

            menu.CustomMinimumSize = new Vector2(300, 24);

            menu.MouseFilter = MouseFilterEnum.Pass;

            var background = new NinePatchRect { Name = "Background" };
            menu.AddChild(background);

            background.MouseFilter = MouseFilterEnum.Ignore;

            background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            background.Texture = GD.Load<CompressedTexture2D>("res://images/ui/tiny_nine_patch.png");
            background.PatchMarginLeft = 12;
            background.PatchMarginTop = 12;
            background.PatchMarginRight = 12;
            background.PatchMarginBottom = 12;

            background.Modulate = new Color(Colors.Black, 0.471f);

            var menuIcon = new TextureRect { Name = "Icon" };
            menu.AddChild(menuIcon);

            menuIcon.MouseFilter = MouseFilterEnum.Ignore;

            menuIcon.SetAnchorsPreset(LayoutPreset.Center);
            menuIcon.OffsetLeft = -12;
            menuIcon.OffsetTop = -12;
            menuIcon.OffsetRight = 12;
            menuIcon.OffsetBottom = 12;

            var menuSvgImage = new Image();

            menuSvgImage.LoadSvgFromString(
                "<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"24px\" viewBox=\"0 -960 960 960\" width=\"24px\" fill=\"#FFFFFF\"><path d=\"M120-240v-80h720v80H120Zm0-200v-80h720v80H120Zm0-200v-80h720v80H120Z\"/></svg>");

            menuIcon.Texture = ImageTexture.CreateFromImage(menuSvgImage);

            var box = new PanelContainer { Name = "Box" };
            content.AddChild(box);

            box.MouseFilter = MouseFilterEnum.Ignore;

            box.Modulate = new Color(Colors.White, 0);

            var styleBox = new StyleBoxTexture();

            styleBox.Texture = GD.Load<CompressedTexture2D>("res://images/ui/tiny_nine_patch.png");
            styleBox.TextureMarginLeft = 12;
            styleBox.TextureMarginTop = 12;
            styleBox.TextureMarginRight = 12;
            styleBox.TextureMarginBottom = 12;

            styleBox.ContentMarginLeft = 12;
            styleBox.ContentMarginTop = 12;
            styleBox.ContentMarginRight = 12;
            styleBox.ContentMarginBottom = 12;

            styleBox.ModulateColor = new Color(Colors.Black, 0.471f);

            box.AddThemeStyleboxOverride("panel", styleBox);

            var container = new VBoxContainer { Name = "Container" };
            box.AddChild(container);

            container.MouseFilter = MouseFilterEnum.Ignore;

            container.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            container.Alignment = BoxContainer.AlignmentMode.Center;

            container.AddThemeConstantOverride("separation", 8);

            var loading = new Control { Name = "Loading" };
            container.AddChild(loading);

            loading.CustomMinimumSize = new Vector2(64, 64);

            loading.MouseFilter = MouseFilterEnum.Ignore;

            var loadingIcon = new LoadingIcon();
            loading.AddChild(loadingIcon);

            loadingIcon.MouseFilter = MouseFilterEnum.Ignore;

            loadingIcon.SetAnchorsPreset(LayoutPreset.Center);
            loadingIcon.OffsetLeft = -32;
            loadingIcon.OffsetTop = -32;
            loadingIcon.OffsetRight = 32;
            loadingIcon.OffsetBottom = 32;

            AddAddressElement(container, "IPAddress", "SlayTheSpire2.LAN.Multiplayer.IP_ADDRESS_TITLE", false);
            AddAddressElement(container, "IPV6IPAddress", "SlayTheSpire2.LAN.Multiplayer.IPV6_ADDRESS_TITLE", true);
            AddAddressContainer(container, "LocalIPAddress", "SlayTheSpire2.LAN.Multiplayer.LOCAL_IP_ADDRESS_TITLE");

            content.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterTop);

            var copiedLabel = new CopiedLabel { Name = "CopiedLabel" };
            ipAddressInfoPanel.AddChild(copiedLabel);

            copiedLabel.MouseFilter = MouseFilterEnum.Ignore;

            return ipAddressInfoPanel;
        }

        private static void AddAddressElement(Node container, string name, string locKeyPrefix, bool isTrim)
        {
            var addressElement = new HBoxContainer { Name = name };
            container.AddChild(addressElement);

            addressElement.CustomMinimumSize = new Vector2(0, 24);

            addressElement.Alignment = BoxContainer.AlignmentMode.Center;

            addressElement.MouseFilter = MouseFilterEnum.Ignore;

            var ipAddressTitleLabel = new IPAddressLabel { Name = "TitleLabel" };
            addressElement.AddChild(ipAddressTitleLabel);

            ipAddressTitleLabel.MouseFilter = MouseFilterEnum.Ignore;

            ipAddressTitleLabel.SetLocalization(locKeyPrefix);

            var ipAddressLabel = new IPAddressLabel { Name = "Label" };
            addressElement.AddChild(ipAddressLabel);

            ipAddressLabel.MouseFilter = MouseFilterEnum.Ignore;

            if (isTrim)
            {
                ipAddressLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                ipAddressLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            }
        }

        private static void AddAddressContainer(Node container, string name, string locKeyPrefix)
        {
            var addressElement = new VBoxContainer { Name = name };
            container.AddChild(addressElement);

            addressElement.CustomMinimumSize = new Vector2(0, 24);

            addressElement.MouseFilter = MouseFilterEnum.Ignore;

            var ipAddressTitleLabel = new IPAddressLabel { Name = "TitleLabel" };
            addressElement.AddChild(ipAddressTitleLabel);

            ipAddressTitleLabel.HorizontalAlignment = HorizontalAlignment.Center;

            ipAddressTitleLabel.SetLocalization(locKeyPrefix);

            var vBoxContainer = new VBoxContainer { Name = "Container" };
            addressElement.AddChild(vBoxContainer);

            vBoxContainer.MouseFilter = MouseFilterEnum.Ignore;

            vBoxContainer.Alignment = BoxContainer.AlignmentMode.Center;
        }

        public override void _Ready()
        {
            _content = GetNode<Control>("Content");
            _menu = GetNode<Control>("Content/Menu");
            _box = GetNode<Control>("Content/Box");

            _loading = GetNode<Control>("Content/Box/Container/Loading");

            _ipAddress = GetNode<Control>("Content/Box/Container/IPAddress");
            _ipAddressTitleLabel = _ipAddress.GetNode<IPAddressLabel>("TitleLabel");
            _ipAddressLabel = _ipAddress.GetNode<IPAddressLabel>("Label");

            _copiedLabel = GetNode<CopiedLabel>("CopiedLabel");

            _ipAddress.GuiInput += inputEvent =>
            {
                if (inputEvent is InputEventMouseButton
                    {
                        ButtonIndex: MouseButton.Left, Pressed: true
                    } inputEventMouseButton &&
                    !string.IsNullOrEmpty(_ipAddressLabel.Text))
                {
                    DisplayServer.ClipboardSet(_ipAddressLabel.Text);
                    _copiedLabel.ShowWithPosition(inputEventMouseButton.GlobalPosition);
                }
            };

            _ipv6Address = GetNode<Control>("Content/Box/Container/IPV6IPAddress");
            _ipv6AddressTitleLabel = _ipv6Address.GetNode<IPAddressLabel>("TitleLabel");
            _ipv6AddressLabel = _ipv6Address.GetNode<IPAddressLabel>("Label");

            _ipv6Address.GuiInput += inputEvent =>
            {
                if (inputEvent is InputEventMouseButton
                    {
                        ButtonIndex: MouseButton.Left, Pressed: true
                    } inputEventMouseButton &&
                    !string.IsNullOrEmpty(_ipv6AddressLabel.Text))
                {
                    DisplayServer.ClipboardSet(_ipv6AddressLabel.Text);
                    _copiedLabel.ShowWithPosition(inputEventMouseButton.GlobalPosition);
                }
            };

            _localIPAddress = GetNode<Control>("Content/Box/Container/LocalIPAddress");
            _localIPAddressTitleLabel = _localIPAddress.GetNode<IPAddressLabel>("TitleLabel");
            _localIPAddressContainer = _localIPAddress.GetNode<Control>("Container");

            UpdateController();

            _menu.MouseEntered += OnMouseEntered;
            _content.MouseExited += OnMouseExited;

            NControllerManager.Instance?.Connect(NControllerManager.SignalName.MouseDetected,
                Callable.From(UpdateController));
            NControllerManager.Instance?.Connect(NControllerManager.SignalName.ControllerDetected,
                Callable.From(UpdateController));
            NInputManager.Instance?.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateController));
        }

        private void UpdateController()
        {
            if (NControllerManager.Instance?.IsUsingController ?? false)
            {
                ShowBox();
            }
        }

        public void Initialize()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();

            TaskHelper.RunSafely(InitializeAsync(_cancellationTokenSource.Token));
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (_ipAddress == null || _localIPAddress == null || _ipv6Address == null || _loading == null ||
                _content == null || _localIPAddressContainer == null)
            {
                Log.Error($"{nameof(IPAddressInfoPanel)} has null element");
                return;
            }

            _ipAddress.Visible = false;
            _localIPAddress.Visible = false;
            _ipv6Address.Visible = false;

            foreach (var child in _localIPAddressContainer.GetChildren())
            {
                child.QueueFree();
            }

            _loading.Visible = true;

            _content.Size = _content.CustomMinimumSize;

            var ipAddress = string.Empty;
            var port = SettingsService.Instance.SettingsModel.HostPort;

            var hasIPAddress = false;

            try
            {
                ipAddress = await HttpClient.GetStringAsync("https://api-ipv4.ip.sb/ip", cancellationToken);
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    _ipAddressLabel?.SetTextAutoSize($"{ipAddress.Replace("\n", string.Empty)}:{port}");
                    hasIPAddress = true;
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
            }

            var hasLocalIPAddress = false;

            var localIPAddressList = GetLocalIPAddressList();

            foreach (var localIPAddress in localIPAddressList)
            {
                if (localIPAddress != ipAddress)
                {
                    var ipAddressLabel = new IPAddressLabel();
                    _localIPAddressContainer.AddChild(ipAddressLabel);

                    ipAddressLabel.MouseFilter = MouseFilterEnum.Pass;

                    ipAddressLabel.HorizontalAlignment = HorizontalAlignment.Center;

                    ipAddressLabel.Text = $"{localIPAddress}:{port}";

                    ipAddressLabel.GuiInput += inputEvent =>
                    {
                        if (inputEvent is InputEventMouseButton
                            {
                                ButtonIndex: MouseButton.Left, Pressed: true
                            } inputEventMouseButton &&
                            !string.IsNullOrEmpty(ipAddressLabel.Text))
                        {
                            DisplayServer.ClipboardSet(ipAddressLabel.Text);
                            _copiedLabel?.ShowWithPosition(inputEventMouseButton.GlobalPosition);
                        }
                    };

                    hasLocalIPAddress = true;
                }
            }

            var hasIPV6Address = false;

            try
            {
                var ipv6Address = await HttpClient.GetStringAsync("https://api-ipv6.ip.sb/ip", cancellationToken);
                _ipv6AddressLabel?.SetTextAutoSize($"[{ipv6Address.Replace("\n", string.Empty)}]:{port}");
                hasIPV6Address = true;
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
            }

            _loading.Visible = false;

            _ipAddress.Visible = hasIPAddress;
            _localIPAddress.Visible = hasLocalIPAddress;
            _ipv6Address.Visible = hasIPV6Address;

            _content.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterTop);
        }

        private static List<string> GetLocalIPAddressList()
        {
            var list = new List<string>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    if (networkInterface.Name.Contains("vEthernet"))
                        continue;

                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            list.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            return list;
        }

        private void OnMouseEntered()
        {
            if (NControllerManager.Instance?.IsUsingController ?? false)
                return;

            ShowBox();
        }

        private void OnMouseExited()
        {
            if (NControllerManager.Instance?.IsUsingController ?? false)
                return;

            HideBox();
        }

        private void ShowBox()
        {
            if (_box == null || _ipAddress == null || _localIPAddress == null || _ipv6Address == null)
                return;

            _box.Modulate = new Color(Colors.White);
            _ipAddress.MouseFilter = MouseFilterEnum.Pass;
            _localIPAddress.MouseFilter = MouseFilterEnum.Pass;
            _ipv6Address.MouseFilter = MouseFilterEnum.Pass;
        }

        private void HideBox()
        {
            if (_box == null || _ipAddress == null || _localIPAddress == null || _ipv6Address == null)
                return;

            _box.Modulate = new Color(Colors.White, 0);
            _ipAddress.MouseFilter = MouseFilterEnum.Ignore;
            _localIPAddress.MouseFilter = MouseFilterEnum.Ignore;
            _ipv6Address.MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _ExitTree()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}