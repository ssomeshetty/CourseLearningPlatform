# Course Learning Platform Angular frontend

A small Angular application split into a few clear layers:

- `src/app/core/models`: types shared between pages and API services.
- `src/app/core/services`: HTTP access and API services for accounts, courses, enrollments, and users.
- `src/app/features`: the auth, courses, learner, and admin-user page components.
- `src/app/shared/components`: reusable course card, pagination, and alert components.
- `src/app`: root component that places the features on the page and passes session/events between them.

Run the backend with its HTTPS profile. Then from `client-app`, run `npm start` and open `http://localhost:4200`. The API base URL is in `src/app/core/services/api.service.ts`. Backend CORS allows the local Angular origin.
