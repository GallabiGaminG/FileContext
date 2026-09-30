using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FileContext.Data;
using FileContext.Models;
using FileContext.Services;

namespace FileContext;

public partial class MainWindow : Window
{
    private List<ContextEntry> _entries = new();

    public MainWindow()
    {
        InitializeComponent();

        LoadEntries();
    }

    private void LoadEntries()
    {
        _entries = Database.GetEntries();

        EntriesList.ItemsSource = _entries;
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

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string search = SearchTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(search))
        {
            EntriesList.ItemsSource = _entries;
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
    }

    private void OpenEntry_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not ContextEntry entry)
        {
            return;
        }

        if (!System.IO.Directory.Exists(entry.Path) &&
            !System.IO.File.Exists(entry.Path))
        {
            MessageBox.Show(
                "Bu path artık mevcut değil:\n\n" + entry.Path,
                "Path bulunamadı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = entry.Path,
            UseShellExecute = true
        });
    }

    private void EditEntry_Click(object sender, RoutedEventArgs e)
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

    private void DeleteEntry_Click(object sender, RoutedEventArgs e)
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

    private async void EverythingTest_Click(object sender, RoutedEventArgs e)
    {
        List<string> results = await EverythingService.SearchAsync("persona");

        MessageBox.Show(
            $"Everything sonucu: {results.Count}\n\n" +
            string.Join("\n", results.Take(10)),
            "Everything Test");
    }
}