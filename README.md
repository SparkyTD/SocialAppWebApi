# SocialAppWebApi
This is a very simple proof-of-concept API microservice for handling posts and likes ona  social media platform. 
Written in C# using ASP.Net Core, Dependency Injection and Entity Framework Core, as well as other supporting libraries.

___

## Design decisions

### Authentication
Since the data model logically requires a User object, I opted to implement a simple JWT-based authentication 
instead of using arbitrary user IDs. This allows the app to simulate more realistic scenarios more accurately.

### Like Counter
I took a hybrid approach to tracking likes on individual posts.

Likes are defined per-user in the data model, which would allow a UI to indicate if a post has already been liked
by the currently authenticated user. Creating a like will simply insert a new `PostLike` record into the database.

Raw like counts are also tracked in a dedicated `CachedLikeCount` column in the `Post` object. This is updated during
both like/unlike operations, as well as periodically from a Quartz/cron scheduled task to ensure data consistency and
self-healing of potential data errors.

The immediate increment/decrement of this counter is implemented as a SQL-side atomic increment operation 
(e.g. `UPDATE Posts set CachedLikeCount = CachedLikeCount + 1 WHERE ...`), which eliminates potential data
corruption as a result of race conditions or Time-of-check to time-of-use scenarios. This combined with the periodic
forced-update of the cache column ensures that the like count displayed to the user is always accurate.

___

## External (Non-MSFT) Libraries Used
- **NUnit**: For unit tests
- **Moq**: For mocking service interfaces in unit tests
- **AutoMapper**: For Entity-to-DTO mapping
- **BCrypt.Net-Next**: For user password hashing
- **Quartz**: Used for scheduling periodic tasks
- **Serilog**: Preferred alternative logging framework
- **Swashbuckle**: Swagger / OpenAPI sandbox UI (hosted at http://localhost:5202/swagger/index.html)