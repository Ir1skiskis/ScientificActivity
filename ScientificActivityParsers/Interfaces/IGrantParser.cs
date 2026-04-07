using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IGrantParser
    {
        Task<List<GrantImportModel>> ParseAsync(CancellationToken cancellationToken = default);
    }
}
