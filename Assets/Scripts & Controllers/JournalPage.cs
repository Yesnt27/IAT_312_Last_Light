using UnityEngine;

// One page of the old keeper's journal (page N is found on Day N), as art from Visual Assets/Journal Assets.
[System.Serializable]
public class JournalPage
{
    [Tooltip("The open book with this day's entry (\"Day N Journal\"), shown when you read the journal with R.")]
    public Sprite bookSpread;
    [Tooltip("The loose page as it's found (\"Day N Pages\"). Empty = the book spread is shown when it's found.")]
    public Sprite foundPage;

    // A day without a book spread yet shows its loose page in the book.
    public Sprite InBook => bookSpread != null ? bookSpread : foundPage;
    public Sprite WhenFound => foundPage != null ? foundPage : bookSpread;
}
