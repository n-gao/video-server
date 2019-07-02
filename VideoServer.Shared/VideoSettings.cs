namespace VideoServer.Shared {
    public interface IVideoSettings {
        string Folder {get; set;}
        string Format {get; set;}
    }

    public class VideoSettings : IVideoSettings {
        public string Folder {get; set;}
        public string Format {get; set;}
    }
}
