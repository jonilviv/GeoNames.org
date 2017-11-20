using System;
using System.Net;

namespace ImportGeoNames
{
    internal class ExtendedWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);

            if (webRequest != null)
            {
                webRequest.Timeout = int.MaxValue;
            }

            return webRequest;
        }
    }
}