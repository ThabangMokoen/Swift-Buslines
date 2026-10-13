# SwiftBus - Complete C# Bus Ticket Booking System

## Platform
- C# Windows Forms
- .NET 8
- Visual Studio 2022

## Included Features

1. Modern SwiftBus booking interface
2. Passenger contact details
3. Multiple passengers per booking
4. Individual passenger ages
5. Automatic passenger categories
6. Age-based discounts:
   - 0-5: 100% discount
   - 6-17: 25% discount
   - 18-59: 0% discount
   - 60+: 30% discount
7. Johannesburg, Pretoria, Durban, Bloemfontein and Cape Town routes
8. Travel date
9. Departure time
10. Automatic ETA
11. Automatic route duration
12. Standard, Comfort and Executive buses
13. Automatic fare calculation
14. 20-seat layout
15. Occupied seat indicators
16. Prevent occupied-seat selection
17. Payment method selection
18. Simulated payment
19. Payment validation before booking
20. Unique booking number
21. Full ticket preview
22. Ticket printing using the Windows print dialog
23. Clear/reset function

## Initial Occupied Seats

Seats 3, 7, 12 and 18 start as occupied. A successfully booked seat is also marked occupied while the application is running.

> Current seat occupancy is stored in memory only. Closing the program resets the initial seat state. For a production system, connect the application to SQLite or SQL Server.

## How to Run

1. Extract the ZIP.
2. Open `SwiftBus.sln` in Visual Studio 2022.
3. Make sure the .NET 8 SDK is installed.
4. Press `F5` to run.

Alternatively, from the `SwiftBus` project folder:

```bash
dotnet run
```

## Project Structure

```text
SwiftBus.sln
└── SwiftBus
    ├── SwiftBus.csproj
    ├── Program.cs
    ├── MainForm.cs
    └── README.md
```

## Notes

The system intentionally uses Windows Forms controls created in C# so no separate Designer file is required. This keeps the project portable and easy to edit.
