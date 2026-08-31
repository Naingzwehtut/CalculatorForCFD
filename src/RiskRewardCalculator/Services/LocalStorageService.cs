using System.Text.Json;
using Microsoft.JSInterop;

namespace RiskRewardCalculator.Services;

/// <summary>
/// Thin C# wrapper around <c>wwwroot/js/interop.js</c>. This is the ONLY place
/// in the app that talks to JavaScript - every other class is pure C#.
/// Browser storage and the clipboard are JavaScript-only APIs, so a small
/// interop call is unavoidable; this class keeps that surface area to one file.
/// </summary>
public class LocalStorageService(IJSRuntime jsRuntime)
{
    private const string InteropNamespace = "riskRewardInterop";

    /// <summary>Serializes <paramref name="value"/> as JSON and saves it under <paramref name="key"/>.</summary>
    public async Task SaveAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await jsRuntime.InvokeVoidAsync($"{InteropNamespace}.localStorageSet", key, json);
    }

    /// <summary>Loads and deserializes the value stored under <paramref name="key"/>, or default if absent/invalid.</summary>
    public async Task<T?> LoadAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>($"{InteropNamespace}.localStorageGet", key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            // Corrupted or outdated data shape - ignore and behave as if nothing was saved.
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync($"{InteropNamespace}.localStorageRemove", key);
    }

    /// <summary>Copies text to the clipboard. Returns false if the browser refused/blocked the copy.</summary>
    public async Task<bool> CopyToClipboardAsync(string text)
    {
        return await jsRuntime.InvokeAsync<bool>($"{InteropNamespace}.copyToClipboard", text);
    }
}
