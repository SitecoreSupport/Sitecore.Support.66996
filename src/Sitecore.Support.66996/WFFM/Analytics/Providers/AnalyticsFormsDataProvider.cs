using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.WFFM.Abstractions;
using Sitecore.WFFM.Abstractions.Analytics;
using Sitecore.WFFM.Abstractions.Data;
using Sitecore.WFFM.Abstractions.Shared;
using Sitecore.WFFM.Analytics.Dependencies;
using Sitecore.WFFM.Analytics.Model;
using Sitecore.WFFM.Analytics.Queries;
using Sitecore.Xdb.Reporting;
using SortDirection = System.Web.UI.WebControls.SortDirection;

namespace Sitecore.Support.WFFM.Analytics.Providers
{
    public class AnalyticsFormsDataProvider : IWffmDataProvider
    {
        private readonly ReportDataProviderBase _reportDataProvider;

        private readonly ILogger _logger;

        private readonly ISettings _settings;

        public AnalyticsFormsDataProvider(ReportDataProviderWrapper reportDataProviderWrapper, ILogger logger, ISettings settings)
        {
            Assert.IsNotNull(reportDataProviderWrapper, "reportDataProviderWrapper");
            Assert.IsNotNull(logger, "logger");
            Assert.ArgumentNotNull(settings, "settings");
            this._reportDataProvider = reportDataProviderWrapper.GetReportDataProviderBase(false);
            this._logger = logger;
            this._settings = settings;
        }

        public virtual IEnumerable<Sitecore.WFFM.Abstractions.Analytics.FormData> GetFormData(Guid formId)
        {
            if (!this._settings.IsXdbEnabled)
            {
                return new List<Sitecore.WFFM.Abstractions.Analytics.FormData>();
            }
            ID formDataReportQuery = Sitecore.WFFM.Abstractions.Analytics.IDs.FormDataReportQuery;
            FormDataReportQuery expr_2B = new FormDataReportQuery(formId, formDataReportQuery, this._reportDataProvider, CachingPolicy.WithCacheDisabled);
            expr_2B.Execute();
            return expr_2B.Data;
        }

        public virtual void InsertFormData(Sitecore.WFFM.Abstractions.Analytics.FormData form)
        {
            if (!this._settings.IsXdbTrackerEnabled)
            {
                this._logger.Warn("Cannot save form data to Db", this);
            }
        }

        public virtual IEnumerable<IFormContactsResult> GetFormsStatisticsByContact(Guid formId, PageCriteria pageCriteria)
        {
            if (!this._settings.IsXdbEnabled)
            {
                return new List<IFormContactsResult>();
            }
            ID formStatisticsByContactsReportQuery = Sitecore.WFFM.Abstractions.Analytics.IDs.FormStatisticsByContactsReportQuery;
            FormStatisticsByContactReportQuery expr_2C = new FormStatisticsByContactReportQuery(formId, formStatisticsByContactsReportQuery, this._reportDataProvider, null, CachingPolicy.WithCacheDisabled);
            expr_2C.Execute();
            //return expr_2C.Data.Skip(pageCriteria.PageIndex).Take(pageCriteria.PageSize);
            return this.Sort(expr_2C.Data, pageCriteria.Sorting).Skip(pageCriteria.PageIndex).Take(pageCriteria.PageSize);
        }

        public virtual IFormStatistics GetFormStatistics(Guid formId)
        {
            if (!this._settings.IsXdbEnabled)
            {
                return new FormStatistics();
            }
            ID formSubmitStatisticsReportQuery = Sitecore.WFFM.Abstractions.Analytics.IDs.FormSubmitStatisticsReportQuery;
            FormSummaryReportQuery formSummaryReportQuery = new FormSummaryReportQuery(formId, formSubmitStatisticsReportQuery, this._reportDataProvider, CachingPolicy.WithCacheDisabled);
            formSummaryReportQuery.Execute();
            return new FormStatistics
            {
                FormId = formId,
                Dropouts = formSummaryReportQuery.Dropouts,
                SubmitsCount = formSummaryReportQuery.SubmitsCount,
                Visits = formSummaryReportQuery.Visits,
                SuccessSubmits = formSummaryReportQuery.Success
            };
        }

        public virtual IEnumerable<IFormFieldStatistics> GetFormFieldsStatistics(Guid formId)
        {
            if (!this._settings.IsXdbEnabled)
            {
                return new List<IFormFieldStatistics>();
            }
            ID formFieldsStatisticsReportQuery = Sitecore.WFFM.Abstractions.Analytics.IDs.FormFieldsStatisticsReportQuery;
            FormFieldsStatisticsReportQuery expr_2B = new FormFieldsStatisticsReportQuery(formId, formFieldsStatisticsReportQuery, this._reportDataProvider, CachingPolicy.WithCacheDisabled);
            expr_2B.Execute();
            return expr_2B.Data;
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
                this._logger.Log(string.Format("An error occurred during messages sorting with {0} argument.", sortCriteria.Field), this, LogMessageType.Error);
                return unsortedList;
            }
        }
    }
}
