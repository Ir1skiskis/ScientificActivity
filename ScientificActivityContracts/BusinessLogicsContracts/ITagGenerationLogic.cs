using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BusinessLogicsContracts
{
    public interface ITagGenerationLogic
    {
        int RegenerateConferenceTags();
        int RegenerateGrantTags();
        int RegenerateJournalTags();
        int RegenerateAllTags();
    }
}
