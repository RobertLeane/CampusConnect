# CampusConnect Development Team Assignments

## Project Structure Overview

This document outlines the file structure and provides guidance for team members to implement their assigned components.

## File Structure

```
api/
├── Functions/
│   ├── ClubFunctions.cs          ✅ COMPLETED
│   ├── UserFunctions.cs          📝 TODO
│   ├── EventFunctions.cs         📝 TODO
│   ├── MemberFunctions.cs        📝 TODO
│   └── EventParticipantFunctions.cs 📝 TODO
├── Models/
│   ├── ClubDto.cs                ✅ COMPLETED
│   ├── UserDto.cs                📝 TODO
│   ├── EventDto.cs               📝 TODO
│   ├── MemberDto.cs              📝 TODO
│   └── EventParticipantDto.cs    📝 TODO
├── Services/
│   ├── SqlService.cs             ✅ COMPLETED (shared)
│   ├── IClubService.cs           ✅ COMPLETED
│   ├── ClubService.cs            ✅ COMPLETED
│   ├── IUserService.cs           📝 TODO
│   ├── UserService.cs            📝 TODO
│   ├── IEventService.cs          📝 TODO
│   ├── EventService.cs           📝 TODO
│   ├── IMemberService.cs         📝 TODO
│   ├── MemberService.cs          📝 TODO
│   ├── IEventParticipantService.cs 📝 TODO
│   └── EventParticipantService.cs  📝 TODO
└── Program.cs                    ✅ COMPLETED (update when services are ready)

client/
├── Models/
│   ├── Club.cs                   ✅ COMPLETED
│   ├── User.cs                   📝 TODO
│   ├── Event.cs                  📝 TODO
│   ├── Member.cs                 📝 TODO
│   └── EventParticipant.cs       📝 TODO
└── Services/
    ├── ClubApi.cs                ✅ COMPLETED
    ├── UserApi.cs                📝 TODO
    ├── EventApi.cs               📝 TODO
    ├── MemberApi.cs              📝 TODO
    └── EventParticipantApi.cs    📝 TODO
```

## Team Member Assignments

### Team Member 1: User Management
**Files to implement:**
- `api/Models/UserDto.cs`
- `api/Services/UserService.cs`
- `api/Functions/UserFunctions.cs`
- `client/Models/User.cs`
- `client/Services/UserApi.cs`

### Team Member 2: Event Management
**Files to implement:**
- `api/Models/EventDto.cs`
- `api/Services/EventService.cs`
- `api/Functions/EventFunctions.cs`
- `client/Models/Event.cs`
- `client/Services/EventApi.cs`

### Team Member 3: Club Membership
**Files to implement:**
- `api/Models/MemberDto.cs`
- `api/Services/MemberService.cs`
- `api/Functions/MemberFunctions.cs`
- `client/Models/Member.cs`
- `client/Services/MemberApi.cs`

### Team Member 4: Event Participation
**Files to implement:**
- `api/Models/EventParticipantDto.cs`
- `api/Services/EventParticipantService.cs`
- `api/Functions/EventParticipantFunctions.cs`
- `client/Models/EventParticipant.cs`
- `client/Services/EventParticipantApi.cs`

## Implementation Guidelines

### 1. Database Schema Reference
Make sure to check your actual database tables for the correct column names and data types.

### 2. Follow the Club Example
Use the completed Club implementation as a reference:
- Look at `api/Services/ClubService.cs` for database operations
- Look at `api/Functions/ClubFunctions.cs` for API endpoints
- Look at `client/Services/ClubApi.cs` for client-side API calls

### 3. Key Implementation Points

#### For DTOs (Models):
- Match your database table schema exactly
- Use nullable types (`string?`, `int?`) for optional fields
- Include navigation properties for display purposes when needed

#### For Services:
- Always check for null/empty server and database configuration
- Use `SqlService.OpenTokenConnectionAsync()` for database connections
- Use parameterized queries to prevent SQL injection
- Handle exceptions and log errors appropriately
- Return the correct data types as defined in interfaces

#### For Functions:
- Use appropriate HTTP methods (GET, POST, PUT, DELETE)
- Define clear routes (e.g., "users", "users/{userId:int}")
- Handle exceptions and return proper HTTP status codes
- Log operations for debugging

#### For Client APIs:
- Use `HttpClient.GetFromJsonAsync<T>()` for GET requests
- Use `HttpClient.PostAsJsonAsync()` for POST requests
- Use `HttpClient.PutAsJsonAsync()` for PUT requests
- Use `HttpClient.DeleteAsync()` for DELETE requests

### 4. When Complete
After implementing your assigned components:
1. Test your API endpoints using a tool like Postman or browser
2. Uncomment the service registration line in `api/Program.cs`
3. Update the team on your progress

## Database Connection Setup
The application uses Azure SQL Database with token-based authentication. The connection details are configured in `api/local.settings.json`.

## Questions or Issues?
If you encounter any issues or have questions about the implementation:
1. Check the completed Club implementation first
2. Review the TODO comments in your assigned files
3. Ask team members or instructor for guidance

Good luck with the implementation! 🚀