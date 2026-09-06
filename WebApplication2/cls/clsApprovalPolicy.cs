using Microsoft.Data.SqlClient;

using System;

using System.Collections.Generic;

using System.Data;

using System.Linq;

using WebApplication2.MainClasses;

using static WebApplication2.MainClasses.clsEnum;



namespace WebApplication2.cls

{

    public class clsApprovalPolicy

    {

        private void EnsureSchema(int companyId)

        {

            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);

        }



        public DataTable SelectPolicies(int companyId, int documentTypeId = 0)

        {

            clsSQL sql = new clsSQL();

            SqlParameter[] prm =

            {

                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },

                new SqlParameter("@DocumentTypeID", SqlDbType.Int) { Value = documentTypeId },

            };



            return sql.ExecuteQueryStatement(@"

SELECT p.*,

       t.AName AS DocumentTypeAName,

       t.EName AS DocumentTypeEName,

       (SELECT COUNT(*) FROM tbl_ApprovalPolicyLevel l WHERE l.PolicyID = p.ID AND l.CompanyID = p.CompanyID) AS LevelCount

FROM tbl_ApprovalPolicy p

LEFT JOIN tbl_JournalVoucherTypes t ON t.id = p.DocumentTypeID

WHERE p.CompanyID = @CompanyID

  AND (@DocumentTypeID = 0 OR p.DocumentTypeID = @DocumentTypeID)

ORDER BY p.DocumentTypeID, p.BranchID", sql.CreateDataBaseConnectionString(companyId), prm);

        }



        public DataTable SelectPolicyLevels(int policyId, int companyId)

        {

            clsSQL sql = new clsSQL();

            SqlParameter[] prm =

            {

                new SqlParameter("@PolicyID", SqlDbType.Int) { Value = policyId },

                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },

            };



            return sql.ExecuteQueryStatement(@"

SELECT l.*,

       ISNULL(e.AName, e.EName) AS ApproverUserName

FROM tbl_ApprovalPolicyLevel l

LEFT JOIN tbl_employee e ON e.ID = l.ApproverUserID

WHERE l.PolicyID = @PolicyID AND l.CompanyID = @CompanyID

ORDER BY l.LevelNo", sql.CreateDataBaseConnectionString(companyId), prm);

        }



        public DataTable SelectLevelMembers(int policyLevelId, int companyId)

        {

            clsSQL sql = new clsSQL();

            SqlParameter[] prm =

            {

                new SqlParameter("@PolicyLevelID", SqlDbType.Int) { Value = policyLevelId },

                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },

            };



            return sql.ExecuteQueryStatement(@"

SELECT m.ApproverUserID,

       ISNULL(e.AName, e.EName) AS ApproverUserName

FROM tbl_ApprovalPolicyLevelMember m

LEFT JOIN tbl_employee e ON e.ID = m.ApproverUserID

WHERE m.PolicyLevelID = @PolicyLevelID AND m.CompanyID = @CompanyID

ORDER BY m.ApproverUserID", sql.CreateDataBaseConnectionString(companyId), prm);

        }



        public ApprovalPolicyRow ResolvePolicy(int companyId, int documentTypeId, int branchId, decimal amount)

        {

            DataTable dt = SelectPolicies(companyId, documentTypeId);

            if (dt == null || dt.Rows.Count == 0) return null;



            ApprovalPolicyRow best = null;

            foreach (DataRow row in dt.Rows)

            {

                if (!Simulate.Bool(row["IsEnabled"])) continue;



                int policyBranch = Simulate.Integer32(row["BranchID"]);

                if (policyBranch != 0 && branchId != 0 && policyBranch != branchId) continue;



                decimal minAmount = Simulate.Decimal(row["MinAmount"]);

                decimal maxAmount = Simulate.Decimal(row["MaxAmount"]);

                if (maxAmount > 0 && amount > maxAmount) continue;

                if (minAmount > 0 && amount < minAmount) continue;



                if (best == null || policyBranch > Simulate.Integer32(best.BranchID))

                {

                    best = MapPolicy(row);

                }

            }



            return best;

        }



        /// <summary>

        /// Finds an enabled policy for display (ignores policy-header amount limits).

        /// </summary>

        public ApprovalPolicyRow ResolveEnabledPolicy(int companyId, int documentTypeId, int branchId)

        {

            var policy = ResolveEnabledPolicyCore(companyId, documentTypeId, branchId);

            if (policy != null) return policy;

            if (branchId != 0)

                return ResolveEnabledPolicyCore(companyId, documentTypeId, 0);

            return null;

        }



        private ApprovalPolicyRow ResolveEnabledPolicyCore(int companyId, int documentTypeId, int branchId)

        {

            DataTable dt = SelectPolicies(companyId, documentTypeId);

            if (dt == null || dt.Rows.Count == 0) return null;



            ApprovalPolicyRow best = null;

            foreach (DataRow row in dt.Rows)

            {

                if (!Simulate.Bool(row["IsEnabled"])) continue;



                int policyBranch = Simulate.Integer32(row["BranchID"]);

                if (policyBranch != 0 && branchId != 0 && policyBranch != branchId) continue;



                if (best == null || policyBranch > Simulate.Integer32(best.BranchID))

                    best = MapPolicy(row);

            }



            return best;

        }



        public ApprovalPolicyRow ResolveEnabledPolicyAnyBranch(int companyId, int documentTypeId)

        {

            DataTable dt = SelectPolicies(companyId, documentTypeId);

            if (dt == null || dt.Rows.Count == 0) return null;



            foreach (DataRow row in dt.Rows)

            {

                if (Simulate.Bool(row["IsEnabled"]))

                    return MapPolicy(row);

            }



            return null;

        }



        public static bool IsLevelApplicableForAmount(ApprovalPolicyLevelRow level, decimal amount)

        {

            if (level == null) return false;

            if (amount < level.MinAmount) return false;

            if (level.MaxAmount > 0 && amount > level.MaxAmount) return false;

            return true;

        }



        public List<ApprovalPolicyLevelRow> GetLevels(int policyId, int companyId)

        {

            EnsureSchema(companyId);

            var list = new List<ApprovalPolicyLevelRow>();

            DataTable dt = SelectPolicyLevels(policyId, companyId);

            if (dt == null) return list;



            foreach (DataRow row in dt.Rows)

            {

                var level = MapLevel(row);

                LoadMembers(level, companyId);

                list.Add(level);

            }



            return list;

        }



        public List<ApprovalPolicyLevelRow> GetApplicableLevels(int policyId, int companyId, decimal amount)

        {

            var applicable = new List<ApprovalPolicyLevelRow>();

            foreach (var level in GetLevels(policyId, companyId))

            {

                if (amount < level.MinAmount) continue;

                if (level.MaxAmount > 0 && amount > level.MaxAmount) continue;

                if (level.MemberUserIds == null || level.MemberUserIds.Count == 0) continue;

                applicable.Add(level);

            }



            return applicable;

        }

        public List<ApprovalPolicyLevelRow> GetAllLevelsWithMembers(int policyId, int companyId)
        {
            var list = new List<ApprovalPolicyLevelRow>();
            foreach (var level in GetLevels(policyId, companyId))
            {
                if (level.MemberUserIds == null || level.MemberUserIds.Count == 0) continue;
                list.Add(level);
            }
            return list;
        }

        public int SavePolicy(ApprovalPolicySaveRequest req, SqlTransaction trn = null)

        {

            EnsureSchema(req.CompanyID);

            clsSQL sql = new clsSQL();

            string con = sql.CreateDataBaseConnectionString(req.CompanyID);

            int policyId = req.ID;



            if (policyId <= 0)

            {

                SqlParameter[] insertPrm =

                {

                    new SqlParameter("@DocumentTypeID", SqlDbType.Int) { Value = req.DocumentTypeID },

                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = req.BranchID },

                    new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = req.IsEnabled },

                    new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = req.MinAmount },

                    new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = req.MaxAmount },

                    new SqlParameter("@AllowSelfApproval", SqlDbType.Bit) { Value = req.AllowSelfApproval },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = req.CompanyID },

                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = req.UserID },

                };



                policyId = Simulate.Integer32(sql.ExecuteScalar(@"

INSERT INTO tbl_ApprovalPolicy

    (DocumentTypeID, BranchID, IsEnabled, MinAmount, MaxAmount, AllowSelfApproval,

     CompanyID, CreationUserID, CreationDate)

OUTPUT INSERTED.ID

VALUES

    (@DocumentTypeID, @BranchID, @IsEnabled, @MinAmount, @MaxAmount, @AllowSelfApproval,

     @CompanyID, @CreationUserID, GETDATE())", insertPrm, con));

            }

            else

            {

                SqlParameter[] updatePrm =

                {

                    new SqlParameter("@ID", SqlDbType.Int) { Value = policyId },

                    new SqlParameter("@DocumentTypeID", SqlDbType.Int) { Value = req.DocumentTypeID },

                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = req.BranchID },

                    new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = req.IsEnabled },

                    new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = req.MinAmount },

                    new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = req.MaxAmount },

                    new SqlParameter("@AllowSelfApproval", SqlDbType.Bit) { Value = req.AllowSelfApproval },

                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = req.UserID },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = req.CompanyID },

                };



                sql.ExecuteNonQueryStatement(@"

UPDATE tbl_ApprovalPolicy SET

    DocumentTypeID = @DocumentTypeID,

    BranchID = @BranchID,

    IsEnabled = @IsEnabled,

    MinAmount = @MinAmount,

    MaxAmount = @MaxAmount,

    AllowSelfApproval = @AllowSelfApproval,

    ModificationUserID = @ModificationUserID,

    ModificationDate = GETDATE()

WHERE ID = @ID AND CompanyID = @CompanyID", con, updatePrm, trn);

            }



            sql.ExecuteNonQueryStatement(@"

DELETE FROM tbl_ApprovalPolicyLevelMember

WHERE PolicyLevelID IN (

    SELECT ID FROM tbl_ApprovalPolicyLevel WHERE PolicyID = " + policyId + " AND CompanyID = " + req.CompanyID + ")",

                con, null, trn);



            sql.ExecuteNonQueryStatement(

                "DELETE FROM tbl_ApprovalPolicyLevel WHERE PolicyID = " + policyId + " AND CompanyID = " + req.CompanyID,

                con, null, trn);



            if (req.Levels != null)

            {

                foreach (var level in req.Levels)

                {

                    var memberIds = ResolveMemberIds(level);

                    int firstApprover = memberIds.Count > 0 ? memberIds[0] : 0;

                    int minRequired = level.RequireAllApprovers ? Math.Max(1, memberIds.Count) : 1;



                    SqlParameter[] levelPrm =

                    {

                        new SqlParameter("@PolicyID", SqlDbType.Int) { Value = policyId },

                        new SqlParameter("@LevelNo", SqlDbType.Int) { Value = level.LevelNo },

                        new SqlParameter("@LevelName", SqlDbType.NVarChar, 200) { Value = level.LevelName ?? "" },

                        new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = level.MinAmount },

                        new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = level.MaxAmount },

                        new SqlParameter("@RequireAllApprovers", SqlDbType.Bit) { Value = level.RequireAllApprovers },

                        new SqlParameter("@ApproverUserID", SqlDbType.Int) { Value = firstApprover },

                        new SqlParameter("@MinApproversRequired", SqlDbType.Int) { Value = minRequired },

                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = req.CompanyID },

                        new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = req.UserID },

                    };



                    int levelId = Simulate.Integer32(sql.ExecuteScalar(@"

INSERT INTO tbl_ApprovalPolicyLevel

    (PolicyID, LevelNo, LevelName, MinAmount, MaxAmount, RequireAllApprovers,

     ApproverUserID, MinApproversRequired, CompanyID, CreationUserID, CreationDate)

OUTPUT INSERTED.ID

VALUES

    (@PolicyID, @LevelNo, @LevelName, @MinAmount, @MaxAmount, @RequireAllApprovers,

     @ApproverUserID, @MinApproversRequired, @CompanyID, @CreationUserID, GETDATE())", levelPrm, con));



                    foreach (int memberId in memberIds.Distinct())

                    {

                        if (memberId <= 0) continue;



                        SqlParameter[] memberPrm =

                        {

                            new SqlParameter("@PolicyLevelID", SqlDbType.Int) { Value = levelId },

                            new SqlParameter("@ApproverUserID", SqlDbType.Int) { Value = memberId },

                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = req.CompanyID },

                        };



                        sql.ExecuteNonQueryStatement(@"

INSERT INTO tbl_ApprovalPolicyLevelMember (PolicyLevelID, ApproverUserID, CompanyID)

VALUES (@PolicyLevelID, @ApproverUserID, @CompanyID)", con, memberPrm, trn);

                    }

                }

            }



            return policyId;

        }



        private static List<int> ResolveMemberIds(ApprovalPolicyLevelRow level)

        {

            var ids = new List<int>();

            if (level.MemberUserIds != null)

            {

                foreach (int id in level.MemberUserIds)

                {

                    if (id > 0 && !ids.Contains(id))

                        ids.Add(id);

                }

            }



            if (ids.Count == 0 && level.ApproverUserID > 0)

                ids.Add(level.ApproverUserID);



            return ids;

        }



        private void LoadMembers(ApprovalPolicyLevelRow level, int companyId)

        {

            level.MemberUserIds = new List<int>();

            level.MemberUserNames = new List<string>();



            DataTable dt = SelectLevelMembers(level.ID, companyId);

            if (dt == null) return;



            foreach (DataRow row in dt.Rows)

            {

                level.MemberUserIds.Add(Simulate.Integer32(row["ApproverUserID"]));

                level.MemberUserNames.Add(Simulate.String(row["ApproverUserName"]));

            }



            if (level.MemberUserIds.Count == 0 && level.ApproverUserID > 0)

            {

                level.MemberUserIds.Add(level.ApproverUserID);

                if (!string.IsNullOrWhiteSpace(level.ApproverUserName))

                    level.MemberUserNames.Add(level.ApproverUserName);

            }

        }



        private static ApprovalPolicyLevelRow MapLevel(DataRow row)

        {

            decimal minAmount = 0;

            decimal maxAmount = 0;

            bool requireAll = false;

            if (row.Table.Columns.Contains("MinAmount"))

                minAmount = Simulate.Decimal(row["MinAmount"]);

            if (row.Table.Columns.Contains("MaxAmount"))

                maxAmount = Simulate.Decimal(row["MaxAmount"]);

            if (row.Table.Columns.Contains("RequireAllApprovers"))

                requireAll = Simulate.Bool(row["RequireAllApprovers"]);



            return new ApprovalPolicyLevelRow

            {

                ID = Simulate.Integer32(row["ID"]),

                PolicyID = Simulate.Integer32(row["PolicyID"]),

                LevelNo = Simulate.Integer32(row["LevelNo"]),

                LevelName = Simulate.String(row["LevelName"]),

                MinAmount = minAmount,

                MaxAmount = maxAmount,

                RequireAllApprovers = requireAll,

                ApproverUserID = Simulate.Integer32(row["ApproverUserID"]),

                ApproverUserName = Simulate.String(row["ApproverUserName"]),

                MinApproversRequired = Math.Max(1, Simulate.Integer32(row["MinApproversRequired"])),

            };

        }



        public int CountUnpostedDocuments(int companyId, int documentTypeId)

        {

            EnsureSchema(companyId);

            if (!clsApprovalDocumentTypes.IsSupported(documentTypeId))

                return 0;



            string query = BuildUnpostedCountQuery(documentTypeId);

            if (string.IsNullOrWhiteSpace(query))

                return 0;



            clsSQL sql = new clsSQL();

            SqlParameter[] prm =

            {

                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },

                new SqlParameter("@DocumentTypeID", SqlDbType.Int) { Value = documentTypeId },

            };



            object scalar = sql.ExecuteScalar(

                query,

                prm,

                sql.CreateDataBaseConnectionString(companyId));



            return Simulate.Integer32(scalar);

        }



        static string BuildUnpostedCountQuery(int documentTypeId)

        {

            const string unpostedFilter = "ISNULL(DocumentStatus, 2) <> 2";



            if (documentTypeId == (int)VoucherType.ManualJV)

            {

                return $@"SELECT COUNT(*)

FROM tbl_JournalVoucherHeader

WHERE CompanyID = @CompanyID

  AND JVTypeID = @DocumentTypeID

  AND {unpostedFilter}";

            }



            if (clsDocumentPostingService.IsCashVoucherType(documentTypeId))

            {

                return $@"SELECT COUNT(*)

FROM tbl_CashVoucherHeader

WHERE CompanyID = @CompanyID

  AND VoucherType = @DocumentTypeID

  AND {unpostedFilter}";

            }



            if (clsDocumentPostingService.IsCreditNoteType(documentTypeId))

            {

                return $@"SELECT COUNT(*)

FROM tbl_CreditNoteHeader

WHERE CompanyID = @CompanyID

  AND VoucherType = @DocumentTypeID

  AND {unpostedFilter}";

            }



            if (clsApprovalDocumentTypes.IsInvoiceHeaderType(documentTypeId))

            {

                return $@"SELECT COUNT(*)

FROM tbl_InvoiceHeader

WHERE CompanyID = @CompanyID

  AND InvoiceTypeID = @DocumentTypeID

  AND {unpostedFilter}";

            }



            if (clsApprovalDocumentTypes.IsHcmType(documentTypeId))

            {

                string tableName = GetHcmTableName(documentTypeId);

                if (tableName == null) return null;

                return $@"SELECT COUNT(*)

FROM {tableName}

WHERE CompanyID = @CompanyID

  AND {unpostedFilter}";

            }



            return null;

        }



        static string GetHcmTableName(int documentTypeId)

        {

            switch (documentTypeId)

            {

                case clsHcmApprovalDocuments.TypeEmployeeContract:

                    return "tbl_EmployeeContract";

                case clsHcmApprovalDocuments.TypeEmployeeSalaryElement:

                    return "tbl_EmployeeSalaryElements";

                case clsHcmApprovalDocuments.TypePayrollPeriod:

                    return "tbl_PayrollPeriod";

                case clsHcmApprovalDocuments.TypePayroll:

                    return "tbl_PayrollHeader";

                case clsHcmApprovalDocuments.TypeEmployeeShiftAssignment:

                    return "tbl_EmployeeShiftAssignment";

                default:

                    return null;

            }

        }



        private static ApprovalPolicyRow MapPolicy(DataRow row)

        {

            return new ApprovalPolicyRow

            {

                ID = Simulate.Integer32(row["ID"]),

                CompanyID = Simulate.Integer32(row["CompanyID"]),

                DocumentTypeID = Simulate.Integer32(row["DocumentTypeID"]),

                BranchID = Simulate.Integer32(row["BranchID"]),

                IsEnabled = Simulate.Bool(row["IsEnabled"]),

                MinAmount = Simulate.Decimal(row["MinAmount"]),

                MaxAmount = Simulate.Decimal(row["MaxAmount"]),

                AllowSelfApproval = Simulate.Bool(row["AllowSelfApproval"]),

                DocumentTypeAName = Simulate.String(row["DocumentTypeAName"]),

                DocumentTypeEName = Simulate.String(row["DocumentTypeEName"]),

            };

        }

    }

}


