using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileContext.Data;
using FileContext.Models;
using FileContext.Services;
using System.IO;
using System.Windows.Threading;

namespace FileContext;

public partial class MainWindow : Window
{
    private List<ContextEntry> _entries = new();

    private CancellationTokenSource? _searchCancellation;

    private const int EverythingPageSize = 50;

    private int _visibleEverythingCount = EverythingPageSize;

    private List<string> _allEverythingResults = new();

    private DispatcherTimer? _diskActivityTimer;

    private List<ProcessIoInfo> _activeProcesses = new();

    private readonly FileIoMonitorService _fileIoMonitor = new();

    private List<ProbableTransferInfo> _probableTransfers = new();

    private bool _isLivePaused;

    public MainWindow()
    {
        InitializeComponent();

        LoadEntries();
        LoadDrives();
        StartDiskActivityTimer();

        _fileIoMonitor.Start();
    }

    private List<DriveInfoModel> LoadDriveInfo()
    {
        List<DriveInfoModel> drives = new();

        Dictionary<string, (string Model, string MediaType, string BusType)>
    hardwareInfo =
        StorageHardwareService.GetLogicalDriveHardwareInfo();

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
                continue;

            long totalSize = drive.TotalSize;
            long freeSpace = drive.AvailableFreeSpace;
            long usedSize = totalSize - freeSpace;

            double usedPercentage =
                totalSize > 0
                    ? (double)usedSize / totalSize * 100
                    : 0;

            string driveLetter =
    drive.Name.TrimEnd('\\');

            hardwareInfo.TryGetValue(
                driveLetter,
                out var hardware);

            drives.Add(new DriveInfoModel
            {
                Name = drive.Name,
                VolumeLabel = drive.VolumeLabel,
                DriveType = drive.DriveType.ToString(),

                TotalSize = totalSize,
                UsedSize = usedSize,
                FreeSpace = freeSpace,

                UsedPercentage = usedPercentage,

                PhysicalModel = hardware.Model ?? "",
                MediaType = hardware.MediaType ?? "",
                BusType = hardware.BusType ?? "",

                DeviceTypeText =
    $"{hardware.MediaType} / {hardware.BusType}".Trim(' ', '/'),

                TotalSizeText = FormatBytes(totalSize),
                UsedSizeText = FormatBytes(usedSize),
                FreeSpaceText = FormatBytes(freeSpace)
            });
        }

        return drives;
    }

    private void StartDiskActivityTimer()
    {
        _diskActivityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _diskActivityTimer.Tick += DiskActivityTimer_Tick;
        _diskActivityTimer.Start();
    }

    private async void DiskActivityTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (_isLivePaused)
            return;

        _diskActivityTimer?.Stop();

        try
        {
            await UpdateDiskActivityAsync();
        }
        finally
        {
            if (!_isLivePaused)
                _diskActivityTimer?.Start();
        }
    }

    private async void ToggleLivePause_Click(
    object sender,
    RoutedEventArgs e)
    {
        _isLivePaused = !_isLivePaused;

        if (_isLivePaused)
        {
            _diskActivityTimer?.Stop();

            LivePauseButton.Content = "Resume";
            LiveStatusText.Text = "PAUSED — live data frozen for inspection";
        }
        else
        {
            LivePauseButton.Content = "Pause";
            LiveStatusText.Text = "LIVE";

            await UpdateDiskActivityAsync();

            _diskActivityTimer?.Start();
        }
    }

    private async Task UpdateDiskActivityAsync()
    {
        var diskActivityTask = Task.Run(
            DiskActivityService.GetLogicalDiskActivity);

        var processActivityTask = Task.Run(
            ProcessActivityService.GetActiveProcesses);

        Dictionary<string, (ulong Read, ulong Write)> activity =
            await diskActivityTask;

        List<ProcessIoInfo> processes =
            await processActivityTask;

        if (DrivesList.ItemsSource is not List<DriveInfoModel> drives)
            return;

        foreach (DriveInfoModel drive in drives)
        {
            string driveLetter =
                drive.Name.TrimEnd('\\');

            if (activity.TryGetValue(
                driveLetter,
                out var values))
            {
                drive.ReadSpeedText =
                    FormatSpeed(values.Read);

                drive.WriteSpeedText =
                    FormatSpeed(values.Write);
            }
            else
            {
                drive.ReadSpeedText = "0 MB/s";
                drive.WriteSpeedText = "0 MB/s";
            }
        }

        DrivesList.Items.Refresh();

        _activeProcesses = processes;

        ActiveProcessesList.ItemsSource =
            _activeProcesses;

        RecentFileIoList.ItemsSource =
        _fileIoMonitor.GetRecentEvents(20);

        List<FileIoActivity> correlationEvents =
        _fileIoMonitor.GetRecentEventsSnapshot();

        _probableTransfers =
            TransferCorrelationService.FindProbableTransfers(
                correlationEvents);

        ProbableTransfersList.ItemsSource =
            _probableTransfers;
    }

    private static string FormatSpeed(ulong bytesPerSecond)
    {
        const double MB = 1024 * 1024;

        double value =
            bytesPerSecond / MB;

        return $"{value:0.0} MB/s";
    }

    private void LoadDrives()
    {
        List<DriveInfoModel> drives = LoadDriveInfo();

        DrivesList.ItemsSource = drives;
    }

    private static string FormatBytes(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;
        const long TB = GB * 1024;

        if (bytes >= TB)
            return $"{(double)bytes / TB:0.00} TB";

        if (bytes >= GB)
            return $"{(double)bytes / GB:0.00} GB";

        if (bytes >= MB)
            return $"{(double)bytes / MB:0.00} MB";

        if (bytes >= KB)
            return $"{(double)bytes / KB:0.00} KB";

        return $"{bytes} B";
    }

    private void LoadEntries()
    {
        _entries = Database.GetEntries();

        EntriesList.ItemsSource = _entries;

        ContextHeader.Text = $"Bağlam Kayıtları ({_entries.Count})";
    }

    private void NewEntry_Click(object sender, RoutedEventArgs e)
    {
        NewEntryWindow window = new()
        {
            Owner = this
        };

        bool? result = window.ShowDialog();

        if (result == true)
        {
            LoadEntries();
        }
    }

    private async void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        string search = SearchTextBox.Text.Trim();

        _searchCancellation?.Cancel();

        _searchCancellation = new CancellationTokenSource();

        CancellationToken token = _searchCancellation.Token;

        if (string.IsNullOrWhiteSpace(search))
        {
            EntriesList.ItemsSource = _entries;

            ContextHeader.Text =
                $"Bağlam Kayıtları ({_entries.Count})";

            EverythingResultsList.ItemsSource = null;

            EverythingHeader.Visibility =
                Visibility.Collapsed;

            return;
        }

        string query = search.ToLowerInvariant();

        List<ContextEntry> filtered = _entries
            .Where(entry =>
                entry.Title.ToLowerInvariant().Contains(query) ||
                entry.Path.ToLowerInvariant().Contains(query) ||
                entry.Description.ToLowerInvariant().Contains(query) ||
                entry.Status.ToLowerInvariant().Contains(query) ||
                entry.NextAction.ToLowerInvariant().Contains(query) ||
                entry.Tags.ToLowerInvariant().Contains(query))
            .ToList();

        EntriesList.ItemsSource = filtered;

        ContextHeader.Text =
            $"Bağlam Kayıtları ({filtered.Count})";

        try
        {
            await Task.Delay(250, token);

            EverythingHeader.Text = "Everything aranıyor...";
            EverythingHeader.Visibility = Visibility.Visible;

            _allEverythingResults =
            await EverythingService.SearchAsync(search);

            if (token.IsCancellationRequested)
            {
                return;
            }

            _visibleEverythingCount = EverythingPageSize;

            ShowEverythingResults();
        }
        catch (TaskCanceledException)
        {
            // Kullanıcı yazmaya devam etti.
            // Eski aramayı göstermiyoruz.
        }
        catch (Exception ex)
        {
            EverythingResultsList.ItemsSource = null;

            EverythingHeader.Text =
                $"Everything kullanılamıyor: {ex.Message}";

            EverythingHeader.Visibility =
                Visibility.Visible;
        }
    }

    private void ShowEverythingResults()
    {
        List<string> visibleResults =
            _allEverythingResults
                .Take(_visibleEverythingCount)
                .ToList();

        EverythingResultsList.ItemsSource =
            visibleResults;

        int shownCount = visibleResults.Count;
        int totalCount = _allEverythingResults.Count;

        EverythingHeader.Text =
            $"Everything Sonuçları ({totalCount}) — gösterilen {shownCount}";

        LoadMoreEverythingButton.Visibility =
            shownCount < totalCount
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void LoadMoreEverything_Click(
    object sender,
    RoutedEventArgs e)
    {
        _visibleEverythingCount += EverythingPageSize;

        ShowEverythingResults();
    }

    private void OpenEntry_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not ContextEntry entry)
        {
            return;
        }

        OpenPath(entry.Path);
    }

    private void OpenEverythingResult_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not string path)
        {
            return;
        }

        OpenPath(path);
    }

    private static void OpenPath(string path)
    {
        if (!System.IO.Directory.Exists(path) &&
            !System.IO.File.Exists(path))
        {
            MessageBox.Show(
                "Bu path şu an mevcut değil:\n\n" + path,
                "Path bulunamadı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private void EditEntry_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not ContextEntry entry)
        {
            return;
        }

        NewEntryWindow window = new(entry)
        {
            Owner = this
        };

        bool? result = window.ShowDialog();

        if (result == true)
        {
            LoadEntries();
        }
    }

    private void DeleteEntry_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not ContextEntry entry)
        {
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"'{entry.Title}' kaydını silmek istediğine emin misin?",
            "Kaydı Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        Database.DeleteEntry(entry.Id);

        LoadEntries();
    }

    private void AddEverythingResult_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not string path)
        {
            return;
        }

        NewEntryWindow window = new(path)
        {
            Owner = this
        };

        bool? result = window.ShowDialog();

        if (result == true)
        {
            LoadEntries();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _fileIoMonitor.Dispose();

        base.OnClosed(e);
    }
}