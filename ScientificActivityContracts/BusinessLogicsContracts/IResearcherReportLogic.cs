using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BusinessLogicsContracts
{
    public interface IResearcherReportLogic
    {
        ResearcherReportViewModel BuildReport(ResearcherReportSettingsBindingModel model);

        byte[] GeneratePdf(ResearcherReportSettingsBindingModel model);

        byte[] GenerateDocx(ResearcherReportSettingsBindingModel model);
    }
}
