# RaceDay — Race Event Management System

**Student:** Mandlakazi Sibiya
**Student Number:** ST10473547
**Module:** PROG6212 — Programming 2B
**Assessment:** Portfolio of Evidence (PoE) — Part 1
**Institution:** The Independent Institute of Education (IIE)
**Due Date:** 22 September 2026

## Project Overview

RaceDay is a full-stack, web-based event management system built for the South African road running, walking, and cycling community. It allows Organizers to create and manage events, define age/distance categories, view enrollments, and capture race results. Participants can browse upcoming events, enroll by selecting a category, and track their personal race history.

This repository currently contains **Part 1** of the PoE: system planning. No application code is written in this part — it consists of an Entity Relationship Diagram, a full API endpoint plan, and a SQL Server database script, all of which the Part 2 (API) and Part 3 (MVC) builds will implement against.

## Roles

- **Organizer** — creates, edits, and deletes events; defines categories per event; views all participant enrollments for their events; captures finish times and positions after an event; uploads event banner images.
- **Participant** — browses upcoming events; enrolls in an event by selecting a category; views their own enrollment history; tracks personal race results; uploads a profile picture and updates their profile.

## Repository Structure
/docs
RaceDay_ERD.png - Entity Relationship Diagram (Section A)
RaceDay_Endpoint_Plan.pdf - Full API endpoint plan, covering Part 2 (API) and Part 3 (MVC) (Section B)
RaceDay_Schema.sql - SQL Server schema, constraints, seed data, and verification queries (Section C)
ci-passing-screenshot.png - Screenshot of the passing GitHub Actions CI run
/.github/workflows
docs-check.yml - CI workflow verifying /docs contains the required files
README.md - This file


## Setup Instructions

1. Clone this repository:git clone https://github.com/Mandlakazi07/emkndn-prog6212-2026-prog6212-poe-mandlakazi07.git

2. Install SQL Server (Express edition is sufficient) and SQL Server Management Studio (SSMS).
3. Open `docs/RaceDay_Schema.sql` in SSMS using **File → Open → File** — do not copy-paste the script's text, as this can silently corrupt quotation marks and break string literals.
4. Connect to your local instance (e.g. `.\SQLEXPRESS`) using Windows Authentication and execute the script (F5).
5. The script creates the `RaceDayDB` database, all six tables with full constraints, seed data (2 organizers, 2 participants, 3 events, categories, enrollments, and results), and a set of readable verification queries at the end that join across tables to confirm the data is correctly linked.

## Entity Relationship Diagram

See `docs/RaceDay_ERD.png`. Six entities: **Users, Venues, Events, Categories, Enrollments, Results**. Every relationship shows a verb label, a direction arrow, and cardinality using `1..1` / `1..*` notation on each end. The schema matches `docs/RaceDay_Schema.sql` exactly.

## API Endpoint Plan

See `docs/RaceDay_Endpoint_Plan.pdf`. Covers Authentication, User Profile, Events, Categories, Enrollments, and Results (the Part 2 API), plus the MVC page routes that will consume them in Part 3. Every endpoint lists HTTP method, route, description, role required, request body, and expected response including failure codes.

## CI/CD

A GitHub Actions workflow (`.github/workflows/docs-check.yml`) runs on every push and verifies that `/docs` contains the ERD image, the SQL script, and the endpoint plan document.

![CI passing](docs/ci-passing-screenshot.png)

## Video

A 5–7 minute unlisted YouTube video walks through the ERD (explaining each relationship's entities, direction, and cardinality reasoning), one endpoint per resource group tied to its functional requirement, and a live run of the SQL script in SSMS showing successful execution and seeded data.

**Video link:** _pending_

## AI Disclosure

AI assistance (Deepseek) was used during the planning and documentation process for this submission — specifically for: structuring and formatting the ERD as a draw.io-compatible diagram from a hand-designed entity model; drafting the SQL schema and seed data script based on that entity model; formatting the API endpoint plan document; troubleshooting PowerShell/Git and SSMS configuration errors; and structuring this README.
All entity design decisions, relationship logic, and the overall system design were directed and reviewed by the student. 
This discloses AI use as required by the PoE brief. https://chat.deepseek.com/share/eh9a3yqtfj3spkx0hf 

## References (IIE Harvard Anglia)

Sibiya, M., 2026. *RaceDay: Portfolio of Evidence, Part 1 — System Planning*. Durban: The Independent Institute of Education (IIE).
