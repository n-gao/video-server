using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VideoServer.Server
{
    public interface IVideoStream : IDisposable
    {
        string FilePath {get; set;}
        float Start {get; set;}
        float Duration {get; set;}

        Task<Stream> ReadToStream();

        Task<FileStream> GetThumbnail();
    }
}
