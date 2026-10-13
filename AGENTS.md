# Ticket System Architecture

This project follows an onion architecture. Dependencies point inward:

`Api -> Services -> Domain`

`Infrastructure -> Domain`

The domain project has no project references.

## Domain

Domain contains the business model and repository interfaces.

Domain objects should own business rules whenever possible. For example:

- `Cart` owns cart mutation through `AddItem(...)`.
- `DailyInventory` owns inventory facts such as `AvailableItemCount`.
- `ParkCalendar` owns calendar queries such as monthly inventory and dated product lookup.
- Constructors protect basic invariants such as required ids, positive quantities, and non-negative prices.

Put logic inside domain objects as much as possible. Services and repositories can retrieve domain objects and delegate to domain methods, but filtering, matching, combining, mutation, and availability calculations should live in domain objects when the behavior is part of the business model.

Repositories in domain are interfaces only. They describe persistence needs without deciding how persistence works.

Repository interfaces should be persistence-oriented:

- Retrieve aggregate/domain objects.
- Save aggregate/domain objects.
- Avoid business behavior such as "add item to cart", date filtering, product matching, or availability combining inside repositories.
- Do not add behavior-specific repository methods for one use case. Services should retrieve the needed domain object or aggregate, then call domain methods for business behavior. For example, a park repository should retrieve a `Park`; the service should ask that `Park` for monthly inventory through its calendar instead of asking the repository for a schedule or filtered availability.

## Services

Services reference only domain.

Services coordinate use cases:

- Load domain objects through repository interfaces.
- Call domain methods to apply business behavior.
- Save changed domain objects through repository interfaces.
- Convert domain objects into service DTOs.

Services do not expose domain objects outside the services layer. Public service methods return DTOs or result objects.

Service result objects are preferred when an operation can fail in an expected business way. For example, `AddToCartResult` returns:

- `Success`
- `CartId`
- `FailureCode`
- `Message`

Expected business failures should not be modeled as thrown exceptions.

## Infrastructure

Infrastructure implements repository interfaces from domain.

Infrastructure can use in-memory collections, databases, files, external APIs, or other persistence details. It should not contain business rules. For example, an in-memory cart repository should store and retrieve `Cart` objects; it should not decide how items are added to a cart.

## API

API references services and infrastructure.

Controllers should be thin:

- Read route/body/query values.
- Call a service method.
- Return the service DTO/result.

Dependency wiring belongs in `Program.cs`. The API project can instantiate services with infrastructure implementations there through DI.

## Products And Inventory

Ticket inventory is modeled as `DailyInventory`, not only as admission tickets. A `ParkCalendar` can contain multiple product types for the same day, such as:

- Park ticket
- Food pass
- Other dated add-ons

Shopping should use `ProductId` to add items to a cart. The caller does not provide or decide the price. Price comes from domain/inventory data loaded through repositories.

## Tests

Service tests should use service methods for setup when practical. Avoid reaching into repositories directly to arrange business state unless the test is specifically about repository behavior.

Tests can use simple in-test repository fakes, but those fakes should follow the same responsibilities as real repositories:

- Persist objects.
- Retrieve objects.
- Avoid adding business logic that belongs in domain objects.

Prefer explicit indexed assertions for small ordered result sets when that makes the test clearer.
