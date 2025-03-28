using TVOnline.Service.DTO;
using TVOnline.Service.Location;
using System.Collections.Generic;

namespace TVOnline.ViewModels.Employer
{
    public class CompaniesListViewModel
    {
        public List<EmployerResponse> Employers { get; set; }
        public List<CitiesResponse> Cities { get; set; }
    }
}
