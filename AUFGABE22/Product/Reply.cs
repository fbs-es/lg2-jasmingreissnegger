using System;

namespace Fbs.Lg2;

/// <summary>
/// A vendor's reply to a single review. Belongs to exactly one review (1:1, enforced by Review.AddReply).
/// </summary>
/// <remarks>
/// Invariants:
///   * ReplyId &gt; 0
///   * Text is never null or whitespace
///   * Review reference is never null
/// </remarks>
public sealed class Reply
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _text;
    private DateTime _replyDate;
    private readonly Review _review;

    /// <summary>Unique reply id (auto-assigned).</summary>
    public int ReplyId => _id;

    /// <summary>The body of the vendor's reply (never null or whitespace).</summary>
    public string Text => _text;

    /// <summary>The date and time the reply was written.</summary>
    public DateTime ReplyDate => _replyDate;

    /// <summary>The review this reply belongs to (never null).</summary>
    public Review Review => _review;

    /// <summary>
    /// Creates a reply for the given review. Internal: only callable from Review.AddReply.
    /// </summary>
    /// <param name="review">The review to reply to; must not be null.</param>
    /// <param name="text">Reply text; must not be null, empty or whitespace.</param>
    /// <param name="replyDate">Reply timestamp.</param>
    /// <exception cref="ArgumentNullException">Precondition violated: review is null.</exception>
    /// <exception cref="InvalidCommentException">Precondition violated: text is null, empty or whitespace.</exception>
    internal Reply(Review review, string text, DateTime replyDate)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidCommentException("Reply text must not be null, empty, or whitespace.");

        _id = _nextId++;
        _review = review;
        _text = text.Trim();
        _replyDate = replyDate;
        CheckInvariants();
    }

    /// <summary>
    /// Updates the reply text.
    /// </summary>
    /// <param name="newText">New text; must not be null, empty or whitespace.</param>
    /// <exception cref="InvalidCommentException">Precondition violated: newText is null, empty or whitespace.</exception>
    public void UpdateText(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            throw new InvalidCommentException("Reply text must not be null, empty, or whitespace.");
        _text = newText.Trim();
        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
            throw new InvariantViolationException(
                $"Invariant violated: reply id ({_id}) must be positive.");
        if (string.IsNullOrWhiteSpace(_text))
            throw new InvariantViolationException(
                "Invariant violated: reply text must not be empty.");
        if (_review is null)
            throw new InvariantViolationException(
                "Invariant violated: reply must reference a review.");
    }
}
