using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using FileContext.Data;
using FileContext.Models;

namespace FileContext;

public partial class NewEntryWindow : Window
{
    private readonly ContextEntry? _entryToEdit;

    public NewEntryWindow()
    {
        InitializeComponent();
    }

    public NewEntryWindow(ContextEntry entry)
    {
        InitializeComponent();

        _entryToEdit = entry;

        Title = "Kaydı Düzenle";

        TitleTextBox.Text = entry.Title;
        PathTextBox.Text = entry.Path;
        DescriptionTextBox.Text = entry.Description;
        NextActionTextBox.Text = entry.NextAction;
        TagsTextBox.Text = entry.Tags;

        foreach (ComboBoxItem item in StatusComboBox.Items)
        {
            if (item.Content?.ToString() == entry.Status)
            {
                StatusComboBox.SelectedItem = item;
                break;
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("Başlık boş bırakılamaz.");
            return;
        }

        string status =
            (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
            ?? "Aktif";

        if (_entryToEdit == null)
        {
            Database.AddEntry(
                TitleTextBox.Text.Trim(),
                PathTextBox.Text.Trim(),
                DescriptionTextBox.Text.Trim(),
                status,
                NextActionTextBox.Text.Trim(),
                TagsTextBox.Text.Trim());
        }
        else
        {
            _entryToEdit.Title = TitleTextBox.Text.Trim();
            _entryToEdit.Path = PathTextBox.Text.Trim();
            _entryToEdit.Description = DescriptionTextBox.Text.Trim();
            _entryToEdit.Status = status;
            _entryToEdit.NextAction = NextActionTextBox.Text.Trim();
            _entryToEdit.Tags = TagsTextBox.Text.Trim();

            Database.UpdateEntry(_entryToEdit);
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new()
        {
            Title = "Klasör Seç",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            PathTextBox.Text = dialog.FolderName;

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                TitleTextBox.Text =
                    System.IO.Path.GetFileName(dialog.FolderName.TrimEnd('\\'));
            }
        }
    }

    private void SelectFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Title = "Dosya Seç",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            PathTextBox.Text = dialog.FileName;

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                TitleTextBox.Text =
                    System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }
}