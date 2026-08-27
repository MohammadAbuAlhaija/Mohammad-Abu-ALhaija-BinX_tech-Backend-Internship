# Week 6 - Day 5: Sprint Review & Retrospective

## Overview

Today was the final day of Sprint 1.

The main focus was not on adding new features, but on reviewing the work completed during the sprint, demonstrating the API through Postman, checking the sprint backlog, and identifying the remaining work that should be carried forward.

---

## Sprint 1 API Demo

I prepared a Postman demo using the running Version 2 API.

The demo covered the main workflow from authentication to the business logic implemented during the sprint:

- Registered a new demo user.
- Logged in and generated a JWT.
- Browsed patients using pagination.
- Applied filtering and sorting to the Patients endpoint.
- Created a complete Patient Visit.
- Verified that the visit created a new Vital Sign record.
- Tested an invalid future measurement and confirmed that the API rejected it correctly.

### Demo Screenshots

#### Patients Catalog with Pagination

![Patients Catalog Demo](Screenshots/week6-day5-patients-catalog-demo.png)

#### Patients Filter and Sort

![Patients Filter and Sort](Screenshots/week6-day5-patients-filter-sort-demo.png)

#### Patient Visit Created Successfully

![Patient Visit Success](Screenshots/week6-day5-patient-visit-success.png)

#### Patient Visit Result Verified

![Patient Visit Verified](Screenshots/week6-day5-patient-visit-verified.png)

#### Future Measurement Rejected

![Future Measurement Rejected](Screenshots/week6-day5-future-measurement-demo.png)

---

## Sprint Backlog Review

I reviewed the Sprint 1 backlog against the sprint acceptance criteria.

Tasks that were fully completed were marked as **Done**, while incomplete API routes, remaining test coverage, and final consistency checks were carried forward for future development.

The Sprint 1 Pull Request was also reviewed and approved by the mentor, with no unresolved review feedback remaining.

---

## Sprint Retrospective

The retrospective focused on what worked well during Sprint 1 and what could be improved.

One issue identified during development was that some existing automated tests were not updated immediately after changes to the V2 domain model.

### Action for the Next Sprint

After changing a domain model, DTO, or important database relationship, I will:

1. Build the project.
2. Run the automated tests.
3. Fix any failures.
4. Continue with the next feature only after the project is stable.

This should help catch compatibility issues earlier during development.

---

## Migration Review

The migration history was reviewed at the end of the sprint:

```text
20260814163131_InitialCreate
20260814203502_AddIdentity
20260815043443_AddSeedData
20260824090330_BuildFullDataModel