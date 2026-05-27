# Mid-Level .NET Engineer — Coding Challenge

## 🏥 **MediTrack: Patient Appointment Management API**

**Difficulty:** Mid-Level (3–7 years experience)
**Time:** 3–4 hours
**Format:** Fix bugs + Implement features + Write tests

---

## The Scenario

You've been handed an **unfinished Patient Appointment Management API** built by an intern who left mid-project. The API manages doctors, patients, and appointments for a small clinic network.

The project compiles, but has **several bugs, performance issues, missing features, and zero tests**. Your job is to get it production-ready.

---

## What's Provided

A working (but broken) .NET 8 Web API with:

| Layer | What Exists | Status |
|-------|------------|--------|
| **Domain Models** | `Doctor`, `Patient`, `Appointment`, `Specialty` | ✅ Complete but has a modeling issue |
| **DbContext** | `MediTrackDbContext` with seed data | ⚠️ Has configuration bugs |
| **Services** | `DoctorService`, `AppointmentService`, `PatientService` | 🐛 Multiple bugs |
| **Controllers** | `DoctorsController`, `AppointmentsController` | ⚠️ Partially implemented |
| **DTOs** | Request/Response DTOs | ✅ Complete |
| **Validation** | FluentValidation validators | 🚧 Stubs only |
| **Tests** | Test project with stubs | 🚧 Need implementation |

---

## Your Tasks

### 🐛 Task 1: Find & Fix Bugs (45 min)

The code has **6 intentional bugs**. Find and fix them all.

---

### 🏗️ Task 2: Implement Missing Features (60 min)

#### 2a. Implement `PatientsController`
The patients controller is empty. Implement these endpoints:
- `GET /api/patients` — List all patients (with pagination: `page`, `pageSize` query params)
- `GET /api/patients/{id}` — Get patient by ID (include appointment history)
- `POST /api/patients` — Create a new patient
- `PUT /api/patients/{id}` — Update patient details
- `DELETE /api/patients/{id}` — Soft-delete a patient (set `IsActive = false`)

#### 2b. Implement Search Endpoint
- `GET /api/doctors/search?specialty=Cardiology&availableOn=2025-03-20`
  - Search doctors by specialty (optional)
  - Filter by those who have available slots on a given date (optional)
  - Return results sorted by name

#### 2c. Implement Appointment Rescheduling
- `PUT /api/appointments/{id}/reschedule`
  - Accept a new `DateTime`
  - Validate the new slot is available (no double-booking)
  - Only allow rescheduling appointments that are in `Scheduled` status
  - Keep a record of the original time (add `OriginalDateTime` field if needed)

---

### ✅ Task 3: Add Validation (30 min)

Complete the `FluentValidation` validators:

- **`CreatePatientValidator`**: 
  - `FirstName` and `LastName` required, max 100 chars
  - `Email` must be valid email format
  - `DateOfBirth` must be in the past, patient must be at least 1 year old
  - `Phone` must match pattern `XXX-XXX-XXXX`

- **`BookAppointmentValidator`**:
  - `PatientId` and `DoctorId` must be > 0
  - `AppointmentDateTime` must be in the future
  - `AppointmentDateTime` must be within business hours (Mon–Fri, 8AM–5PM)
  - `Reason` is required, 10–500 characters

---

### 🧪 Task 4: Write Unit Tests (45 min)

Write tests for the following using **xUnit + Moq** (or NSubstitute):

1. **`AppointmentService.BookAppointmentAsync`**
   - Happy path: booking succeeds
   - Booking fails when slot already taken (double-booking prevention)
   - Booking fails when doctor doesn't exist
   - Booking fails when patient doesn't exist

2. **`DoctorService.GetDoctorsWithAvailabilityAsync`**
   - Returns doctors with correct appointment counts
   - Handles empty list (no doctors)

3. **`CreatePatientValidator`**
   - Valid patient passes
   - Missing first name fails
   - Future date of birth fails
   - Invalid email fails

4. **`AppointmentService.CancelAppointmentAsync`**
   - Successfully cancels a scheduled appointment
   - Fails when appointment is already completed

---

### 🎯 Task 5: Add a Simple Caching Layer (30 min)

The `GetDoctorsWithAvailabilityAsync` method is called frequently. Add **in-memory caching** using `IMemoryCache`:

- Cache the full doctor list for **5 minutes**
- Invalidate the cache when a new appointment is booked or cancelled
- Write a test proving cache invalidation works

---

## Evaluation Criteria

| Criteria | Weight | What We Look For |
|----------|--------|-----------------|
| **Bug Identification & Fixes** | 25% | Found all 6 bugs? Fixes are correct? |
| **Feature Implementation** | 25% | Endpoints work correctly, proper HTTP status codes, pagination works |
| **Code Quality** | 15% | Clean code, SOLID principles, proper abstractions, consistent style |
| **Validation** | 10% | Comprehensive validation rules, good error messages |
| **Unit Tests** | 15% | Tests are meaningful (not just coverage), good assertions, edge cases |
| **Caching** | 10% | Correct cache-aside pattern, proper invalidation |

---

## Rules

1. You may add new files, classes, or interfaces as needed
2. Do NOT change the DTOs (treat them as a contract with frontend)
3. You may add NuGet packages if justified (document why)
4. Commit frequently with meaningful messages
5. Ask clarifying questions if something is ambiguous — that's a positive signal

---

## How to Run

```bash
# Restore and build
dotnet build MediTrack.sln

# Run the API (uses in-memory SQLite by default)
dotnet run --project src/MediTrack.Api

# Run tests
dotnet test tests/MediTrack.Tests

# Swagger UI
# Navigate to https://localhost:5001/swagger
```

---

## Bonus (if time permits)

- Add **global exception handling middleware** that returns consistent error responses
- Add **API response wrapper** (`ApiResponse<T>` with `Success`, `Data`, `Errors` properties)
- Add an **integration test** using `WebApplicationFactory`
- Add **Serilog** structured logging configuration

---

## What This Tests

| Skill | How It's Tested |
|-------|----------------|
| C# / .NET fundamentals | Bug fixes, service implementation |
| ASP.NET Core Web API | Controller implementation, routing, status codes |
| Entity Framework Core | Query optimization (N+1), LINQ, DbContext |
| REST API design | Proper HTTP verbs, pagination, error responses |
| SOLID principles | Service/controller separation, DI usage |
| Debugging skills | Finding and fixing 6 bugs |
| Unit testing | xUnit + mocking, test design |
| Validation | FluentValidation rules |
| Caching | IMemoryCache, cache invalidation |
| Concurrency awareness | Double-booking race condition |
| Structured logging | Fixing string interpolation anti-pattern |

Good luck! 🚀
