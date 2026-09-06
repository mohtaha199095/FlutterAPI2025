using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    /// <summary>
    /// Main-database desktop device registry and admin-controlled update commands.
    /// All tables live in the main DB (CompanyID = 0 connection). Additive only.
    /// </summary>
    public class clsDesktopUpdate
    {
        public const string StatusIdle = "Idle";
        public const string StatusPendingDownload = "PendingDownload";
        public const string StatusDownloading = "Downloading";
        public const string StatusReady = "Ready";
        public const string StatusFailed = "Failed";
        public const string StatusRollbackPending = "RollbackPending";

        private readonly clsSQL _sql = new clsSQL();

        private string MainCon => _sql.MainDataBaseconString;

        public DataTable SelectDevices(int companyId, string search, int topN, int offset, bool staleOnly = false)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId <= 0 ? (object)DBNull.Value : companyId },
                new SqlParameter("@Search", SqlDbType.NVarChar, 200) { Value = search ?? "" },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN <= 0 ? 200 : topN },
                new SqlParameter("@Offset", SqlDbType.Int) { Value = offset < 0 ? 0 : offset },
                new SqlParameter("@StaleOnly", SqlDbType.Bit) { Value = staleOnly },
            };

            const string sql = @"
SELECT
    D.ID,
    D.DeviceGuid,
    D.CompanyID,
    ISNULL(C.EName, C.AName) AS CompanyName,
    D.DeviceLabel,
    D.MachineName,
    D.CurrentVersion,
    D.CurrentBuild,
    D.PreviousVersion,
    D.PreviousBuild,
    D.TargetReleaseID,
    D.UpdateStatus,
    D.LastSeen,
    D.LastError,
    R.AppVersion AS TargetVersion,
    R.BuildNumber AS TargetBuild,
    R.Sha256 AS TargetSha256,
    R.DownloadUrl AS TargetDownloadUrl,
    R.FolderName AS TargetFolderName
FROM tbl_DesktopDevice D
LEFT JOIN tbl_Company C ON C.ID = D.CompanyID
LEFT JOIN tbl_DesktopRelease R ON R.ID = D.TargetReleaseID
WHERE (@CompanyID IS NULL OR D.CompanyID = @CompanyID)
  AND (@Search = '' OR D.DeviceLabel LIKE '%' + @Search + '%'
       OR D.MachineName LIKE '%' + @Search + '%'
       OR CAST(D.DeviceGuid AS NVARCHAR(36)) LIKE '%' + @Search + '%'
       OR ISNULL(C.EName, C.AName) LIKE '%' + @Search + '%')
  AND (@StaleOnly = 0 OR D.LastSeen IS NULL OR D.LastSeen < DATEADD(day, -7, GETDATE()))
ORDER BY D.LastSeen DESC, D.ID DESC
OFFSET @Offset ROWS FETCH NEXT @TopN ROWS ONLY";

            return _sql.ExecuteQueryStatement(sql, MainCon, prm);
        }

        public DataTable SelectReleases(int topN)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN <= 0 ? 50 : topN },
            };

            const string sql = @"
SELECT TOP (@TopN)
    ID, AppVersion, BuildNumber, FolderName, ZipFileName, DownloadUrl,
    Sha256, FileSizeBytes, Notes, IsActive, CreatedAt
FROM tbl_DesktopRelease
ORDER BY BuildNumber DESC, ID DESC";

            return _sql.ExecuteQueryStatement(sql, MainCon, prm);
        }

        public int RegisterRelease(
            string appVersion,
            int buildNumber,
            string folderName,
            string zipFileName,
            string downloadUrl,
            string sha256,
            long fileSizeBytes,
            string notes)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@AppVersion", SqlDbType.VarChar, 20) { Value = appVersion ?? "" },
                new SqlParameter("@BuildNumber", SqlDbType.Int) { Value = buildNumber },
                new SqlParameter("@FolderName", SqlDbType.VarChar, 50) { Value = folderName ?? "" },
                new SqlParameter("@ZipFileName", SqlDbType.VarChar, 260) { Value = zipFileName ?? "" },
                new SqlParameter("@DownloadUrl", SqlDbType.VarChar, 500) { Value = downloadUrl ?? "" },
                new SqlParameter("@Sha256", SqlDbType.VarChar, 64) { Value = sha256 ?? "" },
                new SqlParameter("@FileSizeBytes", SqlDbType.BigInt) { Value = fileSizeBytes },
                new SqlParameter("@Notes", SqlDbType.NVarChar, 500) { Value = notes ?? "" },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
            };

            const string sql = @"
IF EXISTS (SELECT 1 FROM tbl_DesktopRelease WHERE AppVersion = @AppVersion AND BuildNumber = @BuildNumber)
BEGIN
    UPDATE tbl_DesktopRelease SET
        FolderName = @FolderName,
        ZipFileName = @ZipFileName,
        DownloadUrl = @DownloadUrl,
        Sha256 = @Sha256,
        FileSizeBytes = @FileSizeBytes,
        Notes = @Notes,
        IsActive = 1
    WHERE AppVersion = @AppVersion AND BuildNumber = @BuildNumber;
    SELECT ID FROM tbl_DesktopRelease WHERE AppVersion = @AppVersion AND BuildNumber = @BuildNumber;
END
ELSE
BEGIN
    INSERT INTO tbl_DesktopRelease
        (AppVersion, BuildNumber, FolderName, ZipFileName, DownloadUrl, Sha256, FileSizeBytes, Notes, IsActive, CreatedAt)
    VALUES
        (@AppVersion, @BuildNumber, @FolderName, @ZipFileName, @DownloadUrl, @Sha256, @FileSizeBytes, @Notes, 1, @CreatedAt);
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END";

            var dt = _sql.ExecuteQueryStatement(sql, MainCon, prm);
            if (dt != null && dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0][0]);
            return 0;
        }

        /// <summary>
        /// Device heartbeat — upserts device row and returns pending command payload.
        /// </summary>
        public DataTable Heartbeat(
            Guid deviceGuid,
            int companyId,
            string appVersion,
            int buildNumber,
            string machineName,
            string deviceLabel,
            string localStatus)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@DeviceGuid", SqlDbType.UniqueIdentifier) { Value = deviceGuid },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@CurrentVersion", SqlDbType.VarChar, 20) { Value = appVersion ?? "" },
                new SqlParameter("@CurrentBuild", SqlDbType.Int) { Value = buildNumber },
                new SqlParameter("@MachineName", SqlDbType.NVarChar, 200) { Value = machineName ?? "" },
                new SqlParameter("@DeviceLabel", SqlDbType.NVarChar, 200) { Value = deviceLabel ?? "" },
                new SqlParameter("@LocalStatus", SqlDbType.VarChar, 30) { Value = localStatus ?? "" },
                new SqlParameter("@Now", SqlDbType.DateTime) { Value = DateTime.Now },
            };

            const string upsert = @"
IF NOT EXISTS (SELECT 1 FROM tbl_DesktopDevice WHERE DeviceGuid = @DeviceGuid)
BEGIN
    INSERT INTO tbl_DesktopDevice
        (DeviceGuid, CompanyID, DeviceLabel, MachineName, CurrentVersion, CurrentBuild,
         UpdateStatus, LastSeen, CreatedAt)
    VALUES
        (@DeviceGuid, @CompanyID, @DeviceLabel, @MachineName, @CurrentVersion, @CurrentBuild,
         'Idle', @Now, @Now);
END
ELSE
BEGIN
    UPDATE tbl_DesktopDevice SET
        CompanyID = CASE WHEN @CompanyID > 0 THEN @CompanyID ELSE CompanyID END,
        DeviceLabel = CASE WHEN @DeviceLabel <> '' THEN @DeviceLabel ELSE DeviceLabel END,
        MachineName = CASE WHEN @MachineName <> '' THEN @MachineName ELSE MachineName END,
        CurrentVersion = @CurrentVersion,
        CurrentBuild = @CurrentBuild,
        LastSeen = @Now,
        LastError = CASE WHEN @LocalStatus IN ('Ready','Idle') THEN NULL ELSE LastError END,
        UpdateStatus = CASE
            WHEN UpdateStatus = 'PendingDownload' AND @LocalStatus = 'Downloading' THEN 'Downloading'
            WHEN UpdateStatus IN ('PendingDownload','Downloading') AND @LocalStatus = 'Ready' THEN 'Ready'
            WHEN UpdateStatus = 'RollbackPending' AND @LocalStatus = 'Ready' THEN 'Idle'
            WHEN @LocalStatus = 'Failed' THEN 'Failed'
            ELSE UpdateStatus
        END,
        PreviousVersion = CASE
            WHEN @LocalStatus = 'Ready' AND UpdateStatus = 'RollbackPending' THEN CurrentVersion
            ELSE PreviousVersion
        END,
        PreviousBuild = CASE
            WHEN @LocalStatus = 'Ready' AND UpdateStatus = 'RollbackPending' THEN CurrentBuild
            ELSE PreviousBuild
        END
    WHERE DeviceGuid = @DeviceGuid;
END";

            _sql.ExecuteNonQueryStatement(upsert, MainCon, prm);

            const string select = @"
SELECT
    D.DeviceGuid,
    D.UpdateStatus,
    D.TargetReleaseID,
    D.PreviousVersion,
    D.PreviousBuild,
    R.AppVersion AS TargetAppVersion,
    R.BuildNumber AS TargetBuildNumber,
    R.FolderName AS TargetFolderName,
    R.DownloadUrl AS TargetDownloadUrl,
    R.Sha256 AS TargetSha256
FROM tbl_DesktopDevice D
LEFT JOIN tbl_DesktopRelease R ON R.ID = D.TargetReleaseID
WHERE D.DeviceGuid = @DeviceGuid";

            return _sql.ExecuteQueryStatement(select, MainCon, prm);
        }

        public bool DeployToDevices(int releaseId, string deviceGuidsCsv)
        {
            if (releaseId <= 0 || string.IsNullOrWhiteSpace(deviceGuidsCsv)) return false;

            SqlParameter[] prm =
            {
                new SqlParameter("@ReleaseID", SqlDbType.Int) { Value = releaseId },
                new SqlParameter("@Guids", SqlDbType.NVarChar, -1) { Value = deviceGuidsCsv },
                new SqlParameter("@Status", SqlDbType.VarChar, 30) { Value = StatusPendingDownload },
            };

            const string sql = @"
UPDATE D SET
    TargetReleaseID = @ReleaseID,
    UpdateStatus = @Status,
    LastError = NULL
FROM tbl_DesktopDevice D
INNER JOIN STRING_SPLIT(@Guids, ',') S ON TRY_CAST(LTRIM(RTRIM(S.value)) AS UNIQUEIDENTIFIER) = D.DeviceGuid";

            _sql.ExecuteNonQueryStatement(sql, MainCon, prm);
            return true;
        }

        public int DeployToCompany(int releaseId, int companyId)
        {
            if (releaseId <= 0 || companyId <= 0) return 0;

            SqlParameter[] prm =
            {
                new SqlParameter("@ReleaseID", SqlDbType.Int) { Value = releaseId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Status", SqlDbType.VarChar, 30) { Value = StatusPendingDownload },
            };

            const string sql = @"
UPDATE tbl_DesktopDevice SET
    TargetReleaseID = @ReleaseID,
    UpdateStatus = @Status,
    LastError = NULL
WHERE CompanyID = @CompanyID;
SELECT @@ROWCOUNT;";

            var dt = _sql.ExecuteQueryStatement(sql, MainCon, prm);
            if (dt != null && dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0][0]);
            return 0;
        }

        public bool CancelDevices(string deviceGuidsCsv)
        {
            if (string.IsNullOrWhiteSpace(deviceGuidsCsv)) return false;

            SqlParameter[] prm =
            {
                new SqlParameter("@Guids", SqlDbType.NVarChar, -1) { Value = deviceGuidsCsv },
                new SqlParameter("@Idle", SqlDbType.VarChar, 30) { Value = StatusIdle },
            };

            const string sql = @"
UPDATE D SET
    TargetReleaseID = NULL,
    UpdateStatus = @Idle,
    LastError = NULL
FROM tbl_DesktopDevice D
INNER JOIN STRING_SPLIT(@Guids, ',') S ON TRY_CAST(LTRIM(RTRIM(S.value)) AS UNIQUEIDENTIFIER) = D.DeviceGuid";

            _sql.ExecuteNonQueryStatement(sql, MainCon, prm);
            return true;
        }

        public bool RollbackDevices(string deviceGuidsCsv)
        {
            if (string.IsNullOrWhiteSpace(deviceGuidsCsv)) return false;

            SqlParameter[] prm =
            {
                new SqlParameter("@Guids", SqlDbType.NVarChar, -1) { Value = deviceGuidsCsv },
                new SqlParameter("@Status", SqlDbType.VarChar, 30) { Value = StatusRollbackPending },
            };

            const string sql = @"
UPDATE D SET
    UpdateStatus = @Status,
    TargetReleaseID = (
        SELECT TOP 1 R.ID FROM tbl_DesktopRelease R
        WHERE R.AppVersion = D.PreviousVersion AND R.BuildNumber = D.PreviousBuild
        ORDER BY R.ID DESC
    ),
    LastError = NULL
FROM tbl_DesktopDevice D
INNER JOIN STRING_SPLIT(@Guids, ',') S ON TRY_CAST(LTRIM(RTRIM(S.value)) AS UNIQUEIDENTIFIER) = D.DeviceGuid
WHERE D.PreviousVersion IS NOT NULL AND D.PreviousBuild IS NOT NULL";

            _sql.ExecuteNonQueryStatement(sql, MainCon, prm);
            return true;
        }

        public void ReportDeviceError(Guid deviceGuid, string error)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@DeviceGuid", SqlDbType.UniqueIdentifier) { Value = deviceGuid },
                new SqlParameter("@Error", SqlDbType.NVarChar, 500) { Value = error ?? "" },
                new SqlParameter("@Status", SqlDbType.VarChar, 30) { Value = StatusFailed },
            };

            const string sql = @"
UPDATE tbl_DesktopDevice SET UpdateStatus = @Status, LastError = @Error WHERE DeviceGuid = @DeviceGuid";
            _sql.ExecuteNonQueryStatement(sql, MainCon, prm);
        }
    }
}
