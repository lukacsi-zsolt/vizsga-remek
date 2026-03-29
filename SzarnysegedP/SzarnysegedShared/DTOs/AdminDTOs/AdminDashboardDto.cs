namespace SzarnysegedShared.DTOs.AdminDTOs
{
    public class AdminDashboardDto
    {
        public int UsersCount { get; set; }
        public int PostsCount { get; set; }
        public int NewsCount { get; set; }
        public int SpotsCount { get; set; }
        public int SpotSuggestionsCount { get; set; }
        public List<AdminDailyStatDto> Last7Days { get; set; } = new();
    }

    public class AdminDailyStatDto
    {
        public string Label { get; set; } = string.Empty;
        public int Users { get; set; }
        public int Posts { get; set; }
        public int News { get; set; }
        public int Spots { get; set; }
        public int Suggestions { get; set; }
    }
}