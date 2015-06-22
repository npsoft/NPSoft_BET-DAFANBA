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
using CrystalDecisions.Shared;
using PhotoBookmart.Models;
using CrystalDecisions.CrystalReports.Engine;
using System.Data.SqlClient;

namespace PhotoBookmart.Controllers
{
    public class StagingController : Controller
    {
        public ActionResult Index()
        {
            return Content("Here is staging controller!");
        }
        
        public ActionResult ReportCrystalToFile()
        {
            try
            {
                List<MyPhotoCreationRequest> photos = new List<MyPhotoCreationRequest>();
                photos.Add(new MyPhotoCreationRequest() { Photobook_Code = "CODE3", Pages = 2, Product_Id = 1 });
                photos.Add(new MyPhotoCreationRequest() { Photobook_Code = "CODE6", Pages = 5, Product_Id = 4 });

                List<ReportModelSettings> settings = new List<ReportModelSettings>();
                settings.Add(new ReportModelSettings() { Id = 1, Key = "Key1", Value = "Value1", Desc = "Desc1", MaHC = "MaHC1" });
                settings.Add(new ReportModelSettings() { Id = 2, Key = "Key2", Value = "Value2", Desc = "Desc2", MaHC = "MaHC2" });

                ReportDocument report = new ReportDocument();
                report.Load(Path.Combine(Server.MapPath("~/Reports"), "CrystalReportTest1.rpt"));
                report.Database.Tables["PhotoBookmart_Models_MyPhotoCreationRequest"].SetDataSource(photos);
                report.Database.Tables["PhotoBookmart_Models_ReportModelSettings"].SetDataSource(settings);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();

                Stream stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", string.Format("Remote_{0:ddMMyy}.pdf", DateTime.Today));
            }
            catch (Exception ex)
            {
                return Content(string.Format("Here is exception: {0}", ex.Message));
            }
        }
        
        public void ReportCrystalToHttpResponse()
        {
            /* Using: List
            List<MyPhotoCreationRequest> photos = new List<MyPhotoCreationRequest>();
            photos.Add(new MyPhotoCreationRequest() { Photobook_Code = "CODE3", Pages = 2, Product_Id = 1 });
            photos.Add(new MyPhotoCreationRequest() { Photobook_Code = "CODE6", Pages = 5, Product_Id = 4 });*/

            /* Using: Table */
            DataTable photos = new DataTable();
            photos.TableName = "PhotoBookmart_Models_MyPhotoCreationRequest";
            photos.Columns.Add("Photobook_Code", typeof(string));
            photos.Columns.Add("Pages", typeof(int));
            photos.Columns.Add("Product_Id", typeof(long));
            photos.Rows.Add("CODE9", 8, 7);
            photos.Rows.Add("CODE12", 11, 10);
            
            CrystalReportTest2 report = new CrystalReportTest2();
            report.FileName = Server.MapPath("/Reports/CrystalReportTest2.rpt");
            report.Load();
            report.SetDataSource(photos);
            report.SetParameterValue("DOB1", new DateTime(1991, 8, 4));
            report.SetParameterValue("DOB2", new DateTime(1992, 8, 4));
            report.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, string.Format("Cake_{0:ddMMyy}", DateTime.Today));
        }
        
        public void ReportCrystalToHttpResponse1()
        {
            using (ReportClass report = new ReportClass())
            {
                DataTable dtPersonInfo = new DataTable();
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
                dtPersonInfo.Rows.Add("Name1", "Code1", new DateTime(1991, 8, 4), "Gender1", "Passport1", "IdCard1", "JobTitle1", "Group1", "Location1", "Email1", "Addr1");
                dtPersonInfo.Rows.Add("Name2", "Code2", new DateTime(1992, 8, 4), "Gender2", "Passport2", "IdCard2", "JobTitle2", "Group2", "Location2", "Email22", "Addr2");

                DataTable dtCourses = new DataTable();
                dtCourses.Columns.Add("No", typeof(int));
                dtCourses.Columns.Add("Spec", typeof(string));
                dtCourses.Columns.Add("Name", typeof(string));
                dtCourses.Columns.Add("Subject", typeof(string));
                dtCourses.Columns.Add("Inst", typeof(string));
                dtCourses.Columns.Add("Credit", typeof(string));
                dtCourses.Columns.Add("Score", typeof(string));
                dtCourses.Columns.Add("Grade", typeof(string));
                dtCourses.Columns.Add("Result", typeof(string));
                dtCourses.Columns.Add("IsExp", typeof(string));
                dtCourses.Columns.Add("Date", typeof(string));
                dtCourses.Rows.Add(1, "Spec1", "Name1", "Subject1", "Instr1", "Credit1", "Score1", "Grade1", "Result1", "IsExp1", "Date1");

                report.FileName = Server.MapPath("~/" + "//Reports/CrystalReportTest3.rpt");
                report.Load();
                report.SetDataSource(new IndividualTranscript());
                report.Database.Tables["Courses"].SetDataSource(dtCourses);
                report.Database.Tables["PersonInfo"].SetDataSource(dtPersonInfo);
                report.ExportToHttpResponse(ExportFormatType.Excel, System.Web.HttpContext.Current.Response, false, string.Format("Work_{0:ddMMyy}", DateTime.Today));
            }
        }

        public void ReportCrystalToHttpResponse2()
        {
            using (ReportClass report = new ReportClass())
            {
                report.FileName = Path.Combine(Server.MapPath("~/Reports"), "DanhSachDT.rpt");
                report.Load();
                report.SetDataSource(new Customers());
                report.ExportToHttpResponse(ExportFormatType.WordForWindows, System.Web.HttpContext.Current.Response, false, string.Format("Remote_{0:ddMMyy}", DateTime.Today));
            }
        }
    }
}
