using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlDocumentFlow")]
    public class ctlDocumentFlow : Controller
    {
        // ==========================================================
        // SELECT FLOW HEADER (FILTER)
        // ==========================================================
        [HttpGet]
        [Route("SelectFlowHeader")]
        public string SelectFlowHeader(
            int ID,
            string TransactionGuidFrom,
            string TransactionGuidTo,
            int FlowTypeID,
            int FlowActionID,
            int StatusID,
            int CompanyID)
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();

                Guid gFrom = string.IsNullOrEmpty(TransactionGuidFrom) ? Guid.Empty : Guid.Parse(TransactionGuidFrom);
                Guid gTo = string.IsNullOrEmpty(TransactionGuidTo) ? Guid.Empty : Guid.Parse(TransactionGuidTo);

                DataTable dt = obj.SelectFlowHeader(
                    ID,
                    gFrom,
                    gTo,
                    FlowTypeID,
                    FlowActionID,
                    StatusID,
                    CompanyID
                );

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT FLOW DETAILS BY HEADER ID
        // ==========================================================
        [HttpGet]
        [Route("SelectFlowDetailsByHeaderID")]
        public string SelectFlowDetailsByHeaderID(int HeaderID, int CompanyID)
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();
                DataTable dt = obj.SelectFlowDetailsByHeaderID(HeaderID, CompanyID);

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT FLOW HEADER
        // ==========================================================
        [HttpPost]
        [Route("InsertFlowHeader")]
        public int InsertFlowHeader(
            int FlowTypeID,
            int FlowActionID,
            int StatusID,
            string TransactionGuidFrom,
            string TransactionGuidTo,
            int TransactionTypeIDFrom,
            int TransactionTypeIDTo,
            string ReferenceNo,
            string Notes,
            int CreationUserID,
            int CompanyID
        )
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();

                Guid gFrom = string.IsNullOrEmpty(TransactionGuidFrom) ? Guid.Empty : Guid.Parse(TransactionGuidFrom);
                Guid gTo = string.IsNullOrEmpty(TransactionGuidTo) ? Guid.Empty : Guid.Parse(TransactionGuidTo);

                return obj.InsertFlowHeader(
                    FlowTypeID,
                    FlowActionID,
                    StatusID,
                    gFrom,
                    gTo,
                    TransactionTypeIDFrom,
                    TransactionTypeIDTo,
                    Simulate.String(ReferenceNo),
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserID
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE FLOW HEADER STATUS
        // ==========================================================
        [HttpPost]
        [Route("UpdateFlowHeaderStatus")]
        public int UpdateFlowHeaderStatus(int HeaderID, int StatusID, int ModificationUserID, int CompanyID)
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();
                return obj.UpdateFlowHeaderStatus(HeaderID, StatusID, ModificationUserID, CompanyID);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE FLOW HEADER (ALSO DELETES DETAILS)
        // ==========================================================
        [HttpGet]
        [Route("DeleteFlowHeader")]
        public bool DeleteFlowHeader(int HeaderID, int CompanyID)
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();
                return obj.DeleteFlowHeaderByID(HeaderID, CompanyID);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT FLOW DETAIL (QTY)
        // ==========================================================
        [HttpPost]
        [Route("InsertFlowDetailQty")]
        public int InsertFlowDetailQty(
            int HeaderID,
            string TransactionLineGuidFrom,
            string TransactionLineGuidTo,
            string ItemGuid,
            decimal Qty,
            string Notes,
            int CreationUserID,
            int CompanyID
        )
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();

                Guid gFromLine = string.IsNullOrEmpty(TransactionLineGuidFrom) ? Guid.Empty : Guid.Parse(TransactionLineGuidFrom);
                Guid gToLine = string.IsNullOrEmpty(TransactionLineGuidTo) ? Guid.Empty : Guid.Parse(TransactionLineGuidTo);
                Guid gItem = string.IsNullOrEmpty(ItemGuid) ? Guid.Empty : Guid.Parse(ItemGuid);

                return obj.InsertFlowDetailQty(
                    HeaderID,
                    gFromLine,
                    gToLine,
                    gItem,
                    Qty,
                    CompanyID,
                    CreationUserID,
                    Simulate.String(Notes)
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT FLOW DETAIL (AMOUNT) - used for payments
        // ==========================================================
        [HttpPost]
        [Route("InsertFlowDetailAmount")]
        public int InsertFlowDetailAmount(
            int HeaderID,
            decimal Amount,
            int CurrencyID,
            decimal Rate,
            string Notes,
            int CreationUserID,
            int CompanyID
        )
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();

                return obj.InsertFlowDetailAmount(
                    HeaderID,
                    Amount,
                    CurrencyID,
                    Rate,
                    CompanyID,
                    CreationUserID,
                    Simulate.String(Notes)
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT RELATED DOCS (1 LEVEL UP/DOWN) BY TRANSACTION GUID
        // ==========================================================
        [HttpGet]
        [Route("SelectRelatedDocsByTransactionGuid")]
        public string SelectRelatedDocsByTransactionGuid(string TransactionGuid, int CompanyID)
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();

                Guid g = string.IsNullOrEmpty(TransactionGuid) ? Guid.Empty : Guid.Parse(TransactionGuid);

                DataTable dt = obj.SelectRelatedDocsByTransactionGuid(g, CompanyID);

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // OPTIONAL: Insert full flow in one request (Header + Details)
        // BODY JSON FORMAT:
        // {
        //   "Details":[
        //     {"ValueTypeID":1,"TransactionLineGuidFrom":"...","TransactionLineGuidTo":"...","ItemGuid":"...","Qty":2,"Amount":null,"CurrencyID":0,"Rate":1,"Notes":"..."},
        //     {"ValueTypeID":2,"Amount":100,"CurrencyID":784,"Rate":1,"Notes":"Payment alloc"}
        //   ]
        // }
        // ==========================================================
        [HttpPost]
        [Route("InsertFlowFull")]
        public int InsertFlowFull(
            int FlowTypeID,
            int FlowActionID,
            int StatusID,
            string TransactionGuidFrom,
            string TransactionGuidTo,
            int TransactionTypeIDFrom,
            int TransactionTypeIDTo,
            string ReferenceNo,
            string Notes,
            int CreationUserID,
            int CompanyID,
            [FromBody] dynamic body
        )
        {
            try
            {
                clsDocumentFlow obj = new clsDocumentFlow();

                Guid gFrom = string.IsNullOrEmpty(TransactionGuidFrom) ? Guid.Empty : Guid.Parse(TransactionGuidFrom);
                Guid gTo = string.IsNullOrEmpty(TransactionGuidTo) ? Guid.Empty : Guid.Parse(TransactionGuidTo);

                // 1) Insert header
                int headerID = obj.InsertFlowHeader(
                    FlowTypeID,
                    FlowActionID,
                    StatusID,
                    gFrom,
                    gTo,
                    TransactionTypeIDFrom,
                    TransactionTypeIDTo,
                    Simulate.String(ReferenceNo),
                    Simulate.String(Notes),
                    CompanyID,
                    CreationUserID
                );

                // 2) Insert details (optional)
                if (body != null)
                {
                    string json = body.ToString();
                    var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);

                    if (jsonObj["Details"] != null)
                    {
                        var arr = jsonObj["Details"] as Newtonsoft.Json.Linq.JArray;

                        if (arr != null)
                        {
                            foreach (var row in arr)
                            {
                                int valueType = Simulate.Integer32(row["ValueTypeID"]);

                                string lineFrom = Simulate.String(row["TransactionLineGuidFrom"]);
                                string lineTo = Simulate.String(row["TransactionLineGuidTo"]);
                                string itemGuid = Simulate.String(row["ItemGuid"]);
                                string rowNotes = Simulate.String(row["Notes"]);

                                if (valueType == 1) // Qty
                                {
                                    decimal qty = Simulate.decimal_(row["Qty"]);
                                    Guid gLineFrom = string.IsNullOrEmpty(lineFrom) ? Guid.Empty : Guid.Parse(lineFrom);
                                    Guid gLineTo = string.IsNullOrEmpty(lineTo) ? Guid.Empty : Guid.Parse(lineTo);
                                    Guid gItem = string.IsNullOrEmpty(itemGuid) ? Guid.Empty : Guid.Parse(itemGuid);

                                    obj.InsertFlowDetailQty(
                                        headerID,
                                        gLineFrom,
                                        gLineTo,
                                        gItem,
                                        qty,
                                        CompanyID,
                                        CreationUserID,
                                        rowNotes
                                    );
                                }
                                else if (valueType == 2) // Amount
                                {
                                    decimal amount = Simulate.decimal_(row["Amount"]);
                                    int currencyID = Simulate.Integer32(row["CurrencyID"]);
                                    decimal rate = row["Rate"] == null ? 1 : Simulate.decimal_(row["Rate"]);

                                    obj.InsertFlowDetailAmount(
                                        headerID,
                                        amount,
                                        currencyID,
                                        rate,
                                        CompanyID,
                                        CreationUserID,
                                        rowNotes
                                    );
                                }
                                else
                                {
                                    // Ignore unknown
                                }
                            }
                        }
                    }
                }

                return headerID;
            }
            catch
            {
                throw;
            }
        }
    }
}
