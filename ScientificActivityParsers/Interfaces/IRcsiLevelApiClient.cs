using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IRcsiLevelApiClient
    {
        Task<RcsiLevelApiResponseModel?> GetByIssnAsync(string issn, CancellationToken cancellationToken = default);
    }
}
