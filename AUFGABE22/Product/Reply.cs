using System;

namespace Fbs.Lg2;

/// <summary>A vendor's reply to a single review. Belongs to exactly one review (1:1).</summary>
public sealed class Reply
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _text;
    private DateTime _replyDate;
    private readonly Review _review;

    public int ReplyId => _id;
    public string Text => _text;
    public DateTime ReplyDate => _replyDate;
    public Review Review => _review;

    internal Reply(Review review, string text, DateTime replyDate)
    {
        ArgumentNullException.ThrowIfNull(review);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidCommentException("Reply text must not be null, empty, or whitespace.");
        }

        _id = _nextId++;
        _review = review;
        _text = text.Trim();
        _replyDate = replyDate;
        CheckInvariants();
    }

    public void UpdateText(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
        {
            throw new InvalidCommentException("Reply text must not be null, empty, or whitespace.");
        }
        _text = newText.Trim();
        CheckInvariants();
    }

    private void CheckInvariants()
    {
        if (_id <= 0)
        {
            throw new InvariantViolationException(
                $"Invariant violated: reply id ({_id}) must be positive.");
        }
        if (string.IsNullOrWhiteSpace(_text))
        {
            throw new InvariantViolationException(
                "Invariant violated: reply text must not be empty.");
        }
        if (_review is null)
        {
            throw new InvariantViolationException(
                "Invariant violated: reply must reference a review.");
        }
    }
}