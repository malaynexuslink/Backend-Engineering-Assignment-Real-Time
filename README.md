# Real-Time Ride Matching Backend Engine

A scalable, highly concurrent, asynchronous ride-matching backend system built with **ASP.NET Core 8.0**. This solution is designed for cloud-native deployment, focusing on strict concurrency control, non-blocking background processing, spatial efficiency, and mobile-optimized push notifications.

---

## 🏗 Architecture Overview

The system strictly adheres to **Clean Architecture** principles to separate concerns, enforce dependency inversion, and ensure the domain remains isolated from infrastructure volatility.

1. **API / Presentation Layer:** RESTful endpoints for Riders and Drivers.
2. **Application Layer:** Contains Core Business Logic, Data Transfer Objects (DTOs), and Interfaces. No database or external service dependencies exist here.
3. **Domain Layer:** Enums, Entities, and Value Objects (`Coordinates`).
4. **Infrastructure Layer:** Implements data access, spatial search, and external integrations (Firebase).
5. **Background Engine:** A hosted `BackgroundService` that consumes requests via Threading Channels.

### Key Components
* **Database Pipeline:** Uses Entity Framework Core. Currently configured to use an InMemory database to satisfy the 24-hour evaluation constraint with zero-friction setup, but the `DbContext` is perfectly structured to transition to PostgreSQL/SQL Server.
* **Spatial Search:** Custom Haversine trigonometric service backed by an in-memory `ConcurrentDictionary`. This provides $O(1)$ spatial data updates and ultra-fast proximity queries without round-tripping to a database.
* **Push Notifications (FCM):** Replaces WebSocket/SignalR connections with **Firebase Cloud Messaging (FCM)**. This is a deliberate choice for mobile clients to preserve device battery life while ensuring reliable, state-changing wake-up notifications.

---

## ⚡ Scalability & Concurrency Considerations

Designing a ride-matching system inherently introduces complex race conditions (e.g., two riders requesting the exact same driver simultaneously). This solution addresses these challenges head-on:

### 1. Asynchronous Event-Driven Matching
Instead of blocking the HTTP thread while searching for drivers, incoming ride requests are immediately saved as `Requested` and the `RideId` is pushed to a **`System.Threading.Channels.Channel`**. 
* **Benefit:** The API responds in milliseconds (`202 Accepted`). A background worker (`RideMatchingBackgroundWorker`) processes the queue asynchronously, protecting the API layer from traffic spikes.

### 2. Strict Driver Locking (`DriverLockManager`)
To prevent double-booking, the system uses a `ConcurrentDictionary` of `SemaphoreSlim` locks. 
* **Benefit:** When the background worker evaluates candidate drivers, it attempts to acquire a lock on the driver's ID. If another thread is actively evaluating that driver, the worker skips them instantly (`WaitAsync(100ms)`) rather than blocking.

### 3. Resilient Outbound Communications (.NET 8 Polly Pipelines)
Although FCM is implemented directly via the Admin SDK in this iteration, production-grade API-to-API communication (e.g., calling a Maps API or external billing) would utilize **.NET 8 Resilience Pipelines** (`Microsoft.Extensions.Resilience`).
* **Implementation Path:** Standard HTTP factory registrations via `builder.Services.AddHttpClient().AddStandardResilienceHandler()`. This automatically applies a pipeline of Rate Limiting $\rightarrow$ Total Timeout $\rightarrow$ Retry (with Jitter) $\rightarrow$ Circuit Breaker $\rightarrow$ Attempt Timeout, preventing cascading failures across microservices.

### 4. Lightweight Unit of Work (UoW) & EF Core
The `AppDbContext` tracks changes naturally, acting as a Unit of Work. In the background matching engine, the atomic state transition (changing the Driver to `Busy` and the Ride to `Matched`) is wrapped in a single `SaveChangesAsync()` call to maintain data integrity.

---

## 🤔 Assumptions & Trade-offs

* **FCM vs. WebSockets (SignalR):** I chose Firebase Cloud Messaging over SignalR. **Trade-off:** While WebSockets offer sub-millisecond bidirectional communication, keeping persistent sockets open drains mobile batteries and complicates scaling (requiring Redis backplanes). FCM offloads the connection management to the OS, which is the industry standard for ride-hailing state changes (e.g., "Ride Matched").
* **In-Memory Spatial Index:** Used `ConcurrentDictionary` combined with the Haversine formula for finding nearby drivers rather than a geospatial DB extension (like PostGIS). **Trade-off:** This provides blazing-fast reads/writes for driver locations, but if the server restarts, transient driver locations are lost. In production, this would be swapped with **Redis Geospatial Data (`GEOADD`/`GEORADIUS`)**.
* **Driver Location Updates:** FCM is not used for streaming continuous live driver locations due to quota limits and battery drain. In a real-world scenario, the rider app would poll a lightweight, cached `/location` endpoint only while the app is foregrounded.

---

## 🚀 Setup & Run Instructions

### Prerequisites
* .NET 8.0 SDK
* A valid `firebase-adminsdk.json` service account file (for notifications).

### Execution
1. Clone the repository and navigate to the root directory.
2. Ensure you have the FCM credential file: Place your `firebase-adminsdk.json` in the `RideMatchingSystem.API` root directory. *(Note: If this file is missing, the API will still run, but FCM notifications will gracefully fail/log errors without crashing the app).*
3. Run the solution:
   ```bash
   dotnet restore
   dotnet run --project RideMatchingSystem.API
