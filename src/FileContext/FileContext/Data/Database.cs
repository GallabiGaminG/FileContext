using System;
using System.IO;
using FileContext.Models;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace FileContext.Data;

public static class Database
{
    private static readonly string DbPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FileContext",
            "FileContext.db");

    private static string ConnectionString => $"Data Source={DbPath}";

    public static void Initialize()
    {
        string? directory = Path.GetDirectoryName(DbPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using SqliteConnection connection = new(ConnectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS ContextEntries
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Path TEXT NOT NULL,
            Description TEXT NOT NULL,
            Status TEXT NOT NULL,
            NextAction TEXT NOT NULL,
            Tags TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();
    }

    public static void AddEntry(
    string title,
    string path,
    string description,
    string status,
    string nextAction,
    string tags)
    {
        using SqliteConnection connection = new(ConnectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText =
        """
    INSERT INTO ContextEntries
    (
        Title,
        Path,
        Description,
        Status,
        NextAction,
        Tags,
        CreatedAt,
        UpdatedAt
    )
    VALUES
    (
        $title,
        $path,
        $description,
        $status,
        $nextAction,
        $tags,
        $createdAt,
        $updatedAt
    );
    """;

        string now = DateTime.UtcNow.ToString("O");

        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$path", path);
        command.Parameters.AddWithValue("$description", description);
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$nextAction", nextAction);
        command.Parameters.AddWithValue("$tags", tags);
        command.Parameters.AddWithValue("$createdAt", now);
        command.Parameters.AddWithValue("$updatedAt", now);

        command.ExecuteNonQuery();
    }

    public static List<ContextEntry> GetEntries()
    {
        List<ContextEntry> entries = new();

        using SqliteConnection connection = new(ConnectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText =
        """
    SELECT
        Id,
        Title,
        Path,
        Description,
        Status,
        NextAction,
        Tags,
        CreatedAt,
        UpdatedAt
    FROM ContextEntries
    ORDER BY UpdatedAt DESC;
    """;

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            entries.Add(new ContextEntry
            {
                Id = reader.GetInt64(0),
                Title = reader.GetString(1),
                Path = reader.GetString(2),
                Description = reader.GetString(3),
                Status = reader.GetString(4),
                NextAction = reader.GetString(5),
                Tags = reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7)),
                UpdatedAt = DateTime.Parse(reader.GetString(8))
            });
        }

        return entries;
    }

    public static void UpdateEntry(ContextEntry entry)
    {
        using SqliteConnection connection = new(ConnectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText =
        """
    UPDATE ContextEntries
    SET
        Title = $title,
        Path = $path,
        Description = $description,
        Status = $status,
        NextAction = $nextAction,
        Tags = $tags,
        UpdatedAt = $updatedAt
    WHERE Id = $id;
    """;

        command.Parameters.AddWithValue("$title", entry.Title);
        command.Parameters.AddWithValue("$path", entry.Path);
        command.Parameters.AddWithValue("$description", entry.Description);
        command.Parameters.AddWithValue("$status", entry.Status);
        command.Parameters.AddWithValue("$nextAction", entry.NextAction);
        command.Parameters.AddWithValue("$tags", entry.Tags);
        command.Parameters.AddWithValue("$updatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$id", entry.Id);

        command.ExecuteNonQuery();
    }

    public static void DeleteEntry(long id)
    {
        using SqliteConnection connection = new(ConnectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText =
        """
    DELETE FROM ContextEntries
    WHERE Id = $id;
    """;

        command.Parameters.AddWithValue("$id", id);

        command.ExecuteNonQuery();
    }
}