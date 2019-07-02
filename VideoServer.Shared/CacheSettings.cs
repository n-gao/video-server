namespace VideoServer.Shared {
    public interface ICacheSettings {
        string Folder {get; set;}
        int Size {get; set;}
    }

    public class CacheSettings : ICacheSettings {
        public string Folder {get; set;}
        public int Size {get; set;}
    }
}
