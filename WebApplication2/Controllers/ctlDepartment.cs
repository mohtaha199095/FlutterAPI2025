using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
   
        [Route("api/ctlDepartment")]
        public class ctlDepartment : Controller
        {

            [HttpGet]
            [Route("SelectDepartmentByID")]
            public string SelectDepartmentByID(int ID, int CompanyID)
            {
                try
                {

                    clsDepartment clsDepartment = new clsDepartment();
                    DataTable dt = clsDepartment.SelectDepartmentByID(ID, "", "", CompanyID);
                    if (dt != null)
                    {

                        string JSONString = string.Empty;
                        JSONString = JsonConvert.SerializeObject(dt);
                        return JSONString;
                    }
                    else
                    {

                        return "";
                    }
                }
                catch (Exception)
                {

                    throw;
                }




            }
            [HttpGet]
            [Route("DeleteDepartmentByID")]
            public bool DeleteDepartmentByID(int ID, int CompanyID)
            {
                try
                {
                    clsDepartment Countries = new clsDepartment();
                    bool A = Countries.DeleteDepartmentByID(ID, CompanyID);
                    return A;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            [HttpGet]
            [Route("InsertDepartment")]
            public int InsertDepartment(string AName, string EName, int CompanyID, int CreationUserId)
            {
                try
                {
                    clsDepartment clsCountries = new clsDepartment();
                    int A = clsCountries.InsertDepartment(Simulate.String(AName), Simulate.String(EName),
                         CompanyID, CreationUserId);
                    return A;
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
            [HttpGet]
            [Route("UpdateDepartment")]
            public int UpdateDepartment(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
            {
                try
                {
                    clsDepartment clsCountries = new clsDepartment();
                    int A = clsCountries.UpdateDepartment(ID, Simulate.String(AName), Simulate.String(EName),

                        ModificationUserId, CompanyID);
                    return A;
                }
                catch (Exception)
                {

                    throw;
                }

            }

        }
    }
 