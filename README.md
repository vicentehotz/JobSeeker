# Job Seeker

Single-page MVP for AI-assisted job matching.

## What is implemented

- .NET 8 minimal API endpoint at `/api/search` for resume upload and job ranking.
- RSS ingestion from configurable public job feeds.
- PDF and DOCX resume parsing.
- Heuristic ranking fallback when an AI provider is not configured.
- Angular 18 responsive landing page with loading, success, empty, and error states.

## Run locally

### Backend

```powershell
dotnet run --project JobSeeker.Api
```

The API defaults to `http://localhost:5105` and `https://localhost:7066` in development.

### Frontend

```powershell
Set-Location job-seeker-web
npm install
npm start
```

The Angular app calls the backend at `http://localhost:5105` by default.

## AI configuration

Update `JobSeeker.Api/appsettings.json` or `JobSeeker.Api/appsettings.Development.json`:

- Set `Ai.Enabled` to `true`
- Set `Ai.ApiKey`
- Adjust `Ai.Endpoint` and `Ai.Model` for your provider

If AI is disabled or unavailable, the API automatically falls back to deterministic keyword-based ranking so the MVP remains usable.

## Current feed configuration

Configured in `JobSeeker.Api/appsettings*.json`:

- Remote OK
- We Work Remotely
- HN Jobs
- Indeed (Bing RSS)
- Greenhouse (Bing RSS)
- Python.org Jobs
- Django Jobs
- Jobicy
- Smashing Magazine Jobs
- Reddit RemoteJS

## Roadmap

1. Add focused backend and Angular tests for invalid files, empty results, and feed failures.
2. Replace the current PDF extraction package with a cleaner dependency to remove the remaining restore warnings.

## Verification

- `dotnet build JobSeeker.sln`
- `dotnet test JobSeeker.sln`
- `npm run build` in `job-seeker-web`

## Known limitation

The current PDF extraction package restores with NuGet warnings because the available feed resolves custom transitive `PdfPig` packages. The solution builds successfully, but this dependency should be normalized later if you want a fully warning-free restore.