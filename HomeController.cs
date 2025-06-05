using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Bulk_Test_Vexil.Models;

namespace Bulk_Test_Vexil.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Student student)
        {
            var studentDocuments = new List<StudentDocuments>();

            foreach (string fileKey in Request.Files)
            {
                string pattern = @"\[(\d+)\]"; // Regex to match digits inside square brackets

                Match match = Regex.Match(fileKey, pattern);

                var postedFile = Request.Files[fileKey] as HttpPostedFileBase;
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    // Generate a unique file name
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);

                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Save the file to the server
                    postedFile.SaveAs(filePath);

                    // Create a new StudentDocuments object
                    var document = new StudentDocuments
                    {
                        DocumentType = Request.Form["StudentDocumentsList"+ match + ".DocumentType"],
                        FilePath = filePath
                    };
                    studentDocuments.Add(document);
                }
            }

            student.StudentDocumentsList = studentDocuments;

            // Save student and documents to the database
            SaveStudentAndDocuments(student);

            return View();
        }

        //private void SaveStudentAndDocuments(Student student)
        //{
        //    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
        //    {
        //        conn.Open();

        //        // Insert student
        //        string insertStudentQuery = "INSERT INTO Students (Name, MobileNo) VALUES (@Name, @MobileNo); SELECT SCOPE_IDENTITY();";
        //        using (SqlCommand cmd = new SqlCommand(insertStudentQuery, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@Name", student.Name);
        //            cmd.Parameters.AddWithValue("@MobileNo", student.MobileNo);
        //            student.StudentId = Convert.ToInt32(cmd.ExecuteScalar());
        //        }

        //        // Insert documents
        //        foreach (var document in student.StudentDocumentsList)
        //        {
        //            string insertDocumentQuery = "INSERT INTO StudentDocuments (StudentId, DocumentType, FilePath) VALUES (@StudentId, @DocumentType, @FilePath)";
        //            using (SqlCommand cmd = new SqlCommand(insertDocumentQuery, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@StudentId", student.StudentId);
        //                cmd.Parameters.AddWithValue("@DocumentType", document.DocumentType);
        //                cmd.Parameters.AddWithValue("@FilePath", document.FilePath);
        //                cmd.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //}

        private void SaveStudentAndDocuments(Student student)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                conn.Open();

                // Create a DataTable to hold the documents
                DataTable documentsTable = new DataTable();
                documentsTable.Columns.Add("DocumentType", typeof(string));
                documentsTable.Columns.Add("FilePath", typeof(string));

                foreach (var document in student.StudentDocumentsList)
                {
                    documentsTable.Rows.Add(document.DocumentType, document.FilePath);
                }

                // Define the SqlCommand and parameters
                using (SqlCommand cmd = new SqlCommand("dbo.SaveStudentAndDocuments", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Name", student.Name);
                    cmd.Parameters.AddWithValue("@MobileNo", student.MobileNo);

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@Documents", documentsTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.StudentDocumentType";

                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}