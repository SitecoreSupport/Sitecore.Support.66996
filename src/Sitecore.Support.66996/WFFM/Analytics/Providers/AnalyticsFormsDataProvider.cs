using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Sitecore.Analytics.Reporting;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.WFFM.Abstractions;
using Sitecore.WFFM.Abstractions.Analytics;
using Sitecore.WFFM.Abstractions.Data;
using Sitecore.WFFM.Abstractions.Shared;
using Sitecore.WFFM.Analytics.Dependencies;
using Sitecore.WFFM.Analytics.Queries;
using IDs = Sitecore.Form.Core.Configuration.IDs;
using SortDirection = System.Web.UI.WebControls.SortDirection;

namespace Sitecore.Support.WFFM.Analytics.Providers
{
    public class AnalyticsFormsDataProvider : Sitecore.WFFM.Analytics.Providers.AnalyticsFormsDataProvider
    {
        private readonly ReportDataProviderBase reportDataProvider;

        private readonly Sitecore.WFFM.Abstractions.Shared.ILogger logger;

        private readonly IAnalyticsTracker analyticsTracker;

        private readonly ISettings settings;

        public AnalyticsFormsDataProvider(ReportDataProviderWrapper reportDataProviderWrapper, Sitecore.WFFM.Abstractions.Shared.ILogger logger, IAnalyticsTracker analyticsTracker, ISettings settings) : base(reportDataProviderWrapper, logger, analyticsTracker, settings)
        {
            Assert.IsNotNull(reportDataProviderWrapper, "reportDataProviderWrapper");
            Assert.IsNotNull(logger, "logger");
            Assert.IsNotNull(analyticsTracker, "analyticsTracker");
            Assert.ArgumentNotNull(settings, "settings");
            this.reportDataProvider = reportDataProviderWrapper.GetReportDataProviderBase(false);
            this.logger = logger;
            this.analyticsTracker = analyticsTracker;
            this.settings = settings;
        }

        public override IEnumerable<IFormContactsResult> GetFormsStatisticsByContact(Guid formId, PageCriteria pageCriteria)
        {
            bool flag = !this.settings.IsXdbEnabled;
            IEnumerable<IFormContactsResult> result;
            if (flag)
            {
                result = new List<IFormContactsResult>();
            }
            else
            {
                ID formStatisticsByContactsReportQuery = Sitecore.WFFM.Abstractions.Analytics.IDs.FormStatisticsByContactsReportQuery;
                FormStatisticsByContactReportQuery formStatisticsByContactReportQuery = new FormStatisticsByContactReportQuery(formId, formStatisticsByContactsReportQuery, this.reportDataProvider, null, CachingPolicy.WithCacheDisabled);
                formStatisticsByContactReportQuery.Execute();
                result = this.Sort(formStatisticsByContactReportQuery.Data, pageCriteria.Sorting).Skip(pageCriteria.PageIndex).Take(pageCriteria.PageSize);
            }
            return result;
        }

        private Data.SortCriteria CreateSortCriteria(PageCriteria pageCriteria)
        {
            if (pageCriteria.Sorting != null)
            {
                System.Web.UI.WebControls.SortDirection direction = pageCriteria.Sorting.Direction ==
                                          Sitecore.WFFM.Abstractions.Data.SortDirection.Asc
                    ? System.Web.UI.WebControls.SortDirection.Ascending 
                    : SortDirection.Descending;
                return new Data.SortCriteria(pageCriteria.Sorting.Field, direction);
            }
            return null;
        }


        private IQueryable<IFormContactsResult> Sort(IQueryable<IFormContactsResult> unsortedList, Sitecore.WFFM.Abstractions.Data.SortCriteria sortCriteria)
        {
            if (sortCriteria == null)
            {
                return unsortedList;
            }
            try
            {
                IQueryable<IFormContactsResult> result;
                if (sortCriteria.Direction != Sitecore.WFFM.Abstractions.Data.SortDirection.Desc)
                {
                    result = (from info in unsortedList
                              orderby DataBinder.Eval(info, sortCriteria.Field)
                              select info);
                }
                else
                {
                    result = (from info in unsortedList
                              orderby DataBinder.Eval(info, sortCriteria.Field) descending
                              select info);
                }
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Log(string.Format("An error occurred during messages sorting with {0} argument.", sortCriteria.Field), this, LogMessageType.Error);
                return unsortedList;
            }
        }
    }
}