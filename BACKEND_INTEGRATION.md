# Backend integration notes (frontend phase)

## Service boundaries
- `IOnlineCatalogService` (`MockCatalogService`): Discover catalog. Methods: `GetBooksAsync(genre, sort)`, `GetGenresAsync()`, `GetBookAsync(id)`, `SearchAsync(query)`. Replace mock with HTTP client implementation.
- `IReadingListService` (`LocalReadingListService`): saved Discover books, currently device-local via Preferences. Move to server-backed implementation when accounts exist.
- `DatabaseService`: local SQLite for imported books, reading events, bookmarks, annotations. Must remain local and offline.
- `BookImportService`, `BookContentService`, `IPdfRendererService`: local file handling only.

## Models
- `Book` (local imported) and `DiscoverBook` (catalog metadata) are intentionally separate. Do not conflate.
- Future mapping between local and catalog entries must be explicit (store catalog id on Book if needed).

## Mock data
- `MockCatalogService` provides sample public-domain catalog data and simulated latency. Clearly isolated; swap in `MauiProgram.cs`.

## Remaining backend work
- Authentication/account service and Profile data.
- Community search endpoint and pagination.
- Reviews/ratings endpoints.
- Reading list sync.
- Cover image URLs (`DiscoverBook.CoverUrl`) currently unused.
- Error/offline states are wired in UI; connect to real network status and HTTP errors.
