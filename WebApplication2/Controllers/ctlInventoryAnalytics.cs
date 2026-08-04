using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlInventoryAnalytics")]
    public class ctlInventoryAnalytics : Controller
    {
        [HttpGet]
        [Route("GetSerialTrackingReport")]
        public string GetSerialTrackingReport(Guid ItemGuid, int InvoiceType, string SerialNumber,
            DateTime date1, DateTime date2, int CompanyID)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            DataTable dt = analytics.SelectSerialTrackingReport(ItemGuid, InvoiceType, SerialNumber, date1, date2, CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetExpiryLotsReport")]
        public string GetExpiryLotsReport(Guid ItemGuid, int InvoiceType, string LotNumber, int DaysAhead,
            DateTime date1, DateTime date2, int CompanyID)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            DataTable dt = analytics.SelectExpiryLotsReport(ItemGuid, InvoiceType, LotNumber, DaysAhead, date1, date2, CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetInvoiceTaxSummaryReport")]
        public string GetInvoiceTaxSummaryReport(int InvoiceType, int BranchID, int BusinessPartnerID,
            DateTime date1, DateTime date2, int CompanyID)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            DataTable dt = analytics.SelectInvoiceTaxSummaryReport(InvoiceType, BranchID, BusinessPartnerID, date1, date2, CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetInventoryOperationsDashboard")]
        public string GetInventoryOperationsDashboard(int CompanyID,
            DateTime date1, DateTime date2, DateTime compareDate1, DateTime compareDate2)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            var result = new
            {
                PeriodFrom = date1,
                PeriodTo = date2,
                CompareFrom = compareDate1,
                CompareTo = compareDate2,
                Summary = analytics.SelectInventoryOperationsSummary(CompanyID),
                PeriodSummary = analytics.SelectInventoryOperationsPeriodSummary(CompanyID, date1, date2),
                CompareSummary = analytics.SelectInventoryOperationsPeriodSummary(CompanyID, compareDate1, compareDate2),
                MovementTrend = analytics.SelectInventoryMovementTrend(CompanyID, date1, date2),
                TopItems = analytics.SelectInventoryTopItemsByMovement(CompanyID, 10, date1, date2),
                UpcomingExpiry = analytics.SelectInventoryUpcomingExpiry(CompanyID, 10),
            };
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetInvoiceAnalyticsDashboard")]
        public string GetInvoiceAnalyticsDashboard(int CompanyID,
            DateTime date1, DateTime date2, DateTime compareDate1, DateTime compareDate2)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            var result = new
            {
                PeriodFrom = date1,
                PeriodTo = date2,
                CompareFrom = compareDate1,
                CompareTo = compareDate2,
                Summary = analytics.SelectInvoiceAnalyticsSummary(CompanyID, date1, date2),
                CompareSummary = analytics.SelectInvoiceAnalyticsSummary(CompanyID, compareDate1, compareDate2),
                MonthlyTrend = analytics.SelectInvoiceAnalyticsMonthlyTrend(CompanyID, date1, date2),
                TaxBreakdown = analytics.SelectInvoiceAnalyticsTaxBreakdown(CompanyID, date1, date2),
                CompareTaxBreakdown = analytics.SelectInvoiceAnalyticsTaxBreakdown(CompanyID, compareDate1, compareDate2),
                PaymentMix = analytics.SelectInvoiceAnalyticsPaymentMix(CompanyID, date1, date2),
                ComparePaymentMix = analytics.SelectInvoiceAnalyticsPaymentMix(CompanyID, compareDate1, compareDate2),
                RecentInvoices = analytics.SelectInvoiceAnalyticsRecentInvoices(CompanyID, 8, date1, date2),
            };
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetInventoryValuationDashboard")]
        public string GetInventoryValuationDashboard(int CompanyID, int StoreID,
            DateTime date1, DateTime date2)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            var result = new
            {
                PeriodFrom = date1,
                PeriodTo = date2,
                Summary = analytics.SelectInventoryValuationSummary(CompanyID, StoreID),
                ValueByStore = analytics.SelectInventoryValueByStore(CompanyID),
                TopItemsByValue = analytics.SelectTopItemsByValue(CompanyID, 10, StoreID),
                Turnover = analytics.SelectInventoryTurnover(CompanyID, date1, date2, StoreID),
            };
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetStockValuationReport")]
        public string GetStockValuationReport(int CompanyID, int StoreID, Guid ItemGuid)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            DataTable dt = analytics.SelectStockValuationReport(CompanyID, StoreID, ItemGuid);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetSlowMovingReport")]
        public string GetSlowMovingReport(int CompanyID, int Days, int StoreID)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            DataTable dt = analytics.SelectSlowMovingItems(CompanyID, Days, StoreID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetReorderReport")]
        public string GetReorderReport(int CompanyID, int StoreID)
        {
            clsInventoryAnalytics analytics = new clsInventoryAnalytics();
            DataTable dt = analytics.SelectReorderReport(CompanyID, StoreID);
            return JsonConvert.SerializeObject(dt);
        }
    }
}
