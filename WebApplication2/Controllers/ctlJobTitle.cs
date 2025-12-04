using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlJobTitle")]
    public class ctlJobTitle : Controller
    {
        
            [HttpGet]
            [Route("SelectJobTitleByID")]
            public string SelectJobTitleByID(int ID,   int CompanyID)
            {
                try
                {

                clsJobTitle clsJobTitle = new clsJobTitle();
                    DataTable dt = clsJobTitle.SelectJobTitleByID(ID, "", "",   CompanyID);
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
            [Route("DeleteJobTitleByID")]
            public bool DeleteJobTitleByID(int ID, int CompanyID)
            {
                try
                {
                    clsJobTitle Countries = new clsJobTitle();
                    bool A = Countries.DeleteJobTitleByID(ID, CompanyID);
                    return A;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            [HttpGet]
            [Route("InsertJobTitle")]
            public int InsertJobTitle(string AName, string EName,  int CompanyID, int CreationUserId)
            {
                try
                {
                    clsJobTitle clsCountries = new clsJobTitle();
                    int A = clsCountries.InsertJobTitle(Simulate.String(AName), Simulate.String(EName),
                         CompanyID, CreationUserId);
                    return A;
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
            [HttpGet]
            [Route("UpdateJobTitle")]
            public int UpdateJobTitle(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
            {
                try
                {
                    clsJobTitle clsCountries = new clsJobTitle();
                    int A = clsCountries.UpdateJobTitle(ID, Simulate.String(AName), Simulate.String(EName),
                  
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
