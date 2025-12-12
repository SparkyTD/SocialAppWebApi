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

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/v1/auth/register` | Register a new user |
| `POST` | `/v1/auth/login` | Authenticate and receive JWT token |

#### Register User
**POST** `/v1/auth/register`

**Request Body:**
```json
{
  "username": "string",  // 1-32 characters
  "password": "string"   // 4-128 characters
}
```

**Response:** `200 OK`

---

#### Login
**POST** `/v1/auth/login`

**Request Body:**
```json
{
  "username": "string",  // 1-32 characters
  "password": "string"   // 4-128 characters
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### Posts

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/v1/posts` | Retrieve paginated posts |
| `POST` | `/v1/posts` | Create a new post |
| `GET` | `/v1/posts/{id}` | Get a specific post |
| `DELETE` | `/v1/posts/{id}` | Delete a post |

#### Get Posts
**GET** `/v1/posts`

**Query Parameters:**

| Parameter | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| `page` | integer | ≥ 1 | Page number |
| `pageSize` | integer | 1-100 | Items per page |

**Response:**
```json
[
  {
    "id": 1,
    "body": "Post content...",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

---

#### Create Post
**POST** `/v1/posts`

**Request Body:**
```json
{
  "body": "string"  // 1-240 characters
}
```

**Response:** Post object

---

#### Get Post by ID
**GET** `/v1/posts/{id}`

**Path Parameters:**
- `id` - Post ID (integer)

**Response:** Post object or `404 Not Found`

---

#### Delete Post
**DELETE** `/v1/posts/{id}`

**Path Parameters:**
- `id` - Post ID (integer)

**Response:** `200 OK`

---

### Likes

| Method | Endpoint | Description                        |
|--------|----------|------------------------------------|
| `PUT` | `/v1/likes` | Like a post as the current user    |
| `DELETE` | `/v1/likes` | Unlike a post as the current user  |

#### Like Post
**PUT** `/v1/likes`

**Request Body:**
```json
{
  "postId": integer
}
```

**Response:** `200 OK`

---

#### Unlike Post
**DELETE** `/v1/likes`

**Request Body:**
```json
{
  "postId": integer
}
```

**Response:** `200 OK`

___

## External (Non-MSFT) Libraries Used
- **NUnit**: For unit tests
- **Moq**: For mocking service interfaces in unit tests
- **AutoMapper**: For Entity-to-DTO mapping
- **BCrypt.Net-Next**: For user password hashing
- **Quartz**: Used for scheduling periodic tasks
- **Serilog**: Preferred alternative logging framework
- **Swashbuckle**: Swagger / OpenAPI sandbox UI (hosted at http://localhost:5202/swagger/index.html)

___

# Run Instructions
```shell
git clone https://github.com/SparkyTD/SocialAppWebApi
cd SocialAppWebApi
dotnet build
dotnet test # optional
dotnet run --project SocialAppWebApi
```