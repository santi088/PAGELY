using System.ComponentModel;
using System.Text.Json;
using PAGELY.ViewModels;

namespace PAGELY.Views;

[QueryProperty(nameof(BookId), nameof(BookId))]
public partial class ReaderPage : ContentPage
{
    private readonly ReaderViewModel _viewModel;
    private int _lastPdfPage = -1;

    public ReaderPage(ReaderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.PdfJumpRequested += OnPdfJumpRequested;
        _viewModel.MarkRemoved += OnMarkRemoved;
        _viewModel.PinRemoved += OnPinRemoved;
    }

    public string BookId { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // The book opens full screen: bars and menu stay hidden until the user taps.
        _viewModel.AreBarsVisible = false;
        _viewModel.IsMenuOpen = false;
        _lastPdfPage = -1;
        Shell.SetTabBarIsVisible(this, false);

        if (int.TryParse(BookId, out var id))
        {
            await _viewModel.LoadAsync(id);

            if (_viewModel.IsPdfMode && _viewModel.CurrentPdfPage > 0)
            {
                PdfReaderView.ScrollToPage(_viewModel.CurrentPdfPage);
            }
        }
    }

    private void OnPdfPageScrolled(object? sender, Controls.PdfPageScrolledEventArgs e)
    {
        _viewModel.UpdatePdfPage(e.PageIndex);

        // Scrolling on to another page puts the bars away again.
        if (_lastPdfPage != -1 &&
            e.PageIndex != _lastPdfPage &&
            _viewModel.AreBarsVisible &&
            !_viewModel.IsMenuOpen)
        {
            _viewModel.AreBarsVisible = false;
        }

        _lastPdfPage = e.PageIndex;
    }

    protected override async void OnDisappearing()
    {
        if (!_viewModel.IsPdfMode)
        {
            var percent = await ReadScrollPercentAsync();

            await _viewModel.PersistPositionAsync(
                percent,
                recordHistory: true);

            await _viewModel.MarkFinishedIfNeededAsync(percent);
        }
        else
        {
            await _viewModel.PersistPdfPositionAsync(
                recordHistory: true);
        }

        base.OnDisappearing();
    }

    protected override bool OnBackButtonPressed()
    {
        // The phone's back button closes the menu first.
        if (_viewModel.IsMenuOpen)
        {
            _viewModel.CloseMenu();
            return true;
        }

        return base.OnBackButtonPressed();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        if (_viewModel is IDisposable d)
        {
            d.Dispose();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // The app's tab bar follows the reader's bars: hidden while reading.
        if (e.PropertyName == nameof(ReaderViewModel.AreBarsVisible))
        {
            Shell.SetTabBarIsVisible(this, _viewModel.AreBarsVisible);
        }
    }

    private void OnPdfJumpRequested(object? sender, int page)
    {
        PdfReaderView.ScrollToPage(page);
    }

    private async void OnMarkRemoved(object? sender, int id)
    {
        try
        {
            await ReaderView.EvaluateJavaScriptAsync($"pagelyRemove({id});");
        }
        catch
        {
            // The mark disappears the next time the chapter loads.
        }
    }

    private async void OnPinRemoved(object? sender, string key)
    {
        try
        {
            await ReaderView.EvaluateJavaScriptAsync($"pagelyRemovePin('{key}');");
        }
        catch
        {
            // The pin disappears the next time the chapter loads.
        }
    }

    // ---- Messages from the book page ----

    /// <summary>
    /// The book page talks to the app with pagely:// addresses: toggle (a tap),
    /// save (highlight, underline or note on selected text), mark (a tap on a mark)
    /// and pin (a tap on a bookmark or note marker). They are cancelled here so the
    /// page itself never navigates.
    /// </summary>
    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        const string prefix = "pagely://";
        if (!e.Url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        e.Cancel = true;

        var rest = e.Url[prefix.Length..];
        var queryStart = rest.IndexOf('?');
        var action = (queryStart >= 0 ? rest[..queryStart] : rest).Trim('/').ToLowerInvariant();
        var query = queryStart >= 0
            ? ParseQuery(rest[(queryStart + 1)..])
            : new Dictionary<string, string>();

        try
        {
            switch (action)
            {
                case "toggle":
                    _viewModel.ToggleBars();
                    break;
                case "save":
                    await HandleSaveAsync(query);
                    break;
                case "mark":
                    await HandleMarkTapAsync(query);
                    break;
                case "pin":
                    await HandlePinTapAsync(query);
                    break;
            }
        }
        catch
        {
            // A failed action must never crash the reader.
        }
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = pair.IndexOf('=');
            var key = eq >= 0 ? pair[..eq] : pair;
            var value = eq >= 0 ? pair[(eq + 1)..] : string.Empty;

            try
            {
                value = Uri.UnescapeDataString(value);
            }
            catch
            {
                // Keep the raw value.
            }

            result[key] = value;
        }

        return result;
    }

    private async Task HandleSaveAsync(Dictionary<string, string> query)
    {
        if (_viewModel.IsPdfMode)
            return;

        var kind = query.GetValueOrDefault("kind") ?? "Highlight";
        var color = query.GetValueOrDefault("color") ?? string.Empty;
        var text = query.GetValueOrDefault("text") ?? string.Empty;
        var offset = int.TryParse(query.GetValueOrDefault("offset"), out var parsed) ? parsed : -1;

        if (string.IsNullOrWhiteSpace(text))
            return;

        if (kind != "Highlight" && kind != "Underline" && kind != "Note")
            kind = "Highlight";

        if (kind != "Note" && string.IsNullOrWhiteSpace(color))
            color = "Yellow";

        var noteText = string.Empty;
        if (kind == "Note")
        {
            var entered = await DisplayPromptAsync(
                "Add note",
                $"Selected text:\n“{Shorten(text, 120)}”\n\nWrite your note:",
                "Save",
                "Cancel",
                maxLength: 500);

            if (string.IsNullOrWhiteSpace(entered))
                return;

            noteText = entered.Trim();
        }

        var percent = await ReadScrollPercentAsync();
        var annotation = await _viewModel.AddMarkAsync(kind, color, text, offset, noteText, percent);
        if (annotation is null)
            return;

        var json = _viewModel.BuildMarksJson(new[] { annotation });
        var drawn = await ReaderView.EvaluateJavaScriptAsync($"pagelyApply({json});");

        if (!int.TryParse(drawn, out var count) || count < 1)
        {
            await DisplayAlert(
                "Saved",
                "It is saved in the menu, but it could not be drawn on this page.",
                "OK");
        }
    }

    private async Task HandleMarkTapAsync(Dictionary<string, string> query)
    {
        if (!int.TryParse(query.GetValueOrDefault("id"), out var id))
            return;

        var annotation = await _viewModel.GetAnnotationAsync(id);
        if (annotation is null)
            return;

        var title = annotation.Kind == "Note"
            ? $"Note: {annotation.NoteText}"
            : $"{annotation.Kind} · {annotation.Color}";

        var choice = await DisplayActionSheet(Shorten(title, 200), "Close", "Delete");
        if (choice == "Delete")
        {
            await _viewModel.RemoveAnnotationAsync(annotation);
        }
    }

    private async Task HandlePinTapAsync(Dictionary<string, string> query)
    {
        var key = query.GetValueOrDefault("key") ?? string.Empty;
        var dash = key.IndexOf('-');
        if (dash <= 0 || !int.TryParse(key[(dash + 1)..], out var id))
            return;

        var type = key[..dash];

        if (type == "bookmark")
        {
            var bookmark = await _viewModel.GetBookmarkAsync(id);
            if (bookmark is null)
                return;

            var choice = await DisplayActionSheet(
                Shorten($"Bookmark: {bookmark.Label}", 200), "Close", "Delete");
            if (choice == "Delete")
            {
                await _viewModel.RemoveBookmarkAsync(bookmark);
            }
        }
        else if (type == "note")
        {
            var note = await _viewModel.GetAnnotationAsync(id);
            if (note is null)
                return;

            var choice = await DisplayActionSheet(
                Shorten($"Note: {note.NoteText}", 200), "Close", "Delete");
            if (choice == "Delete")
            {
                await _viewModel.RemoveAnnotationAsync(note);
            }
        }
    }

    private void OnWebViewNavigated(
        object? sender,
        WebNavigatedEventArgs e)
    {
        // The page draws its own marks and restores its own position as it loads,
        // so nothing needs to happen here.
    }

    private async Task<double> ReadScrollPercentAsync()
    {
        try
        {
            var result = await ReaderView.EvaluateJavaScriptAsync(
                "(document.body.scrollHeight <= window.innerHeight) ? 100 : (window.scrollY / (document.body.scrollHeight - window.innerHeight) * 100)");

            return double.TryParse(
                result,
                out var value)
                ? Math.Clamp(value, 0, 100)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // ---- Bookmarks and notes from the menu ----

    private async void OnBookmarkClicked(object? sender, EventArgs e)
    {
        double percent = 0;
        int? offset = null;
        string? snippet = null;

        if (!_viewModel.IsPdfMode)
        {
            percent = await ReadScrollPercentAsync();
            (offset, snippet) = await ReadTopTextAsync();
        }

        var (message, created) = await _viewModel.AddBookmarkAsync(percent, offset, snippet);
        await _viewModel.RefreshSavedAsync();

        if (created?.TextOffset is int pinOffset && pinOffset >= 0)
        {
            var pin = _viewModel.BuildPinJson("bookmark", created.Id, pinOffset);
            await ReaderView.EvaluateJavaScriptAsync($"pagelyAddPins({pin});");
        }

        await DisplayAlert("Bookmark", message, "OK");
    }

    private async void OnNoteClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Book is null)
            return;

        var text = await DisplayPromptAsync(
            "Add note",
            "Write your note for this spot.",
            "Save",
            "Cancel",
            maxLength: 500);

        if (string.IsNullOrWhiteSpace(text))
            return;

        double percent = 0;
        int? offset = null;

        if (!_viewModel.IsPdfMode)
        {
            percent = await ReadScrollPercentAsync();
            (offset, _) = await ReadTopTextAsync();
        }

        var (message, created) = await _viewModel.AddNoteAsync(text.Trim(), null, percent, offset);
        await _viewModel.RefreshSavedAsync();

        if (created?.TextOffset is int pinOffset && pinOffset >= 0)
        {
            var pin = _viewModel.BuildPinJson("note", created.Id, pinOffset);
            await ReaderView.EvaluateJavaScriptAsync($"pagelyAddPins({pin});");
        }

        await DisplayAlert("Note", message, "OK");
    }

    /// <summary>The text position and first words at the top of the screen.</summary>
    private async Task<(int? Offset, string? Snippet)> ReadTopTextAsync()
    {
        try
        {
            var result = await ReaderView.EvaluateJavaScriptAsync(
                "window.pagelyTop ? window.pagelyTop() : null");
            return ParseTopText(result);
        }
        catch
        {
            return (null, null);
        }
    }

    private static (int? Offset, string? Snippet) ParseTopText(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return (null, null);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return ParseTopText(root.GetString());

            if (root.ValueKind != JsonValueKind.Object)
                return (null, null);

            int? offset = null;
            if (root.TryGetProperty("o", out var o) &&
                o.ValueKind == JsonValueKind.Number &&
                o.GetInt32() >= 0)
            {
                offset = o.GetInt32();
            }

            string? snippet = root.TryGetProperty("t", out var t) ? t.GetString() : null;
            snippet = string.IsNullOrWhiteSpace(snippet) ? null : snippet.Trim();

            return (offset, snippet);
        }
        catch
        {
            return (null, null);
        }
    }

    private static string Shorten(string text, int max) =>
        text.Length <= max ? text : text[..max].TrimEnd() + "…";
}