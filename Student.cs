using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bulk_Test_Vexil.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public List<StudentDocuments> StudentDocumentsList { get; set; }
    }

    public class StudentDocuments
    {
        public int DocumentId { get; set; }
        public string DocumentType { get; set; }
        public HttpPostedFileBase Document { get; set; }
        public string FilePath { get; set; }
    }

}