using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.Common;
using PhotoBookmart.Reports;

namespace PhotoBookmart.Controllers
{
    public class StagingController : Controller
    {
        public ActionResult Index()
        {
            return Content("Here is staging controller!");
        }
        
        public ActionResult ReportCrystalPdf()
        {
            CrystalReportTest1 report = new CrystalReportTest1();
            IndividualTranscript transcript = new IndividualTranscript() { };
            
            DataTable dtPersonInfo = new DataTable();
            dtPersonInfo.TableName = "PersonInfo";
            dtPersonInfo.Columns.Add("Name", typeof(string));
            dtPersonInfo.Columns.Add("Code", typeof(string));
            dtPersonInfo.Columns.Add("DOB", typeof(DateTime));
            dtPersonInfo.Columns.Add("Gender", typeof(string));
            dtPersonInfo.Columns.Add("Passport", typeof(string));
            dtPersonInfo.Columns.Add("IdCard", typeof(string));
            dtPersonInfo.Columns.Add("JobTitle", typeof(string));
            dtPersonInfo.Columns.Add("Group", typeof(string));
            dtPersonInfo.Columns.Add("Location", typeof(string));
            dtPersonInfo.Columns.Add("Email", typeof(string));
            dtPersonInfo.Columns.Add("Addr", typeof(string));
            dtPersonInfo.Rows.Add("Dinh Van Chinh", "00001", new DateTime(1991, 8, 4), "", "", "", "", "", "", "", "");
            dtPersonInfo.Rows.Add("Dinh Van Duong", "00002", new DateTime(1990, 8, 4), "", "", "", "", "", "", "", "");
            transcript.Tables.Add(dtPersonInfo);

            report.FileName = Server.MapPath("/Reports/CrystalReportTest1.rpt");
            report.Load();
            report.SetDataSource(transcript);
            Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }
        
        public ActionResult Test1()
        {
            return Content("Here is test #1!");
        }
        
        public ActionResult Test2()
        {
            return Content("Here is test #2!");
        }
        
        public ActionResult Test3()
        {
            return Content("Here is test #3");
        }
    }
}
