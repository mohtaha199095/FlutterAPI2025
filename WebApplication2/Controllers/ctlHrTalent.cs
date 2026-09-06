using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlHrTalent")]
    public class ctlHrTalent : Controller
    {
        readonly clsHrTalent _talent = new clsHrTalent();

        [HttpGet]
        [Route("SelectJobOpenings")]
        public string SelectJobOpenings(int ID = 0, int CompanyID = 0)
        {
            DataTable dt = _talent.SelectJobOpenings(ID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertJobOpening")]
        public int InsertJobOpening(string Title, string Department, string Status, string Notes, int CompanyID, int UserID = 1)
        {
            return _talent.InsertJobOpening(Title, Department, Status, Notes, CompanyID, UserID);
        }

        [HttpGet]
        [Route("SelectPerformanceReviews")]
        public string SelectPerformanceReviews(int ID = 0, int EmployeeID = 0, int CompanyID = 0)
        {
            DataTable dt = _talent.SelectPerformanceReviews(ID, EmployeeID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertPerformanceReview")]
        public int InsertPerformanceReview(int EmployeeID, DateTime ReviewDate, decimal Rating, string Summary, int CompanyID, int UserID = 1)
        {
            return _talent.InsertPerformanceReview(EmployeeID, ReviewDate, Rating, Summary, CompanyID, UserID);
        }

        [HttpGet]
        [Route("SelectDisciplinaryActions")]
        public string SelectDisciplinaryActions(int ID = 0, int EmployeeID = 0, int CompanyID = 0)
        {
            DataTable dt = _talent.SelectDisciplinaryActions(ID, EmployeeID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertDisciplinaryAction")]
        public int InsertDisciplinaryAction(int EmployeeID, DateTime ActionDate, string ActionType, string Description, int CompanyID, int UserID = 1)
        {
            return _talent.InsertDisciplinaryAction(EmployeeID, ActionDate, ActionType, Description, CompanyID, UserID);
        }

        [HttpGet]
        [Route("SelectEmployeeDocuments")]
        public string SelectEmployeeDocuments(int ID = 0, int EmployeeID = 0, int CompanyID = 0)
        {
            DataTable dt = _talent.SelectEmployeeDocuments(ID, EmployeeID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertEmployeeDocument")]
        public int InsertEmployeeDocument(int EmployeeID, string DocumentName, string DocumentType,
            DateTime IssueDate, DateTime ExpiryDate, string Notes, int CompanyID, int UserID = 1)
        {
            return _talent.InsertEmployeeDocument(EmployeeID, DocumentName, DocumentType,
                IssueDate, ExpiryDate, Notes, CompanyID, UserID);
        }
    }
}
