using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Handlers
{
    public class RetrypolicyDelegatingHandler : DelegatingHandler
    {
        private readonly int _maximumAmountOfRetries = 3;

        public RetrypolicyDelegatingHandler(int maximumAmountOfRetries) : base()
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        public RetrypolicyDelegatingHandler(HttpMessageHandler innerHandler, int maximumAmountOfRetries) : base(innerHandler)
        {
            _maximumAmountOfRetries = maximumAmountOfRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
        {
            for (int i = 0; i < _maximumAmountOfRetries; i++)
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
