using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EveCommon.Interfaces;

namespace EveCommon.Models
{
    public class GlobalHttpClient : IHttpClient, IDisposable
    {
        public GlobalHttpClient()
        {
            HttpClient = new HttpClient();
        }

        public HttpClient HttpClient { get; }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
