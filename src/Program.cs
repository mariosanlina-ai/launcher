using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiestaLauncher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LauncherForm());
    }
}

internal sealed class LauncherConfig
{
    [JsonPropertyName("gamePath")]
    public string GamePath { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;

    [JsonPropertyName("serverIp")]
    public string ServerIp { get; set; } = string.Empty;

    [JsonPropertyName("serverPort")]
    public string ServerPort { get; set; } = string.Empty;
}

internal sealed class LauncherForm : Form
{
    private const string AppTitle = "Fiesta Online Launcher";

    private readonly TextBox _gamePathBox = new() { PlaceholderText = "Pfad zu Fiesta.exe" };
    private readonly TextBox _argumentsBox = new() { PlaceholderText = "z. B. -windowed -novsync" };
    private readonly TextBox _serverIpBox = new() { PlaceholderText = "127.0.0.1" };
    private readonly TextBox _serverPortBox = new() { PlaceholderText = "9010" };
    private readonly Label _statusLabel = new() { Text = "Bereit", AutoSize = true };

    private readonly string _configDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FiestaLauncher");

    private string ConfigPath => Path.Combine(_configDirectory, "config.json");

    public LauncherForm()
    {
        Text = AppTitle;
        Width = 720;
        Height = 360;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 28, 36);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f);

        BuildUi();
        LoadConfigIntoForm();
    }

    private void BuildUi()
    {
        var title = new Label
        {
            Text = "Fiesta Online Launcher",
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.FromArgb(120, 196, 255),
            Location = new Point(18, 12)
        };

        var subtitle = new Label
        {
            Text = "Stylischer Launcher mit Server-IP und erweiterten Startfunktionen",
            AutoSize = true,
            ForeColor = Color.FromArgb(200, 210, 220),
            Location = new Point(20, 48)
        };

        var gamePathLabel = CreateLabel("Fiesta.exe", 20, 92);
        _gamePathBox.SetBounds(20, 114, 540, 32);

        var browseButton = CreateButton("Durchsuchen", 570, 113, 120, 34, (_, _) => BrowseGamePath());

        var argumentsLabel = CreateLabel("Startargumente", 20, 154);
        _argumentsBox.SetBounds(20, 176, 670, 32);

        var ipLabel = CreateLabel("Server IP", 20, 216);
        _serverIpBox.SetBounds(20, 238, 220, 32);

        var portLabel = CreateLabel("Port", 260, 216);
        _serverPortBox.SetBounds(260, 238, 120, 32);

        var saveButton = CreateButton("Speichern", 400, 236, 90, 36, (_, _) => SaveFromForm());
        var startButton = CreateButton("Spiel starten", 500, 236, 190, 36, (_, _) => StartGame());

        _statusLabel.Location = new Point(20, 286);
        _statusLabel.ForeColor = Color.FromArgb(152, 242, 162);

        Controls.AddRange(
        [
            title, subtitle,
            gamePathLabel, _gamePathBox, browseButton,
            argumentsLabel, _argumentsBox,
            ipLabel, _serverIpBox,
            portLabel, _serverPortBox,
            saveButton, startButton,
            _statusLabel
        ]);
    }

    private static Label CreateLabel(string text, int x, int y) => new()
    {
        Text = text,
        AutoSize = true,
        Location = new Point(x, y),
        ForeColor = Color.FromArgb(220, 224, 231)
    };

    private static Button CreateButton(string text, int x, int y, int width, int height, EventHandler clickHandler)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(61, 111, 201),
            ForeColor = Color.White
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += clickHandler;
        return button;
    }

    private void BrowseGamePath()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Fiesta.exe auswählen",
            Filter = "Executable (*.exe)|*.exe|Alle Dateien (*.*)|*.*"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _gamePathBox.Text = dialog.FileName;
            _statusLabel.Text = "Datei ausgewählt.";
        }
    }

    private LauncherConfig ReadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            return new LauncherConfig();
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<LauncherConfig>(json) ?? new LauncherConfig();
        }
        catch
        {
            return new LauncherConfig();
        }
    }

    private void LoadConfigIntoForm()
    {
        var config = ReadConfig();

        _gamePathBox.Text = config.GamePath;
        _argumentsBox.Text = config.Arguments;
        _serverIpBox.Text = config.ServerIp;
        _serverPortBox.Text = config.ServerPort;
    }

    private void SaveFromForm()
    {
        Directory.CreateDirectory(_configDirectory);

        var config = new LauncherConfig
        {
            GamePath = _gamePathBox.Text.Trim(),
            Arguments = _argumentsBox.Text.Trim(),
            ServerIp = _serverIpBox.Text.Trim(),
            ServerPort = _serverPortBox.Text.Trim()
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);

        _statusLabel.Text = "Einstellungen gespeichert.";
    }

    private void StartGame()
    {
        SaveFromForm();

        var exePath = _gamePathBox.Text.Trim();
        if (!File.Exists(exePath))
        {
            MessageBox.Show("Fiesta.exe wurde nicht gefunden.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            _statusLabel.Text = "Start fehlgeschlagen: Datei nicht gefunden.";
            return;
        }

        var launchArgs = _argumentsBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(_serverIpBox.Text))
        {
            launchArgs = $"{launchArgs} --ip {_serverIpBox.Text.Trim()}".Trim();
        }

        if (!string.IsNullOrWhiteSpace(_serverPortBox.Text))
        {
            launchArgs = $"{launchArgs} --port {_serverPortBox.Text.Trim()}".Trim();
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath) ?? Environment.CurrentDirectory,
            Arguments = launchArgs,
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
            _statusLabel.Text = "Spiel gestartet.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Start fehlgeschlagen: {ex.Message}", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            _statusLabel.Text = "Start fehlgeschlagen.";
        }
    }
}
