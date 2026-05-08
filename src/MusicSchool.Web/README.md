# MusicSchool.Web

Angular 18+ operational workspace for the Music School modular monolith.

## Commands

```powershell
npm install
npm start
npm run build
npm run extract-i18n
```

The app uses Angular Material, compile-time Angular i18n, and an HTTP interceptor that sends `X-Tenant-Id` to the API. The API base URL is configured in `src/environments/environment.ts`.

The toolbar includes a four-flag language selector for English US, Portuguese Portugal, Portuguese Brazil, and Spanish Spain. The visible application shell uses runtime translations so language changes are applied immediately in the running app and persisted in `localStorage`.

The left navigation includes Teacher Register, which opens the teacher management area for registering, updating, inactivating teachers, and maintaining teacher schedules.

Configured locales:

- `en-US`: source locale.
- `en-GB`: British English build.
- `pt-PT`: Portuguese Portugal build.
- `pt-BR`: Portuguese Brazil build.
- `es-ES`: Spanish Spain build.
