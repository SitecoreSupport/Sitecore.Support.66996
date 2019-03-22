using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Support.WFFM.Analytics.Providers.Utils;
using Sitecore.Web.Http.Filters;
using Sitecore.WFFM.Abstractions.Analytics;
using Sitecore.WFFM.Abstractions.Data;
using Sitecore.WFFM.Abstractions.Dependencies;
using Sitecore.WFFM.Abstractions.Shared;
using Sitecore.WFFM.Services.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Sitecore.Support.WFFM.Services.Requests.Controllers
{
    [AuthorizeSitecore(Roles = "sitecore\\Sitecore Client Users")]
    public class SupportFormReportsController : Controller
    {
        private readonly IItemRepository itemRepository;

        public IWffmDataProvider FormsDataProvider
        {
            get;
            private set;
        }

        public SupportFormReportsController() : this(DependenciesManager.DataProvider, DependenciesManager.Resolve<IItemRepository>())
        {
        }

        public SupportFormReportsController(IWffmDataProvider formsDataProvider, IItemRepository itemRepository)
        {
            Assert.ArgumentNotNull(formsDataProvider, "formsDataProvider");
            Assert.ArgumentNotNull(itemRepository, "itemRepository");
            this.FormsDataProvider = formsDataProvider;
            this.itemRepository = itemRepository;
        }

        [ValidateHttpAntiForgeryToken]
        public ActionResult GetFormContactsPage(Guid id, PageCriteria pageCriteria, string sortCriteria)
        {
            Assert.ArgumentNotNull(pageCriteria, "pageCriteria");
            pageCriteria.Sorting = SortingUtil.Create(sortCriteria);
            IEnumerable<IFormContactsResult> formsStatisticsByContact = this.FormsDataProvider.GetFormsStatisticsByContact(id, pageCriteria);
            return base.Json(new
            {
                Items = JsonConvert.SerializeObject(formsStatisticsByContact),
                HasResults = formsStatisticsByContact.Any<IFormContactsResult>()
            }, JsonRequestBehavior.AllowGet);
        }

        [ValidateHttpAntiForgeryToken]
        public ActionResult GetFormSummary(Guid id)
        {
            IFormStatistics formStatistics = this.FormsDataProvider.GetFormStatistics(id);
            string displayName = this.itemRepository.GetItemFromMasterDatabase(new ID(id)).DisplayName;
            formStatistics.Title = displayName;
            return base.Json(JsonConvert.SerializeObject(formStatistics), JsonRequestBehavior.AllowGet);
        }

        [ValidateHttpAntiForgeryToken]
        public ActionResult GetFormFieldsStatistics(Guid id)
        {
            IEnumerable<IFormFieldStatistics> formFieldsStatistics = this.FormsDataProvider.GetFormFieldsStatistics(id);
            foreach (IFormFieldStatistics current in formFieldsStatistics)
            {
                Item itemFromMasterDatabase = this.itemRepository.GetItemFromMasterDatabase(new ID(current.FieldId));
                if (itemFromMasterDatabase != null && this.itemRepository.CreateFieldItem(itemFromMasterDatabase).ParametersDictionary.ContainsKey("islist"))
                {
                    current.IsList = true;
                }
            }
            return base.Json(new
            {
                Items = JsonConvert.SerializeObject(formFieldsStatistics),
                legend = DependenciesManager.TranslationProvider.Text("Responses")
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
