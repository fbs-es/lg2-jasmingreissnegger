# API Reference — AUFGABE22 Review Portal

Generated automatically from `///` XML doc comments in the source code.
Each entry lists preconditions (`Throws`), postconditions (summary), and invariants (`Remarks` on classes).

## Contents

- [`Customer`](#customer)
- [`DomainException`](#domainexception)
- [`InvalidNameException`](#invalidnameexception)
- [`InvalidEmailException`](#invalidemailexception)
- [`InvalidPriceException`](#invalidpriceexception)
- [`InvalidStarsException`](#invalidstarsexception)
- [`InvalidCommentException`](#invalidcommentexception)
- [`ReviewAlreadyRepliedException`](#reviewalreadyrepliedexception)
- [`InvariantViolationException`](#invariantviolationexception)
- [`Manufacturer`](#manufacturer)
- [`Product`](#product)
- [`Reply`](#reply)
- [`Review`](#review)

## `Customer`

A buyer who can submit reviews for products.

> Invariants:
> * CustomerId &gt; 0
> * Name is never null or whitespace
> * Reviews is never null

### Members

#### `CustomerId`

```csharp
public int CustomerId => _id;
```

Unique customer id (auto-assigned, never reused).

#### `Name`

```csharp
public string Name => _name;
```

Customer's full name (never null or whitespace).

#### `Reviews`

```csharp
public IReadOnlyList<Review> Reviews => _reviews;
```

Reviews authored by this customer (read-only snapshot).

#### `Customer`

```csharp
public Customer(string name);
```

Creates a new customer.

**Parameters**

- `name` — Full name; must not be null, empty or whitespace.

**Throws (preconditions)**

- `InvalidNameException` — Precondition violated: name is null, empty or whitespace.

#### `UpdateName`

```csharp
public void UpdateName(string newName);
```

Updates the customer's name.

**Parameters**

- `newName` — New name; must not be null, empty or whitespace.

**Throws (preconditions)**

- `InvalidNameException` — Precondition violated: newName is null, empty or whitespace.

## `DomainException`

Base type for every domain-specific exception thrown by the review-portal model.

## `InvalidNameException`

Thrown when a textual argument is null, empty or whitespace.

## `InvalidEmailException`

Thrown when an email-like value is missing the basic format markers ('@' or '.' in domain).

## `InvalidPriceException`

Thrown when a price argument is negative.

## `InvalidStarsException`

Thrown when the star rating is outside the allowed 1..5 range.

## `InvalidCommentException`

Thrown when a comment-like argument is null, empty or whitespace.

## `ReviewAlreadyRepliedException`

Thrown when an attempt is made to add a second reply to a review that already has one.

## `InvariantViolationException`

Thrown when an internal invariant has been violated - indicates a programming error.

## `Manufacturer`

A manufacturer that produces one or more products.

> Invariants:
> * ManufacturerId &gt; 0
> * Name is never null or whitespace
> * SupportEmail is a valid e-mail address (contains '@' with non-empty local/domain; domain has a '.')
> * Products is never null

### Members

#### `ManufacturerId`

```csharp
public int ManufacturerId => _id;
```

Unique manufacturer id (auto-assigned).

#### `Name`

```csharp
public string Name => _name;
```

Manufacturer's display name (never null or whitespace).

#### `SupportEmail`

```csharp
public string SupportEmail => _supportEmail;
```

Contact e-mail for support inquiries (always a valid address).

#### `Products`

```csharp
public IReadOnlyList<Product> Products => _products;
```

Products produced by this manufacturer (read-only snapshot).

#### `Manufacturer`

```csharp
public Manufacturer(string name, string supportEmail);
```

Creates a new manufacturer.

**Parameters**

- `name` — Manufacturer name; must not be null, empty or whitespace.
- `supportEmail` — Support e-mail; must contain '@' with non-empty local and domain parts, and the domain must include a '.'.

**Throws (preconditions)**

- `InvalidNameException` — Precondition violated: name is null, empty or whitespace.
- `InvalidEmailException` — Precondition violated: supportEmail is not a valid address.

#### `UpdateName`

```csharp
public void UpdateName(string newName);
```

Updates the manufacturer name.

**Parameters**

- `newName` — New name; must not be null, empty or whitespace.

**Throws (preconditions)**

- `InvalidNameException` — Precondition violated: newName is null, empty or whitespace.

#### `UpdateSupportEmail`

```csharp
public void UpdateSupportEmail(string newEmail);
```

Updates the support e-mail.

**Parameters**

- `newEmail` — New support e-mail; must be a valid address.

**Throws (preconditions)**

- `InvalidEmailException` — Precondition violated: newEmail is not a valid address.

## `Product`

A product sold through the e-commerce portal.

> Invariants:
> * ProductId &gt; 0
> * Name is never null or whitespace
> * Price is always &gt;= 0
> * Manufacturer is never null (set at construction, never reassigned)
> * Reviews is never null

### Members

#### `ProductId`

```csharp
public int ProductId => _id;
```

Unique product id (auto-assigned).

#### `Name`

```csharp
public string Name => _name;
```

Product's display name (never null or whitespace).

#### `Price`

```csharp
public decimal Price => _price;
```

Current price in EUR; always &gt;= 0.

#### `Manufacturer`

```csharp
public Manufacturer Manufacturer => _manufacturer;
```

Manufacturer that produces this product (set once at construction, never reassigned).

#### `Reviews`

```csharp
public IReadOnlyList<Review> Reviews => _reviews;
```

Reviews submitted for this product (read-only snapshot).

#### `Product`

```csharp
public Product(string name, decimal price, Manufacturer manufacturer);
```

Creates a new product and registers it with the manufacturer.

**Parameters**

- `name` — Product name; must not be null, empty or whitespace.
- `price` — Product price in EUR; must be &gt;= 0.
- `manufacturer` — Manufacturer; must not be null.

**Throws (preconditions)**

- `InvalidNameException` — Precondition violated: name is null, empty or whitespace.
- `InvalidPriceException` — Precondition violated: price is negative.
- `ArgumentNullException` — Precondition violated: manufacturer is null.

#### `UpdateName`

```csharp
public void UpdateName(string newName);
```

Updates the product name.

**Parameters**

- `newName` — New name; must not be null, empty or whitespace.

**Throws (preconditions)**

- `InvalidNameException` — Precondition violated: newName is null, empty or whitespace.

#### `UpdatePrice`

```csharp
public void UpdatePrice(decimal newPrice);
```

Updates the product price.

**Parameters**

- `newPrice` — New price in EUR; must be &gt;= 0.

**Throws (preconditions)**

- `InvalidPriceException` — Precondition violated: newPrice is negative.

#### `AverageStars`

```csharp
public decimal AverageStars();
```

Computes the arithmetic mean of all review star ratings.

**Returns** — The mean of all Stars values, in the range 1.0 - 5.0. Returns 0 if the product has no reviews.

## `Reply`

A vendor's reply to a single review. Belongs to exactly one review (1:1, enforced by Review.AddReply).

> Invariants:
> * ReplyId &gt; 0
> * Text is never null or whitespace
> * Review reference is never null

### Members

#### `ReplyId`

```csharp
public int ReplyId => _id;
```

Unique reply id (auto-assigned).

#### `Text`

```csharp
public string Text => _text;
```

The body of the vendor's reply (never null or whitespace).

#### `ReplyDate`

```csharp
public DateTime ReplyDate => _replyDate;
```

The date and time the reply was written.

#### `Review`

```csharp
public Review Review => _review;
```

The review this reply belongs to (never null).

#### `UpdateText`

```csharp
public void UpdateText(string newText);
```

Updates the reply text.

**Parameters**

- `review` — The review to reply to; must not be null.
- `text` — Reply text; must not be null, empty or whitespace.
- `replyDate` — Reply timestamp.
- `newText` — New text; must not be null, empty or whitespace.

**Throws (preconditions)**

- `ArgumentNullException` — Precondition violated: review is null.
- `InvalidCommentException` — Precondition violated: text is null, empty or whitespace.
- `InvalidCommentException` — Precondition violated: newText is null, empty or whitespace.

## `Review`

A review submitted by a customer for a product, optionally with a vendor reply.

> Invariants:
> * ReviewId &gt; 0
> * Stars is in 1..5
> * Comment is never null or whitespace
> * Customer and Product references are never null
> * Reply is either null or unique to this review (1:1, enforced by AddReply)

### Members

#### `ReviewId`

```csharp
public int ReviewId => _id;
```

Unique review id (auto-assigned).

#### `Stars`

```csharp
public int Stars => _stars;
```

Star rating, in the inclusive range 1-5.

#### `Comment`

```csharp
public string Comment => _comment;
```

Personal comment by the reviewer (never null or whitespace).

#### `CreatedAt`

```csharp
public DateTime CreatedAt => _createdAt;
```

When the review was first created.

#### `VerifiedPurchase`

```csharp
public bool VerifiedPurchase => _verifiedPurchase;
```

True iff the reviewer is verified to have bought the product.

#### `Customer`

```csharp
public Customer Customer => _customer;
```

The customer who wrote this review (never null).

#### `Product`

```csharp
public Product Product => _product;
```

The product this review is about (never null).

#### `Reply`

```csharp
public Reply? Reply => _reply;
```

The vendor reply, or null if no reply has been posted yet.

#### `Review`

```csharp
public Review(Customer customer, Product product, int stars, string comment, DateTime createdAt, bool verifiedPurchase);
```

Creates a new review and attaches it to both customer and product.

**Parameters**

- `customer` — The reviewer; must not be null.
- `product` — The reviewed product; must not be null.
- `stars` — Star rating; must be in 1..5.
- `comment` — Personal comment; must not be null, empty or whitespace.
- `createdAt` — Creation timestamp.
- `verifiedPurchase` — Whether the reviewer is verified to have bought the product.

**Throws (preconditions)**

- `ArgumentNullException` — Precondition violated: customer or product is null.
- `InvalidStarsException` — Precondition violated: stars is outside 1..5.
- `InvalidCommentException` — Precondition violated: comment is null, empty or whitespace.

#### `UpdateStars`

```csharp
public void UpdateStars(int newStars);
```

Updates the star rating.

**Parameters**

- `newStars` — New star rating; must be in 1..5.

**Throws (preconditions)**

- `InvalidStarsException` — Precondition violated: newStars is outside 1..5.

#### `UpdateComment`

```csharp
public void UpdateComment(string newComment);
```

Updates the comment.

**Parameters**

- `newComment` — New comment; must not be null, empty or whitespace.

**Throws (preconditions)**

- `InvalidCommentException` — Precondition violated: newComment is null, empty or whitespace.

#### `UpdateVerifiedPurchase`

```csharp
public void UpdateVerifiedPurchase(bool verified);
```

Updates the verified-purchase flag.

**Parameters**

- `verified` — True if the reviewer is verified to have bought the product.

#### `AddReply`

```csharp
public Reply AddReply(string text, DateTime replyDate);
```

Adds a vendor reply to this review.

**Parameters**

- `text` — Reply text; must not be null, empty or whitespace.
- `replyDate` — Reply timestamp.

**Returns** — The newly-created reply.

**Throws (preconditions)**

- `ReviewAlreadyRepliedException` — Precondition violated: this review already has a reply.
- `InvalidCommentException` — Precondition violated: text is null, empty or whitespace.

#### `RemoveReply`

```csharp
public bool RemoveReply();
```

Removes the vendor reply from this review, if present.

**Returns** — True if a reply was removed; false if there was no reply to remove.
