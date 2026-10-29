using System.Text.RegularExpressions;
using PAGELY.Models;
using VersOne.Epub;

namespace PAGELY.Services;

public record BookParseResult(
    string Title,
    string Author,
    string Genre,
    int ChapterCount,
    byte[]? CoverBytes,
    List<string> Chapters);

public class BookContentService
{
    public async Task<BookParseResult> ParseAsync(string filePath, string fileType, string? originalFileName = null)
    {
        return fileType.ToLowerInvariant() switch
        {
            "epub" => await ParseEpubAsync(filePath),
            "html" or "htm" => await ParsePlainAsync(filePath, wrapHtml: false),
            "pdf" => ParsePdf(filePath, originalFileName),
            _ => await ParsePlainAsync(filePath, wrapHtml: true)
        };
    }

    public async Task<List<string>> LoadChaptersAsync(Book book)
    {
        try
        {
            if (!File.Exists(book.FilePath))
            {
                return ["<p>This file is no longer on the device. Import it again from the library.</p>"];
            }

            if (string.Equals(book.FileType, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ["__PDF_VIEWER__"];
            }

            var parsed = await ParseAsync(book.FilePath, book.FileType);
            return parsed.Chapters.Count == 0
                ? ["<p>No readable text was found in this file.</p>"]
                : parsed.Chapters;
        }
        catch (Exception ex)
        {
            return [$"<p>Error loading book content: {ex.Message}</p>"];
        }
    }

    /// <summary>
    /// Wraps book HTML in the reader's stylesheet. The colors, text size, line
    /// spacing and font come from the Settings page. <paramref name="initJson"/>
    /// carries the chapter's saved marks and the position to open at, so the page
    /// draws its own highlights as it loads.
    /// </summary>
    public static string WrapHtml(string body, string? initJson = null)
    {
        var p = ReaderSettings.GetPalette();
        var size = ReaderSettings.FontSize.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        var spacing = ReaderSettings.LineSpacing.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        var font = ReaderSettings.FontStack;
        var init = string.IsNullOrWhiteSpace(initJson) ? "null" : initJson;

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <style>
            * { -webkit-tap-highlight-color: transparent; }
            html, body {
              margin: 0;
              padding: 0;
              background: {{p.Background}};
              color: {{p.Text}};
            }
            body {
              font-family: {{font}};
              font-size: {{size}}px;
              line-height: {{spacing}};
              padding: 22px 22px 64px;
              max-width: 42em;
              margin: 0 auto;
              -webkit-text-size-adjust: 100%;
              text-rendering: optimizeLegibility;
              word-wrap: break-word;
              overflow-wrap: break-word;
            }
            p { margin: 0 0 1.15em; }
            h1, h2, h3, h4 { font-weight: 600; line-height: 1.3; margin: 1.6em 0 0.6em; }
            h1:first-child, h2:first-child, h3:first-child, h4:first-child { margin-top: 0; }
            a { color: {{p.Link}}; text-decoration: underline; }
            img, svg, video {
              max-width: 100%;
              height: auto;
              display: block;
              margin: 1.2em auto;
              border-radius: 8px;
            }
            blockquote {
              margin: 1.4em 0;
              padding: 0.1em 0 0.1em 1em;
              border-left: 3px solid {{p.Accent}};
              color: {{p.Quote}};
            }
            hr { border: 0; border-top: 1px solid {{p.Rule}}; margin: 2em 0; }
            ul, ol { padding-left: 1.3em; margin: 0 0 1.15em; }
            li { margin: 0.3em 0; }
            pre {
              white-space: pre-wrap;
              font-family: inherit;
              font-size: 1em;
              line-height: inherit;
              margin: 0 0 1.15em;
            }
            code, kbd, samp { font-family: Menlo, Consolas, monospace; font-size: 0.9em; }
            table { width: 100%; border-collapse: collapse; margin: 1.2em 0; }
            th, td { padding: 6px 8px; border-bottom: 1px solid {{p.Rule}}; text-align: left; }

            mark.pm { color: inherit; padding: 0; }
            span.pm-pin { cursor: pointer; font-size: 0.8em; margin: 0 2px; }
            @keyframes pmflash {
              0% { outline: 3px solid rgba(255, 140, 0, 0.95); outline-offset: 2px; }
              100% { outline: 3px solid rgba(255, 140, 0, 0); outline-offset: 2px; }
            }
            mark.pm-flash { animation: pmflash 1.8s ease-out 1; }

            .pagely-tb {
              position: fixed;
              left: 50%;
              bottom: 96px;
              transform: translateX(-50%);
              display: none;
              flex-direction: column;
              align-items: center;
              gap: 8px;
              padding: 10px 14px;
              border-radius: 22px;
              background: {{p.Text}};
              color: {{p.Background}};
              font-family: Roboto, Arial, sans-serif;
              font-size: 13px;
              line-height: 1.2;
              box-shadow: 0 4px 18px rgba(0, 0, 0, 0.35);
              z-index: 9999;
              -webkit-user-select: none;
              user-select: none;
            }
            .pagely-row { display: flex; align-items: center; gap: 8px; }
            .pagely-tb button {
              background: transparent;
              border: 0;
              color: inherit;
              font-size: 13px;
              padding: 6px 10px;
              border-radius: 14px;
            }
            .pagely-tb button.on { background: rgba(127, 127, 127, 0.4); }
            .pagely-tb button.pagely-dot {
              width: 26px;
              height: 26px;
              padding: 0;
              border-radius: 13px;
              border: 2px solid rgba(255, 255, 255, 0.8);
            }
          </style>
        </head>
        <body>{{body}}
          <script>
            (function () {
              var INIT = {{init}};
              var HL = { Yellow: 'rgba(255,212,59,0.5)', Green: 'rgba(81,207,102,0.45)', Pink: 'rgba(247,131,172,0.5)', Blue: 'rgba(77,171,247,0.45)', Orange: 'rgba(255,169,77,0.5)' };
              var UL = { Yellow: '#F59F00', Green: '#2F9E44', Pink: '#D6336C', Blue: '#1C7ED6', Orange: '#E8590C' };
              var SOLID = { Yellow: '#FFD43B', Green: '#51CF66', Pink: '#F783AC', Blue: '#4DABF7', Orange: '#FFA94D' };
              var NAMES = ['Yellow', 'Green', 'Pink', 'Blue', 'Orange'];

              function isSkipped(node) {
                var p = node.parentNode;
                while (p && p !== document.body) {
                  var n = p.nodeName;
                  if (n === 'SCRIPT' || n === 'STYLE') { return true; }
                  if (p.classList && (p.classList.contains('pagely-tb') || p.classList.contains('pm-pin'))) { return true; }
                  p = p.parentNode;
                }
                return false;
              }

              // Every visible character of the page with spaces collapsed, and where each one lives.
              function buildIndex() {
                var walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, null, false);
                var nodes = [];
                var text = '';
                var map = [];
                var lastSpace = true;
                var n;
                while ((n = walker.nextNode())) {
                  if (isSkipped(n)) { continue; }
                  var ni = nodes.length;
                  nodes.push(n);
                  var s = n.nodeValue;
                  for (var j = 0; j < s.length; j++) {
                    var c = s.charAt(j);
                    if (/\s/.test(c)) {
                      if (lastSpace) { continue; }
                      c = ' ';
                      lastSpace = true;
                    } else {
                      lastSpace = false;
                    }
                    text += c;
                    map.push([ni, j]);
                  }
                }
                return { nodes: nodes, text: text, map: map };
              }

              function posFromRange(idx, node, off) {
                var ni = idx.nodes.indexOf(node);
                if (ni < 0) { return -1; }
                for (var p = 0; p < idx.map.length; p++) {
                  var m = idx.map[p];
                  if (m[0] > ni || (m[0] === ni && m[1] >= off)) { return p; }
                }
                return -1;
              }

              function rangeOfNode(node) {
                var rg = document.createRange();
                rg.selectNodeContents(node);
                return rg;
              }

              function styleMark(m, kind, color) {
                m.className = 'pm';
                if (kind === 'Underline') {
                  m.style.backgroundColor = 'transparent';
                  m.style.textDecoration = 'underline';
                  m.style.textDecorationColor = UL[color] || '#E8590C';
                  m.style.textDecorationThickness = '2px';
                  m.style.textUnderlineOffset = '3px';
                } else if (kind === 'Note') {
                  m.style.backgroundColor = 'transparent';
                  m.style.textDecoration = 'underline dotted';
                  m.style.textDecorationColor = '#A98654';
                  m.style.textDecorationThickness = '2px';
                  m.style.textUnderlineOffset = '3px';
                } else {
                  m.style.backgroundColor = HL[color] || HL.Yellow;
                  m.style.borderRadius = '2px';
                }
              }

              function markRange(idx, start, length, kind, color, id) {
                var first = idx.map[start];
                var last = idx.map[start + length - 1];
                if (!first || !last) { return false; }
                for (var ni = first[0]; ni <= last[0]; ni++) {
                  var node = idx.nodes[ni];
                  var s = (ni === first[0]) ? first[1] : 0;
                  var e = (ni === last[0]) ? last[1] + 1 : node.nodeValue.length;
                  if (e <= s) { continue; }
                  var rg = document.createRange();
                  rg.setStart(node, s);
                  rg.setEnd(node, e);
                  var m = document.createElement('mark');
                  m.setAttribute('data-id', id);
                  styleMark(m, kind, color);
                  rg.surroundContents(m);
                }
                return true;
              }

              function applyOne(a) {
                if (!a || !a.text) { return false; }
                if (document.querySelector('mark[data-id="' + a.id + '"]')) { return true; }
                var q = String(a.text).replace(/\u2026$/, '');
                if (!q) { return false; }
                var idx = buildIndex();
                var pos = -1;
                if (typeof a.offset === 'number' && a.offset >= 0 && idx.text.substr(a.offset, q.length) === q) {
                  pos = a.offset;
                }
                if (pos < 0) { pos = idx.text.indexOf(q); }
                if (pos < 0) { return false; }
                return markRange(idx, pos, q.length, a.kind, a.color, a.id);
              }

              function pinChar(type) {
                return type === 'bookmark' ? '\uD83D\uDD16' : '\uD83D\uDCDD';
              }

              function addPin(p) {
                if (!p) { return false; }
                var key = p.type + '-' + p.id;
                if (document.querySelector('span.pm-pin[data-key="' + key + '"]')) { return true; }
                var idx = buildIndex();
                if (p.offset < 0 || p.offset >= idx.map.length) { return false; }
                var m = idx.map[p.offset];
                var node = idx.nodes[m[0]];
                var after = node.splitText(m[1]);
                var span = document.createElement('span');
                span.className = 'pm-pin';
                span.setAttribute('data-key', key);
                span.textContent = pinChar(p.type);
                node.parentNode.insertBefore(span, after);
                return true;
              }

              window.pagelyApply = function (list) {
                var count = 0;
                if (!list) { return count; }
                for (var i = 0; i < list.length; i++) {
                  try { if (applyOne(list[i])) { count++; } } catch (err) { }
                }
                return count;
              };

              window.pagelyAddPins = function (list) {
                var count = 0;
                if (!list) { return count; }
                for (var i = 0; i < list.length; i++) {
                  try { if (addPin(list[i])) { count++; } } catch (err) { }
                }
                return count;
              };

              window.pagelyRemove = function (id) {
                var ms = document.querySelectorAll('mark[data-id="' + id + '"]');
                for (var i = 0; i < ms.length; i++) {
                  var m = ms[i];
                  var parent = m.parentNode;
                  while (m.firstChild) { parent.insertBefore(m.firstChild, m); }
                  parent.removeChild(m);
                  parent.normalize();
                }
              };

              window.pagelyRemovePin = function (key) {
                var ps = document.querySelectorAll('span.pm-pin[data-key="' + key + '"]');
                for (var i = 0; i < ps.length; i++) {
                  var parent = ps[i].parentNode;
                  parent.removeChild(ps[i]);
                  parent.normalize();
                }
              };

              window.pagelyFocus = function (id, quiet) {
                var ms = document.querySelectorAll('mark[data-id="' + id + '"]');
                if (!ms.length) { return false; }
                ms[0].scrollIntoView({ block: 'center' });
                if (!quiet) {
                  for (var i = 0; i < ms.length; i++) { ms[i].classList.add('pm-flash'); }
                  setTimeout(function () {
                    for (var k = 0; k < ms.length; k++) { ms[k].classList.remove('pm-flash'); }
                  }, 2000);
                }
                return true;
              };

              window.pagelyGoto = function (off) {
                var idx = buildIndex();
                if (off < 0 || off >= idx.map.length) { return false; }
                var m = idx.map[off];
                var node = idx.nodes[m[0]];
                var rg = document.createRange();
                rg.setStart(node, Math.min(m[1], node.nodeValue.length));
                rg.setEnd(node, Math.min(m[1] + 1, node.nodeValue.length));
                var rect = rg.getBoundingClientRect();
                window.scrollTo(0, Math.max(0, rect.top + window.scrollY - 24));
                return true;
              };

              // Where the screen starts: used by bookmarks and notes.
              window.pagelyTop = function () {
                var idx = buildIndex();
                var pos = -1;
                var r = document.caretRangeFromPoint
                  ? document.caretRangeFromPoint(Math.floor(window.innerWidth / 2), 28)
                  : null;
                if (r && r.startContainer && r.startContainer.nodeType === 3) {
                  pos = posFromRange(idx, r.startContainer, r.startOffset);
                }
                if (pos < 0) {
                  for (var i = 0; i < idx.nodes.length; i++) {
                    var rect = rangeOfNode(idx.nodes[i]).getBoundingClientRect();
                    if (rect.height > 0 && rect.bottom > 8) {
                      pos = posFromRange(idx, idx.nodes[i], 0);
                      break;
                    }
                  }
                }
                if (pos < 0) { return { o: -1, t: '' }; }
                return { o: pos, t: idx.text.substr(pos, 90) };
              };

              // ---- Selection toolbar ----
              var mode = 'Highlight';
              var lastSel = null;
              var tbActive = false;
              var hadSel = false;
              var selTimer = null;
              var hideTimer = null;

              var tb = document.createElement('div');
              tb.className = 'pagely-tb';
              var row1 = document.createElement('div');
              row1.className = 'pagely-row';
              var row2 = document.createElement('div');
              row2.className = 'pagely-row';

              function makeButton(label) {
                var b = document.createElement('button');
                b.type = 'button';
                b.textContent = label;
                return b;
              }

              var bH = makeButton('Highlight');
              var bU = makeButton('Underline');
              var bN = makeButton('Note');
              bH.className = 'on';

              function setMode(m) {
                mode = m;
                bH.className = (m === 'Highlight') ? 'on' : '';
                bU.className = (m === 'Underline') ? 'on' : '';
              }

              function commit(kind, color) {
                if (!lastSel) { return; }
                var q = lastSel.text;
                var off = lastSel.offset;
                lastSel = null;
                tbActive = false;
                hadSel = false;
                tb.style.display = 'none';
                var s = window.getSelection();
                if (s) { s.removeAllRanges(); }
                window.location.href = 'pagely://save?kind=' + kind + '&color=' + (color || '') + '&offset=' + off + '&text=' + encodeURIComponent(q);
              }

              bH.addEventListener('click', function (e) { e.stopPropagation(); setMode('Highlight'); });
              bU.addEventListener('click', function (e) { e.stopPropagation(); setMode('Underline'); });
              bN.addEventListener('click', function (e) { e.stopPropagation(); commit('Note', ''); });

              row1.appendChild(bH);
              row1.appendChild(bU);
              row1.appendChild(bN);

              for (var k = 0; k < NAMES.length; k++) {
                (function (name) {
                  var d = document.createElement('button');
                  d.type = 'button';
                  d.className = 'pagely-dot';
                  d.style.background = SOLID[name];
                  d.setAttribute('aria-label', name);
                  d.addEventListener('click', function (e) {
                    e.stopPropagation();
                    commit(mode, name);
                  });
                  row2.appendChild(d);
                })(NAMES[k]);
              }

              tb.appendChild(row1);
              tb.appendChild(row2);
              document.body.appendChild(tb);

              tb.addEventListener('touchstart', function () {
                tbActive = true;
                if (hideTimer) { clearTimeout(hideTimer); hideTimer = null; }
                setTimeout(function () { tbActive = false; }, 1500);
              }, true);

              function scheduleHide() {
                if (hideTimer) { clearTimeout(hideTimer); }
                hideTimer = setTimeout(function () {
                  if (!tbActive) {
                    tb.style.display = 'none';
                    lastSel = null;
                  }
                }, 500);
              }

              function readSelection() {
                var sel = window.getSelection();
                var raw = sel ? sel.toString() : '';
                if (!sel || sel.rangeCount === 0 || !raw || !/\S/.test(raw)) {
                  if (!tbActive) { scheduleHide(); }
                  return;
                }
                var idx = buildIndex();
                var rg = sel.getRangeAt(0);
                var s = posFromRange(idx, rg.startContainer, rg.startOffset);
                var e = posFromRange(idx, rg.endContainer, rg.endOffset);
                var q = '';
                var off = -1;
                if (s >= 0 && e > s) {
                  q = idx.text.substring(s, e);
                  var lead = q.length - q.replace(/^\s+/, '').length;
                  s += lead;
                  q = q.replace(/^\s+|\s+$/g, '');
                  off = s;
                }
                if (!q) {
                  q = raw.replace(/\s+/g, ' ').replace(/^\s+|\s+$/g, '');
                  off = -1;
                }
                if (q.length > 500) { q = q.substring(0, 500); }
                lastSel = { text: q, offset: off };
                if (hideTimer) { clearTimeout(hideTimer); hideTimer = null; }
                tb.style.display = 'flex';
              }

              document.addEventListener('selectionchange', function () {
                if (selTimer) { clearTimeout(selTimer); }
                selTimer = setTimeout(readSelection, 200);
              });

              // ---- Taps ----
              document.addEventListener('touchstart', function () {
                var t = window.getSelection().toString();
                hadSel = (t && t.length > 0) || tb.style.display === 'flex';
              }, true);

              document.addEventListener('click', function (e) {
                if (e.target && e.target.closest && e.target.closest('.pagely-tb')) { return; }
                if (hadSel) {
                  hadSel = false;
                  tb.style.display = 'none';
                  lastSel = null;
                  return;
                }
                var t = window.getSelection().toString();
                if (t && t.length > 0) { return; }
                if (e.target && e.target.closest) {
                  if (e.target.closest('a')) { return; }
                  var pin = e.target.closest('span.pm-pin');
                  if (pin) {
                    window.location.href = 'pagely://pin?key=' + pin.getAttribute('data-key');
                    return;
                  }
                  var m = e.target.closest('mark.pm');
                  if (m) {
                    window.location.href = 'pagely://mark?id=' + m.getAttribute('data-id');
                    return;
                  }
                }
                window.location.href = 'pagely://toggle';
              });

              // ---- Draw saved marks and open at the right place ----
              if (INIT) {
                if (INIT.marks) { window.pagelyApply(INIT.marks); }
                if (INIT.pins) { window.pagelyAddPins(INIT.pins); }
              }

              function settle(quiet) {
                if (!INIT) { return; }
                if (INIT.focus != null && window.pagelyFocus(INIT.focus, quiet)) { return; }
                if (INIT.gotoOffset != null && INIT.gotoOffset >= 0 && window.pagelyGoto(INIT.gotoOffset)) { return; }
                if (INIT.scroll > 0) {
                  window.scrollTo(0, document.body.scrollHeight * (INIT.scroll / 100));
                }
              }

              settle(false);
              window.addEventListener('load', function () {
                setTimeout(function () { settle(true); }, 80);
              });
            })();
          </script>
        </body>
        </html>
        """;
    }

    private static BookParseResult ParsePdf(string filePath, string? originalFileName = null)
    {
        var info = PdfService.ReadInfo(filePath, originalFileName);

        // The page count is not known here: the document has to be opened with the
        // platform PDF engine for that. BookImportService does exactly that right
        // after parsing and overwrites this placeholder chapter count with the real
        // page count from IPdfRendererService.
        return new BookParseResult(info.Title, info.Author, "Uncategorized", 1, info.CoverBytes, ["__PDF_VIEWER__"]);
    }

    private static async Task<BookParseResult> ParseEpubAsync(string filePath)
    {
        var book = await EpubReader.ReadBookAsync(filePath);
        var title = string.IsNullOrWhiteSpace(book.Title) ? Path.GetFileNameWithoutExtension(filePath) : book.Title;
        var author = book.AuthorList is { Count: > 0 }
            ? string.Join(", ", book.AuthorList)
            : (string.IsNullOrWhiteSpace(book.Author) ? "Unknown author" : book.Author);

        var genre = "Uncategorized";
        var subjects = book.Schema?.Package?.Metadata?.Subjects;
        if (subjects is { Count: > 0 } && !string.IsNullOrWhiteSpace(subjects[0].Subject))
        {
            genre = subjects[0].Subject;
        }

        // Pictures live as separate files inside the EPUB. Collect them once so each
        // chapter's <img> tags can be pointed at the real image data.
        var images = BuildImageLookup(book);

        var chapters = new List<string>();
        foreach (var item in book.ReadingOrder)
        {
            var html = item.Content;
            if (string.IsNullOrWhiteSpace(html))
            {
                continue;
            }

            html = ExtractBody(html);
            html = InlineImages(html, item.Key, images);
            chapters.Add(html);
        }

        if (chapters.Count == 0)
        {
            chapters.Add($"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1><p>This EPUB did not include readable HTML chapters.</p>");
        }

        return new BookParseResult(title, author, genre, chapters.Count, book.CoverImage, chapters);
    }

    private sealed class ImageLookup
    {
        public Dictionary<string, (byte[] Data, string Mime)> ByPath { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, (byte[] Data, string Mime)> ByFileName { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }

    private static ImageLookup BuildImageLookup(EpubBook book)
    {
        var lookup = new ImageLookup();

        foreach (var image in book.Content.Images.Local)
        {
            if (image.Content is null || image.Content.Length == 0)
            {
                continue;
            }

            var key = NormalizePath(image.Key);
            var entry = (image.Content, GuessMime(key));
            lookup.ByPath[key] = entry;
            lookup.ByFileName[Path.GetFileName(key)] = entry;
        }

        return lookup;
    }

    /// <summary>Keeps only what is inside &lt;body&gt;, so the reader's own page is not nested in another one.</summary>
    private static string ExtractBody(string html)
    {
        var match = Regex.Match(
            html,
            @"<body[^>]*>(.*?)</body>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : html;
    }

    /// <summary>
    /// Replaces image references (img src and SVG image href) with the picture data
    /// itself, so the web view can show it without needing the EPUB's internal files.
    /// </summary>
    private static string InlineImages(string html, string chapterKey, ImageLookup images)
    {
        var normalizedKey = NormalizePath(chapterKey);
        var slash = normalizedKey.LastIndexOf('/');
        var chapterDir = slash >= 0 ? normalizedKey[..slash] : "";

        string Replace(Match m)
        {
            var src = m.Groups[3].Value;
            if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return m.Value;
            }

            var relative = Uri.UnescapeDataString(src.Split('#')[0]);
            var combined = chapterDir.Length == 0 ? relative : chapterDir + "/" + relative;
            var resolved = NormalizePath(combined);

            if (!images.ByPath.TryGetValue(resolved, out var image) &&
                !images.ByFileName.TryGetValue(Path.GetFileName(resolved), out image))
            {
                return m.Value;
            }

            var quote = m.Groups[2].Value;
            return $"{m.Groups[1].Value}{quote}data:{image.Mime};base64,{Convert.ToBase64String(image.Data)}{quote}";
        }

        const RegexOptions options = RegexOptions.Singleline | RegexOptions.IgnoreCase;
        html = Regex.Replace(html, @"(<img\b[^>]*?\bsrc\s*=\s*)([""'])(.*?)\2", Replace, options);
        html = Regex.Replace(html, @"(<image\b[^>]*?\bhref\s*=\s*)([""'])(.*?)\2", Replace, options);
        return html;
    }

    /// <summary>Turns "Text/../Images/a.jpg" style paths into one clean "Images/a.jpg" form.</summary>
    private static string NormalizePath(string path)
    {
        var parts = new List<string>();
        foreach (var part in path.Replace('\\', '/').Split('/'))
        {
            if (part.Length == 0 || part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }
            }
            else
            {
                parts.Add(part);
            }
        }

        return string.Join("/", parts);
    }

    private static string GuessMime(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

    private static async Task<BookParseResult> ParsePlainAsync(string filePath, bool wrapHtml)
    {
        var text = await File.ReadAllTextAsync(filePath);
        var title = Path.GetFileNameWithoutExtension(filePath);
        var chunks = ChunkText(text);
        var chapters = chunks
            .Select(chunk => wrapHtml
                ? $"<pre>{System.Net.WebUtility.HtmlEncode(chunk)}</pre>"
                : chunk)
            .ToList();

        return new BookParseResult(title, "Unknown author", "Uncategorized", chapters.Count, null, chapters);
    }

    private static List<string> ChunkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ["This file is empty."];
        }

        const int size = 4000;
        var chunks = new List<string>();
        var remaining = text.Replace("\r\n", "\n");
        while (remaining.Length > 0)
        {
            if (remaining.Length <= size)
            {
                chunks.Add(remaining.Trim());
                break;
            }

            var cut = remaining.LastIndexOf('\n', size);
            if (cut < size / 2)
            {
                cut = size;
            }

            chunks.Add(remaining[..cut].Trim());
            remaining = remaining[cut..];
        }

        return chunks.Where(c => c.Length > 0).DefaultIfEmpty("This file is empty.").ToList();
    }
}