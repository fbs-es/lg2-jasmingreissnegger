using System;

namespace Fbs.Lg2;

/// <summary>
/// A review submitted by a customer for a product, optionally with a vendor reply.
/// </summary>
/// <remarks>
/// Invariants:
///   * ReviewId &gt; 0
///   * Stars is in 1..5
///   * Comment is never null or whitespace
///   * Customer and Product references are never null
///   * Reply is either null or unique to this review (1:1, enforced by AddReply)
/// </remarks>
public sealed class Review
{
    private static int _nextId = 1;

    private readonly int _id;
    private int _stars;
    private string _comment;
    private DateTime _createdAt;
    private bool _verifiedPurchase;
    private readonly Customer _customer;
    private readonly Product _product;
    private Reply? _reply;

    /// <summary>Unique review id (auto-assigned).</summary>
    public int ReviewId => _id;

    /// <summary>Star rating, in the inclusive range 1-5.</summary>
    public int Stars => _stars;

    /// <summary>Personal comment by the reviewer (never null or whitespace).</summary>
    public string Comment => _comment;

    /// <summary>When the review was first created.</summary>
    public DateTime CreatedAt => _createdAt;

    /// <summary>True iff the reviewer is verified to have bought the product.</summary>
    public bool VerifiedPurchase => _verifiedPurchase;

    /// <summary>The customer who wrote this review (never null).</summary>
    public Customer Customer => _customer;

    /// <summary>The product this review is about (never null).</summary>
    public Product Product => _product;

    /// <summary>The vendor reply, or null if no reply has been posted yet.</summary>
    public Reply? Reply => _reply;

    /// <summary>
    /// Creates a new review and attaches it to both customer and product.
    /// </summary>
    /// <param name="customer">The reviewer; must not be null.</param>
    /// <param name="product">The reviewed product; must not be null.</param>
    /// <param name="stars">Star rating; must be in 1..5.</param>
    /// <param name="comment">Personal comment; must not be null, empty or whitespace.</param>
    /// <param name="createdAt">Creation timestamp.</param>
    /// <param name="verifiedPurchase">Whether the reviewer is verified to have bought the product.</param>
    /// <exception cref="ArgumentNullException">Precondition violated: customer or product is null.</exception>
    /// <exception cref="InvalidStarsException">Precondition violated: stars is outside 1..5.</exception>
    /// <exception cref="InvalidCommentException">Precondition violated: comment is null, empty or whitespace.</exception>
    public Review(Customer customer, Product product, int stars, string comment, DateTime createdAt, bool verifiedPurchase)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(product);
        if (stars < 1 || stars > 5)
            throw new InvalidStarsException($"Star rating ({stars}) must be in 1..5.");
        if (string.IsNullOrWhiteSpace(comment))
            throw new InvalidCommentException("Review comment must not be null, empty, or whitespace.");

        _id = _nextId++;
        _customer = customer;
        _product = product;
        _stars = stars;
        _comment = comment.Trim();
        _createdAt = createdAt;
        _verifiedPurchase = verifiedPurchase;

        _customer.AttachReview(this);
        _product.AttachReview(this);

        CheckInvariants();
    }

    /// <summary>
    /// Updates the star rating.
    /// </summary>
    /// <param name="newStars">New star rating; must be in 1..5.</param>
    /// <exception cref="InvalidStarsException">Precondition violated: newStars is outside 1..5.</exception>
    public void UpdateStars(int newStars)
    {
        if (newStars < 1 || newStars > 5)
            throw new InvalidStarsException($"Star rating ({newStars}) must be in 1..5.");
        _stars = newStars;
        CheckInvariants();
    }

    /// <summary>
    /// Updates the comment.
    /// </summary>
    /// <param name="newComment">New comment; must not be null, empty or whitespace.</param>
    /// <exception cref="InvalidCommentException">Precondition violated: newComment is null, empty or whitespace.</exception>
    public void UpdateComment(string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment))
            throw new InvalidCommentException("Review comment must not be null, empty, or whitespace.");
        _comment = newComment.Trim();
        CheckInvariants();
    }

    /// <summary>
    /// Updates the verified-purchase flag.
    /// </summary>
    /// <param name="verified">True if the reviewer is verified to have bought the product.</param>
    public void UpdateVerifiedPurchase(bool verified)
    {
        _verifiedPurchase = verified;
        CheckInvariants();
    }

    /// <summary>
    /// Adds a vendor reply to this review.
    /// </summary>
    /// <param name="text">Reply text; must not be null, empty or whitespace.</param>
    /// <param name="replyDate">Reply timestamp.</param>
    /// <returns>The newly-created reply.</returns>
    /// <exception cref="ReviewAlreadyRepliedException">Precondition violated: this review already has a reply.</exception>
    /// <exception cref="InvalidCommentException">Precondition violated: text is null, empty or whitespace.</exception>
    public Reply AddReply(string text, DateTime replyDate)
    {
        if (_reply is not null)
            throw new ReviewAlreadyRepliedException(
                $"Review {_id} already has a reply (id {_reply.ReplyId}).");
        var reply = new Reply(this, text, replyDate);
        _reply = reply;
        CheckInvariants();
        return reply;
    }

    /// <summary>
    /// Removes the vendor reply from this review, if present.
    /// </summary>
    /// <returns>True if a reply was removed; false if there was no reply to remove.</returns>
    public bool RemoveReply()
    {
        if (_reply is null)
            return false;
        _reply = null;
        CheckInvariants();
        return true;
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
            throw new InvariantViolationException(
                $"Invariant violated: review id ({_id}) must be positive.");
        if (_stars < 1 || _stars > 5)
            throw new InvariantViolationException(
                $"Invariant violated: stars ({_stars}) must be in 1..5.");
        if (string.IsNullOrWhiteSpace(_comment))
            throw new InvariantViolationException(
                "Invariant violated: review comment must not be empty.");
        if (_customer is null)
            throw new InvariantViolationException(
                "Invariant violated: review must reference a customer.");
        if (_product is null)
            throw new InvariantViolationException(
                "Invariant violated: review must reference a product.");
    }
}
