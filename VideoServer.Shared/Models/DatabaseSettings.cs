namespace VideoServer.Shared.Models {
    public interface IDatabaseSettings {
        string EpisodeCollectionName {get; set;}
        string QuoteCollectionName {get; set;}
        string ConnectionString {get; set;}
        string DatabaseName {get; set;}
    }

    public class DatabaseSettings : IDatabaseSettings {
        public string EpisodeCollectionName {get; set;}
        public string QuoteCollectionName {get; set;}
        public string ConnectionString {get; set;}
        public string DatabaseName {get; set;}
    }
}
