using System;

namespace Fbs.Lg2;

/// <summary>A review submitted by a customer for a product, optionally with a vendor reply.</summary>
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

    public int ReviewId => _id;
    public int Stars => _stars;
    public string Comment => _comment;
    public DateTime CreatedAt => _createdAt;
    public bool VerifiedPurchase => _verifiedPurchase;
    public Customer Customer => _customer;
    public Product Product => _product;
    public Reply? Reply => _reply;

    public Review(Customer customer, Product product, int stars, string comment, DateTime createdAt, bool verifiedPurchase)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(product);
        if (stars < 1 || stars > 5)
        {
            throw new InvalidStarsException($"Star rating ({stars}) must be in 1..5.");
        }
        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new InvalidCommentException("Review comment must not be null, empty, or whitespace.");
        }

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

    public void UpdateStars(int newStars)
    {
        if (newStars < 1 || newStars > 5)
        {
            throw new InvalidStarsException($"Star rating ({newStars}) must be in 1..5.");
        }
        _stars = newStars;
        CheckInvariants();
    }

    public void UpdateComment(string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment))
        {
            throw new InvalidCommentException("Review comment must not be null, empty, or whitespace.");
        }
        _comment = newComment.Trim();
        CheckInvariants();
    }

    public void UpdateVerifiedPurchase(bool verified)
    {
        _verifiedPurchase = verified;
        CheckInvariants();
    }

    public Reply AddReply(string text, DateTime replyDate)
    {
        if (_reply is not null)
        {
            throw new ReviewAlreadyRepliedException(
                $"Review {_id} already has a reply (id {_reply.ReplyId}).");
        }
        var reply = new Reply(this, text, replyDate);
        _reply = reply;
        CheckInvariants();
        return reply;
    }

    public bool RemoveReply()
    {
        if (_reply is null)
        {
            return false;
        }
        _reply = null;
        CheckInvariants();
        return true;
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: review id ({_id}) must be positive.");
        }
        if (_stars < 1 || _stars > 5)
        {
            throw new InvariantViolationException(
                $"Invariant violated: stars ({_stars}) must be in 1..5.");
        }
        if (string.IsNullOrWhiteSpace(_comment))
        {
            throw new InvariantViolationException(
                "Invariant violated: review comment must not be empty.");
        }
        if (_customer is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: review must reference a customer.");
        }
        if (_product is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: review must reference a product.");
        }
    }
}