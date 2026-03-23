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
            var ipAddressInfoPanel = new IPAddressInfoPanel { MouseFilter = MouseFilterEnum.Ignore };
            ipAddressInfoPanel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var content = new VBoxContainer { Name = "Content", MouseFilter = MouseFilterEnum.Stop };
            ipAddressInfoPanel.AddChildSafely(content);

            var menu = new Control
                { Name = "Menu", CustomMinimumSize = new Vector2(300, 24), MouseFilter = MouseFilterEnum.Pass };
            content.AddChildSafely(menu);

            var background = new NinePatchRect
            {
                Name = "Background", MouseFilter = MouseFilterEnum.Ignore,
                Texture = GD.Load<CompressedTexture2D>("res://images/ui/tiny_nine_patch.png"), PatchMarginLeft = 12,
                PatchMarginTop = 12, PatchMarginRight = 12, PatchMarginBottom = 12,
                Modulate = new Color(Colors.Black, 0.471f)
            };

            menu.AddChildSafely(background);
            background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var menuIcon = new TextureRect { Name = "Icon", MouseFilter = MouseFilterEnum.Ignore };

            var menuSvgImage = new Image();
            menuSvgImage.LoadSvgFromString(
                "<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"24px\" viewBox=\"0 -960 960 960\" width=\"24px\" fill=\"#FFFFFF\"><path d=\"M120-240v-80h720v80H120Zm0-200v-80h720v80H120Zm0-200v-80h720v80H120Z\"/></svg>");
            menuIcon.Texture = ImageTexture.CreateFromImage(menuSvgImage);

            menu.AddChildSafely(menuIcon);
            menuIcon.SetAnchorsPreset(LayoutPreset.Center);
            menuIcon.OffsetLeft = -12;
            menuIcon.OffsetTop = -12;
            menuIcon.OffsetRight = 12;
            menuIcon.OffsetBottom = 12;

            var box = new PanelContainer
                { Name = "Box", MouseFilter = MouseFilterEnum.Ignore, Modulate = new Color(Colors.White, 0) };

            var styleBox = new StyleBoxTexture
            {
                Texture = GD.Load<CompressedTexture2D>("res://images/ui/tiny_nine_patch.png"), TextureMarginLeft = 12,
                TextureMarginTop = 12, TextureMarginRight = 12, TextureMarginBottom = 12, ContentMarginLeft = 12,
                ContentMarginTop = 12, ContentMarginRight = 12, ContentMarginBottom = 12,
                ModulateColor = new Color(Colors.Black, 0.471f)
            };

            box.AddThemeStyleboxOverride("panel", styleBox);
            content.AddChildSafely(box);

            var container = new VBoxContainer
            {
                Name = "Container", MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.Center
            };
            container.AddThemeConstantOverride("separation", 8);
            box.AddChildSafely(container);
            container.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var loading = new Control
                { Name = "Loading", CustomMinimumSize = new Vector2(64, 64), MouseFilter = MouseFilterEnum.Ignore };
            container.AddChildSafely(loading);

            var loadingIcon = new LoadingIcon { MouseFilter = MouseFilterEnum.Ignore };
            loading.AddChildSafely(loadingIcon);
            loadingIcon.SetAnchorsPreset(LayoutPreset.Center);
            loadingIcon.OffsetLeft = -32;
            loadingIcon.OffsetTop = -32;
            loadingIcon.OffsetRight = 32;
            loadingIcon.OffsetBottom = 32;

            AddAddressElement(container, "IPAddress", "SlayTheSpire2.LAN.Multiplayer.IP_ADDRESS_TITLE", false);
            AddAddressElement(container, "IPV6IPAddress", "SlayTheSpire2.LAN.Multiplayer.IPV6_ADDRESS_TITLE", true);
            AddAddressContainer(container, "LocalIPAddress", "SlayTheSpire2.LAN.Multiplayer.LOCAL_IP_ADDRESS_TITLE");

            content.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterTop);

            var copiedLabel = new CopiedLabel { Name = "CopiedLabel", MouseFilter = MouseFilterEnum.Ignore };
            ipAddressInfoPanel.AddChildSafely(copiedLabel);

            return ipAddressInfoPanel;
        }

        private static void AddAddressElement(Node container, string name, string locKeyPrefix, bool isTrim)
        {
            var addressElement = new HBoxContainer
                { Name = name, CustomMinimumSize = new Vector2(0, 24), MouseFilter = MouseFilterEnum.Ignore };

            var ipAddressTitleLabel = new IPAddressLabel { Name = "TitleLabel", MouseFilter = MouseFilterEnum.Ignore };
            addressElement.AddChildSafely(ipAddressTitleLabel);
            ipAddressTitleLabel.SetLocalization(locKeyPrefix);

            var ipAddressLabel = new IPAddressLabel { Name = "Label", MouseFilter = MouseFilterEnum.Ignore };

            if (isTrim)
            {
                ipAddressLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                ipAddressLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            }

            addressElement.AddChildSafely(ipAddressLabel);

            container.AddChildSafely(addressElement);
        }

        private static void AddAddressContainer(Node container, string name, string locKeyPrefix)
        {
            var addressElement = new VBoxContainer
                { Name = name, CustomMinimumSize = new Vector2(0, 24), MouseFilter = MouseFilterEnum.Ignore };

            var ipAddressTitleLabel = new IPAddressLabel
                { Name = "TitleLabel", HorizontalAlignment = HorizontalAlignment.Center };
            addressElement.AddChildSafely(ipAddressTitleLabel);
            ipAddressTitleLabel.SetLocalization(locKeyPrefix);

            var vBoxContainer = new VBoxContainer
            {
                Name = "Container", MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.Center
            };

            addressElement.AddChildSafely(vBoxContainer);

            container.AddChildSafely(addressElement);
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
                    _copiedLabel.Show(inputEventMouseButton.GlobalPosition);
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
                    _copiedLabel.Show(inputEventMouseButton.GlobalPosition);
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
                child.QueueFreeSafely();
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

            foreach (var localIPAddress in GetLocalIPAddress())
            {
                if (localIPAddress != ipAddress)
                {
                    var ipAddressLabel = new IPAddressLabel
                        { MouseFilter = MouseFilterEnum.Pass, HorizontalAlignment = HorizontalAlignment.Center };

                    ipAddressLabel.SetTextAutoSize($"{localIPAddress}:{port}");

                    _localIPAddressContainer.AddChildSafely(ipAddressLabel);

                    ipAddressLabel.GuiInput += inputEvent =>
                    {
                        if (inputEvent is InputEventMouseButton
                            {
                                ButtonIndex: MouseButton.Left, Pressed: true
                            } inputEventMouseButton &&
                            !string.IsNullOrEmpty(ipAddressLabel.Text))
                        {
                            DisplayServer.ClipboardSet(ipAddressLabel.Text);
                            _copiedLabel?.Show(inputEventMouseButton.GlobalPosition);
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

        private static IEnumerable<string> GetLocalIPAddress()
        {
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
                            yield return ip.Address.ToString();
                    }
                }
            }
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