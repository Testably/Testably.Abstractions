# Testably.Abstractions documentation site

The documentation site for [Testably.Abstractions](https://github.com/Testably/Testably.Abstractions), built with [Docusaurus](https://docusaurus.io/) and published to <https://docs.testably.org>.

## Prerequisites

- Node.js 20 or newer

## Local development

```powershell
npm install
npm run start
```

This starts a dev server on <http://localhost:3000> with live reload.

## Production build

```powershell
npm run build
npm run serve   # serves the static build locally for verification
```

The built site is written to `Docs/pages/build`.

## Deployment

Two workflows publish the docs:

- **`.github/workflows/build.yml`** (`name: Build`) - the main CI workflow runs on every push to `main` and on release tags. Its `build-pages` job (`needs: [ pack ]`) builds and deploys the docs after tests, static analysis and packaging have all succeeded. This is the normal channel for in-repo doc changes.
- **`.github/workflows/pages.yml`** (`name: Pages`) - triggered by `repository_dispatch` events of type `extension-documentation-updated-event`. Extension repos use this to ask the site to rebuild after their own docs change, without going through the full CI build.

Both workflows run the same deploy steps: setup .NET → `./build.sh Pages` (downloads the `Docs/pages/` folder of every extension repo listed in `Pipeline/Build.Pages.cs` into `Docs/pages/docs/extensions/project/<Name>/`) → npm install → docusaurus build → publish to the `gh-pages` branch via [`peaceiris/actions-gh-pages`](https://github.com/peaceiris/actions-gh-pages).

In repository settings, GitHub Pages source must be set to **Deploy from a branch** → `gh-pages` / `(root)`.

The custom domain `docs.testably.org` is configured via the `static/CNAME` file. DNS for that subdomain must point to `testably.github.io` (CNAME record) for the custom domain to resolve.

### Adding an extension project

1. Add an entry to the `projects` dictionary in [`Pipeline/Build.Pages.cs`](../../Pipeline/Build.Pages.cs):
   ```csharp
   { "Testably.Abstractions.MyExtension", "MyExtension" }
   ```
   The key is the GitHub repository name under the `Testably` org; the value is the directory under `Docs/pages/docs/extensions/project/`.
2. In the extension repo, place documentation pages under `Docs/pages/` and an optional `README.md` (the section from the first `##` header onwards is substituted into any local file beginning with `00-` that contains the `{README}` placeholder).
3. From the extension repo's CI, dispatch the `extension-documentation-updated-event` event to this repo so it rebuilds.

## Editing content

- Documentation pages live under `docs/`.
- Top-level navigation order is controlled by `sidebar_position` frontmatter on individual pages and by `_category_.json` in folder roots.
- The landing page is `src/pages/index.tsx`; the feature cards underneath are in `src/components/HomepageFeatures/`.
- Site-wide config (title, navbar, footer) lives in `docusaurus.config.ts`.

When editing example pages under `docs/examples/`, keep them in sync with the source READMEs at the top of the corresponding `Examples/<name>/` folder of the repository.
