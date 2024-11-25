using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsCashVoucherHeader
    {

        public DataTable SelectCashVoucherHeaderByGuid(string guid, DateTime date1, DateTime date2, int VoucherType, int BranchID, int CompanyID, string RelatedFinancingGuid, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },
 new SqlParameter("@date1", SqlDbType.DateTime) { Value =Simulate.StringToDate(  date1 )},
  new SqlParameter("@date2", SqlDbType.DateTime) { Value = Simulate.StringToDate(  date2 )},
   new SqlParameter("@VoucherType", SqlDbType.Int) { Value = VoucherType },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
            new SqlParameter("@RelatedFinancingGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( RelatedFinancingGuid) },

          
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_CashVoucherHeader.*   ,
tbl_Branch.AName as BranchAName,tbl_CashVoucherHeader.AccountID,
case when tbl_CashVoucherHeader.accountid=(select AccountID from tbl_AccountSetting where CompanyID = @companyid and AccountRefID = 5 and Active  =1) 
then  tbl_CashDrawer.AName  else tbl_Banks.AName end  as CashDrawerAName,tbl_CostCenter.AName as CostCenterAName,
 tbl_JournalVoucherTypes.aname as JournalVoucherTypesAname
 ,

 (SELECT ';' + isnull( tbl_BusinessPartner.AName ,tbl_Accounts.AName ) 
          FROM tbl_Accounts inner join tbl_CashVoucherDetails on tbl_CashVoucherDetails.AccountID = tbl_Accounts.ID
		  left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_CashVoucherDetails.SubAccountID
		  where tbl_CashVoucherDetails.HeaderGuid =tbl_CashVoucherHeader.Guid 
		    FOR XML PATH('')) as DetailsAccountsName
 ,

 (SELECT ';' + isnull( tbl_BusinessPartner.empcode , '' ) 
          FROM tbl_Accounts inner join tbl_CashVoucherDetails on tbl_CashVoucherDetails.AccountID = tbl_Accounts.ID
		  left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_CashVoucherDetails.SubAccountID
		  where tbl_CashVoucherDetails.HeaderGuid =tbl_CashVoucherHeader.Guid 
		    FOR XML PATH('')) as DetailsEMPCode,
			tbl_PaymentMethod.AName as PaymentMethodAName
 from tbl_CashVoucherHeader
 left join tbl_Branch on tbl_Branch.ID =tbl_CashVoucherHeader.BranchID
     left join tbl_Banks on tbl_Banks.ID =tbl_CashVoucherHeader.CashID
    left join tbl_CostCenter on tbl_CostCenter.ID =tbl_CashVoucherHeader.CostCenterID
  left join tbl_CashDrawer on tbl_CashDrawer.ID =tbl_CashVoucherHeader.CashID
  left join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID =tbl_CashVoucherHeader.VoucherType
   left join tbl_PaymentMethod on tbl_PaymentMethod.ID =tbl_CashVoucherHeader.PaymentMethodTypeID
where 
(tbl_CashVoucherHeader.Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' )  
and (tbl_CashVoucherHeader.RelatedFinancingGuid=@RelatedFinancingGuid or @RelatedFinancingGuid='00000000-0000-0000-0000-000000000000' )   
and (tbl_CashVoucherHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (tbl_CashVoucherHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_CashVoucherHeader.VoucherType=@VoucherType or @VoucherType=0 )
and cast( tbl_CashVoucherHeader.VoucherDate as date) between  cast(@date1 as date) and  cast(@date2 as date) 
order by voucherdate desc  ,voucherno desc
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteCashVoucherHeaderByGuid(string guid, int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_CashVoucherHeader where (guid=@guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertCashVoucherHeader(DBCashVoucherHeader DbCashVoucherHeader, SqlTransaction trn)
        {
            try
            {
                SqlParameter[] prm =
                   {
                    new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = DbCashVoucherHeader.VoucherDate },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DbCashVoucherHeader.BranchID },
                      new SqlParameter("@AccountID", SqlDbType.Int) { Value = DbCashVoucherHeader.AccountID },
                    
                    new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = DbCashVoucherHeader.CostCenterID },
                   new SqlParameter("@CashID", SqlDbType.Int) { Value = DbCashVoucherHeader.CashID },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = DbCashVoucherHeader.Amount },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = DbCashVoucherHeader.JVGuid },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.Note },
                    new SqlParameter("@VoucherNo", SqlDbType.Int) { Value = DbCashVoucherHeader.VoucherNo },
                    new SqlParameter("@ManualNo", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.ManualNo },
                    new SqlParameter("@VoucherType", SqlDbType.Int) { Value = DbCashVoucherHeader.VoucherType },
                    new SqlParameter("@RelatedInvoiceGuid", SqlDbType.UniqueIdentifier) { Value = DbCashVoucherHeader.RelatedInvoiceGuid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = DbCashVoucherHeader.CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = DbCashVoucherHeader.CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                 
                     new SqlParameter("@PaymentMethodTypeID", SqlDbType.Int) { Value = DbCashVoucherHeader.PaymentMethodTypeID },
                      new SqlParameter("@DueDate", SqlDbType.DateTime) { Value = DbCashVoucherHeader.DueDate },
                       new SqlParameter("@ChequeNote", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.ChequeNote },
                        new SqlParameter("@ChequeName", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.ChequeName },
                                new SqlParameter("@RelatedFinancingGuid", SqlDbType.UniqueIdentifier) { Value = DbCashVoucherHeader.RelatedFinancingGuid },

                };

                string a = @"insert into tbl_CashVoucherHeader (VoucherDate,BranchID,CostCenterID,AccountID,CashID,Amount,JVGuid,
                                                                Note,VoucherNo,ManualNo,VoucherType,RelatedInvoiceGuid,
                                                               CompanyID,CreationUserID,CreationDate,PaymentMethodTypeID ,DueDate, ChequeNote ,ChequeName,RelatedFinancingGuid)  
OUTPUT INSERTED.Guid  
values (@VoucherDate,@BranchID,@CostCenterID,@AccountID,@CashID,@Amount,@JVGuid,
                                                               @Note,@VoucherNo,@ManualNo,@VoucherType,@RelatedInvoiceGuid,
                                                               @CompanyID,@CreationUserID,@CreationDate,@PaymentMethodTypeID ,@DueDate, @ChequeNote ,@ChequeName,@RelatedFinancingGuid)  ";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(DbCashVoucherHeader.CompanyID), trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public string UpdateCashVoucherHeader(DBCashVoucherHeader DbCashVoucherHeader,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = DbCashVoucherHeader.Guid },
                    new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = DbCashVoucherHeader.VoucherDate },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DbCashVoucherHeader.BranchID },
                     new SqlParameter("@AccountID", SqlDbType.Int) { Value = DbCashVoucherHeader.AccountID },
                    new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = DbCashVoucherHeader.CostCenterID },
                    new SqlParameter("@CashID", SqlDbType.Int) { Value = DbCashVoucherHeader.CashID },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = DbCashVoucherHeader.Amount },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = DbCashVoucherHeader.JVGuid },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.Note },

                    new SqlParameter("@ManualNo", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.ManualNo },
                    new SqlParameter("@VoucherType", SqlDbType.Int) { Value = DbCashVoucherHeader.VoucherType },
                    new SqlParameter("@RelatedInvoiceGuid", SqlDbType.UniqueIdentifier) { Value = DbCashVoucherHeader.RelatedInvoiceGuid },

                     new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = DbCashVoucherHeader.ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                      new SqlParameter("@PaymentMethodTypeID", SqlDbType.Int) { Value = DbCashVoucherHeader.PaymentMethodTypeID },
                      new SqlParameter("@DueDate", SqlDbType.DateTime) { Value = DbCashVoucherHeader.DueDate },
                       new SqlParameter("@ChequeNote", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.ChequeNote },
                        new SqlParameter("@ChequeName", SqlDbType.NVarChar,-1) { Value = DbCashVoucherHeader.ChequeName },
                };
                string a = @"update tbl_CashVoucherHeader set  

 VoucherDate=@VoucherDate,
BranchID=@BranchID,
CostCenterID=@CostCenterID,
Amount=@Amount,
JVGuid=@JVGuid,
Note=@Note,
AccountID=@AccountID,
 CashID=@CashID,
ManualNo=@ManualNo,
VoucherType=@VoucherType,
RelatedInvoiceGuid=@RelatedInvoiceGuid,
ModificationUserID=@ModificationUserID,
ModificationDate=@ModificationDate   ,

PaymentMethodTypeID=@PaymentMethodTypeID,
DueDate=@DueDate,
ChequeNote=@ChequeNote,
ChequeName=@ChequeName


 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public string UpdateCashVoucherHeaderJVGuid(string Guid, string JVGuid,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(JVGuid) },

                };
                string a = @"update tbl_CashVoucherHeader set  

  
 JVGuid=@JVGuid  
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public bool InsertCashVoucherJournalVoucher(string CashVoucherGuid,int CashAccount, int BranchID, int CostCenterID, int CashID, decimal Amount, string Note, DateTime VoucherDate, List<DBCashVoucherDetails> dbCashVoucherDetails, string JVGuid, int JVTypeID, int CompanyID, int CreationUserID, SqlTransaction trn)
        {
            try
            {
                bool IsSaved = true;

                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dtMaxJVNumber = clsJournalVoucherHeader.SelectMaxJVNo(JVGuid, JVTypeID, CompanyID, trn);
                int MaxJVNumber = 0;
                if (dtMaxJVNumber != null && dtMaxJVNumber.Rows.Count > 0)
                {

                    MaxJVNumber = Simulate.Integer32(dtMaxJVNumber.Rows[0][0]) + 1;
                }
                else { MaxJVNumber = 1; }
                if (JVGuid == "")
                {

                    JVGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, CostCenterID, Note, Simulate.String(MaxJVNumber), JVTypeID, CompanyID, VoucherDate, CreationUserID, "", 0,trn);
                }
                else
                {
                    clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, CostCenterID, Note, Simulate.String(MaxJVNumber), JVTypeID, VoucherDate, JVGuid, CreationUserID, "", 0,CompanyID, trn);

                    clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid,CompanyID, trn);
                }
                if (JVGuid == "")
                {

                    IsSaved = false;
                }
                UpdateCashVoucherHeaderJVGuid(CashVoucherGuid, JVGuid,CompanyID, trn);
                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting(); clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();

                DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, CompanyID, trn);
              //  int CashAccount = 0;
              //  CashAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CashAccount), 2);

                if (JVTypeID == (int)clsEnum.VoucherType.CashPayment)
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount
                                   , CashID, 0, Amount, -1 * Amount
                                   , BranchID, CostCenterID, DateTime.Now, Simulate.String(Note), CompanyID
                                   , CreationUserID, "",trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }
                }
                else
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount
                                   , CashID, Amount, 0, Amount
                                   , BranchID, CostCenterID, DateTime.Now, Simulate.String(Note), CompanyID
                                   , CreationUserID, "",trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }

                }
                for (int i = 0; i < dbCashVoucherDetails.Count; i++)
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, i + 1, dbCashVoucherDetails[i].AccountID
                            , dbCashVoucherDetails[i].SubAccountID, dbCashVoucherDetails[i].Debit, dbCashVoucherDetails[i].Credit, dbCashVoucherDetails[i].Debit - dbCashVoucherDetails[i].Credit
                            , dbCashVoucherDetails[i].BranchID, dbCashVoucherDetails[i].CostCenterID, DateTime.Now, Simulate.String(dbCashVoucherDetails[i].Note), dbCashVoucherDetails[i].CompanyID
                            , CreationUserID, "",trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }
                }
                bool test = clsJournalVoucherHeader.CheckJVMatch(JVGuid,CompanyID, trn);
                if (!test)
                {
                    IsSaved = false;
                }
                return IsSaved;
            }
            catch (Exception)
            {

                return false;
            }




        }



    }
    public class DBCashVoucherHeader
    {


        public Guid? Guid { get; set; }
        public DateTime VoucherDate { get; set; }
        public int BranchID { get; set; }
        public int CostCenterID { get; set; }
        public int CashID { get; set; }
        public int AccountID { get; set; }
        
        public decimal Amount { get; set; }
        public Guid JVGuid { get; set; }
        public string Note { get; set; }
        public int VoucherNo { get; set; }
        public string ManualNo { get; set; }
        public int VoucherType { get; set; }
        public Guid RelatedInvoiceGuid { get; set; }
        public int CompanyID { get; set; }
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }
        public int? ModificationUserID { get; set; }
        public DateTime? ModificationDate { get; set; }


        public DateTime DueDate { get; set; }
        public int PaymentMethodTypeID { get; set; }
        public string ChequeNote { get; set; }

        public string ChequeName { get; set; }
        public Guid RelatedFinancingGuid { get; set; }

        

    }
}
