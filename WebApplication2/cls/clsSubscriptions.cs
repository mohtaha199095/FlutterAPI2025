using System.Data.SqlClient;
using System.Data;
using System;

namespace WebApplication2.cls
{
    public class clsSubscriptions
    {

        public DataTable SelectSubscriptions(int Id, int BusinessPartnerID, 
            int SubscriptionTypeID, int TransactionStatusID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
  new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID },
   new SqlParameter("@SubscriptionTypeID", SqlDbType.Int) { Value = SubscriptionTypeID },
    new SqlParameter("@TransactionStatusID", SqlDbType.Int) { Value = TransactionStatusID },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_Subscriptions.*
,tbl_SubscriptionsStatus.AName as SubscriptionsStatusAName
,tbl_SubscriptionsTypes.AName as SubscriptionsTypesAName
,tbl_BusinessPartner.AName as BusinessPartnerAName
,tbl_BusinessPartner.empcode as EmpCode
,tbl_SubscriptionsTypes.code as subscriptionsTypesCode
,tbl_BusinessPartner.job as Job
 from tbl_Subscriptions
left join tbl_SubscriptionsStatus on tbl_SubscriptionsStatus.ID = tbl_Subscriptions.TransactionStatusID
left join tbl_SubscriptionsTypes on tbl_SubscriptionsTypes.ID = tbl_Subscriptions.SubscriptionTypeID
left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_Subscriptions.BusinessPartnerID




 
where (tbl_Subscriptions.id=@Id or @Id=0 ) 
and (tbl_Subscriptions.BusinessPartnerID=@BusinessPartnerID or @BusinessPartnerID='' ) 
and (tbl_Subscriptions.SubscriptionTypeID=@SubscriptionTypeID or @SubscriptionTypeID='' )   
and (tbl_Subscriptions.TransactionStatusID=@TransactionStatusID or @TransactionStatusID='' ) 
and (tbl_Subscriptions.CompanyID=@CompanyID or @CompanyID=0 )
                     ", prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteSubscriptionsByID(int Id)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Subscriptions where (id=@Id  )", prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertSubscriptions(
          
            int BusinessPartnerID,
            int SubscriptionTypeID,
            DateTime TransactionDate,
            int TransactionStatusID,
            double Amount,            
            int CompanyID,
            int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 {
              new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID },
              new SqlParameter("@SubscriptionTypeID", SqlDbType.Int) { Value = SubscriptionTypeID },
              new SqlParameter("@TransactionDate", SqlDbType.DateTime) { Value = TransactionDate },
              new SqlParameter("@TransactionStatusID", SqlDbType.Int) { Value = TransactionStatusID },
              new SqlParameter("@Amount", SqlDbType.Decimal) { Value = Amount },
              new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
              new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
              new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_Subscriptions
(BusinessPartnerID,SubscriptionTypeID,
TransactionDate,TransactionStatusID,Amount,
CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.ID values(@BusinessPartnerID,@SubscriptionTypeID,
@TransactionDate,@TransactionStatusID,@Amount
,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm));

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
