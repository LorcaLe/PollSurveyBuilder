namespace PollSurveyBuilder.Domain.Entities
{
    /// <summary>
    /// One selectable answer for a poll (or one star-rating value, or the
    /// fixed Yes/No pair). Not used for OpenText polls.
    /// </summary>
    public class PollOption
    {
        public int Id { get; set; }

        public int PollId { get; set; }
        public Poll Poll { get; set; } = default!;

        public string Text { get; set; } = default!;

        public int OrderIndex { get; set; }

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
