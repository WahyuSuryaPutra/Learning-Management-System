using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using LMS.Models;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Antlr.Runtime.Tree;
using Microsoft.Ajax.Utilities;

namespace LMS.Controllers
{
    public class AdminController : Controller
    {
        string DataBase = ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString.ToString();

        public ActionResult Login()
        {
            Response.Cache.SetNoStore();
            if (Session["uid"] != null && Session["FName"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                Response.Cache.SetNoStore();
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection, Login log)
        {
            if (Session["uid"] != null && Session["FName"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "SELECT * FROM Faculty WHERE UID = @uid AND Password = @password";
                    string sql2 = "UPDATE Faculty SET lastlogin = CURRENT_TIMESTAMP WHERE facultyID = @ID";
                    int n = 0;
                    using (SqlCommand cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", log.uname);
                        cmd.Parameters.AddWithValue("@password", log.password);
                        SqlDataReader rd = cmd.ExecuteReader();
                        while (rd.Read())
                        {
                            if (rd.GetString(3) == log.uname && rd.GetString(4) == log.password)
                            {
                                Session["uid"] = rd.GetString(3);
                                Session["FName"] = rd.GetString(1);
                                Session["tableID"] = rd.GetInt32(0);
                                Session["EID"] = rd.GetInt32(5);
                                Session["FID"] = rd.GetString(9);
                                n++;
                            }
                        }
                        rd.Close();

                    }
                    if (n != 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql2, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", Convert.ToInt32(Session["tableID"]));
                            cmd.ExecuteNonQuery();
                        }
                        Response.Write(log.remember);
                        return RedirectToAction("Index");

                    }
                    else
                    {
                        Session["uid"] = null;
                        Session["tableID"] = null;
                        Session["FName"] = null;
                        Session["EID"] = null;
                        ViewBag.Status = "failed";
                        return View();
                        /*return RedirectToAction("Login", new { @loginstatus = false });*/
                    }
                }

            }
        }

        public ActionResult Index()
        {
            Response.Cache.SetNoStore();
            if (Session["uid"] == null && Session["FName"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Response.Cache.SetNoStore();
                return View();

            }
        }

        public ActionResult Logout()
        {
            Session["uid"] = null;
            Session["tableID"] = null;
            Session["FName"] = null;
            Session["EID"] = null;
            return RedirectToAction("Login");
        }

        public ActionResult Lockscreen()
        {
            Response.Cache.SetNoStore();
            return View();
        }

        public ActionResult FileUpload()
        {
            if (Session["uid"] == null && Session["FName"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                DataTable dataTable = new DataTable();
                DataTable dataTable1 = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql = "select * from Diploma_CS where ParentID = 0";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql, conn))
                    {
                        ad.Fill(dataTable);
                    }
                    string sql1 = "select * from Diploma_CS";


                    /*string sql1 = "SELECT Diploma_CS.* FROM Diploma_CS INNER JOIN Faculty ON" +
                        " Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name , '%') WHERE" +
                        " Faculty.facultyID = " + Session["tableID"];*/


                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(dataTable1);
                    }
                }
                Session["subt"] = dataTable;
                Session["subt1"] = dataTable1;

                return View();
            }

        }

        [HttpPost]
        public ActionResult FileUpload(Uploader model)
        {

            if (model.filee != null && model.filee.ContentLength > 0)
            {
                try
                {
                    string ext = Path.GetExtension(model.filee.FileName);
                    DateTime cur = DateTime.Now;
                    string dtt = cur.Year.ToString() + cur.Month.ToString() + cur.Day.ToString() + cur.Hour.ToString() + cur.Minute.ToString() + cur.Second.ToString();
                    string path = Path.Combine("D:/LMS", model.upload_type, dtt + "_" + model.title.Replace(' ', '_').Trim() + ext);

                    int parentI = 0;

                    using (SqlConnection conn = new SqlConnection(DataBase))
                    {
                        string sql2;
                        if (model.upload_type == "Assignment")
                        {
                            CultureInfo cultures = new CultureInfo("en-US");
                            String date = model.lastDateOnly;
                            String time = model.lastTimeOnly;
                            String val = date + " " + time;
                            DateTime res = Convert.ToDateTime(val, cultures);
                            sql2 = "INSERT INTO Docs_Diploma_CS ([Title], [Type], [Path], [SubjectID], [uploadedOn], [facultyID], [lastDate]) " +
                            "VALUES (@title, @type, @path, @id, CURRENT_TIMESTAMP, @facultyID, '" + res.ToString("MMMM dd yyyy h:mm tt") + "')";
                        }
                        else
                        {
                            sql2 = "INSERT INTO Docs_Diploma_CS ([Title], [Type], [Path], [SubjectID], [uploadedOn], [facultyID]) " +
                            "VALUES (@title, @type, @path, @id, CURRENT_TIMESTAMP, @facultyID)";
                        }

                        Console.WriteLine("Converted DateTime value...");
                        conn.Open();
                        string sql1 = "SELECT Id FROM Diploma_CS WHERE Name = @subject";
                        using (SqlCommand cmd = new SqlCommand(sql1, conn))
                        {
                            cmd.Parameters.AddWithValue("@subject", model.subb);
                            SqlDataReader rd = cmd.ExecuteReader();
                            while (rd.Read())
                            {
                                parentI = rd.GetInt32(0);
                            }
                            rd.Close();
                        }
                        if (parentI > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(sql2, conn))
                            {
                                cmd.Parameters.AddWithValue("@title", model.title);
                                cmd.Parameters.AddWithValue("@type", model.upload_type);
                                cmd.Parameters.AddWithValue("@path", path);
                                cmd.Parameters.AddWithValue("@id", parentI);
                                cmd.Parameters.AddWithValue("@facultyID", Convert.ToInt32(Session["ID"]));

                                cmd.ExecuteNonQuery();
                            }
                            model.filee.SaveAs(path);
                            ViewBag.UploadSatatus = "success";
                        }
                        else
                        {
                            ViewBag.UploadSatatus = "error";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.UploadSatatus = "error" + ex.Message;
                }
            }
            else
            {
                ViewBag.UploadSatatus = "nofile";
            }
            return View();
        }

        public ActionResult Assignments()
        {
            int nextID = 0;
            bool isNextID = false;

            string evalStatus = "no";

            if (TempData["aaID"] != null)
            {
                nextID = Convert.ToInt32(TempData["aaID"]);
                isNextID = true;
            }

            if (TempData["evalStatus"] != null)
            {
                evalStatus = TempData["evalStatus"].ToString();
            }

            TempData["aaID"] = null;
            TempData["evalStatus"] = null;

            DataTable data = new DataTable();
            DataTable subjects = new DataTable();


            using (SqlConnection conn = new SqlConnection(DataBase))
            {
                string sql1 = "SELECT Docs_Diploma_CS.* FROM Docs_Diploma_CS INNER JOIN " +
                    "Diploma_CS ON Diploma_CS.Id = Docs_Diploma_CS.SubjectID INNER JOIN " +
                    "Faculty ON Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name, '%') " +
                    "WHERE Docs_Diploma_CS.Type = 'Assignment' AND Faculty.facultyID = " + Session["tableID"];



                /*string sql1 = "SELECT Docs_Diploma_CS.* FROM Assignments INNER JOIN Docs_Diploma_CS ON " +
                    "Assignments.documentID = Docs_Diploma_CS.documentID INNER JOIN Diploma_CS ON " +
                    "Diploma_CS.Id = Docs_Diploma_CS.SubjectID INNER JOIN Faculty ON " +
                    "Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name, '%') WHERE Faculty.facultyID = " + Session["tableID"];*/



                string sql2 = "SELECT Id, Name FROM Diploma_CS";
                using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                {
                    ad.Fill(data);
                }

                using (SqlDataAdapter ad = new SqlDataAdapter(sql2, conn))
                {
                    ad.Fill(subjects);
                }
            }

            ViewBag.nextID = nextID;
            ViewBag.isNextID = isNextID;
            ViewBag.evalStatus = evalStatus;

            ViewData["data"] = data;
            ViewData["Subjects"] = subjects;
            return View();
        }

        public ActionResult ShowAssignment(int docID = 0)
        {
            DataTable dtAssignment = new DataTable();
            DataTable dtStudents = new DataTable();
            using (SqlConnection conn = new SqlConnection(DataBase))
            {
                conn.Open();
                string sql1 = "SELECT * FROM Assignments WHERE documentID = " + docID;
                string sql2 = "SELECT studentID, Name FROM Students WHERE course = 'Diploma' AND Branch = 'CS'";

                using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                {
                    ad.Fill(dtAssignment);
                }

                using (SqlDataAdapter ad = new SqlDataAdapter(sql2, conn))
                {
                    ad.Fill(dtStudents);
                }
            }

            ViewData["dataAssignemnts"] = dtAssignment;
            ViewData["dataStudents"] = dtStudents;
            return View();
        }

        public ActionResult Evaluate(int assignmentID)
        {
            DataTable assignment = new DataTable();
            DataTable students = new DataTable();

            using (SqlConnection conn = new SqlConnection(DataBase))
            {
                conn.Open();
                string sql1 = "SELECT * FROM Assignments WHERE assignmentID = " + assignmentID;
                using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                {
                    ad.Fill(assignment);
                }

                string sql2 = "SELECT * FROM Students WHERE course = 'Diploma' AND Branch = 'CS'";
                using (SqlDataAdapter ad = new SqlDataAdapter(sql2, conn))
                {
                    ad.Fill(students);
                }

            }

            ViewData["assignment"] = assignment;
            ViewData["students"] = students;
            return View();
        }

        public ActionResult DummyEvaluate(int assignmentID)
        {
            TempData["aaID"] = assignmentID;
            return RedirectToAction("Assignments");
        }

        [HttpPost]
        public ActionResult Evaluate(EvalAssignment model)
        {
            string status = "NaN";
            try
            {
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string marks = model.mo.ToString() + " / " + model.mm.ToString();
                    string sql1 = "UPDATE Assignments SET marks = @marks, remark = @remark," +
                        " Evaluater = @facID WHERE assignmentID = @id";

                    using (SqlCommand cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", model.Id);
                        cmd.Parameters.AddWithValue("@marks", marks);
                        cmd.Parameters.AddWithValue("@remark", model.remark);
                        cmd.Parameters.AddWithValue("@facID", Convert.ToInt32(Session["tableID"]));

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }

            TempData["evalStatus"] = status;
            return RedirectToAction("Assignments");
        }

        public ActionResult Subjects()
        {
            if (Session["tableID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                List<dynamic> data = (List<dynamic>)TempData["UploadData"];

                if (data != null)
                {
                    ViewBag.hasUpload = true;
                    ViewBag.uploadType = data[1];
                    ViewBag.subID = data[0];
                }
                else
                {
                    ViewBag.hasUpload = false;
                }

                DataTable subjets = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "SELECT Diploma_CS.* FROM Diploma_CS INNER JOIN Faculty ON " +
                        "Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name, '%') WHERE" +
                        " Faculty.facultyID = " + Session["tableID"];

                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(subjets);
                    }

                }
                ViewData["Subjects"] = subjets;
                return View();
            }
        }

        public ActionResult USyllabus(int subId = 0)
        {
            if (Session["tableID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                DataTable data = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    string sql1 = "SELECT Docs_Diploma_CS.* FROM Docs_Diploma_CS INNER JOIN Diploma_CS ON " +
                   "Docs_Diploma_CS.SubjectID = Diploma_CS.Id INNER JOIN Faculty ON " +
                   "Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name, '%') WHERE " +
                   "Docs_Diploma_CS.Type = 'Syllabus' AND Faculty.facultyID = " + Session["tableID"];

                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(data);
                    }

                    ViewData["Data"] = data;
                }
                return View();
            }
        }

        public ActionResult UNotes(int subId = 0)
        {
            if (Session["tableID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                DataTable data = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    string sql1 = "SELECT Docs_Diploma_CS.* FROM Docs_Diploma_CS INNER JOIN Diploma_CS ON " +
                   "Docs_Diploma_CS.SubjectID = Diploma_CS.Id INNER JOIN Faculty ON " +
                   "Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name, '%') WHERE " +
                   "Docs_Diploma_CS.Type = 'Notes' AND Faculty.facultyID = " + Session["tableID"];

                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(data);
                    }

                    ViewData["Data"] = data;
                }
                return View();
            }
        }

        public ActionResult UAssignments(int subId = 0)
        {
            if (Session["tableID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                DataTable data = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    string sql1 = "SELECT Docs_Diploma_CS.* FROM Docs_Diploma_CS INNER JOIN Diploma_CS ON " +
                   "Docs_Diploma_CS.SubjectID = Diploma_CS.Id INNER JOIN Faculty ON " +
                   "Faculty.Subjects LIKE CONCAT('%', Diploma_CS.Name, '%') WHERE " +
                   "Docs_Diploma_CS.Type = 'Assignment' AND Faculty.facultyID = "
                   + Session["tableID"] + " AND Diploma_CS.Id = " + subId;

                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(data);
                    }

                    ViewData["Data"] = data;
                }
                return View();
            }
        }

        public ActionResult Delete(string path)
        {
            FileInfo file = new FileInfo(path);
            string fileName = file.Name;
            using (SqlConnection conn = new SqlConnection(DataBase))
            {
                int docID = 0;
                conn.Open();
                string sql1 = "SELECT documentID FROM Docs_Diploma_CS WHERE Path LIKE '%" + fileName + "%'";
                using (SqlCommand cmd = new SqlCommand(sql1, conn))
                {
                    SqlDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        docID = r.GetInt32(0);
                    }
                    r.Close();
                }

                string sql2 = "DELETE FROM Docs_Diploma_CS WHERE documentID = " + docID;
                using (SqlCommand cmd = new SqlCommand(sql2, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string sql3 = "DELETE FROM Assignments WHERE documentID = " + docID;
                using (SqlCommand cmd = new SqlCommand(sql3, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                if (docID > 0)
                {
                    file.Delete();
                    TempData["Delete_st"] = "Done";
                }
                else
                {
                    TempData["Delete_st"] = "No";
                }
            }
            return RedirectToAction("Subjects");
        }

        public ActionResult DummyUpload(int subID, string type)
        {
            List<dynamic> d = new List<dynamic>();

            d.Add(subID);
            d.Add(type);

            TempData["UploadData"] = d;

            return RedirectToAction("Subjects");
        }

        public ActionResult Upload(int subID, string type)
        {
            using (SqlConnection conn = new SqlConnection(DataBase))
            {
                conn.Open();


                int subP = 0;
                List<string> data = new List<string>();
                data.Add(subID.ToString());
                data.Add(type.ToString());

                string sql1 = "SELECT Name, ParentID FROM Diploma_CS WHERE Id = @subid";
                using (SqlCommand cmd = new SqlCommand(sql1, conn))
                {
                    cmd.Parameters.AddWithValue("@subid", subID);

                    SqlDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        data.Add(r.GetString(0));

                        subP = r.GetInt32(1);
                    }
                    r.Close();
                }


                using (SqlCommand cmd = new SqlCommand(sql1, conn))
                {
                    cmd.Parameters.AddWithValue("@subid", subP);

                    SqlDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        data.Add(r.GetString(0));
                        subP = r.GetInt32(1);
                    }

                    r.Close();
                }


                using (SqlCommand cmd = new SqlCommand(sql1, conn))
                {
                    cmd.Parameters.AddWithValue("@subid", subP);

                    SqlDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        data.Add(r.GetString(0));
                        subP = r.GetInt32(1);
                    }
                    r.Close();
                }


                ViewData["data"] = data;

            }
            return View();
        }

        [HttpPost]
        public ActionResult Upload(UploadFromSubjects model)
        {
            if (model.filee != null && model.filee.ContentLength > 0)
            {
                try
                {
                    string ext = Path.GetExtension(model.filee.FileName);
                    DateTime cur = DateTime.Now;
                    string dtt = cur.Year.ToString() + cur.Month.ToString() + cur.Day.ToString() + cur.Hour.ToString() + cur.Minute.ToString() + cur.Second.ToString();
                    string path = Path.Combine("D:/LMS", model.type, dtt + "_" + model.title.Replace(' ', '_').Trim() + ext);


                    using (SqlConnection conn = new SqlConnection(DataBase))
                    {
                        string sql2;
                        if (model.type == "Assignment")
                        {
                            CultureInfo cultures = new CultureInfo("en-US");
                            String date = model.lastDateOnly;
                            String time = model.lastTimeOnly;
                            String val = date + " " + time;
                            DateTime res = Convert.ToDateTime(val, cultures);
                            sql2 = "INSERT INTO Docs_Diploma_CS ([Title], [Type], [Path], [SubjectID], [uploadedOn], [facultyID], [lastDate]) " +
                            "VALUES (@title, @type, @path, @id, CURRENT_TIMESTAMP, @facultyID, '" + res.ToString("MMMM dd yyyy h:mm tt") + "')";
                        }
                        else
                        {
                            sql2 = "INSERT INTO Docs_Diploma_CS ([Title], [Type], [Path], [SubjectID], [uploadedOn], [facultyID]) " +
                            "VALUES (@title, @type, @path, @id, CURRENT_TIMESTAMP, @facultyID)";
                        }

                        Console.WriteLine("Converted DateTime value...");
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(sql2, conn))
                        {
                            cmd.Parameters.AddWithValue("@title", model.title);
                            cmd.Parameters.AddWithValue("@type", model.type);
                            cmd.Parameters.AddWithValue("@path", path);
                            cmd.Parameters.AddWithValue("@id", model.subId);
                            cmd.Parameters.AddWithValue("@facultyID", Convert.ToInt32(Session["ID"]));

                            cmd.ExecuteNonQuery();

                            model.filee.SaveAs(path);
                            ViewBag.UploadSatatus = "success";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.UploadSatatus = "error" + ex.Message;
                }
            }
            else
            {
                ViewBag.UploadSatatus = "nofile";
            }
            return RedirectToAction("Subjects");
        }

        public ActionResult Plan(int subId = 0)
        {
            //
            return View();
        }
        public ActionResult createPlan(int subId)
        {
            return View();
        }
    }
}
